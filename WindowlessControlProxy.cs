using System;

using System.Collections.Generic;
using System.Text;

namespace WindowlessControls
{
    public class WindowlessControlProxy : IWindowlessControl
    {
        public WindowlessControlProxy()
        {
            myControls = new SerializableControlCollection(this);
        }

        IWindowlessControl myControl;

        public IWindowlessControl Control
        {
            get { return myControl; }
            set
            {
                myControl = value;
                myControls.Add(value);
            }
        }

        #region IWindowlessControl Members

        public string Name
        {
            get
            {
                return myControl.Name;
            }
            set
            {
                myControl.Name = value;
            }
        }

        public int Width
        {
            get { return myControl.Width; }
        }

        public int Height
        {
            get { return myControl.Height; }
        }

        int myLeft;
        public int Left
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

        int myTop;
        public int Top
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

        public HorizontalAlignment HorizontalAlignment
        {
            get
            {
                return myControl.HorizontalAlignment;
            }
            set
            {
                myControl.HorizontalAlignment = value;
            }
        }

        public VerticalAlignment VerticalAlignment
        {
            get
            {
                return myControl.VerticalAlignment;
            }
            set
            {
                myControl.VerticalAlignment = value;
            }
        }

        IWindowlessControl myParent;
        public IWindowlessControl Parent
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

        SerializableControlCollection myControls;
        public WindowlessControlCollection Controls
        {
            get { return myControls; }
        }

        public bool NeedsMeasure
        {
            get
            {
                return myControl.NeedsMeasure;
            }
            set
            {
                myControl.NeedsMeasure = value;
            }
        }

        public void Measure(System.Drawing.Size bounds)
        {
            myControl.Left = 0;
            myControl.Top = 0;
            myControl.Measure(bounds);
        }

        public void Remeasure()
        {
            if (myParent != null)
                myParent.Remeasure();
        }

        public bool Visible
        {
            get
            {
                return myControl.Visible;
            }
            set
            {
                myControl.Visible = value;
            }
        }

        public object Layout
        {
            get
            {
                return myControl.Layout;
            }
            set
            {
                myControl.Layout = value;
            }
        }

        #endregion
    }
}
