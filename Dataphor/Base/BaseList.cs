/*
	Dataphor
	© Copyright 2000-2008 Alphora
	This file is licensed under a modified BSD-license which can be found here: http://dataphor.org/dataphor_license.txt
*/
using System;
using System.Collections;

namespace Alphora.Dataphor
{
	/// <summary>
	///		<c>List</c> which can "own" it's chidren and provides internal validate and
	///		notification of add/remove operations
	///	</summary>
	/// <remarks>
	///		<c>List</c> can manage the life of it's contained items by taking "ownership"
	///		of them.  If an item is added to the list that implements 
	///		<see cref="IDisposable"><c>IDisposable</c></see>, it will be "disposed" when the
	///		list is disposed.  <c>List</c> can optionally disallow null items.
	///	</remarks>
	/// <seealso cref="DisposableList.ItemsOwned"/>
	[Serializable]
	public class List : ArrayList
	{
		public List() : base() {}
		
		public List(bool AAllowNulls) : base(4)
		{
			FAllowNulls = AAllowNulls;
		}

		protected bool FAllowNulls;
		/// <summary> <c>AllowNulls</c> determines whether the list can contain null references. </summary>
		/// <remarks>
		///		If this property is changes while items are in the list, the items 
		///		are validated.
		/// </remarks>
		public bool AllowNulls
		{
			get { return FAllowNulls; }
			set
			{
				if (FAllowNulls != value)
				{
					if (!value)
						foreach(object LItem in this)
							InternalValidate(LItem, false);
					FAllowNulls = value;
				}
			}
		}
		
		/// <summary> Changes the index of an item. </summary>
		public virtual void Move(int AOldIndex, int ANewIndex)
		{
			Insert(ANewIndex, RemoveItemAt(AOldIndex));
		}
		
		/// <summary>
		///		<c>InternalValidate</c> is used internally by List to test potential validation
		///		scenarios against the list.
		///	</summary>
		protected void InternalValidate(object AValue, bool AAllowNulls)
		{
			if (!AAllowNulls && (AValue == null))
				throw new BaseException(BaseException.Codes.CannotAddNull);
		}
		
		/// <summary> Validate is called before an item is added or set in a List </summary>
		/// <remarks>
		///		The default behavior is to throw on an attempt to add a null reference.  Override 
		///		this method and throw an exception in order to perform other item validation for
		///		the list.
		/// </remarks>
		protected virtual void Validate(object AValue)
		{
			InternalValidate(AValue, FAllowNulls);
		}
		
		/// <summary> Adding is called when an item is added to the list </summary>
		/// <remarks>
		///		This should NOT be used for validation, but is a good place to put 
		///		code that interacts with items in the list.  
		///	</remarks>
		protected virtual void Adding(object AValue, int AIndex){}
		
		/// <summary> Removing is called when an item is being removed from the List </summary>
		protected virtual void Removing(object AValue, int AIndex){}
		
		/// <summary> ArrayList override - captures notification/validation </summary>
		public override int Add(object AValue)
		{
			int LIndex = Count;
			Insert(LIndex, AValue);
			return LIndex;
		}
		
		/// <summary> ArrayList override - captures notification/validation </summary>
		public override void AddRange(ICollection ACollection)
		{
			foreach (object LObject in ACollection)
				Add(LObject);
		}
		
		/// <summary> ArrayList override - captures notification/validation </summary>
		public override void Insert(int AIndex, object AValue)
		{
			Validate(AValue);
			base.Insert(AIndex, AValue);
			Adding(AValue, AIndex);
		}

		/// <summary> ArrayList override - captures notification/validation </summary>
		public override void InsertRange(int AIndex, ICollection ACollection)
		{
			for (int LIndex = 0; LIndex < ACollection.Count; LIndex++)
				Insert(AIndex + LIndex, ((IList)ACollection)[LIndex]);
		}
		
		/// <summary> ArrayList override - captures notification/validation </summary>
		public override void Remove(object AValue)
		{
		    RemoveAt(IndexOf(AValue));
		}

		/// <summary>Removes AValue if it is found in the list, does nothing otherwise.</summary>		
		public void SafeRemove(object AValue)
		{
			int LIndex = IndexOf(AValue);
			if (LIndex >= 0)
				Remove(AValue);
		}
		
		public virtual object RemoveItemAt(int AIndex)
		{
			object LObject = base[AIndex];
			try
			{
				Removing(LObject, AIndex);
			}
			finally
			{
				base.RemoveAt(AIndex);
			}
			return LObject;
		}

