using System;
using System.Collections.Generic;
using System.Text;

namespace WindowlessControls
{
    public class PropertyListener
    {
        internal static List<PropertyListener> myListeners = new List<PropertyListener>();

        List<DependencyPropertyEventArgs> myChangeList = new List<DependencyPropertyEventArgs>();

        internal void Add(DependencyPropertyEventArgs e)
        {
            myChangeList.Add(e);
        }

        public void StartListen()
        {
            myListeners.Add(this);
        }

        public void StopListen()
        {
            myListeners.Remove(this);
        }

        public void Undo()
        {
            foreach (DependencyPropertyEventArgs e in myChangeList)
            {
                if (e.Storage.Value == null)
                {
                    if (e.NewValue == null)
                        e.Storage.Value = e.OldValue;
                    return;
                }
                if (e.Storage.Value.Equals(e.NewValue))
                    e.Storage.Value = e.OldValue;
            }
        }
    }

    public class DependencyPropertyEventArgs
    {
        object myOldValue;

        public object OldValue
        {
            get { return myOldValue; }
        }
        object myNewValue;

        public object NewValue
        {
            get { return myNewValue; }
        }

        DependencyPropertyStorage myStorage;

        public DependencyPropertyStorage Storage
        {
            get { return myStorage; }
        }
        public DependencyPropertyEventArgs(DependencyPropertyStorage storage, object oldValue, object newValue)
        {
            myStorage = storage;
            myOldValue = oldValue;
            myNewValue = newValue;
        }
    }

    public delegate void DependencyPropertyChangedEvent(object sender, DependencyPropertyEventArgs e);

    public class DependencyPropertyStorage
    {
        object myOwner;
        public DependencyPropertyStorage(object owner, object initialValue, DependencyPropertyChangedEvent changed)
        {
            myChanged = changed;
            myOwner = owner;
            myValue = initialValue;
        }

        object myValue;
        public object Value
        {
            get
            {
                return myValue;
            }
            set
            {
                DependencyPropertyEventArgs e = new DependencyPropertyEventArgs(this, myValue, value);
                myValue = value;

                if ((e.OldValue == null && e.NewValue != null) || (e.OldValue != null && !e.OldValue.Equals(e.NewValue)))
                {
                    foreach (PropertyListener pl in PropertyListener.myListeners)
                    {
                        pl.Add(e);
                    }

                    if (myChanged != null)
                        myChanged(this, e);
                }
            }
        }

        DependencyPropertyChangedEvent myChanged;
    }

    public class DependencyPropertyStorage<T> : DependencyPropertyStorage
    {
        public DependencyPropertyStorage(object owner, object initialValue, DependencyPropertyChangedEvent changed)
            : base(owner, initialValue, changed)
        {
        }

        public new T Value
        {
            get
            {
                return (T)base.Value;
            }
            set
            {
                base.Value = value;
            }
        }

        public static implicit operator T(DependencyPropertyStorage<T> o)
        {
            return o.Value;
        }
    }
}
