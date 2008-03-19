/*
	Dataphor
	© Copyright 2000-2008 Alphora
	This file is licensed under a modified BSD-license which can be found here: http://dataphor.org/dataphor_license.txt
*/
namespace Alphora.Dataphor.DAE.Runtime.Data
{
	using System;
	using System.Collections;
	using System.Diagnostics;

	using Alphora.Dataphor;
	using Alphora.Dataphor.DAE;
	using Alphora.Dataphor.DAE.Server;
	using Alphora.Dataphor.DAE.Streams;
	using Alphora.Dataphor.DAE.Runtime;
	using Alphora.Dataphor.DAE.Runtime.Data;
	using Alphora.Dataphor.DAE.Runtime.Instructions;
	using Alphora.Dataphor.DAE.Language.D4;
	using Alphora.Dataphor.DAE.Device.Memory;
	using Schema = Alphora.Dataphor.DAE.Schema;

    public class ExtendTable : Table
    {
		public ExtendTable(ExtendNode ANode, ServerProcess AProcess) : base(ANode, AProcess){}

        public new ExtendNode Node { get { return (ExtendNode)FNode; } }
        
		protected DataVar FSourceObject;        
		protected Table FSourceTable;
		protected Row FSourceRow;
        
        protected override void InternalOpen()
        {
			FSourceObject = Node.Nodes[0].Execute(Process);
			try
			{
				FSourceTable = (Table)FSourceObject.Value;
				FSourceRow = new Row(Process, FSourceTable.DataType.RowType);
			}
			catch
			{
				FSourceObject.Value.Dispose();
				throw;
			}
        }
        
        protected override void InternalClose()
        {
			if (FSourceTable != null)
			{
				FSourceTable.Dispose();
				FSourceTable = null;
			}

            if (FSourceRow != null)
            {
				FSourceRow.Dispose();
                FSourceRow = null;
            }
        }
        
        protected override void InternalReset()
        {
            FSourceTable.Reset();
        }
        
        protected override void InternalSelect(Row ARow)
        {
            FSourceTable.Select(FSourceRow);
            FSourceRow.CopyTo(ARow);
            int LIndex;
            int LColumnIndex;
	        Process.Context.Push(new DataVar(String.Empty, FSourceRow.DataType, FSourceRow));
            try
            {
				for (LIndex = 1; LIndex < Node.Nodes.Count; LIndex++)
				{
					LColumnIndex = ARow.DataType.Columns.IndexOfName(Node.DataType.Columns[Node.ExtendColumnOffset + LIndex - 1].Name);
					if (LColumnIndex >= 0)
					{
						DataVar LObject = Node.Nodes[LIndex].Execute(Process);
						if (LObject.DataType is Schema.ScalarType)
							((Schema.ScalarType)LObject.DataType).ValidateValue(Process, LObject);
						ARow[LColumnIndex] = LObject.Value;
					}
				}
            }
            finally
            {
				Process.Context.Pop();
            }
        }
        
        protected override bool InternalNext()
        {
            return FSourceTable.Next();
        }
        
        protected override bool InternalBOF()
        {
            return FSourceTable.BOF();
        }
        
        protected override bool InternalEOF()
        {
            return FSourceTable.EOF();
        }
    }
}