		/// <summary> ArrayList override - captures notification/validation </summary>
		public override void RemoveAt(int AIndex)
		{
			RemoveItemAt(AIndex);
		}
		
		/// <summary> ArrayList override - captures notification/validation </summary>
		public override void RemoveRange(int AIndex, int ACount)
		{
			for (int LIndex = 0; LIndex < ACount; LIndex++)
				RemoveAt(AIndex);
		}
		
		/// <summary> ArrayList override - captures notification/validation </summary>
		public override void Clear()
		{
			while (Count > 0)
				RemoveAt(0);
		}
		
		/// <summary> ArrayList override - captures notification/validation </summary>
		public override void SetRange(int AIndex, ICollection ACollection)
		{
			for (int LIndex = 0; LIndex < ACollection.Count; LIndex++)
				this[LIndex + AIndex] = ((IList)ACollection)[LIndex];
		}
		
		/// <summary> ArrayList override - captures notification/validation </summary>
		public override object this[int AIndex]
		{
			get { return base[AIndex]; }
			set
			{
				RemoveAt(AIndex);
				Insert(AIndex, value);
			}
		}
	}
	
	[Serializable]
	public class DisposableList : List, IDisposable
	{
		public DisposableList() : base(){}
		
		/// <summary> Allows the initialization of the ItemsOwned and AllowNulls properties. </summary>
		/// <param name="AItemsOwned"> See <see cref="DisposableList.ItemsOwned"/> </param>
		/// <param name="AAllowNulls"> See <see cref="List.AllowNulls"/> </param>
		public DisposableList(bool AItemsOwned, bool AAllowNulls) : base(AAllowNulls)
		{
			FItemsOwned = AItemsOwned;
		}
		
		protected bool FItemsOwned = true;
		/// <summary>
		///		ItemsOwned controls whether or not the List "owns" the contained items.  
		///		"Owns" means that if the item supports the IDisposable interface, the will be
		///		disposed when the list is disposed or when an item is removed.  
		///	</summary>
		public bool ItemsOwned
		{
			get { return FItemsOwned; }
			set { FItemsOwned = value; }
		}

		/// <summary> <c>ItemDispose</c> is called by contained items when they are disposed. </summary>
		/// <remarks>
		///		This method simply removes the item from the list.  <c>ItemDispose</c> is 
		///		only called if the item is not disposed by this list.
		///	</remarks>
		protected virtual void ItemDispose(object ASender, EventArgs AArgs)
		{
			Disown(ASender);
			//Remove(ASender);
		}
		
		///	<remarks> Hooks the Disposed event of the item if the item implements IDisposable. </remarks>
		protected override void Adding(object AValue, int AIndex)
		{
			if (AValue is IDisposableNotify)
				((IDisposableNotify)AValue).Disposed += new EventHandler(ItemDispose);
		}

		/// <remarks> If the item is owned, it is disposed. </remarks>
		protected override void Removing(object AValue, int AIndex)
		{
			if (AValue is IDisposableNotify)
        	    ((IDisposableNotify)AValue).Disposed -= new EventHandler(ItemDispose);

        	if (FItemsOwned && !(FDisowning) && (AValue is IDisposable))
		        ((IDisposable)AValue).Dispose();
		}

		public override void Move(int AOldIndex, int ANewIndex)
		{
			FDisowning = true;
			try
			{
				base.Move(AOldIndex, ANewIndex);
			}
			finally
			{
				FDisowning = false;
			}
		}
		
		/// <summary> IDisposable implementation </summary>
		public event EventHandler Disposed;

		/// <summary> IDisposable implementation </summary>
		public void Dispose()
		{
			#if USEFINALIZER
			System.GC.SuppressFinalize(this);
			#endif
			Dispose(true);
		}

		protected virtual void Dispose(bool ADisposing)
		{
			FDisposed = true;
			if (Disposed != null)
				Disposed(this, EventArgs.Empty);

			Exception LException = null;
			while (Count > 0)
				try
				{
					RemoveAt(0);
				}
				catch (Exception E)
				{
					LException = E;
				}
				
			if (LException != null)
				throw LException;
		}

		#if USEFINALIZER
		~DisposableList()
		{
			#if THROWINFINALIZER
			throw new BaseException(BaseException.Codes.FinalizerInvoked);
			#else
			Dispose();
			#endif
		}
		#endif

		protected bool FDisposed;
		public bool IsDisposed { get { return FDisposed; } }

		protected bool FDisowning;
		
