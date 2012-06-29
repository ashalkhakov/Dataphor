﻿/*
	Dataphor
	© Copyright 2000-2012 Alphora
	This file is licensed under a modified BSD-license which can be found here: http://dataphor.org/dataphor_license.txt
*/
using System;
using System.IO;
using System.Collections;
using System.Collections.Specialized;

using Alphora.Dataphor.BOP;
using Alphora.Dataphor.DAE.Schema;
using Alphora.Dataphor.DAE.Streams;
using Alphora.Dataphor.DAE.Language;
using Alphora.Dataphor.DAE.Language.D4;
using Alphora.Dataphor.DAE.Compiling;
using Alphora.Dataphor.DAE.Runtime;
using Alphora.Dataphor.DAE.Runtime.Data;
using Alphora.Dataphor.DAE.Runtime.Instructions;
using Alphora.Dataphor.DAE.Server;
using Alphora.Dataphor.DAE.Device.Memory;
using Schema = Alphora.Dataphor.DAE.Schema;
using System.Collections.Generic;
using Alphora.Fastore.Client;


namespace Alphora.Dataphor.DAE.Device.Fastore
{ 
	//TODO: FIX NASTY ASSUMPTIONS! (First column is ID column -- This is clearly wrong, but I'm just getting things up and running)
	public class FastoreTable : System.Object
	{
        private static int[] PodIdColumn = { 300 };
        private static int[] ColumnColumns = { 0, 1, 2, 3, 4 };
        private static int[] PodColumnColumns = { 400, 401 };

		//Tied Directly to device for time being...
		public FastoreTable(IValueManager manager, Schema.TableVar tableVar, FastoreDevice device)
		{
			TableVar = tableVar;
			Device = device;

			EnsureColumns();
		}

		public FastoreDevice Device;
		public Schema.TableVar TableVar;

		private int[] _columns;
		public int[] Columns { get { return _columns; } }


		protected string MapTypeNames(string tname)
		{
			string name = "";
			switch (tname)
			{
				case "System.String":
					name = "String";
					break;
				case "System.Integer":
					name = "Int";
					break;
				case "System.Boolean":
					name = "Bool";
					break;
				case "System.Long":
					name = "Long";
					break;
				default:
					break;
			}

			return name;
		}

		protected void EnsureColumns()
		{
            //Pull a list of current pods so we know where to distribute new columns.
            Range podQuery = new Range();

            //300 is the pods column
            podQuery.ColumnID = PodIdColumn[0];
            podQuery.Ascending = true;

            var podIds = Device.Database.GetRange(PodIdColumn, podQuery, int.MaxValue);
            if (podIds.Count == 0)
                throw new Exception("FastoreDevice can't create a new table. Hive has no workers. Hive must be initialized first.");

            //Start distributing on a random pod. Otherwise, we will always start on the first pod
            int startPod = new Random().Next(podIds.Count - 1);

            //This is so we have quick access to all the ids (for queries). Otherwise, we have to iterate the 
            //TableVar Columns and pull the id each time.
            List<int> columnIds = new List<int>();
			for (int i = 0; i < TableVar.DataType.Columns.Count; i++)
			{
				var col = TableVar.DataType.Columns[i];

				columnIds.Add(col.ID);

                Range query = new Range();
                query.ColumnID = ColumnColumns[0];
                query.Start = new RangeBound() { Bound = col.ID, Inclusive = true };
                query.End = new RangeBound() { Bound = col.ID, Inclusive = true };

                var result = Device.Database.GetRange(new int[] { ColumnColumns[0] }, query , int.MaxValue);

                if (result.Count == 0)
                {
                    //These two includes should probably happen in a transaction so no one see any columns that don't have
                    //Repos.
                    Device.Database.Include
                    (
                        ColumnColumns,
                        col.ID,
                        new object[] { col.ID, col.Description, MapTypeNames(col.DataType.Name), "Int", i == 1 }
                    );

                    Device.Database.Include
                    (
                        PodColumnColumns,
                        0, /* need a unique rowId -- This is a many to many table, so it can't be either the podId or the ColumnId */ 
                        new object[] { podIds[startPod++ % podIds.Count].Values[0], col.ID }
                    );

                    Schema.Order order = new Schema.Order();
                    Schema.TableVarColumn tvc = new TableVarColumn(TableVar.DataType.Columns[i]);
                    Schema.OrderColumn ordercolumn = new OrderColumn(tvc, true);
                    order.Columns.Add(ordercolumn);

                    if (!TableVar.Orders.Contains(order))
                    {
                        TableVar.Orders.Add(order);
                    }
                }
			}

			_columns = columnIds.ToArray();
		}

		public void Insert(IValueManager manager, Row row, Database db)
		{
			if (row.HasValue(0))
			{
				object[] items = ((NativeRow)row.AsNative).Values;

				db.Include(_columns, items[0], items);
			}
		}

		public void Update(IValueManager manager, Row oldrow, Row newrow, Database db)
		{
			if (oldrow.HasValue(0))
			{
				var id = oldrow[0];
				for (int i = 0; i < _columns.Length; i++)
				{
					db.Exclude(_columns, id);
				}
			}

			if (newrow.HasValue(0))
			{
				object[] items = ((NativeRow)newrow.AsNative).Values;
				db.Include(_columns, items[0], items);
			}
		}

		public void Delete(IValueManager manager, Row row, Database db)
		{
			//If no value at zero, no ID (Based on our wrong assumptions)
			if (row.HasValue(0))
			{
				db.Exclude(_columns, row[0]);
			}
		}

		public void Drop(IValueManager manager)
		{
			foreach (var col in _columns)
			{
                //Pull a list of the current repos so we can drop them all.
                Range repoQuery = new Range();

                //401 is the column we want on the Pod-Column table
                repoQuery.ColumnID = PodColumnColumns[1];
                repoQuery.Ascending = true;
                repoQuery.Start = new RangeBound() { Bound = col, Inclusive = true };
                repoQuery.End = new RangeBound() { Bound = col, Inclusive = true };                

                var repoIds = Device.Database.GetRange(new int[] { PodColumnColumns[1] }, repoQuery, int.MaxValue);

                for (int i = 0; i < repoIds.Count; i++)
                {
                    Device.Database.Exclude(PodColumnColumns, repoIds[i].ID);
                }

				Range query = new Range();
                query.ColumnID = 0;
                query.Start = new RangeBound() { Bound = col, Inclusive = true };
                query.End = new RangeBound() { Bound = col, Inclusive = true };

                var columnExists = Device.Database.GetRange(new int[] { ColumnColumns[0] }, query, int.MaxValue);

                if (columnExists.Count > 0)
                {
                    Device.Database.Exclude(ColumnColumns, col);
                }
			}
		}
	}
}