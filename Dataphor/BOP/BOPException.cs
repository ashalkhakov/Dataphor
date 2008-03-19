/*
	Dataphor
	© Copyright 2000-2008 Alphora
	This file is licensed under a modified BSD-license which can be found here: http://dataphor.org/dataphor_license.txt
*/
using System;
using System.Resources;
using System.Reflection;

namespace Alphora.Dataphor.BOP
{
	[Serializable]
	public class BOPException : DataphorException
	{
		public enum Codes : int
		{
			/// <summary>Error code 101100: "A referenced object ({0}) is not found."</summary>
			ReferenceNotFound = 101100,

			/// <summary>Error code 101101: "Multiple attributes found.  Expecting single attribute ({0})."</summary>
			ExpectingSingleAttribute = 101101,

			/// <summary>Error code 101102: "Type ({0}) does not have a designated name.  Use the [PublishName] attribute to specify the type's name."</summary>
			InvalidElementName = 101102,

			/// <summary>Error code 101103: "Invalid attribute name ({0})."</summary>
			InvalidAttribute = 101103,

			/// <summary>Error code 101105: "A constructor's argument source ({0}) is not found in type ({1})."</summary>
			ConstructorArgumentRefNotFound = 101105,

			/// <summary>Error code 101106: "A list name was not specified, and no default list is identified for type ({0})."</summary>
			DefaultListNotFound = 101106,

			/// <summary>Error code 101107: "Unable to construct instance of type ({0})."</summary>
			UnableToConstruct = 101107,

			/// <summary>Error code 101108: "A constructor matching the default constructor signature ({0}) is not found."</summary>
			DefaultConstructorNotFound = 101108,

			/// <summary>Error code 101109: "Invalid DILX document node ({0})."</summary>
			InvalidNode = 101109,

			/// <summary>Error code 101110: "'document' element is required in a DILX document."</summary>
			DocumentElementRequired = 101110,

			/// <summary>Error code 101111: "'document' element must be the last element in a DILX document."</summary>
			DocumentElementLast = 101111,

			/// <summary>Error code 101112: "Document requests that property ({0}) of type ({1}) is to be set to its default, but no [DefaultValue] attribute is specified for the property."</summary>
			DefaultNotSpecified = 101112,

			/// <summary>Error code 101113: "Constructor argument source ({0}) is not specified in type ({1}).  [PublishSource] attribute is missing for the constructor's argument." </summary>
			ConstructorArgumentRefNotSpecified = 101113,

			/// <summary>Error code 101114: "Serialization/Deserialization of delegate (event) types is not supported."</summary>
			DelegatesNotSupported = 101114,

			/// <summary>Error code 101115: "Warning: The read element type name ({0}) doesn't match the instance's class name ({1})."</summary>
			TypeNameMismatch = 101115
		};

		// Resource manager for this exception class
		private static ResourceManager FResourceManager = new ResourceManager("Alphora.Dataphor.BOP.BOPException", typeof(BOPException).Assembly);

		// Constructors
		public BOPException(Codes AErrorCode) : base(FResourceManager, (int)AErrorCode, ErrorSeverity.Application, null, null) {}
		public BOPException(Codes AErrorCode, params object[] AParams) : base(FResourceManager, (int)AErrorCode, ErrorSeverity.Application, null, AParams) {}
		public BOPException(Codes AErrorCode, Exception AInnerException) : base(FResourceManager, (int)AErrorCode, ErrorSeverity.Application, AInnerException, null) {}
		public BOPException(Codes AErrorCode, Exception AInnerException, params object[] AParams) : base(FResourceManager, (int)AErrorCode, ErrorSeverity.Application, AInnerException, AParams) {}
		public BOPException(Codes AErrorCode, ErrorSeverity ASeverity) : base(FResourceManager, (int)AErrorCode, ASeverity, null, null) {}
		public BOPException(Codes AErrorCode, ErrorSeverity ASeverity, params object[] AParams) : base(FResourceManager, (int)AErrorCode, ASeverity, null, AParams) {}
		public BOPException(Codes AErrorCode, ErrorSeverity ASeverity, Exception AInnerException) : base(FResourceManager, (int)AErrorCode, ASeverity, AInnerException, null) {}
		public BOPException(Codes AErrorCode, ErrorSeverity ASeverity, Exception AInnerException, params object[] AParams) : base(FResourceManager, (int)AErrorCode, ASeverity, AInnerException, AParams) {}
		public BOPException(System.Runtime.Serialization.SerializationInfo AInfo, System.Runtime.Serialization.StreamingContext AContext) : base(AInfo, AContext) {}
	}
}