		/// <summary> Removes the specified object without disposing it. </summary>
		public virtual object Disown(object AValue)
		{
			FDisowning = true;
			try
			{
				Remove(AValue);
				return AValue;
			}
			finally
			{
				FDisowning = false;
			}
		}
		
		/// <summary> Removes the specified object index without disposing it. </summary>
		public virtual object DisownAt(int AIndex)
		{
			object LValue = this[AIndex];
			FDisowning = true;
			try
			{
				RemoveAt(AIndex);
				return LValue;
			}
			finally
			{
				FDisowning = false;
			}
		}
	}

	public delegate void ListEventHandler(object ASender, object AItem);
	
	/// <summary> Specifies a list who can notify of item changes </summary>
	public interface INotifyList : IList
	{
		event ListEventHandler OnAdding;
		event ListEventHandler OnRemoving;
		event ListEventHandler OnValidate;
		event ListEventHandler OnChanged;
	}
	
	public delegate void ListItemEventHandler(object ASender);
	
	public interface INotifyListItem
	{
		event ListItemEventHandler OnChanged;
	}
	
	/// <summary> A <c>List</c> descendant that implements <see cref="INotifyList">INotifyList</see> </summary>
	public class NotifyList : List, INotifyList
	{
		public NotifyList() : base(){}
		public NotifyList(bool AAllowNulls) : base(AAllowNulls){}
		
		public event ListEventHandler OnAdding;
		public event ListEventHandler OnRemoving;
		public event ListEventHandler OnValidate;
		public event ListEventHandler OnChanged;
		
		protected override void Adding(object AObject, int AIndex)
		{
			if (OnAdding != null)
				OnAdding(this, AObject);
			base.Adding(AObject, AIndex);
			Changed(AObject);
			INotifyListItem LItem = AObject as INotifyListItem;
			if (LItem != null)
				LItem.OnChanged += new ListItemEventHandler(ItemChanged);
		}
		
		protected override void Removing(object AObject, int AIndex)
		{
			INotifyListItem LItem = AObject as INotifyListItem;
			if (LItem != null)
				LItem.OnChanged -= new ListItemEventHandler(ItemChanged);
			if (OnRemoving != null)
				OnRemoving(this, AObject);
			base.Removing(AObject, AIndex);
			Changed(AObject);
		}
		
		protected override void Validate(object AObject)
		{
			if (OnValidate != null)
				OnValidate(this, AObject);
			base.Validate(AObject);
		}
		
		protected virtual void Changed(object AObject)
		{
			if (OnChanged != null)
				OnChanged(this, AObject);
		}
		
		protected virtual void ItemChanged(object ASender)
		{
			Changed(ASender);
		}
	}	
	
	public class DisposableNotifyList : DisposableList, INotifyList
	{
		public DisposableNotifyList() : base(){}
		public DisposableNotifyList(bool AItemsOwned, bool AAllowNulls) : base(AItemsOwned, AAllowNulls){}
		
		public event ListEventHandler OnAdding;
		public event ListEventHandler OnRemoving;
		public event ListEventHandler OnValidate;
		public event ListEventHandler OnChanged;
		
		protected override void Adding(object AObject, int AIndex)
		{
			if (OnAdding != null)
				OnAdding(this, AObject);
			base.Adding(AObject, AIndex);
			Changed(AObject);
		}
		
		protected override void Removing(object AObject, int AIndex)
		{
			if (OnRemoving != null)
				OnRemoving(this, AObject);
			base.Removing(AObject, AIndex);
			Changed(AObject);
		}
		
		protected override void Validate(object AObject)
		{
			if (OnValidate != null)
				OnValidate(this, AObject);
			base.Validate(AObject);
		}
		
		protected virtual void Changed(object AObject)
		{
			if (OnChanged != null)
				OnChanged(this, AObject);
		}
	}
	
	/// <summary> A class that validates the type of each item against a specified type </summary>
	public class TypedList : List
	{
		public TypedList() : base() {}
		
		public TypedList(Type AItemType) : base()
		{
			FItemType = AItemType;
		}
	
		/// <summary> This constructor allows the initialization of the TypedList's properties </summary>
		public TypedList(Type AItemType, bool AAllowNulls) : base(AAllowNulls)
		{
			FItemType = AItemType;
		}
		
		protected Type FItemType;
		/// <summary> Determines the type of items "allowed" in this list. </summary>
		/// <remarks>
		///		In order to be successfully added to this list, items must be this type or a 
		///		descendant thereof.  If <c>ItemType == null</c> then any type is allowed in the list.
		///		When this property is set, existing items (if any) are validated to ensure they 
		///		are of the proper type.
		///	</remarks>
		public Type ItemType
		{
			get { return FItemType; }
			set
			{
				if (value != FItemType)
				{
					foreach (object LItem in this)
						InternalValidate(LItem, value);
					FItemType = value;
				}
			}
		}
		
