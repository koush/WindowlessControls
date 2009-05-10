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
    public class DockLayout
    {
        public LayoutMeasurement LayoutMeasurement;
        public DockStyle DockStyle;

        public double Measurement
        {
            get
            {
                return LayoutMeasurement.Measurement;
            }
            set
            {
                LayoutMeasurement.Measurement = value;
            }
        }

        public LayoutUnit Unit
        {
            get
            {
                return LayoutMeasurement.Unit;
            }
            set
            {
                LayoutMeasurement.Unit = value;
            }
        }


        public DockLayout(LayoutMeasurement measurement, DockStyle dockStyle)
        {
            LayoutMeasurement = measurement;
            DockStyle = dockStyle;
        }
    }

    public class DockPanel : WindowlessPanel
    {
        DependencyPropertyStorage<bool> myCapDimension;
        public DockPanel()
        {
            myCapDimension = new DependencyPropertyStorage<bool>(this, false, new DependencyPropertyChangedEvent(CapDimensionChanged));
        }

        void CapDimensionChanged(object sender, DependencyPropertyEventArgs e)
        {
            Remeasure();
        }

        public bool CapDimension
        {
            get
            {
                return myCapDimension;
            }
            set
            {
                myCapDimension.Value = value;
            }
        }

        public override bool MeasureUnpadded(Size bounds, bool boundsChange)
        {
            ClientWidth = 0;
            ClientHeight = 0;
            if (Controls.Count == 0)
                return false;

            int newWidth = bounds.Width;
            int newHeight = bounds.Height;

            Rectangle r = new Rectangle(Margin.Left, Margin.Top, newWidth, newHeight);

            bool layoutChange = false;
            // lay everything out first
            int maxLeft = Margin.Left;
            int maxRight = Margin.Left;
            int maxTop = Margin.Top;
            int maxBottom = Margin.Top;
            foreach (IWindowlessControl control in Controls)
            {
                if (!control.Visible)
                    continue;
                Rectangle oldRect = new Rectangle(control.Left, control.Top, control.Width, control.Height);

                DockLayout layout = control.Layout as DockLayout;
                if ((layout.Measurement > 1 || layout.Measurement < 0) && layout.Unit == LayoutUnit.Star)
                {
                    throw new Exception("Measurement invalid.");
                }

                // just do set the value for both width and height, only one value is really used.
                int layoutWidth = (int)Math.Round(layout.Measurement, 0);
                int layoutHeight = layoutWidth;
                int newRectWidth = r.Width;
                int newRectHeight = r.Height;

                switch (layout.DockStyle)
                {
                    case DockStyle.Top:
                    case DockStyle.Bottom:
                        if (layout.Unit == LayoutUnit.Star)
                        {
                            if (layout.Measurement == 0)
                                layoutHeight = r.Height;
                            else
                                layoutHeight = (int)Math.Round(r.Height * layout.Measurement, 0);
                        }
                        control.Measure(new Size(r.Width, layoutHeight));
                        if (layout.Unit == LayoutUnit.Star && layout.Measurement == 0)
                            layoutHeight = control.Height;
                        if (bounds.Height == Int32.MaxValue)
                            newRectHeight = Int32.MaxValue;
                        else
                            newRectHeight = r.Height - layoutHeight;
                        break;
                    case DockStyle.Left:
                    case DockStyle.Right:
                        if (layout.Unit == LayoutUnit.Star)
                        {
                            if (layout.Measurement == 0)
                                layoutWidth = r.Width;
                            else
                                layoutWidth = (int)Math.Round(r.Width * layout.Measurement, 0);
                        }
                        control.Measure(new Size(layoutWidth, r.Height));
                        if (layout.Unit == LayoutUnit.Star && layout.Measurement == 0)
                            layoutWidth = control.Width;
                        if (bounds.Width == Int32.MaxValue)
                            newRectWidth = Int32.MaxValue;
                        else
                            newRectWidth = r.Width - layoutWidth;
                        break;
                    default:
                        throw new ArgumentException("Invalid DockStyle.");
                }


                if (bounds.Width == Int32.MaxValue && myCapDimension && (layout.DockStyle == DockStyle.Top || layout.DockStyle == DockStyle.Bottom))
                    r.Width = bounds.Width = control.Width;

                if (bounds.Height == Int32.MaxValue && myCapDimension && (layout.DockStyle == DockStyle.Left || layout.DockStyle == DockStyle.Right))
                    r.Height = bounds.Height = control.Height;

                switch (layout.DockStyle)
                {
                    case DockStyle.Left:
                        LayoutControl(control, new Rectangle(r.Left, r.Top, layoutWidth, r.Height));
                        r = new Rectangle(r.Left + layoutWidth, r.Top, newRectWidth, r.Height);
                        break;
                    case DockStyle.Top:
                        LayoutControl(control, new Rectangle(r.Left, r.Top, r.Width, layoutHeight));
                        r = new Rectangle(r.Left, r.Top + layoutHeight, r.Width, newRectHeight);
                        break;
                    case DockStyle.Right:
                        LayoutControl(control, new Rectangle(r.Right - layoutWidth, r.Top, layoutWidth, r.Height));
                        r = new Rectangle(r.Left, r.Top, newRectWidth, r.Height);
                        break;
                    case DockStyle.Bottom:
                        LayoutControl(control, new Rectangle(r.Left, r.Bottom - layoutHeight, r.Width, layoutHeight));
                        r = new Rectangle(r.Left, r.Top, r.Width, newRectHeight);
                        break;
                }
                Rectangle newRect = new Rectangle(control.Left, control.Top, control.Width, control.Height);
                layoutChange |= oldRect != newRect;
                maxLeft = Math.Max(maxLeft, newRect.Left);
                maxRight = Math.Max(maxRight, newRect.Right);
                maxTop = Math.Max(maxTop, newRect.Top);
                maxBottom = Math.Max(maxBottom, newRect.Bottom);

                if (bounds.Width == Int32.MaxValue)
                {
                    if (newWidth == Int32.MaxValue)
                        newWidth = control.Left + control.Width;
                    else
                        newWidth = Math.Max(newWidth, control.Left + control.Width);
                }

                if (bounds.Height == Int32.MaxValue)
                {
                    if (newHeight == Int32.MaxValue)
                        newHeight = control.Top + control.Height;
                    else
                        newHeight = Math.Max(newHeight, control.Top + control.Height);
                }
            }

            if (HorizontalAlignment == HorizontalAlignment.Stretch)
                ClientWidth = newWidth;
            else
            {
                ClientWidth = maxRight - Margin.Left;
            }
            if (VerticalAlignment == VerticalAlignment.Stretch)
                ClientHeight = newHeight;
            else
            {
                ClientHeight = maxBottom - Margin.Top;
            }
            return layoutChange;
        }
    }
}