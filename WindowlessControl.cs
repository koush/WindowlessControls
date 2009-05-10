//////////////////////////////////////////////////////////////
// Koushik Dutta - 9/1/2007
//////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Collections;
using System.Xml.Serialization;

namespace WindowlessControls
{
    public interface IContentPresenter : IWindowlessControl
    {
        object Content
        {
            get;
            set;
        }
    }

    public interface IInteractiveContentPresenter : IInteractiveStyleControl, IContentPresenter
    {
    }

    public interface IInteractiveStyleControl : IWindowlessControl
    {
        void ApplyFocusedStyle();
        void ApplyClickedStyle();
    }

    // In this concept of layout, the parent sizes a control by via the Measure method. It tells the control how much room it has to
    // use, and the control in turn uses as much as it needs.
    // If the control ever internally detects that it's dimensions need to change, it sets its NeedsRemeasure to true
    // and call's the parent's Remeasure method. The parent will then call it's parent's Remeasure, all the way up to the 
    // WindowlessControlHost. The host dictates the root dimensions. It will call measure on all the children that have
    // the NeedsMeasure flag set to true. Once measured, that flag is set to false. 
    // When a control is setting up, it should not relay any Remeasure commands until it is completely situated, and then call
    // Remeasure a single time at the end.
    // IWindowlessControl implementations can be used to either lay out a bunch of System.Windows.Forms.Controls, or lay out
    // a complex paint operation. Due to Windows limitations, it can not to both.
    // Once an IWindowlessControl hosts an actual System.Windows.Forms.Control, it's Paint will no longer work. ---- note: not sure about this anymore
    public interface IWindowlessControl
    {
        string Name
        {
            get;
            set;
        }

        int Width
        {
            get;
        }

        int Height
        {
            get;
        }

        int Left
        {
            get;
            set;
        }

        int Top
        {
            get;
            set;
        }

        HorizontalAlignment HorizontalAlignment
        {
            get;
            set;
        }

        VerticalAlignment VerticalAlignment
        {
            get;
            set;
        }

        IWindowlessControl Parent
        {
            get;
            set;
        }

        WindowlessControlCollection Controls
        {
            get;
        }

        bool NeedsMeasure
        {
            get;
            set;
        }

        void Measure(Size bounds);
        void Remeasure();

        bool Visible
        {
            get;
            set;
        }

        object Layout
        {
            get;
            set;
        }
    }

    public class WindowlessControl : IWindowlessControl
    {
        public WindowlessControl()
        {
            myVisible = new DependencyPropertyStorage<bool>(this, true, new DependencyPropertyChangedEvent(VisibleChanged));
            myMaxHeight = new DependencyPropertyStorage<int>(this, Int32.MaxValue, new DependencyPropertyChangedEvent(MaxHeightChanged));
            myMaxWidth = new DependencyPropertyStorage<int>(this, Int32.MaxValue, new DependencyPropertyChangedEvent(MaxWidthChanged));
            myMargin = new DependencyPropertyStorage<Thickness>(this, Thickness.Empty, new DependencyPropertyChangedEvent(MarginChanged));
        }

        public void InitializeControls()
        {
            myControls = new SerializableControlCollection(this);
        }

        public WindowlessControl(Thickness padding)
            : this()
        {
            Margin = padding;
        }

        #region IWindowlessControl Members
        string myName;
        [XmlAttribute]
        public string Name
        {
            get
            {
                return myName;
            }
            set
            {
                myName = value;
            }
        }

        object myLayout;
        public object Layout
        {
            get
            {
                return myLayout;
            }
            set
            {
                if (myLayout != value)
                {
                    myLayout = value;
                    //Remeasure();
                }
            }
        }


        int myWidth = 0;
        public virtual int Width
        {
            get
            {
                return myWidth;
            }
        }

        int myHeight = 0;
        public virtual int Height
        {
            get
            {
                return myHeight;
            }
        }

        int myLeft = 0;
        public virtual int Left
        {
            get
            {
                return myLeft;
            }
            set
            {
                myLeft = value;
            }
        }

        int myTop = 0;
        public virtual int Top
        {
            get
            {
                return myTop;
            }
            set
            {
                myTop = value;
            }
        }

        IWindowlessControl myParent = null;
        public virtual IWindowlessControl Parent
        {
            get
            {
                return myParent;
            }
            set
            {
                if (myParent != null)
                {
                    myParent.Controls.Remove(this);
                }
                myParent = value;
                if (myParent != null && !myParent.Controls.Contains(this))
                    myParent.Controls.Add(this);
                Remeasure();
            }
        }

        WindowlessControlCollection myControls;
        public virtual WindowlessControlCollection Controls
        {
            get
            {
                return myControls;
            }
        }