		/// <summary>
		///		<c>InternalValidate</c> is used internally to test potential validation
		///		scenarios against the list.
		///	</summary>
		protected void InternalValidate(object AValue, Type AItemType)
		{
			if ((AValue != null) && (AItemType != null) && !(AItemType.IsInstanceOfType(AValue) || AValue.GetType().IsSubclassOf(AItemType)))
				throw new BaseException(BaseException.Codes.CollectionOfType, AValue.GetType(), AItemType.ToString());
		}
		
		/// <summary>
		///		<c>TypedList</c> overrides the Validate method to ensure that items in the 
		///		list are of the appropriate type (see <see cref="TypedList.ItemType">ItemType</see>).
		///	</summary>
		protected override void Validate(object AValue)
		{
			base.Validate(AValue);
			InternalValidate(AValue, FItemType);
		}
	}
	
	[Serializable]
	public class DisposableTypedList : DisposableList
	{
		public DisposableTypedList() : base(){}
		public DisposableTypedList(Type AItemType) : base()
		{
			FItemType = AItemType;
		}
		
		public DisposableTypedList(Type AItemType, bool AItemsOwned, bool AAllowNulls) : base(AItemsOwned, AAllowNulls)
		{
			FItemType = AItemType;
		}

		protected Type FItemType;
		/// <summary> Determines the type of items "allowed" in this list. </summary>
		/// <remarks>
		///		In order to be successfully added to this list, items must be this type or a 
		///		descendant thereof.  If <c>ItemType == null</c> then any type is allowed in the list.
		///		When this property is set, existing items (if any) are validated to ensure they 
		///		are of the proper type.
		///	</remarks>
		public Type ItemType
		{
			get { return FItemType; }
			set
			{
				if (value != FItemType)
				{
					foreach (object LItem in this)
						InternalValidate(LItem, value);
					FItemType = value;
				}
			}
		}
		
		/// <summary>
		///		<c>InternalValidate</c> is used internally to test potential validation
		///		scenarios against the list.
		///	</summary>
		protected void InternalValidate(object AValue, Type AItemType)
		{
			if ((AItemType != null) && !(AItemType.IsInstanceOfType(AValue) || AValue.GetType().IsSubclassOf(AItemType)))
				throw new BaseException(BaseException.Codes.CollectionOfType, AValue.GetType(), AItemType.ToString());
		}
		
		/// <summary>
		///		<c>TypedList</c> overrides the Validate method to ensure that items in the 
		///		list are of the appropriate type (see <see cref="TypedList.ItemType">ItemType</see>).
		///	</summary>
		protected override void Validate(object AValue)
		{
			base.Validate(AValue);
			InternalValidate(AValue, FItemType);
		}
	}

    /// <summary>
    ///     Provides a list which accesses the information in another list, 
    ///     limited to the members of that list of a given Type.
    /// </summary>    
	public class ListCover : IList, ICollection, IEnumerable
    {
        public ListCover(IList AList, Type AType) : base()
        {
            FList = AList;
            FType = AType;
        }

        protected IList FList;
        protected Type FType;
        
        // IList

		public object this[int AIndex]
        {
            get
            {
                int LIndex = AIndex;
                object LReturnObject = null;
                foreach(object LObject in FList)
                {
                    if (FType.IsInstanceOfType(LObject) || LObject.GetType().IsSubclassOf(FType))
                    {
                        LIndex--;
                        if (LIndex < 0)
                        {
                            LReturnObject = LObject;
                            break;
                        }
                    }
                }
                if (LReturnObject == null)
                    throw new BaseException(BaseException.Codes.InvalidListIndex, AIndex.ToString());
                return LReturnObject;
            }
            set
            {
                throw new BaseException(BaseException.Codes.ListCoverIsReadOnly);
            }
        }

		public int Add(object AValue)
		{
			throw new BaseException(BaseException.Codes.ListCoverIsReadOnly);
		}

        public void Clear()
        {
			throw new BaseException(BaseException.Codes.ListCoverIsReadOnly);
		}
        
		public bool Contains(object AValue)
        {
            foreach(object LObject in FList)
            {
                if ((FType.IsInstanceOfType(LObject) || LObject.GetType().IsSubclassOf(FType)) && (LObject.Equals(AValue)))
                    return true;
            }
            return false;
        }

