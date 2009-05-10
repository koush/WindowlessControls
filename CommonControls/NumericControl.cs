using System;
using WindowlessControls;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace WindowlessControls.CommonControls
{
    public class WindowlessUpDownArrows : WindowlessPaintControl
    {
        public WindowlessUpDownArrows()
        {
            Margin = new Thickness(2, 2, 2, 2);
            MaxHeight = 13;
            MaxWidth = 12;
            HorizontalAlignment = HorizontalAlignment.Stretch;
            VerticalAlignment = VerticalAlignment.Stretch;
        }

        protected override void OnPaint(WindowlessPaintEventArgs e)
        {
            base.OnPaint(e);

            int arrowHeight = (ClientHeight - 1) / 2;

            e.Graphics.FillPolygon(new SolidBrush(SystemColors.ControlDarkDark), new Point[] { new Point((int)Math.Ceiling((double)e.Origin.X + (double)ClientWidth / 2.0), e.Origin.Y), new Point(e.Origin.X, e.Origin.Y + arrowHeight), new Point(e.Origin.X + ClientWidth, e.Origin.Y + arrowHeight) });
            e.Graphics.FillPolygon(new SolidBrush(SystemColors.ControlDarkDark), new Point[] { new Point(e.Origin.X, e.Origin.Y + arrowHeight + 2), new Point(e.Origin.X + ClientWidth, e.Origin.Y + arrowHeight + 2), new Point((int)Math.Floor((double)e.Origin.X + (double)ClientWidth / 2.0), e.Origin.Y + arrowHeight + 2 + arrowHeight) });
        }
    }

    public class NumericControl : TextControl
    {
        int myOldValue = 0;
        public NumericControl()
            : this(0)
        {
        }

        public NumericControl(int value)
        {
            myOldValue = value;
            Text = value.ToString();
        }

        public int Value
        {
            get
            {
                return Int32.Parse(Text);
            }
            set
            {
                Text = value.ToString();
            }
        }
    }

    public class NumericUpDownControl : NumericControl
    {
        protected override void OnWindowlessKeyDown(Control sender, KeyEventArgs e)
        {
            if (!e.Handled && IsEditing)
            {
                if (e.KeyCode == Keys.Up)
                {
                    Value++;
                    e.Handled = true;
                }
                else if (e.KeyCode == Keys.Down)
                {
                    Value--; 
                    e.Handled = true;
                }
            }

            base.OnWindowlessKeyDown(sender, e);
        }

        protected override void OnSetupDockPanel(DockPanel dockPanel)
        {
            base.OnSetupDockPanel(dockPanel);
            WindowlessUpDownArrows arrows = new WindowlessUpDownArrows();
            arrows.Layout = new DockLayout(new LayoutMeasurement(0, LayoutUnit.Star), DockStyle.Left);
            dockPanel.Controls.Add(arrows);
        }
    }
}