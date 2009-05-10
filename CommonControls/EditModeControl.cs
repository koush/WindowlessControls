using System;
using WindowlessControls;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace WindowlessControls.CommonControls
{
    public class EditModeControl : ButtonBase
    {
        public EditModeControl()
        {
            Focusable = true;
        }

        bool myIsEditing = false;
        public bool IsEditing
        {
            get
            {
                return myIsEditing;
            }
            set
            {
                if (value != myIsEditing)
                {
                    myIsEditing = value;
                    OnEditingChanged();
                }
            }
        }

        //protected override void OnWindowlessKeyPress(Control sender, KeyPressEventArgs e)
        //{
        //    base.OnWindowlessKeyPress(sender, e);

        //    if (!e.Handled && e.KeyChar == '\r')
        //    {
        //        IsEditing = !IsEditing;
        //    }
        //}

        protected override void OnWindowlessClick(WindowlessControlHost sender, WindowlessMouseEventArgs e)
        {
            if (!e.Handled)
            {
                e.Handled = true;
                IsEditing = !IsEditing;
            }
            base.OnWindowlessClick(sender, e);
        }

        protected virtual bool HasEditFocus
        {
            get
            {
                return FindFocusedControl(this) != null;
            }
        }

        protected override void OnWindowlessLostFocus(WindowlessControlHost sender, EventArgs e)
        {
            base.OnWindowlessLostFocus(sender, e);

            if (!HasEditFocus && IsEditing)
            {
                IsEditing = false;
            }
        }

        protected virtual void OnEditingChanged()
        {
        }
    }
}
