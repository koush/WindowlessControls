using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace WindowlessControls
{
    public class OverlayPanel : WindowlessPanel
    {
        IWindowlessControl myFitWidthControl = null;

        public IWindowlessControl FitWidthControl
        {
            get { return myFitWidthControl; }
            set { myFitWidthControl = value; }
        }

        IWindowlessControl myFitHeightControl = null;

        public IWindowlessControl FitHeightControl
        {
            get { return myFitHeightControl; }
            set { myFitHeightControl = value; }
        }


        public override bool MeasureUnpadded(Size bounds, bool boundsChange)
        {
            bool layoutChange = false;
            int maxWidth = 0;
            int maxHeight = 0;
            bool fitChange = false;

            if (myFitWidthControl != null)
            {
                if (!Controls.Contains(myFitWidthControl))
                    throw new Exception("The Fit Control is not in the control collection.");

                Rectangle oldRect = new Rectangle(myFitWidthControl.Left, myFitWidthControl.Top, myFitWidthControl.Width, myFitWidthControl.Height);
                myFitWidthControl.Measure(bounds);

                maxWidth = Math.Max(maxWidth, myFitWidthControl.Width);
                maxHeight = Math.Max(maxHeight, myFitWidthControl.Height);

                layoutChange |= fitChange = oldRect != new Rectangle(myFitWidthControl.Left, myFitWidthControl.Top, myFitWidthControl.Width, myFitWidthControl.Height);
                bounds.Width = maxWidth = myFitWidthControl.Width;
            }
            if (myFitHeightControl != null)
            {
                if (!Controls.Contains(myFitHeightControl))
                    throw new Exception("The Fit Control is not in the control collection.");

                if (myFitHeightControl != myFitWidthControl)
                {
                    Rectangle oldRect = new Rectangle(myFitHeightControl.Left, myFitHeightControl.Top, myFitHeightControl.Width, myFitHeightControl.Height);
                    myFitHeightControl.Measure(bounds);

                    maxWidth = Math.Max(maxWidth, myFitHeightControl.Width);
                    maxHeight = Math.Max(maxHeight, myFitHeightControl.Height);

                    layoutChange |= fitChange = oldRect != new Rectangle(myFitHeightControl.Left, myFitHeightControl.Top, myFitHeightControl.Width, myFitHeightControl.Height);
                }
                bounds.Height = maxHeight = myFitHeightControl.Height;
            }

            foreach (IWindowlessControl control in Controls)
            {
                if (control == myFitWidthControl || control == myFitHeightControl)
                    continue;

                Rectangle oldRect = new Rectangle(control.Left, control.Top, control.Width, control.Height);
                if (control.NeedsMeasure || boundsChange || fitChange)
                    control.Measure(bounds);

                maxWidth = Math.Max(maxWidth, control.Width);
                maxHeight = Math.Max(maxHeight, control.Height);
                layoutChange |= oldRect != new Rectangle(control.Left, control.Top, control.Width, control.Height);
            }

            ClientWidth = HorizontalAlignment == HorizontalAlignment.Stretch && bounds.Width != Int32.MaxValue ? bounds.Width : maxWidth;
            ClientHeight = VerticalAlignment == VerticalAlignment.Stretch && bounds.Height != Int32.MaxValue ? bounds.Height : maxHeight;

            foreach (IWindowlessControl control in Controls)
            {
                LayoutControl(control, new Rectangle(Margin.Left, Margin.Top, ClientWidth, ClientHeight));
            }

            return layoutChange;
        }
    }
}