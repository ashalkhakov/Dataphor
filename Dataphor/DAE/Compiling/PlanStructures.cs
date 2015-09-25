﻿/*
	Alphora Dataphor
	© Copyright 2000-2009 Alphora
	This file is licensed under a modified BSD-license which can be found here: http://dataphor.org/dataphor_license.txt
*/

using System;
using System.Collections.Generic;
using System.Text;

namespace Alphora.Dataphor.DAE.Compiling
{
	public enum StatementType { Select, Insert, Update, Delete, Assignment }
	
	public class StatementContext : System.Object
	{
		public StatementContext(StatementType statementType) : base()
		{
			_statementType = statementType;
		}
		
		private StatementType _statementType;
		public StatementType StatementType { get { return _statementType; } }
	}
	
	public class StatementContexts : List<StatementContext> { }

	public class SecurityContext : System.Object
	{
		public SecurityContext(Schema.User user) : base()
		{
			_user = user;
		}
		
		private Schema.User _user;
		public Schema.User User { get { return _user; } }
		internal void SetUser(Schema.User user)
		{
			_user = user;
		}
	}
	
	public class SecurityContexts : List<SecurityContext> { }

	public class CursorContext : System.Object
	{
		public CursorContext() : base() {}
		public CursorContext(CursorType cursorType, CursorCapability capabilities, CursorIsolation isolation) : base()
		{
			_cursorType = cursorType;
			_cursorCapabilities = capabilities;
			_cursorIsolation = isolation;
		}
		// CursorType
		private CursorType _cursorType;
		public CursorType CursorType
		{
			get { return _cursorType; }
			set { _cursorType = value; }
		}
		
		// CursorCapabilities
		private CursorCapability _cursorCapabilities;
		public CursorCapability CursorCapabilities
		{
			get { return _cursorCapabilities; }
			set { _cursorCapabilities = value; }
		}
		
		// CursorIsolation
		private CursorIsolation _cursorIsolation;
		public CursorIsolation CursorIsolation
		{
			get { return _cursorIsolation; }
			set { _cursorIsolation = value; }
		}
	}
	
	public class CursorContexts : List<CursorContext> { }

    // Loop contexts
    public class LoopContext : System.Object
    {
        public LoopContext(System.Reflection.Emit.Label cont, System.Reflection.Emit.Label brk) : base()
        {
            _continue = cont;
            _break = brk;
        }

        private System.Reflection.Emit.Label _continue;
        private System.Reflection.Emit.Label _break;

        public System.Reflection.Emit.Label Continue { get { return _continue; } }
        public System.Reflection.Emit.Label Break { get { return _break; } }

    }

    public class LoopContexts : List<LoopContext> { }

    // Exception contexts
    public class ExceptionContext : System.Object
    {
        public ExceptionContext(System.Reflection.Emit.Label leave) : base()
        {
            _leave = leave;
        }

        private System.Reflection.Emit.Label _leave;

        public System.Reflection.Emit.Label Leave { get { return _leave; } }
    }

    public class ExceptionContexts : List<ExceptionContext> { }
}
