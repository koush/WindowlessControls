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
    public class StackPanel : WindowlessPanel
    {
        public override bool MeasureUnpadded(Size bounds, bool boundsChange)
        {
            if (!(bounds.Width != Int32.MaxValue ^ bounds.Height != Int32.MaxValue))
                throw new Exception("Stackpanel needs to have one infinite dimension.");

            bool layoutChange = false;
            int maxWidth = 0;
            int maxHeight = 0;
            foreach (IWindowlessControl control in Controls)
            {
                if (!control.Visible)
                    continue;
                Rectangle oldRect = new Rectangle(control.Left, control.Top, control.Width, control.Height);
                if (control.NeedsMeasure || boundsChange)
                    control.Measure(bounds);

                maxWidth = Math.Max(maxWidth, control.Width);
                maxHeight = Math.Max(maxHeight, control.Height);
                //layoutChange |= oldRect != new Rectangle(control.Left, control.Top, control.Width, control.Height);
                //layoutChange |= oldRect.Height != control.Height;
            }

            if (HorizontalAlignment == HorizontalAlignment.Stretch && bounds.Width != Int32.MaxValue)
                maxWidth = bounds.Width;
            if (VerticalAlignment == VerticalAlignment.Stretch && bounds.Height != Int32.MaxValue)
                maxHeight = bounds.Height;

            int offset;
            if (bounds.Width == Int32.MaxValue)
            {
                offset = 0;
                foreach (IWindowlessControl control in Controls)
                {
                    if (!control.Visible)
                        continue;
                    Rectangle oldRect = new Rectangle(control.Left, control.Top, control.Width, control.Height);
                    LayoutControl(control, new Rectangle(Margin.Left + offset, Margin.Top, control.Width, maxHeight));
                    //layoutChange |= oldRect != new Rectangle(control.Left, control.Top, control.Width, control.Height);
                    layoutChange |= control.Left != oldRect.Left || control.Width != oldRect.Width;
                    offset += control.Width;
                }
                ClientWidth = offset;
                ClientHeight = maxHeight;
            }
            else
            {
                offset = 0;
                foreach (IWindowlessControl control in Controls)
                {
                    if (!control.Visible)
                        continue;
                    Rectangle oldRect = new Rectangle(control.Left, control.Top, control.Width, control.Height);
                    LayoutControl(control, new Rectangle(Margin.Left, Margin.Top + offset, maxWidth, control.Height));
                    //layoutChange |= oldRect != new Rectangle(control.Left, control.Top, control.Width, control.Height);
                    layoutChange |= control.Top != oldRect.Top || control.Height != oldRect.Height;
                    offset += control.Height;
                }
                ClientHeight = offset;
                ClientWidth = maxWidth;
            }
            return layoutChange;
        }
    }
}