        public int IndexOf(object AValue)
        {
            int LIndex = 0;
            foreach(object LObject in FList)
            {
                if (FType.IsInstanceOfType(LObject) || LObject.GetType().IsSubclassOf(FType))
                {
                    if (LObject.Equals(AValue))
                        return LIndex;
                    LIndex++;
                }
            }
            return -1;
        }

        public void Insert(int AIndex, object AValue)
        {
			throw new BaseException(BaseException.Codes.ListCoverIsReadOnly);
		}

        public void Remove(object AValue)
        {
			throw new BaseException(BaseException.Codes.ListCoverIsReadOnly);
		}

        public void RemoveAt(int AIndex)
        {
			throw new BaseException(BaseException.Codes.ListCoverIsReadOnly);
		}
        
        // IList interface
        public int Count
        {
            get
            {
                int LIndex = 0;
                foreach(object LObject in FList)
                {
                    if (FType.IsInstanceOfType(LObject) || LObject.GetType().IsSubclassOf(FType))
                        LIndex ++;
                }
                return LIndex;
            }
        }

        public bool IsReadOnly
        {
            get { return true; }    
        }

        public bool IsSynchronized
        {
            get { return true; }
        }

		public bool IsFixedSize
		{
			get { return false; }
		}

        public object SyncRoot
        {
            get { return this; }
        }

        public void CopyTo(Array AArray, int AIndex)
        {
            foreach(object LObject in FList)
            {
                if (FType.IsInstanceOfType(LObject) || LObject.GetType().IsSubclassOf(FType))
                {
                    ((IList)AArray)[AIndex] = LObject;
                    AIndex++;
                }
            }
        }
        
        // IEnumerable

        public IEnumerator GetEnumerator()
        {
            return new ListCoverEnumerator(this);
        }

		public class ListCoverEnumerator : IEnumerator
        {
            public ListCoverEnumerator(ListCover AListCover) : base()
            {
                FListCover = AListCover;
            }
            
            private int FCurrent = -1;
            private ListCover FListCover;

            public object Current
            {
                get { return FListCover[FCurrent]; }
            }

            public void Reset()
            {
                FCurrent = -1;
            }

            public bool MoveNext()
            {
				FCurrent++;
				return FCurrent < FListCover.Count;
            }
        }
    }

	/// <summary>A Hashtable descendent which implements the IList interface.</summary>
	public abstract class HashtableList : Hashtable, IList
    {
		public HashtableList() : base () {}
		public HashtableList(IDictionary ADictionary) : base(ADictionary) {}
		public HashtableList(int ACapacity) : base(ACapacity) {}
		public HashtableList(IDictionary ADictionary, float ALoadFactor) : base(ADictionary, ALoadFactor) {}
		public HashtableList(IEqualityComparer AComparer) : base(AComparer) {}
		public HashtableList(int ACapacity, float ALoadFactor) : base(ACapacity, ALoadFactor) {}
		public HashtableList(IDictionary ADictionary, IEqualityComparer AComparer) : base (ADictionary, AComparer) {}

		public abstract int Add(object AValue);
		
		public object this[int AIndex]
		{
			get 
			{
				int LIndex = 0;
				foreach (object LObject in Keys)
				{
					if (LIndex == AIndex)
						return this[LObject];
					LIndex++;
				}
				throw new BaseException(BaseException.Codes.ObjectAtIndexNotFound, AIndex.ToString());
			}
			set 
			{
				throw new BaseException(BaseException.Codes.InsertNotSupported);
			}
		}

		public int IndexOf(object AValue)
		{
			int LIndex = 0;
			foreach (object LObject in Keys)
			{
				if (LObject.Equals(AValue))
					return LIndex;
				LIndex++;
			}
			return -1;
		}
		
		public void Insert(int AIndex, object AValue)
		{
			throw new BaseException(BaseException.Codes.InsertNotSupported);
		}
		
		public void RemoveAt(int AIndex)
		{
			Remove(this[AIndex]);
		}

		public new HashtableListEnumerator GetEnumerator()
		{
			return new HashtableListEnumerator(this);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return new HashtableListEnumerator(this);
		}

		public IDictionaryEnumerator GetDictionaryEnumerator()
		{
			return base.GetEnumerator();
		}

		public class HashtableListEnumerator : IEnumerator
		{
			public HashtableListEnumerator(HashtableList AHashtableList) : base()
			{
				FEnumerator = AHashtableList.GetDictionaryEnumerator();
			}
        
