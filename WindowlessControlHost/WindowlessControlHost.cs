//////////////////////////////////////////////////////////////
// Koushik Dutta - 9/1/2007
//////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Collections;
using System.Drawing.Imaging;
using System.Xml.Serialization;


namespace WindowlessControls
{
    public partial class WindowlessControlHost : ScrollableControl, IWindowlessControl
    {
        Orientation myOrientation = Orientation.None;
        WindowlessControlCollection myControls;
        DependencyPropertyStorage<bool> myPaintSelf;
        DependencyPropertyStorage<bool> myPaintChildren;
        DependencyPropertyStorage<bool> myClipToBounds;

        public WindowlessControlHost()
        {
            myPaintSelf = new DependencyPropertyStorage<bool>(this, true, new DependencyPropertyChangedEvent(PaintSelfChanged));
            myPaintChildren = new DependencyPropertyStorage<bool>(this, true, new DependencyPropertyChangedEvent(PaintChildrenChanged));
            myClipToBounds = new DependencyPropertyStorage<bool>(this, true, new DependencyPropertyChangedEvent(ClipToBoundsChanged));
            myOldSize = Size;
            myControls = new SerializableControlCollection(this);
            TabStop = false;
        }

        [XmlIgnore]
        public virtual IWindowlessControl Control
        {
            get
            {
                if (myControls.Count == 0)
                    return null;
                return myControls[0];
            }
            set
            {
                if (Control != null)
                {
                    myControls.Remove(Control);
                }
                if (value != null)
                {
                    myControls.Add(value);
                }
            }
        }

        [XmlAttribute]
        public Orientation Orientation
        {
            get
            {
                return myOrientation;
            }
            set
            {
                myOrientation = value;
            }
        }

        public static Point WindowlessPointToHost(IWindowlessControl control, Point point)
        {
            Point ret = point;
            if (control.Parent == null)
                return point;
            IWindowlessControl host = WindowlessControlHost.GetHost(control);
            while (control != host)
            {
                ret.X += control.Left;
                ret.Y += control.Top;
                control = control.Parent;
            }
            return ret;
        }

        public static Point WindowlessPointToForm(Control control, Point point)
        {
            Control parent = control.Parent;
            while (parent != null)
            {
                point.X += control.Left;
                point.Y += control.Top;
                control = parent;
                parent = control.Parent;
            }
            return point;
        }

        Size MeasureBounds
        {
            get
            {
                Size size = ClientSize;
                switch (myOrientation)
                {
                    case Orientation.Vertical:
                        size.Height = Int32.MaxValue;
                        break;
                    case Orientation.Horizontal:
                        size.Width = Int32.MaxValue;
                        break;
                }
                return size;
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
                    Remeasure();
                }
            }
        }

        int myLeft;
        int IWindowlessControl.Left
        {
            get
            {
                return myLeft;
            }
            set
            {
                myLeft = value;
                Point p = WindowlessPointToHost(this, Point.Empty);
                if (Left != p.X)
                    Left = p.X;
            }
        }

