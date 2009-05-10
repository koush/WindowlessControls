using System;
using System.Collections.Generic;
using System.Collections;

namespace WindowlessControls
{

    public class EncapsulatedList<T> : IList<T>, ICollection
    {
        List<T> myList = new List<T>();
        #region IList<T> Members

        public int IndexOf(T item)
        {
            return myList.IndexOf(item);
        }

        public virtual void Insert(int index, T item)
        {
            myList.Insert(index, item);
        }

        public virtual void RemoveAt(int index)
        {
            myList.RemoveAt(index);
        }

        public virtual T this[int index]
        {
            get
            {
                return myList[index];
            }
            set
            {
                myList[index] = value;
            }
        }

        #endregion

        #region ICollection<T> Members

        public virtual void Add(T item)
        {
            myList.Add(item);
        }

        public virtual void Clear()
        {
            myList.Clear();
        }

        public bool Contains(T item)
        {
            return myList.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            myList.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get
            {
                return myList.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public virtual bool Remove(T item)
        {
            return myList.Remove(item);
        }

        #endregion

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            return myList.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return myList.GetEnumerator();
        }

        #endregion
        #region ICollection Members

        public void CopyTo(Array array, int index)
        {
            T[] arr = array as T[];
            CopyTo(arr, index);
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
    }
}