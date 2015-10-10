/*
	Dataphor
	© Copyright 2000-2008 Alphora
	This file is licensed under a modified BSD-license which can be found here: http://dataphor.org/dataphor_license.txt
*/
#define NILPROPOGATION

namespace Alphora.Dataphor.DAE.Runtime.Instructions
{
	using System; 
	using System.Reflection;

	using Alphora.Dataphor.DAE;
	using Alphora.Dataphor.DAE.Server;
	using Alphora.Dataphor.DAE.Streams;
	using Alphora.Dataphor.DAE.Language;
	using Alphora.Dataphor.DAE.Language.D4;
	using Alphora.Dataphor.DAE.Runtime;
	using Alphora.Dataphor.DAE.Runtime.Data;
	using Alphora.Dataphor.DAE.Runtime.Instructions;
	using Alphora.Dataphor.DAE.Device.Catalog;
	using Schema = Alphora.Dataphor.DAE.Schema;
	
	public sealed class BooleanUtility
	{
        public static bool? UnboxBoolean(object tempValue)
        {
            if (tempValue == null)
                return new Nullable<bool>();
            else
                return new Nullable<bool>((bool)tempValue);
        }

		public static bool? Not(bool? tempValue)
		{
			#if NILPROPOGATION
			if (tempValue == null)
				return null;
			else
			#endif
				return !((bool)tempValue);
		}

		public static bool? And(bool? leftValue, bool? rightValue)
		{
			#if NILPROPOGATION
			if ((leftValue == null))
			{
				if ((rightValue != null) && !(bool)rightValue)
					return false;
				else
					return null;
			}
			else
			{
				if ((bool)leftValue)
					if (rightValue == null)
						return null;
					else
						return (bool)rightValue;
				else
					return false;
			}
			#else
			return (bool)ALeftValue && (bool)ARightValue;
			#endif
		}
		
		public static bool? Or(bool? leftValue, bool? rightValue)
		{
			#if NILPROPOGATION
			if (leftValue == null)
			{
				if ((rightValue != null) && (bool)rightValue)
					return true;
				else
					return null;
			}
			else
			{
				if ((bool)leftValue)
					return true;
				else if (rightValue == null)
					return null;
				else
					return (bool)rightValue;
			}
			#else
			return new Scalar(AProcess, ADataType, ALeftValue.AsBoolean || ARightValue.AsBoolean);
			#endif
		}

		public static bool? Xor(bool? leftValue, bool? rightValue)
		{
			return 
				Or
				(
					And
					(
						leftValue, 
						Not(rightValue)
					), 
					And
					(
						Not(leftValue), 
						rightValue
					)
				);
		}		
	}

	/// <remarks>operator System.iNot(System.Boolean) : System.Boolean</remarks>    
    public class BooleanNotNode : UnaryInstructionNode
    {
		public override object InternalExecute(Program program, object argument)
		{
			return BooleanUtility.Not(BooleanUtility.UnboxBoolean(argument));
		}

        public static bool? InternalExecute(Program program, bool? argument)
        {
            return BooleanUtility.Not(argument);
        }
    }

    /// <remarks> 
    /// operator System.iAnd(System.Boolean, System.Boolean) : System.Boolean
    /// Be aware!!! D4 does NOT do short circuit evaluation...
    /// </remarks>
    public class BooleanAndNode : BinaryInstructionNode
    {
		public override object InternalExecute(Program program, object argument1, object argument2)
		{
			return BooleanUtility.And(BooleanUtility.UnboxBoolean(argument1), BooleanUtility.UnboxBoolean(argument2));
		}

        public static bool? InternalExecute(Program program, bool? argument1, bool? argument2)
        {
            return BooleanUtility.And(argument1, argument2);
        }
    }

    /// <remarks>operator System.iOr(System.Boolean, System.Boolean) : System.Boolean</remarks>
    public class BooleanOrNode : BinaryInstructionNode
    {
		public override object InternalExecute(Program program, object argument1, object argument2)
		{
			return BooleanUtility.Or(BooleanUtility.UnboxBoolean(argument1), BooleanUtility.UnboxBoolean(argument2));
		}

        public static bool? InternalExecute(Program program, bool? argument1, bool? argument2)
        {
            return BooleanUtility.Or(argument1, argument2);
        }
    }

    /// <remarks>operator System.iXor(System.Boolean, System.Boolean) : System.Boolean</remarks>
    public class BooleanXorNode : BinaryInstructionNode
    {
		public override object InternalExecute(Program program, object argument1, object argument2)
		{
			return BooleanUtility.Xor(BooleanUtility.UnboxBoolean(argument1), BooleanUtility.UnboxBoolean(argument2));
		}

        public static bool? InternalExecute(Program program, bool? argument1, bool? argument2)
        {
            return BooleanUtility.Xor(argument1, argument2);
        }
    }
}