        bool myNeedsMeasure = true;
        public virtual bool NeedsMeasure
        {
            get
            {
                return myNeedsMeasure;
            }
            set
            {
                myNeedsMeasure = true;
            }
        }

        Size myLastMeasureBounds = Size.Empty;
        public virtual void Measure(Size bounds)
        {
            myNeedsMeasure = false;

            // cap a control's dimensions if desired
            bounds.Width = Math.Min(bounds.Width, myMaxWidth);
            bounds.Height = Math.Min(bounds.Height, myMaxHeight);

            Size newBounds = bounds;
            if (bounds.Width != Int32.MaxValue)
                newBounds.Width = bounds.Width - myMargin.Value.Left - myMargin.Value.Right;
            if (bounds.Height != Int32.MaxValue)
                newBounds.Height = bounds.Height - myMargin.Value.Top - myMargin.Value.Bottom;

            bool boundsChange = myLastMeasureBounds != newBounds;
            myLastMeasureBounds = newBounds;

            if (MeasureUnpadded(newBounds, boundsChange))
                WindowlessControlHost.WindowlessInvalidate(this);
        }

        DependencyPropertyStorage<int> myMaxHeight;
        public int MaxHeight
        {
            get { return myMaxHeight; }
            set { myMaxHeight.Value = value; }
        }

        void MaxHeightChanged(object sender, DependencyPropertyEventArgs e)
        {
            Remeasure();
        }

        DependencyPropertyStorage<int> myMaxWidth;
        public int MaxWidth
        {
            get { return myMaxWidth; }
            set { myMaxWidth.Value = value; }
        }
        void MaxWidthChanged(object sender, DependencyPropertyEventArgs e)
        {
            Remeasure();
        }

        public virtual void Remeasure()
        {
            //IWindowlessControl parent = this;
            //IWindowlessControl last = null;
            //while (parent != null)
            //{
            //    parent.NeedsMeasure = true;
            //    last = parent;
            //    parent = parent.Parent;
            //}
            //if (last != this)
            //    last.Remeasure();

            myNeedsMeasure = true;
            if (myParent != null)
                myParent.Remeasure();
        }

        protected void SetupDefaultClip(Graphics g)
        {
            g.Clip = new Region(new Rectangle(Left, Top, Width, Height));
        }

        HorizontalAlignment myHorizontalAlignment = HorizontalAlignment.Left;
        [XmlAttribute]
        public virtual HorizontalAlignment HorizontalAlignment
        {
            get
            {
                return myHorizontalAlignment;
            }
            set
            {
                myHorizontalAlignment = value;
                Remeasure();
            }
        }

        VerticalAlignment myVerticalAlignment = VerticalAlignment.Top;
        [XmlAttribute]
        public virtual VerticalAlignment VerticalAlignment
        {
            get
            {
                return myVerticalAlignment;
            }
            set
            {
                myVerticalAlignment = value;
                Remeasure();
            }
        }

        void VisibleChanged(object sender, DependencyPropertyEventArgs e)
        {
            WindowlessControlHost.WindowlessInvalidate(this);
            Remeasure();
        }

        DependencyPropertyStorage<bool> myVisible;
        public bool Visible
        {
            get { return myVisible; }
            set
            {
                myVisible.Value = value;
            }
        }

        #endregion


        void MarginChanged(object sender, DependencyPropertyEventArgs e)
        {
            WindowlessControlHost.WindowlessInvalidate(this);
            Remeasure();
        }

        DependencyPropertyStorage<Thickness> myMargin;
        public Thickness Margin
        {
            get { return myMargin; }
            set
            {
                myMargin.Value = value;
            }
        }

        public int ClientLeft
        {
            get
            {
                return Left + myMargin.Value.Left;
            }
        }
        public int ClientTop
        {
            get
            {
                return Top + myMargin.Value.Top;
            }
        }

        public int ClientWidth
        {
            get
            {
                return Width - myMargin.Value.Left - myMargin.Value.Right;
            }
            set
            {
                myWidth = value + myMargin.Value.Left + myMargin.Value.Right;
            }
        }

        public int ClientHeight
        {
            get
            {
                return Height - myMargin.Value.Top - myMargin.Value.Bottom;
            }
            set
            {
                myHeight = value + myMargin.Value.Top + myMargin.Value.Bottom;
            }
        }

        public virtual bool MeasureUnpadded(Size bounds, bool boundsChange)
        {
            if (HorizontalAlignment == HorizontalAlignment.Stretch && bounds.Width != Int32.MaxValue)
                ClientWidth = bounds.Width;
            else
                ClientWidth = 0;
            if (VerticalAlignment == VerticalAlignment.Stretch && bounds.Height != Int32.MaxValue)
                ClientHeight = bounds.Height;
            else
                ClientHeight = 0;
            return false;
        }
    }
}