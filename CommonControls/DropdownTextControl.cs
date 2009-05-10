using System;
using WindowlessControls;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace WindowlessControls.CommonControls
{
    public class DropdownTextControl : TextControl
    {
        DropdownControl myDropdown;
        ItemsControl myItemsControl = new ItemsControl();

        public ItemsControl ItemsControl
        {
            get { return myItemsControl; }
        }

        class DropdownLabel : WindowlessLabelPresenter
        {
            public DropdownLabel()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch;
                Margin = new Thickness(1, 0, 1, 0);
            }
        }

        public DropdownTextControl()
        {
            myItemsControl.ContentPresenter = typeof(DropdownLabel);
            myItemsControl.Control = new StackPanel();
            myItemsControl.DeselectOnFocusLost = true;
            myDropdown = new DropdownControl(this, myItemsControl);
            myDropdown.WindowlessClick += new WindowlessMouseEvent(myDropdown_WindowlessClick);
        }

        void myDropdown_WindowlessClick(WindowlessControlHost sender, WindowlessMouseEventArgs e)
        {
            Content = myItemsControl.CurrentSelection;
            IsEditing = false;
        }

        protected override void OnEditingChanged()
        {
            base.OnEditingChanged();

            if (IsEditing)
            {
                myDropdown.PositionControl();
                myDropdown.Visible = true;
            }
            else
            {
                if (FindFocusedControl(myDropdown) != null)
                    Focus();
                myDropdown.Visible = false;
            }
        }

        protected override bool HasEditFocus
        {
            get
            {
                return base.HasEditFocus || FindFocusedControl(myDropdown) != null;
            }
        }

        protected override void OnWindowlessNavigate(Control sender, WindowlessNavigateEventArgs e)
        {
            if (IsEditing && (e.Key == Keys.Down || e.Key == Keys.Up) && !e.Handled)
            {
                e.Handled = true;
                e.Destination = myDropdown;
            }

            base.OnWindowlessNavigate(sender, e);
        }

        protected override void OnSetupDockPanel(DockPanel dockPanel)
        {
            WindowlessDropDownArrow arrow = new WindowlessDropDownArrow();
            arrow.Layout = new DockLayout(new LayoutMeasurement(0, LayoutUnit.Star), DockStyle.Right);
            dockPanel.Controls.Add(arrow);
            base.OnSetupDockPanel(dockPanel);
        }
    }
}