			private IDictionaryEnumerator FEnumerator;

			object IEnumerator.Current
			{
				get { return FEnumerator.Entry.Value; }
			}

            public object Current
            {
                get { return FEnumerator.Entry.Value; }
            }

			public void Reset()
			{
				FEnumerator.Reset();
			}

			public bool MoveNext()
			{
				return FEnumerator.MoveNext();
			}
		}
	}

	/// <summary> Fixed size cache list. </summary>
	/// <remarks> Currently implemented as a LRU (Least Recently Used) algorithm. </remarks>
	public class FixedSizeCache : IDictionary
	{
		/// <param name="ASize"> The size of the cache (in entries). </param>
		public FixedSizeCache(int ASize)
		{
			if (ASize < 2)
				throw new BaseException(BaseException.Codes.MinimumSize);
			FSize = ASize;
			FEntries = new Hashtable(ASize);
		}

		private Hashtable FEntries;
		private int FSize;
		public int Size { get { return FSize; } }

		/// <summary> The number of cache entries. </summary>
		public int Count
		{
			get { return FEntries.Count; }
		}

		#region LRU maintenance

		internal class Entry
		{
			internal Entry FNext;
			internal Entry FPrior;
			internal object FKey;
			internal object FValue;
		}
		
		private Entry FHead;
		private Entry FTail;

		private void RemoveEntry(Entry AEntry)
		{
			if (AEntry.FPrior != null)
				AEntry.FPrior.FNext = AEntry.FNext;
			if (AEntry.FNext != null)
				AEntry.FNext.FPrior = AEntry.FPrior;
			if (AEntry == FTail)
				FTail = AEntry.FNext;
			if (AEntry == FHead)
				FHead = AEntry.FPrior;
			AEntry.FPrior = null;
			AEntry.FNext = null;
		}

		private void AddEntry(Entry AEntry)
		{
			AEntry.FPrior = FHead;
			AEntry.FNext = null;
			if (FHead != null)
				FHead.FNext = AEntry;
			else
				FTail = AEntry;
			FHead = AEntry;
		}

		private void ClearEntries()
		{
			FHead = null;
			FTail = null;
		}

		#endregion

		/// <summary> Submits a reference to the cache to be added or reprioritized. </summary>
		/// <param name="AValue"> The item to be added to the cache. </param>
		/// <returns> The value that was removed because the available size was completely allocated, or null if no item was removed. </returns>
		public object Reference(object AKey, object AValue)
		{
			object LResult = null;
			Entry LEntry = (Entry)FEntries[AKey];
			if (LEntry == null)
			{
				if (FEntries.Count >= FSize)
				{
					LEntry = FTail;
					LResult = FTail.FValue;
					FEntries.Remove(FTail.FKey);
					RemoveEntry(FTail);
				}
				else
					LEntry = new Entry();

				LEntry.FKey = AKey;
				AddEntry(LEntry);
				FEntries.Add(AKey, LEntry);
			}
			else
			{
				// Move the item to the head of the MRU
				RemoveEntry(LEntry);
				AddEntry(LEntry);
			}
			// Adjust the entry's value
			LEntry.FValue = AValue;
			return LResult;
		}

		#region IDictionary Members

		public bool IsReadOnly { get { return false; } }

		public IDictionaryEnumerator GetEnumerator()
		{
			return new DictionaryEnumerator(this);
		}

		internal class DictionaryEnumerator : IDictionaryEnumerator
		{
			public DictionaryEnumerator(FixedSizeCache ACache)
			{
				FEnumerator = ACache.FEntries.GetEnumerator();
			}

			private IDictionaryEnumerator FEnumerator;

			#region IDictionaryEnumerator

			public object Key
			{
				get { return FEnumerator.Key; }
			}

			public object Value
			{
				get { return ((Entry)FEnumerator.Value).FValue; }
			}

			public DictionaryEntry Entry
			{
				get { return new DictionaryEntry(FEnumerator.Key, ((Entry)FEnumerator.Value).FValue); }
			}

			#endregion

			#region IEnumerator Members

			public void Reset()
			{
				FEnumerator.Reset();
			}

			public object Current
			{
				get { return Entry; }
			}

			public bool MoveNext()
			{
				return FEnumerator.MoveNext();
			}

			#endregion
		}

		public object this[object AKey]
		{
			get
			{
				Entry LEntry = (Entry)FEntries[AKey];
				if (LEntry != null)
					return LEntry.FValue;
				else
					return null;
			}
			set
			{
				// Remove the old entry if it exists
				Entry LEntry = (Entry)FEntries[AKey];
				if (LEntry != null)
				{
					FEntries.Remove(AKey);
					RemoveEntry(LEntry);
				}
				// Add the new one if it is not null
				if (value != null)
					Add(AKey, value);
			}
		}

