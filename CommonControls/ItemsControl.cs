using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Collections;

namespace WindowlessControls
{
    public class ItemsControl : WindowlessControlHost
    {
        public ItemsControl()
        {
            base.Focusable = true;
        }

        public override bool Focusable
        {
            get
            {
                if (Items.Count == 0)
                    return false;
                return base.Focusable;
            }
            set
            {
                base.Focusable = value;
            }
        }

        Type myContentPresenter = null;
        public Type ContentPresenter
        {
            get
            {
                return myContentPresenter;
            }
            set
            {
                myContentPresenter = value;
            }
        }

        bool myDeselectOnFocusLost = false;
        public bool DeselectOnFocusLost
        {
            get { return myDeselectOnFocusLost; }
            set { myDeselectOnFocusLost = value; }
        }

        protected override void OnWindowlessLostFocus(WindowlessControlHost sender, EventArgs e)
        {
            if (myDeselectOnFocusLost && sender == this)
                ChangeSelection(null);
            base.OnWindowlessLostFocus(sender, e);
        }

        ItemCollection myItemCollection;
        public override IWindowlessControl Control
        {
            get
            {
                return base.Control;
            }
            set
            {
                base.Control = value;
                ItemCollection oldCollection = myItemCollection;
                myItemCollection = new ItemCollection(this, Control as WindowlessPanel);

                if (oldCollection != null)
                {
                    foreach (object o in oldCollection)
                    {
                        myItemCollection.Add(o);
                    }
                }
            }
        }

        public ItemCollection Items
        {
            get
            {
                return myItemCollection;
            }
        }

        public int SelectedIndex
        {
            get
            {
                return Items.IndexOf(CurrentSelection);
            }
            set
            {
                CurrentSelection = Items[value];
            }
        }

        public object CurrentSelection
        {
            get
            {
                int index = Control.Controls.IndexOf(myCurrentSelection);
                if (index != -1)
                {
                    return Items[index];
                }
                return null;
            }
            set
            {
                int newIndex = myItemCollection.IndexOf(value);
                IInteractiveContentPresenter newSelection = null;
                if (newIndex != -1)
                    newSelection = Control.Controls[newIndex] as IInteractiveContentPresenter;
                ChangeSelection(newSelection);
            }
        }

        void ChangeSelection(IInteractiveContentPresenter newSelection)
        {
            if (newSelection == myCurrentSelection)
                return;

            myCurrentSelection = newSelection;

            if (mySelectionListener != null)
            {
                mySelectionListener.Undo();
                mySelectionListener = null;
            }

            if (newSelection != null)
            {
                mySelectionListener = new PropertyListener();
                mySelectionListener.StartListen();
                myCurrentSelection.ApplyFocusedStyle();
                mySelectionListener.StopListen();

                WindowlessBringIntoView(newSelection);
            }
            Update();
            OnSelectionChanged();
        }

        IInteractiveContentPresenter myCurrentSelection = null;

        public IInteractiveContentPresenter CurrentSelectedControl
        {
            get { return myCurrentSelection; }
        }

        SelectionChangedEventHandler mySelectionChangedEvent;
        public event SelectionChangedEventHandler SelectionChanged
        {
            add
            {
                mySelectionChangedEvent += value;
            }
            remove
            {
                mySelectionChangedEvent -= value;
            }
        }
        protected virtual void OnSelectionChanged()
        {
            if (mySelectionChangedEvent != null)
                mySelectionChangedEvent(this);
        }

        PropertyListener mySelectionListener;
        PropertyListener myClickListener;
        protected override void OnWindowlessMouseDown(WindowlessControlHost sender, WindowlessMouseEventArgs e)
        {
            base.OnWindowlessMouseDown(sender, e);
            IWindowlessControl control = FindControlAtPoint(new Point(e.X, e.Y));
            if (control == null && control != myCurrentSelection)
                return;
            e.Handled = true;
            ChangeSelection(control as IInteractiveContentPresenter);
            OnClickDown();
        }

        IWindowlessControl FindControlAtPoint(Point p)
        {
            foreach (IWindowlessControl control in Control.Controls)
            {
                Rectangle rect = new Rectangle(control.Left, control.Top, control.Width, control.Height);
                if (rect.Contains(p.X, p.Y))
                {
                    return control;
                }
            }
            return null;
        }

        protected override void OnWindowlessMouseFocus(WindowlessControlHost sender, MouseEventArgs e)
        {
            base.OnWindowlessMouseFocus(sender, e);
            if (sender != this)
                return;
            IWindowlessControl control = FindControlAtPoint(new Point(e.X, e.Y));
            if (control == null && control != myCurrentSelection)
                return;
            ChangeSelection(control as IInteractiveContentPresenter);
        }

        protected override void OnWindowlessClick(WindowlessControlHost sender, WindowlessMouseEventArgs e)
        {
            if (sender == this)
                OnClickUp();
            base.OnWindowlessClick(sender, e);
        }