        int myTop;
        int IWindowlessControl.Top
        {
            get
            {
                return myTop;
            }
            set
            {
                myTop = value;
                Point p = WindowlessPointToHost(this, Point.Empty);
                if (Top != p.Y)
                    Top = p.Y;
            }
        }

        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);

            // Do not add a WindowlessControlHost directly into the Controls collection of another WindowlessControlHost.
            // WRONG: host.Controls.add(otherHost);
            // Instead, you should add the Control to hosting control's Control collection:
            // CORECT: host.Control.Controls.add(otherHost);
            System.Diagnostics.Debug.Assert(!(Parent is WindowlessControlHost) ^ (this as IWindowlessControl).Parent != null);

            // give this tabstop so it can pick the first focused control
            if (Parent is Form && !Focusable)
                TabStop = true;
            Remeasure();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            if (Control == null)
                return;

            // resize is called whenever the control is moved or resized
            Size size = Size;
            if (size == myOldSize)
            {
                // the control was moved
                // this should mean that it was repositioned by the host or autoscroll
                // if the control has transparencies, invalidate it.
                if (BackColor == Color.Transparent)
                    WindowlessInvalidate();
            }
            else
            {
                myOldSize = size;

                // the control was resized for whatever reason
                // we must repaint everything.
                WindowlessInvalidate();

                // this requires a remeasure, since its the root.
                Remeasure();
            }
        }

        protected void PrepareControl()
        {
            Control host = WindowlessControlHost.GetHost(this);
            if (host == null)
                return;
            if (Parent != host)
                Parent = host;
        }

        public WindowlessControlCollection SerializableControls
        {
            get
            {
                return myControls;
            }
        }

        #region IWindowlessControl Members
        string myName;
        [XmlAttribute]
        public new string Name
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

        bool myVisible = true;
        bool IWindowlessControl.Visible
        {
            get
            {
                return myVisible;
            }
            set
            {
                if (myVisible != value)
                {
                    myVisible = value;
                    Remeasure();
                    Visible = myVisible;
                }
            }
        }

        IWindowlessControl myParent;
        [XmlIgnore]
        IWindowlessControl IWindowlessControl.Parent
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
                    myParent.Remeasure();
                }
                myParent = value;
                //if (myParent != null && !myParent.Controls.Contains(this))
                //    myParent.Controls.Add(this);
                // remove this from the parent form/control
                if (myParent == null)
                    Parent = null;

                Remeasure();
            }
        }
        
        WindowlessControlCollection IWindowlessControl.Controls
        {
            get 
            {
                return myControls;
            }
        }

        bool myNeedsMeasure = true;
        [XmlIgnore]
        public bool NeedsMeasure
        {
            get 
            {
                return myNeedsMeasure;
            }
            set
            {
                myNeedsMeasure = value;
            }
        }

        public virtual void Measure(Size bounds)
        {
            myNeedsMeasure = false;

            PrepareControl();

            Size newBounds = bounds;
            Size clientSize = ClientSize;
            Size size = Size;
            if (clientSize.Width != size.Width && bounds.Width != Int32.MaxValue)
                newBounds.Width -= (Width - clientSize.Width);
            if (clientSize.Height != size.Height && bounds.Height != Int32.MaxValue)
                newBounds.Height -= (size.Height - clientSize.Height);

            if (Orientation == Orientation.Horizontal)
                newBounds.Width = Int32.MaxValue;
            else if (Orientation == Orientation.Vertical)
                newBounds.Height = Int32.MaxValue;

            Size clientAdjust = new Size();
            clientAdjust.Width = (size.Width - clientSize.Width);
            clientAdjust.Height = (size.Height - clientSize.Height);

            if (newBounds.Width == Int32.MaxValue && newBounds.Height == Int32.MaxValue)
                throw new Exception("WindowlessControlHost has two infinite dimensions.");

            if (Control != null)
            {
                bool boundsChange = bounds.Width != size.Width || bounds.Height != size.Height;
                if (Control.NeedsMeasure || boundsChange)
                    Control.Measure(newBounds);
                Size newSize = new Size(Math.Min(Control.Width + clientAdjust.Width, bounds.Width), Math.Min(Control.Height + clientAdjust.Height, bounds.Height));
                if (bounds.Width != Int32.MaxValue && HorizontalAlignment == HorizontalAlignment.Stretch)
                    newSize.Width = bounds.Width;
                if (bounds.Height != Int32.MaxValue && VerticalAlignment == VerticalAlignment.Stretch)
                    newSize.Height = bounds.Height;
                if (size != newSize)
                {
                    myOldSize = newSize;
                    Size = newSize;
                }
            }
            else
            {
                if (bounds.Width != Int32.MaxValue && HorizontalAlignment == HorizontalAlignment.Stretch)
                    Width = bounds.Width;
                if (bounds.Height != Int32.MaxValue && VerticalAlignment == VerticalAlignment.Stretch)
                    Height = bounds.Height;
            }

            SyncWindows();
        }

        bool myRemeasureCalledWithin = false;
        bool myInRemeasure = false;
        public virtual void Remeasure()
        {
            NeedsMeasure = true;
            if (mySuspendRemeasure)
                return;
            
            if (myParent != null)
                myParent.Remeasure();
            else if (Control != null && Parent != null)
            {
                if (myInRemeasure)
                {
                    myRemeasureCalledWithin = true;
                    return;
                }

                try
                {
                    myInRemeasure = true;
                    do
                    {
                        myRemeasureCalledWithin = false;
                        Control.Measure(MeasureBounds);
                        SyncWindows();
                    }
                    while (myRemeasureCalledWithin);
                }
                finally
                {
                    myInRemeasure = false;
                }
            }
        }
        
        HorizontalAlignment myHorizontalAlignment;
        [XmlAttribute]
        public HorizontalAlignment HorizontalAlignment
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

        VerticalAlignment myVerticalAlignment;
        [XmlAttribute]
        public VerticalAlignment VerticalAlignment
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

        void SyncWindows()
        {
            if (Control != null)
            {
                if (AutoScrollPosition.Y != Control.Top || AutoScrollPosition.X != Control.Left)
                {
                    Control.Left = AutoScrollPosition.X;
                    Control.Top = AutoScrollPosition.Y;
                    WindowlessInvalidate();
                }
            }

            foreach (Control control in Controls)
            {
                IWindowlessControl wc = control as IWindowlessControl;
                if (wc == null)
                    continue;
                Point p = WindowlessControlHost.WindowlessPointToHost(wc, Point.Empty);
                if (control.Left != p.X)
                {
                    control.Left = p.X;
                }
                if (control.Top != p.Y)
                {
                    control.Top = p.Y;
                }
            }

            if (myAutoScroll)
            {
                if (Control.Width <= ClientSize.Width && Control.Height <= ClientSize.Height)
                {
                    if (base.AutoScroll)
                        base.AutoScroll = false;
                }
                else
                {
                    if (!base.AutoScroll)
                        base.AutoScroll = true;
                }
            }

            // note: this code causes infinite loop
            // note: this code no longer seems necessary, no wierd scrollbars as of late...
            // see if we got scroll bars
            // fix a bug with windows being horrible at making scrollbars disappear.
            //if (AutoScroll)
            //{
            //    SuspendRemeasure();
            //    foreach (Control control in Controls)
            //    {
            //        if (control.Bottom > ClientSize.Height || control.Right > ClientSize.Width || control.Left < 0 || control.Top < 0 && control.Visible)
            //        {
            //            control.Visible = false;
            //            control.Visible = true;
            //            break;
            //        }
            //    }
            //    ResumeRemeasure();
            //}
        }

        #endregion
        bool mySuspendRemeasure = false;

        public bool RemeasureSuspended
        {
            get { return mySuspendRemeasure; }
        }
        public void SuspendRemeasure()
        {
            mySuspendRemeasure = true;
        }

        public void ResumeRemeasure()
        {
            mySuspendRemeasure = false;
        }

        public static WindowlessControlHost GetHost(IWindowlessControl control)
        {
            control = control.Parent;
            while (control != null && !(control is WindowlessControlHost))
            {
                control = control.Parent;
            }
            return control as WindowlessControlHost;
        }

        public static Control FormlessTopLevelControl(Control control)
        {
            Control last;
            do
            {
                last = control;
                control = control.Parent;
            }
            while (control != null);
            return last;
        }

        public static WindowlessControlHost GetRoot(WindowlessControlHost host)
        {
            WindowlessControlHost last;
            do
            {
                last = host;
                host = host.Parent as WindowlessControlHost;
            }
            while (host != null);
            return last;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            DisposeBackBuffer();
        }

        public void HostControl(Control control, bool matchHeight, bool matchWidth)
        {
            control.Dock = DockStyle.None;
            if (!Controls.Contains(control))
                Controls.Add(control);
            
            if (matchHeight)
                Height = control.Height;
            else
                VerticalAlignment = VerticalAlignment.Stretch;
            if (matchWidth)
                Width = control.Width;
            else
                HorizontalAlignment = HorizontalAlignment.Stretch;

            control.Dock = DockStyle.Fill;
        }

        public static WindowlessControlHost CreateHostControl(Control control, bool matchHeight, bool matchWidth)
        {
            WindowlessControlHost ret = new WindowlessControlHost();
            ret.HostControl(control, matchHeight, matchWidth);
            return ret;
        }

        public static bool LoopParent<T>(T control, ControlLoopHandler<T> handler) where T : Control
        {
            while (control != null)
            {
                if (!handler(control))
                    return false;
            }
            return true;
        }
    
        public static bool RecurseWindowlessControls(IWindowlessControl control, WindowlessControlRecursionHandler pre, WindowlessControlRecursionHandler post)
        {
            if (pre != null)
            {
                if (!pre(control))
                    return false;
            }

            if (control.Controls != null)
            {
                foreach (IWindowlessControl child in control.Controls)
                {
                    if (!RecurseWindowlessControls(child, pre, post))
                        return false;
                }
            }

            if (post != null)
            {
                if (!post(control))
                    return false;
            }

            return true;
        }

        public static bool RecurseControls(Control control, ControlRecursionHandler pre, ControlRecursionHandler post)
        {
            if (pre != null)
            {
                if (!pre(control))
                    return false;
            }

            foreach (Control child in control.Controls)
            {
                if (!RecurseControls(child, pre, post))
                    return false;
            }

            if (post != null)
            {
                if (!post(control))
                    return false;
            }

            return true;
        }

        public bool myAutoScroll = false;
        public override bool AutoScroll
        {
            get
            {
                return myAutoScroll;
            }
            set
            {
                myAutoScroll = value;
            }
        }
    }

    public delegate bool ControlLoopHandler<T>(T control) where T: Control;
    public delegate bool WindowlessControlRecursionHandler(IWindowlessControl control);
    public delegate bool ControlRecursionHandler(Control control);

    public enum Orientation
    {
        Vertical,
        Horizontal,
        None
    }
}
