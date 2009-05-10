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


namespace WindowlessControls
{
    public partial class WindowlessControlHost : ScrollableControl, IWindowlessControl
    {
        public void InvokeClick()
        {
            // fire the click event
            OnClick(null);
        }

        protected override void OnClick(EventArgs e)
        {
            Point p = PointToClient(new Point(MousePosition.X, MousePosition.Y));
            Control parent = this;
            WindowlessMouseEventArgs we = new WindowlessMouseEventArgs(MouseButtons.Left, 1, p.X, p.Y, 0, false);
            while (parent != null)
            {
                WindowlessControlHost parentHost = parent as WindowlessControlHost;
                if (parentHost != null)
                    parentHost.OnWindowlessClick(this, we);
                we = new WindowlessMouseEventArgs(MouseButtons.Left, 1, p.X + Left, p.Y + Top, 0, we.Handled);
                parent = parent.Parent;
            }
            base.OnClick(e);
        }

        protected override void OnDoubleClick(EventArgs e)
        {
            Point p = PointToClient(new Point(MousePosition.X, MousePosition.Y));
            Control parent = this;
            WindowlessMouseEventArgs we = new WindowlessMouseEventArgs(MouseButtons.Left, 1, p.X, p.Y, 0, false);
            while (parent != null)
            {
                WindowlessControlHost parentHost = parent as WindowlessControlHost;
                if (parentHost != null)
                    parentHost.OnWindowlessDoubleClick(this, we);
                we = new WindowlessMouseEventArgs(MouseButtons.Left, 1, p.X + Left, p.Y + Top, 0, we.Handled);
                parent = parent.Parent;
            }
            base.OnDoubleClick(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            OnWindowlessMouseMove(this, new WindowlessMouseEventArgs(e));
            base.OnMouseMove(e);
        }

        protected virtual void OnWindowlessMouseFocus(WindowlessControlHost sender, MouseEventArgs e)
        {
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (Focusable && !Focused)
            {
                WindowlessControlHost control = this;
                while (control != null)
                {
                    control.OnWindowlessMouseFocus(this, e);
                    control = control.Parent as WindowlessControlHost;
                }
                Focus();
            }
            OnWindowlessMouseDown(this, new WindowlessMouseEventArgs(e));
            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            OnWindowlessMouseUp(this, new WindowlessMouseEventArgs(e));
            base.OnMouseUp(e);
        }

        protected virtual void OnWindowlessMouseMove(WindowlessControlHost sender, WindowlessMouseEventArgs e)
        {
        }

        protected virtual void OnWindowlessMouseDown(WindowlessControlHost sender, WindowlessMouseEventArgs e)
        {
        }

        protected virtual void OnWindowlessMouseUp(WindowlessControlHost sender, WindowlessMouseEventArgs e)
        {
        }

        protected virtual void OnWindowlessClick(WindowlessControlHost sender, WindowlessMouseEventArgs e)
        {
            if (WindowlessClick != null)
                WindowlessClick(sender, e);
        }

        protected virtual void OnWindowlessDoubleClick(WindowlessControlHost sender, WindowlessMouseEventArgs e)
        {
            if (WindowlessDoubleClick != null)
                WindowlessDoubleClick(sender, e);
        }

        bool myFocusable = false;
        public virtual bool Focusable
        {
            get { return myFocusable; }
            set { myFocusable = value; }
        }

        public event WindowlessMouseEvent WindowlessClick;
        public event WindowlessMouseEvent WindowlessDoubleClick;
    }

    public delegate void WindowlessMouseEvent(WindowlessControlHost sender, WindowlessMouseEventArgs e);

    public class WindowlessMouseEventArgs : MouseEventArgs
    {
        public WindowlessMouseEventArgs(MouseEventArgs e)
            : base (e.Button, 1, e.X, e.Y, 0)
        {
            Handled = false;
        }
        bool myHandled;

        public bool Handled
        {
            get { return myHandled; }
            set { myHandled = value; }
        }

        public WindowlessMouseEventArgs(MouseButtons button, int clicks, int x, int y, int delta, bool handled)
            : base(button, clicks, x, y, delta)
        {
            myHandled = handled;
        }
    }
}