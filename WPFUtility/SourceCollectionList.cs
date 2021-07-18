﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Wildgoat.WPFUtility.Collections
{
    /// <summary>
    /// A base collection that can be used as a source of items
    /// </summary>
    /// <typeparam name="T">Type of the items in the list</typeparam>
    public class SourceCollectionList<T> : IBaseCollectionSource, IList<T>, IList, IReadOnlyList<T>, ICollection<T>, ICollection, IReadOnlyCollection<T>, IEnumerable<T>, IEnumerable, INotifyCollectionChanged, INotifyPropertyChanged
    {
        public SourceCollectionList(IList<T> underlyingCollection)
        {
            UnderlyingCollection = underlyingCollection;
            SyncRoot = new object();
        }

        public SourceCollectionList() : this(new List<T>())
        {
        }

        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        public event PropertyChangedEventHandler? PropertyChanged;

        public int Count => UnderlyingCollection.Count;
        public bool IsFixedSize => UnderlyingCollection is IList list ? list.IsFixedSize : false;
        public bool IsReadOnly => UnderlyingCollection.IsReadOnly;
        public bool IsSynchronized => UnderlyingCollection is ICollection collection ? collection.IsSynchronized : false;
        public object SyncRoot { get; }
        private IList<T> UnderlyingCollection { get; }

        object? IList.this[int index]
        {
            get => UnderlyingCollection[index];
            set
            {
                if (value is T item)
                    UnderlyingCollection[index] = item;
                else
                    throw new InvalidOperationException($"The item of type {value?.GetType()} can not be added as {typeof(T)}");
            }
        }

        public T this[int index] { get => UnderlyingCollection[index]; set => UnderlyingCollection[index] = value; }

        public void Add(T item)
        {
            UnderlyingCollection.Add(item);
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, Count - 1));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
        }

        int IList.Add(object? value)
        {
            if (value is T item)
            {
                Add(item);
                return Count - 1;
            }
            else
                throw new InvalidOperationException($"The item of type {value?.GetType()} can not be added as {typeof(T)}");
        }

        /// <summary>
        /// Adds many items to the collection.
        /// </summary>
        /// <param name="items">List of items to add</param>
        public void AddRange(IEnumerable<T> items)
        {
            var startIndex = Count;
            foreach (var item in items)
                UnderlyingCollection.Add(item);
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, items.ToList(), startIndex));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
        }

        /// <summary>
        /// Adds many items to the collection.
        /// </summary>
        /// <param name="items">List of items to add</param>
        public void AddRange(params T[] items) => AddRange(items.AsEnumerable());

        public void Clear()
        {
            var removedItems = UnderlyingCollection.ToList();
            UnderlyingCollection.Clear();
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removedItems, 0));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
        }

        public bool Contains(T item) => UnderlyingCollection.Contains(item);

        bool IList.Contains(object? value)
        {
            if (value is T item)
                return Contains(item);
            else
                return false;
        }

        public void CopyTo(T[] array, int arrayIndex) => UnderlyingCollection.CopyTo(array, arrayIndex);

        void ICollection.CopyTo(Array array, int index)
        {
            foreach (var item in UnderlyingCollection)
                array.SetValue(item, index++);
        }

        public IEnumerator<T> GetEnumerator() => UnderlyingCollection.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public int IndexOf(T item) => UnderlyingCollection.IndexOf(item);

        int IList.IndexOf(object? value)
        {
            if (value is T item)
                return IndexOf(item);
            else
                return -1;
        }

        public void Insert(int index, T item)
        {
            UnderlyingCollection.Insert(index, item);
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
        }

        void IList.Insert(int index, object? value)
        {
            if (value is T item)
                Insert(index, item);
            else
                throw new InvalidOperationException($"The item of type {value?.GetType()} can not be added as {typeof(T)}");
        }

        public bool Remove(T item)
        {
            var index = IndexOf(item);
            var result = UnderlyingCollection.Remove(item);
            if (result)
            {
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
            }
            return result;
        }

        void IList.Remove(object? value)
        {
            if (value is T item)
                Remove(item);
        }

        public void RemoveAt(int index)
        {
            var removedItem = UnderlyingCollection[index];
            UnderlyingCollection.RemoveAt(index);
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removedItem, index));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
        }
    }
}