		public bool Contains(object AKey)
		{
			return FEntries.Contains(AKey);
		}

		/// <summary> Adds the specified key/value to the cache. </summary>
		/// <remarks> Note that this may remove another item.  To determine what item is removed upon 
		///	entry, use Reference rather than Add. </remarks>
		public void Add(object AKey, object AValue)
		{
			Reference(AKey, AValue);
		}

		/// <summary> Removes the specified key from the cache. </summary>
		public void Remove(object AKey)
		{
			Entry LEntry = (Entry)FEntries[AKey];
			if (LEntry != null)
			{
				RemoveEntry(LEntry);
				FEntries.Remove(AKey);
			}
		}

		/// <summary> Clears the cache of all entries. </summary>
		public void Clear()
		{
			FEntries.Clear();
			ClearEntries();
		}

		/// <remarks> Unimplemented. </remarks>
		public ICollection Keys
		{
			get { return null; }
		}

		/// <remarks> Unimplemented. </remarks>
		public ICollection Values
		{
			get { return null; }
		}

		public bool IsFixedSize
		{
			get { return false; }
		}

		#endregion

		#region ICollection Members

		public void CopyTo(Array ATarget, int AIndex)
		{
			foreach (DictionaryEntry LEntry in FEntries)
			{
				ATarget.SetValue(new DictionaryEntry(LEntry.Key, ((Entry)LEntry.Value).FValue), AIndex);
				AIndex++;
			}
		}

		public bool IsSynchronized
		{
			get { return false; }
		}

		public object SyncRoot
		{
			get { return null; }
		}

		#endregion

		#region IEnumerable Members

		IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		#endregion
	}

	/// <summary> This class must be implemented by objects stored as children of <see cref="LinkedCollection"/>. </summary>
	/// <remarks>
	///		Classes implementing this interface are not to modify the members introduced by 
	///		this interface, and they cannot be added to multiple LinkedCollection lists.
	///		Note that these restrictions are not enforced and their violation will result
	///		in the LinkedCollection entering an invalid state.  In short... you will break
	///		the list(s)!
	/// </remarks>
	public interface ILinkedItem
	{
		ILinkedItem Prior { get; set; }
		ILinkedItem Next { get; set; }
	}

	/// <summary> A list that is optimized for adding, removing and enumerating. </summary>
	/// <remarks>
	///		Index operations on this list are order n (slow).  Additionally, this collection
	///		requires that it's children implement ILinkedItem.  This indirectly means that
	///		children cannot be a part of more than one LinkedCollection and that part of the
	///		"contract" of being part of this list means that the children are on scouts honor
	///		not to modify their <see cref="ILinkedItem"/> members; they should let the LinkedCollection
	///		completely handle their ILinkedItem members..
	/// </remarks>
	public class LinkedCollection : IList
	{
		private ILinkedItem FFirstItem;
		public ILinkedItem FirstItem
		{
			get { return FFirstItem; }
		}

		private ILinkedItem FLastItem;
		public ILinkedItem LastItem
		{
			get { return FLastItem; }
		}

		// IEnumerable

		IEnumerator IEnumerable.GetEnumerator()
		{
			return new LinkedEnumerator(this);
		}

		public LinkedEnumerator GetEnumerator()
		{
			return new LinkedEnumerator(this);
		}

		public class LinkedEnumerator : IEnumerator
		{
			public LinkedEnumerator(LinkedCollection ACollection)
			{
				FCollection = ACollection;
				FCurrent = FCollection.FirstItem;
			}

			private ILinkedItem FCurrent;
			private LinkedCollection FCollection;
			private bool FInitialized;

			object IEnumerator.Current
			{
				get { return FCurrent; }
			}
			
			public ILinkedItem Current
			{
				get { return FCurrent; }
			}

			public bool MoveNext()
			{
				if (!FInitialized)
					FInitialized = true;
				else
				{
					if (FCurrent != null)
						FCurrent = FCurrent.Next;
				}
				return FCurrent != null;
			}

			public void Reset()
			{
				FCurrent = FCollection.FirstItem;
			}
		}

		// ICollection

		public int Count
		{
			get
			{
				int LResult = 0;
				ILinkedItem FCurrent = FFirstItem;
				while (FCurrent != null)
				{
					LResult++;
					FCurrent = FCurrent.Next;
				}
				return LResult;
			}
		}