        protected override void OnWindowlessMouseUp(WindowlessControlHost sender, WindowlessMouseEventArgs e)
        {
            if (sender == this)
                OnClickUp();
            base.OnWindowlessMouseUp(sender, e);
        }

        protected override void OnWindowlessKeyDown(Control sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.Handled)
                return;
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    OnClickDown();
                    break;
                case Keys.F19:
                case Keys.F20:
                    {
                        int firstIndex = -1;
                        int prevIndex = -1;
                        int nextIndex = -1;
                        int lastIndex = -1;
                        bool found = false;
                        for (int i = 0; i < Control.Controls.Count; i++)
                        {
                            IWindowlessControl current = Control.Controls[i] ;
                            if (current == myCurrentSelection)
                            {
                                found = true;
                                continue;
                            }

                            if (!current.Visible)
                                continue;

                            if (firstIndex != -1)
                                firstIndex = i;

                            if (!found)
                                prevIndex = i;
                            if (found && nextIndex == -1)
                                nextIndex = i;
                            lastIndex = i;
                        }

                        int newIndex = -1;
                        if (e.KeyCode == Keys.F19)
                            newIndex = prevIndex;
                        else
                            newIndex = nextIndex;

                        if (newIndex >= 0 && newIndex < Items.Count && newIndex != SelectedIndex)
                        {
                            e.Handled = true;
                            SelectedIndex = newIndex;
                        }
                    }
                    break;
                case Keys.Up:
                case Keys.Down:
                case Keys.Right:
                case Keys.Left:
                    if (CurrentSelection != null)
                    {
                        IWindowlessControl control = Control.Controls[Items.IndexOf(CurrentSelection)];
                        Rectangle rect = new Rectangle(control.Left, control.Top, control.Width, control.Height);
                        e.Handled = NavigateSelection(rect, e.KeyCode);
                    }
                    else if (Items.Count != 0)
                    {
                        SelectedIndex = 0;
                    }
                    break;
            }
            base.OnWindowlessKeyDown(sender, e);
        }

        protected override void OnWindowlessGotFocus(WindowlessControlHost sender, EventArgs e)
        {
            if (CurrentSelection == null && Items.Count > 0) 
            {
                CurrentSelection = Items[0];
            }
            int index = Items.IndexOf(CurrentSelection);
            if (index != -1)
                WindowlessBringIntoView(Control.Controls[index]);
            base.OnWindowlessGotFocus(sender, e);
        }

        protected override void OnWindowlessKeyPress(Control sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                // fire the click event
                OnClick(null);
            }
            base.OnWindowlessKeyPress(sender, e);
        }

        protected override Rectangle GetNavigationSourceRectangle()
        {
            if (myCurrentSelection != null)
                return new Rectangle(myCurrentSelection.Left, myCurrentSelection.Top, myCurrentSelection.Width, myCurrentSelection.Height);
            return base.GetNavigationSourceRectangle();
        }

        bool NavigateSelection(Rectangle rect, Keys key)
        {
            int index = Items.IndexOf(CurrentSelection);
            IWindowlessControl selected = null;
            if (index != -1)
                selected = Control.Controls[index];
            ulong bestScore = UInt32.MaxValue;
            IWindowlessControl best = null;
            foreach (IWindowlessControl control in Control.Controls)
            {
                if (control == selected || !control.Visible)
                    continue;
                long distance;
                bool positiveDirection = false;
                if (key == Keys.Up)
                {
                    distance = GetEdgeDistance(control.Top + control.Height, control.Left, control.Width, rect.Top, rect.Left, rect.Width, positiveDirection);
                }
                else if (key == Keys.Down)
                {
                    positiveDirection = true;
                    distance = GetEdgeDistance(control.Top, control.Left, control.Width, rect.Bottom, rect.Left, rect.Width, positiveDirection);
                }
                else if (key == Keys.Left)
                {
                    distance = GetEdgeDistance(control.Left + control.Width, control.Top, control.Height, rect.Left, rect.Top, rect.Height, positiveDirection);
                }
                else
                {
                    positiveDirection = true;
                    distance = GetEdgeDistance(control.Left, control.Top, control.Height, rect.Right, rect.Top, rect.Height, positiveDirection);
                }


                ulong score = ScoreDistance(distance, positiveDirection);
                if (score < bestScore)
                {
                    bestScore = score;
                    best = control;
                }
            }

            if (bestScore >= Int32.MaxValue)
                return false;

            if (best == null)
                return false;

            CurrentSelection = Items[Control.Controls.IndexOf(best)];
            return true;
        }

        protected override void OnWindowlessNavigatingTo(WindowlessControlHost sender, WindowlessNavigateEventArgs e)
        {
            base.OnWindowlessNavigatingTo(sender, e);

            if (sender != this)
                return;
            // if we have a selection already, just leave it
            if (myCurrentSelection != null)
                return;
            if (Items.Count == 0)
                return;

            Point me = WindowlessPointToForm(this, Point.Empty);
            Point other = WindowlessPointToForm(e.Source, Point.Empty);
            Rectangle otherRect = e.SourceRectangle;
            otherRect.X += other.X;
            otherRect.Y += other.Y;
            Rectangle rect = new Rectangle(otherRect.X - me.X, otherRect.Y - me.Y, otherRect.Width, otherRect.Height);
            NavigateSelection(rect, e.Key);
        }

        protected override void OnWindowlessKeyUp(Control sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == System.Windows.Forms.Keys.Enter)
            {
                OnClickUp();
            }
            base.OnWindowlessKeyUp(sender, e);
        }

        void OnClickUp()
        {
            if (myClickListener != null)
                myClickListener.Undo();
            myClickListener = null;
        }

        void OnClickDown()
        {
            if (myClickListener == null)
            {
                myClickListener = new PropertyListener();
                myClickListener.StartListen();
                myCurrentSelection.ApplyClickedStyle();
                myClickListener.StopListen();
            }
        }
    }


    public delegate void SelectionChangedEventHandler(IWindowlessControl sender);

    public class WindowlessLabelPresenter : WindowlessLabel, IInteractiveContentPresenter
    {
        #region IInteractiveStyleControl Members

        public virtual void ApplyFocusedStyle()
        {
            ForeColor = SystemColors.HighlightText;
            BackColor = SystemColors.Highlight;
        }

        public void ApplyClickedStyle()
        {
            ForeColor = SystemColors.HighlightText;
            BackColor = Color.Red;
        }
        #endregion
    }

    public class ItemCollection : IList
    {
        ArrayList myList = new ArrayList();
        WindowlessPanel myLayoutControl = null;
        ItemsControl myControl;

        internal ItemCollection(ItemsControl control, WindowlessPanel panel)
        {
            myLayoutControl = panel;
            myControl = control;
        }

        #region IList Members

        public int Add(object value)
        {
            IInteractiveContentPresenter contentPresenter;
            if (myControl.ContentPresenter == null)
            {
                contentPresenter = new WindowlessLabelPresenter();
            }
            else
            {
                contentPresenter = Activator.CreateInstance(myControl.ContentPresenter) as IInteractiveContentPresenter;
            }

            int ret = myList.Add(value);
            myLayoutControl.Controls.Add(contentPresenter);
            // changing the content may trigger resizes. so get the control situated in the container first,
            // so when it resizes it has a context.
            contentPresenter.Content = value;
            if (myControl.Focused && myList.Count != 0)
                myControl.SelectedIndex = 0;
            return ret;
        }

        public void Clear()
        {
            myList.Clear();
            myLayoutControl.Controls.Clear();
            myControl.CurrentSelection = null;
        }

        public bool Contains(object value)
        {
            return myList.Contains(value);
        }

        public int IndexOf(object value)
        {
            return myList.IndexOf(value);
        }

        public void Insert(int index, object value)
        {
            IInteractiveContentPresenter contentPresenter;
            if (myControl.ContentPresenter == null)
            {
                contentPresenter = new WindowlessLabelPresenter();
            }
            else
            {
                contentPresenter = Activator.CreateInstance(myControl.ContentPresenter) as IInteractiveContentPresenter;
            }

            myList.Insert(index, value);
            myLayoutControl.Controls.Insert(index, contentPresenter);
            // changing the content may trigger resizes. so get the control situated in the container first,
            // so when it resizes it has a context.
            contentPresenter.Content = value;
        }

        public bool IsFixedSize
        {
            get
            {
                return false;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public void Remove(object value)
        {
            int index = myList.IndexOf(value);
            if (index == -1)
                return;
            RemoveAt(index);
            if (value == myControl.CurrentSelection)
                myControl.CurrentSelection = null;
        }

        public void RemoveAt(int index)
        {
            myList.RemoveAt(index);
            myLayoutControl.Controls.RemoveAt(index);
        }

        public object this[int index]
        {
            get
            {
                return myList[index];
            }
            set
            {
                myList[index] = value;
                (myLayoutControl.Controls[index] as IContentPresenter).Content = value;
            }
        }

        #endregion

        #region ICollection Members

        public void CopyTo(Array array, int index)
        {
            myList.CopyTo(array, index);
        }

        public int Count
        {
            get
            {
                return myList.Count;
            }
        }

        public bool IsSynchronized
        {
            get
            {
                throw new Exception("The method or operation is not supported.");
            }
        }

        public object SyncRoot
        {
            get
            {
                throw new Exception("The method or operation is not supported.");
            }
        }

        #endregion

        #region IEnumerable Members

        public IEnumerator GetEnumerator()
        {
            return myList.GetEnumerator();
        }

        #endregion
    }

}
