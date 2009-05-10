using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using WindowlessControls;

namespace WindowlessControls
{
    public class ButtonBase<T> : ButtonBase where T : class, IWindowlessControl, new()
    {
        public ButtonBase()
        {
            base.Control = new T();
        }

        public new T Control
        {
            get
            {
                return base.Control as T;
            }
        }
    }

    public class ButtonBase : WindowlessControlHost
    {
        public ButtonBase()
        {
            Focusable = true;
        }

        private new IInteractiveStyleControl Control
        {
            get
            {
                return base.Control as IInteractiveStyleControl;
            }
            set
            {
                base.Control = value;
            }
        }

        PropertyListener myClickListener;
        void OnClickDown()
        {
            if (myClickListener == null)
            {
                myClickListener = new PropertyListener();
                myClickListener.StartListen();
                OnApplyClickedStyle();
                myClickListener.StopListen();
                Update();
            }
        }

        protected virtual void OnApplyClickedStyle()
        {
            if (Control != null)
                Control.ApplyClickedStyle();
        }

        void OnClickUp()
        {
            if (myClickListener != null)
            {
                myClickListener.Undo();
                Update();
            }
            myClickListener = null;
        }

        protected override void OnWindowlessMouseDown(WindowlessControlHost sender, WindowlessMouseEventArgs e)
        {
            if (sender == this)
                OnClickDown();
            base.OnWindowlessMouseDown(sender, e);
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

        protected override void OnWindowlessKeyDown(System.Windows.Forms.Control sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == System.Windows.Forms.Keys.Enter && sender == this)
            {
                e.Handled = true;
                OnClickDown();
            }
            base.OnWindowlessKeyDown(sender, e);
        }

        protected override void OnWindowlessKeyUp(System.Windows.Forms.Control sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == System.Windows.Forms.Keys.Enter && sender == this)
            {
                OnClickUp();
            }
            base.OnWindowlessKeyUp(sender, e);
        }

        protected override void OnWindowlessKeyPress(System.Windows.Forms.Control sender, System.Windows.Forms.KeyPressEventArgs e)
        {
            if (!e.Handled && e.KeyChar == '\r' && sender == this)
            {
                InvokeClick();
                e.Handled = true;
            }
            base.OnWindowlessKeyPress(sender, e);
        }

        PropertyListener myFocusListener;
        protected override void OnWindowlessGotFocus(WindowlessControlHost sender, EventArgs e)
        {
            if (myFocusListener == null)
            {
                myFocusListener = new PropertyListener();
                myFocusListener.StartListen();
                OnApplyFocusedStyle();
                myFocusListener.StopListen();
                Update();
                base.OnWindowlessGotFocus(sender, e);
            }
        }

        protected virtual void OnApplyFocusedStyle()
        {
            if (Control != null)
            {
                Control.ApplyFocusedStyle();
            }
        }

        protected override void OnWindowlessLostFocus(WindowlessControlHost sender, EventArgs e)
        {
            if (myFocusListener != null)
            {
                myFocusListener.Undo();
                Update();
            }
            myFocusListener = null;
            base.OnWindowlessLostFocus(sender, e);
        }
    }

    public class WindowlessImagePresenter : WindowlessImage, IInteractiveStyleControl
    {
        PlatformBitmap myFocusedBitmap;

        public PlatformBitmap FocusedBitmap
        {
            get { return myFocusedBitmap; }
            set { myFocusedBitmap = value; }
        }

        PlatformBitmap myClickedBitmap;

        public PlatformBitmap ClickedBitmap
        {
            get { return myClickedBitmap; }
            set { myClickedBitmap = value; }
        }

        #region IInteractiveStyleControl Members

        public void ApplyFocusedStyle()
        {
            if (myFocusedBitmap != null)
                Bitmap = myFocusedBitmap;
        }

        public void ApplyClickedStyle()
        {
            if (myClickedBitmap != null)
                Bitmap = myClickedBitmap;
        }

        #endregion
    }
}