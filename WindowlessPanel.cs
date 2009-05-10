//////////////////////////////////////////////////////////////
// Koushik Dutta - 9/1/2007
//////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Collections;

namespace WindowlessControls
{
    public class WindowlessPanel : WindowlessControl
    {
        public WindowlessPanel()
        {
            InitializeControls();
        }

        protected void LayoutControl(IWindowlessControl control, Rectangle rect)
        {
            switch (control.HorizontalAlignment)
            {
                case HorizontalAlignment.Stretch:
                case HorizontalAlignment.Left:
                    control.Left = rect.Left;
                    break;
                case HorizontalAlignment.Center:
                    control.Left = (rect.Left + rect.Right - control.Width) / 2;
                    break;
                case HorizontalAlignment.Right:
                    control.Left = rect.Right - control.Width;
                    break;
            }
            switch (control.VerticalAlignment)
            {
                case VerticalAlignment.Stretch:
                case VerticalAlignment.Top:
                    control.Top = rect.Top;
                    break;
                case VerticalAlignment.Center:
                    control.Top = (rect.Top + rect.Bottom - control.Height) / 2;
                    break;
                case VerticalAlignment.Bottom:
                    control.Top = rect.Bottom - control.Height;
                    break;
            }
        }

        bool mySuspendRemeasure = false;
        public void SuspendRemeasure()
        {
            mySuspendRemeasure = true;
        }

        public void ResumeRemeasure()
        {
            mySuspendRemeasure = false;
        }

        public override void Remeasure()
        {
            NeedsMeasure = true;
            if (!mySuspendRemeasure)
                base.Remeasure();
        }
    }

    public class SerializableControlCollection : WindowlessControlCollection
    {
        public SerializableControlCollection(IWindowlessControl control)
            : base(control)
        {
        }

        public void Add(object o)
        {
            base.Add(o as IWindowlessControl);
        }

        public new object this[int index]
        {
            get
            {
                return base[index];
            }
            set
            {
                base[index] = value as IWindowlessControl;
            }
        }
    }

    public class WindowlessControlCollection : EncapsulatedList<IWindowlessControl>
    {
        IWindowlessControl myControl;
        public WindowlessControlCollection(IWindowlessControl control)
        {
            myControl = control;
        }

        public override void Add(IWindowlessControl item)
        {
            base.Add(item);
            item.Parent = myControl;
            myControl.Remeasure();
        }

        public override void Insert(int index, IWindowlessControl item)
        {
            base.Insert(index, item);
            item.Parent = myControl;
            myControl.Remeasure();
        }

        // deparenting removes it from the list
        public override bool Remove(IWindowlessControl item)
        {
            bool ret = base.Remove(item);
            // only set the parent to null if it was successful. possible infinite loop otherwise.
            if (ret)
            {
                item.Parent = null;
                myControl.Remeasure();
            }
            return ret;
        }

        public override void RemoveAt(int index)
        {
            IWindowlessControl item = this[index];
            Remove(item);
        }

        public override void Clear()
        {
            // todo: prevent the million remeasures.
            // maybe add suspend remeasure to the interface?
            IWindowlessControl[] arr = new IWindowlessControl[Count];
            CopyTo(arr, 0);
            base.Clear();
            foreach (IWindowlessControl control in arr)
            {
                control.Parent = null;
            }
            myControl.Remeasure();
        }
    }

    public enum LayoutUnit
    {
        Star,
        Pixel
    }
    public struct LayoutMeasurement
    {
        public LayoutMeasurement(double measurement, LayoutUnit unit)
        {
            Measurement = measurement;
            Unit = unit;
        }

        public LayoutUnit Unit;
        public double Measurement;
    }
}