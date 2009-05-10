using System;
using WindowlessControls;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace WindowlessControls.CommonControls
{
    public class TextControl : EditModeControl, IContentPresenter
    {
        WindowlessLabelPresenter myLabel = new WindowlessLabelPresenter();
        TextBox myTextBox = new TextBox();
        WindowlessControlHost myTextBoxHost = new WindowlessControlHost();

        public override Font Font
        {
            get
            {
                return myLabel.Font;
            }
            set
            {
                myLabel.Font = myTextBox.Font = base.Font = value;
                myTextBoxHost.HostControl(myTextBox, true, false);
            }
        }

        protected TextBox TextBox
        {
            get { return myTextBox; }
            set { myTextBox = value; }
        }

        public TextControl()
        {
            SuspendRemeasure();

            myTextBox.TextChanged += new EventHandler(myTextBox_TextChanged);
            myTextBoxHost.BackColor = Color.Transparent;
            myTextBox.LostFocus += new EventHandler(myTextBox_LostFocus);
            myTextBox.Font = myLabel.Font;
            myTextBox.BorderStyle = BorderStyle.None;
            myTextBox.Visible = false;

            OverlayPanel overlay = new OverlayPanel();
            WindowlessRectangle rectangle = new WindowlessRectangle();
            rectangle.Color = SystemColors.ControlDarkDark;
            rectangle.Filled = false;

            DockPanel dockPanel = new DockPanel();
            dockPanel.Margin = new Thickness(2, 2, 2, 2);
            OnSetupDockPanel(dockPanel);

            overlay.Controls.Add(rectangle);
            overlay.Controls.Add(dockPanel);
            overlay.FitWidthControl = overlay.FitHeightControl = dockPanel;

            Control = overlay;

            ResumeRemeasure();
            Remeasure();
        }

        void myTextBox_TextChanged(object sender, EventArgs e)
        {
            // this triggers a remeasure
            if (myLabel.Text != myTextBox.Text)
                myLabel.Text = Text;
        }

        protected override void OnWindowlessNavigate(Control sender, WindowlessNavigateEventArgs e)
        {
            if (!e.Handled && IsEditing)
            {
                e.Handled = true;
                e.Destination = null;
            }
            base.OnWindowlessNavigate(sender, e);
        }

        protected override void OnApplyFocusedStyle()
        {
            myLabel.ApplyFocusedStyle();
            base.OnApplyFocusedStyle();
        }

        protected override void OnApplyClickedStyle()
        {
            //base.OnApplyClickedStyle();
        }

        protected virtual void OnSetupDockPanel(DockPanel dockPanel)
        {
            OverlayPanel overlay = new OverlayPanel();
            overlay.Layout = new DockLayout(new LayoutMeasurement(0, LayoutUnit.Star), DockStyle.Left);

            myLabel.VerticalAlignment = VerticalAlignment.Center;
            myLabel.Margin = new Thickness(0, 0, 6, 0);
            overlay.Controls.Add(myLabel);

            myTextBoxHost.HostControl(myTextBox, true, false);
            overlay.Controls.Add(myTextBoxHost);

            overlay.FitHeightControl = overlay.FitWidthControl = myLabel;

            dockPanel.Controls.Add(overlay);
        }

        void myTextBox_LostFocus(object sender, EventArgs e)
        {
            if (IsEditing && !HasEditFocus)
                IsEditing = false;
        }

        protected override void OnWindowlessKeyPress(Control sender, KeyPressEventArgs e)
        {
            if (!e.Handled && e.KeyChar == '\r')
            {
                IsEditing = !IsEditing;
                e.Handled = true;
            }

            base.OnWindowlessKeyPress(sender, e);
        }

        protected override void OnEditingChanged()
        {
            base.OnEditingChanged();

            if (IsEditing)
            {
                myTextBox.Visible = true;
                myTextBox.Focus();
                Focusable = false;
            }
            else
            {
                Content = myTextBox.Text;
                Focusable = true;
                Focus();
                myTextBox.Visible = false;
            }
        }

        public override string Text
        {
            get
            {
                return myTextBox.Text;
            }
            set
            {
                myLabel.Text = myTextBox.Text = base.Text = value;
            }
        }

        #region IContentPresenter Members

        public object Content
        {
            get
            {
                // always favor text
                if (myLabel.Content.ToString() != myTextBox.Text)
                    return myTextBox.Text;
                return myLabel.Content;
            }
            set
            {
                myLabel.Content = value;
                myTextBox.Text = base.Text = value.ToString();
            }
        }

        #endregion
    }
}