		public bool IsSynchronized
		{
			get { return false; }
		}

		public object SyncRoot
		{
			get { return this; }
		}

		public void CopyTo(Array ATarget, int AStartIndex)
		{
			ILinkedItem FCurrent = FFirstItem;
			while (FCurrent != null)
			{
				ATarget.SetValue(FCurrent, AStartIndex);
				AStartIndex++;
				FCurrent = FCurrent.Next;
			}
		}

		// IList

		public bool IsFixedSize
		{
			get { return false; }
		}

		public bool IsReadOnly
		{
			get { return false; }
		}

		object IList.this[int AIndex]
		{
			get { return this[AIndex]; }
			set { this[AIndex] = (ILinkedItem)value; }
		}

		public ILinkedItem this[int AIndex]
		{
			get
			{
				int LCount = 0;
				ILinkedItem FCurrent = FFirstItem;
				while ((FCurrent != null) && (LCount < AIndex))
				{
					LCount++;
					FCurrent = FCurrent.Next;
				}
				if ((LCount < AIndex) || (FCurrent == null))
					throw new BaseException(BaseException.Codes.IndexOutOfBounds, AIndex, LCount);
				return FCurrent;
			}
			set
			{
				RemoveAt(AIndex);
				Insert(AIndex, value);
			}
		}

		int IList.Add(object AValue)
		{
			Add((ILinkedItem)AValue);
			return Count - 1;
		}

		public void Add(ILinkedItem AValue)
		{
			if (FLastItem != null)
			{
				FLastItem.Next = AValue;
				AValue.Prior = FLastItem;
				AValue.Next = null;
				FLastItem = AValue;
			}
			else
			{
				FFirstItem = AValue;
				FLastItem = AValue;
				AValue.Next = null;
				AValue.Prior = null;
			}
		}

		public void Clear()
		{
			FFirstItem = null;
			FLastItem = null;
		}

		public bool Contains(object AValue)
		{
			ILinkedItem LCurrent = FFirstItem;
			while (LCurrent != null)
			{
				if (LCurrent == AValue)
					return true;
				LCurrent = LCurrent.Next;
			}
			return false;
		}

		public int IndexOf(object AValue)
		{
			ILinkedItem LCurrent = FFirstItem;
			int LIndex = 0;
			while (LCurrent != null)
			{
				if (LCurrent == AValue)
					return LIndex;
				LCurrent = LCurrent.Next;
			}
			return -1;
		}

		void IList.Insert(int AIndex, object AValue)
		{
			Insert(AIndex, (ILinkedItem)AValue);
		}

		public void Insert(int AIndex, ILinkedItem AValue)
		{
			ILinkedItem LCurrent = FFirstItem;
			while ((LCurrent != null) && (AIndex > 0))
			{
				AIndex--;
				LCurrent = LCurrent.Next;
			}
			if (AIndex != 0)
				throw new BaseException(BaseException.Codes.IndexOutOfBounds, String.Empty, String.Empty);
			if (LCurrent == null)
			{
				AValue.Next = null;
				AValue.Prior = null;
				FFirstItem = AValue;
				FLastItem = AValue;
			}
			else
			{
				if (LCurrent == FFirstItem)
					FFirstItem = AValue;
				else
					LCurrent.Prior.Next = AValue;
				AValue.Prior = LCurrent.Prior;
				LCurrent.Prior = AValue;
				AValue.Next = LCurrent;
			}
		}

		void IList.Remove(object AValue)
		{
			Remove((ILinkedItem)AValue);
		}

		public void Remove(ILinkedItem AValue)
		{
			if (AValue == FFirstItem)
				FFirstItem = AValue.Next;
			else
				AValue.Prior.Next = AValue.Next;
			if (AValue == FLastItem)
				FLastItem = AValue.Prior;
			else
				AValue.Next.Prior = AValue.Prior;
		}

		public void RemoveAt(int AIndex)
		{
			Remove(this[AIndex]);
		}
	}
	
	public static class CollectionUtility
	{
		/// <summary> Finds the first item in the given collection that matches the specified predicate. </summary>
		public static bool FindMatch<T>(ICollection ACollection, Predicate<T> APredicate, out T LMatch) where T : class
		{
			foreach (object LItem in ACollection)
			{
				T LTypedItem = LItem as T;
				if ((LTypedItem != null) && APredicate(LTypedItem))
				{
					LMatch = LTypedItem;
					return true;
				}
			}
			LMatch = null;
			return false;
		}
	}
}
