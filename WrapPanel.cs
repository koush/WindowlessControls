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
    public enum WrapStyle
    {
        Greedy,
        Uniform,
        UniformGrid,
        UniformDistributionGrid,
    }

    public class WrapPanel : WindowlessPanel
    {
        WrapStyle myWrapStyle = WrapStyle.Greedy;

        public WrapStyle WrapStyle
        {
            get { return myWrapStyle; }
            set
            {
                myWrapStyle = value;
                Remeasure();
            }
        }

        public override bool MeasureUnpadded(Size bounds, bool boundsChange)
        {
            // todo: this method is fuckin huge.
            // can refactor by using reflection so the width/height crap can be done in a single spot.
            if (!(bounds.Width != Int32.MaxValue ^ bounds.Height != Int32.MaxValue))
                throw new Exception("WrapPanel needs to have one infinite dimension.");

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

                layoutChange |= oldRect != new Rectangle(control.Left, control.Top, control.Width, control.Height);
            }

            int newWidth = 0;
            int newHeight = 0;
            if (bounds.Width == Int32.MaxValue)
            {
                // vertical wrapping
                int left = 0;
                int top = 0;
                int maxColumnWidth = 0;

                //if (WrapStyle == WrapStyle.UniformGrid)
                //{
                //    maxHeight = bounds.Height / (bounds.Height / maxHeight);
                //}

                foreach (IWindowlessControl control in Controls)
                {
                    if (!control.Visible)
                        continue;
                    int controlHeight = WrapStyle == WrapStyle.Greedy ? control.Height : maxHeight;

                    // start a new column if necessary
                    if (controlHeight + top > bounds.Height)
                    {
                        top = 0;
                        left += WrapStyle == WrapStyle.UniformGrid ? maxWidth : maxColumnWidth;
                        maxColumnWidth = 0;
                        newHeight = Math.Max(newHeight, top);
                    }

                    control.Top = top + Margin.Top;
                    control.Left = left + Margin.Left;
                    top += controlHeight;

                    maxColumnWidth = Math.Max(maxColumnWidth, control.Width);
                }

                newWidth = left + maxColumnWidth;
                newHeight = Math.Max(newHeight, top);
            }
            else
            {
                // horizontal wrapping
                int left = 0;
                int top = 0;
                int maxRowHeight = 0;

                if (WrapStyle == WrapStyle.UniformGrid && HorizontalAlignment == HorizontalAlignment.Stretch)
                {
                    maxWidth = bounds.Width / (bounds.Width / maxWidth);
                }

                //foreach (IWindowlessControl control in Controls)
                int start = 0;
                int end = 0;
                for (int i = 0; i < Controls.Count; i++)
                {
                    IWindowlessControl control = Controls[i];
                    if (!control.Visible)
                        continue;
                    int controlWidth = WrapStyle == WrapStyle.Greedy ? control.Width : maxWidth;

                    // start a new column if necessary
                    if (controlWidth + left > bounds.Width)
                    {
                        HorizontalLayoutControls(top + Margin.Top, maxWidth, WrapStyle == WrapStyle.Greedy ? maxHeight : maxRowHeight, start, end);
                        start = end;
                        end = i;
                        newWidth = Math.Max(newWidth, left);
                        left = 0;
                        top += WrapStyle == WrapStyle.UniformGrid ? maxHeight : maxRowHeight;
                        maxRowHeight = 0;
                    }
                    end++;

                    //control.Top = top + Margin.Top;
                    //control.Left = left + Margin.Left;
                    left += controlWidth;

                    maxRowHeight = Math.Max(maxRowHeight, control.Height);
                }
                if (end <= Controls.Count)
                    HorizontalLayoutControls(top + Margin.Top, maxWidth, WrapStyle == WrapStyle.Greedy ? maxHeight : maxRowHeight, start, Controls.Count);

                newHeight = top + maxRowHeight;
                newWidth = Math.Max(newWidth, left);
            }

            if (HorizontalAlignment == HorizontalAlignment.Stretch && bounds.Width != Int32.MaxValue)
                ClientWidth = bounds.Width;
            else
                ClientWidth = newWidth;
            if (VerticalAlignment == VerticalAlignment.Stretch && bounds.Height != Int32.MaxValue)
                ClientHeight = bounds.Height;
            else
                ClientHeight = newHeight;

            return layoutChange;
        }

        void HorizontalLayoutControls(int top, int maxWidth, int maxHeight, int start, int end)
        {
            int offset = 0;
            for (int i = start; i < end; i++)
            {
                IWindowlessControl control = Controls[i];
                if (!control.Visible)
                    continue;
                int controlWidth = WrapStyle == WrapStyle.Greedy ? control.Width : maxWidth;
                LayoutControl(control, new Rectangle(Margin.Left + offset, top, controlWidth, maxHeight));
                offset += controlWidth;
            }
        }
    }
}
