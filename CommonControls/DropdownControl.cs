using System;
using WindowlessControls;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace WindowlessControls.CommonControls
{
    public class WindowlessDropDownArrow : WindowlessPaintControl
    {
        public WindowlessDropDownArrow()
        {
            Margin = new Thickness(2, 2, 2, 2);
            MaxHeight = 8;
            MaxWidth = 11;
            HorizontalAlignment = HorizontalAlignment.Stretch;
            VerticalAlignment = VerticalAlignment.Stretch;
        }

        protected override void OnPaint(WindowlessPaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.FillPolygon(new SolidBrush(SystemColors.ControlDarkDark), new Point[] { new Point(e.Origin.X, e.Origin.Y), new Point(e.Origin.X + ClientWidth, e.Origin.Y), new Point((int)Math.Round((double)e.Origin.X + (double)ClientWidth / 2.0), e.Origin.Y + ClientHeight) });
        }
    }
 
    public class DropdownControl : WindowlessControlHost
    {
        WindowlessControlHost myDropdownSource;
        public DropdownControl(WindowlessControlHost dropdownSource, IWindowlessControl dropdownContent)
        {
            AutoScroll = true;
            Orientation = Orientation.Vertical;
            myDropdownSource = dropdownSource;

            OverlayPanel overlay = new OverlayPanel();
            WindowlessRectangle rectangle = new WindowlessRectangle();
            rectangle.Color = SystemColors.ControlDarkDark;
            rectangle.Filled = false;
            overlay.Controls.Add(rectangle);

            OverlayPanel inner = new OverlayPanel();
            inner.Margin = new Thickness(1, 1, 1, 1);
            inner.Controls.Add(dropdownContent);
            overlay.Controls.Add(inner);
            overlay.FitHeightControl = overlay.FitWidthControl = inner;
            Control = overlay;
        }

        protected override void OnWindowlessNavigate(Control sender, WindowlessNavigateEventArgs e)
        {
            if ((e.Key == Keys.Down || e.Key == Keys.Up) && e.Destination == null && !e.Handled)
            {
                e.Handled = true;
                e.Destination = myDropdownSource;
            }

            base.OnWindowlessNavigate(sender, e);
        }

        public void PositionControl()
        {
            Control topLevelControl = WindowlessControlHost.GetHost(myDropdownSource).TopLevelControl;
            if (Parent != topLevelControl)
                Parent = topLevelControl;

            Point topLeft = WindowlessPointToForm(myDropdownSource, Point.Empty);
            Point bottomLeft = WindowlessPointToForm(myDropdownSource, new Point(0, myDropdownSource.Height));

            int bottomSpaceAvailable = topLevelControl.Height - bottomLeft.Y;
            int maxHeight = Math.Max(topLeft.Y, bottomSpaceAvailable);
            //int maxWidth = TopLevelControl.Width - topLeft.X;
            int maxWidth = myDropdownSource.Width;


            Width = maxWidth;
            Height = maxHeight;
            Remeasure();

            Size newClientSize = ClientSize;
            if (Control.Height < ClientSize.Height)
                newClientSize.Height = Control.Height;

            if (Control.Width < ClientSize.Width)
                newClientSize.Width = Control.Width;

            if (newClientSize != ClientSize)
                ClientSize = newClientSize;

            if (Height < bottomSpaceAvailable || bottomSpaceAvailable >= topLeft.Y)
                Location = bottomLeft;
            else
                Location = topLeft;

            BringToFront();
        }

        //IInteractiveContentPresenter myValueControl;
        //ItemsControl myDropdown = new ItemsControl();

        //public class DropdownLabel : WindowlessLabelPresenter
        //{
        //    public DropdownLabel()
        //    {
        //        HorizontalAlignment = HorizontalAlignment.Stretch;
        //        Margin = new Thickness(1, 0, 1, 0);
        //    }
        //}

        //public Type ContentPresenter
        //{
        //    get { return myDropdown.ContentPresenter; }
        //    set { myDropdown.ContentPresenter = value; }
        //}

        //public ItemCollection Items
        //{
        //    get
        //    {
        //        return myDropdown.Items;
        //    }
        //}

        //public IInteractiveContentPresenter ContentControl
        //{
        //    get
        //    {
        //        return myValueControl;
        //    }
        //    set
        //    {
        //        DockPanel dockPanel = myValueControl.Parent as DockPanel;
        //        dockPanel.SuspendRemeasure();
        //        dockPanel.Controls.Remove(myValueControl);
        //        myValueControl = value;
        //        myValueControl.Layout = new DockLayout(new LayoutMeasurement(0, LayoutUnit.Star), DockStyle.Left);
        //        dockPanel.Controls.Add(myValueControl);
        //        dockPanel.ResumeRemeasure();
        //        dockPanel.Remeasure();
        //    }
        //}

        //protected override void OnEditingChanged()
        //{
        //    base.OnEditingChanged();
        //    if (IsEditing)
        //    {
        //        Point bottomLeft = WindowlessPointToForm(this, new Point(0, Height));
        //        Point topLeft = WindowlessPointToForm(this, new Point(0, 0));

        //        int bottomSpaceAvailable = TopLevelControl.Height - bottomLeft.Y;

        //        int maxHeight = Math.Max(topLeft.Y, bottomSpaceAvailable);
        //        //int maxWidth = TopLevelControl.Width - topLeft.X;
        //        int maxWidth = Width;

        //        myScrollHost.Parent = TopLevelControl;
        //        myScrollHost.Width = maxWidth;
        //        myScrollHost.Height = maxHeight;
        //        myScrollHost.Remeasure();

        //        if (myScrollHost.Control.Height < myScrollHost.Height)
        //            myScrollHost.Height = myScrollHost.Control.Height;

        //        //// now resize the width to fit the dropdown
        //        //int dif = myScrollHost.Width - myScrollHost.ClientSize.Width;
        //        //int newClientWidth = Math.Max(Width - dif, myMarginControl.Width);
        //        //Size newClient = new Size(newClientWidth, myScrollHost.ClientSize.Height);
        //        //myScrollHost.ClientSize = newClient;

        //        if (myScrollHost.Height < bottomSpaceAvailable || bottomSpaceAvailable >= topLeft.Y)
        //            myScrollHost.Location = bottomLeft;
        //        else
        //            myScrollHost.Location = topLeft;

        //        myScrollHost.Visible = true;
        //        myScrollHost.BringToFront();
        //    }
        //    else
        //    {
        //        if (FindFocusedControl(myScrollHost) != null)
        //            Focus();
        //        myScrollHost.Visible = false;
        //    }
        //}

        //protected override void OnWindowlessNavigate(Control sender, WindowlessNavigateEventArgs e)
        //{
        //    if ((e.Key == Keys.Down || e.Key == Keys.Up) && IsEditing)
        //        e.Destination = myScrollHost;

        //    base.OnWindowlessNavigate(sender, e);
        //}

        //public DropdownControl(Type contentPresenter)
        //    : this(contentPresenter, Activator.CreateInstance(contentPresenter) as IInteractiveContentPresenter)
        //{
        //}

        //StackPanel myMarginControl;
        //VerticalStackPanelHost myScrollHost = new VerticalStackPanelHost();
        //public DropdownControl(Type contentPresenter, IInteractiveContentPresenter valueControl)
        //{
        //    myScrollHost.WindowlessLostFocus += new WindowlessFocusEvent(myScrollHost_WindowlessLostFocus);
        //    myDropdown.Click += new EventHandler(myDropdown_Click);

        //    OverlayPanel overlay = new OverlayPanel();
        //    WindowlessRectangle rectangle = new WindowlessRectangle();
        //    rectangle.Color = SystemColors.ControlDarkDark;
        //    rectangle.Filled = false;
        //    overlay.Controls.Add(rectangle);

        //    myMarginControl = new StackPanel();
        //    myMarginControl.Margin = new Thickness(1, 1, 1, 1);
        //    myMarginControl.Controls.Add(myDropdown);
        //    overlay.Controls.Add(myMarginControl);
        //    overlay.FitHeightControl = myMarginControl;

        //    StackPanel itemsPanel = new StackPanel();
        //    myDropdown.Control = itemsPanel;
        //    myDropdown.ContentPresenter = contentPresenter;
        //    myDropdown.DeselectOnFocusLost = true;

        //    myValueControl = valueControl;

        //    myScrollHost.Control.HorizontalAlignment = HorizontalAlignment.Stretch;
        //    myScrollHost.AutoScroll = true;
        //    myScrollHost.Control.Controls.Add(overlay);

        //    OverlayPanel controlOverlay = new OverlayPanel();
        //    DockPanel dockPanel = new DockPanel();
        //    controlOverlay.FitHeightControl = controlOverlay.FitWidthControl = dockPanel;
        //    WindowlessRectangle controlRectangle = new WindowlessRectangle();
        //    controlRectangle.Color = SystemColors.ControlDarkDark;
        //    controlRectangle.Filled = false;
        //    controlOverlay.Controls.Add(controlRectangle);
        //    controlOverlay.Controls.Add(dockPanel);
        //    dockPanel.Margin = new Thickness(1, 1, 1, 1);
        //    Control = controlOverlay;

        //    StackPanel arrowMargin = new StackPanel();
        //    arrowMargin.Layout = new DockLayout(myArrowButtonMeasurement, DockStyle.Right);
        //    arrowMargin.Margin = new Thickness(2, 2, 2, 2);
        //    arrowMargin.Controls.Add(myArrowButton);
        //    myArrowButton.HorizontalAlignment = HorizontalAlignment.Stretch;
        //    myArrowButton.MaxHeight = 4;
        //    myArrowButton.MaxWidth = 8;
        //    myArrowButton.VerticalAlignment = VerticalAlignment.Stretch;
        //    myValueControl.Layout = new DockLayout(new LayoutMeasurement(0, LayoutUnit.Star), DockStyle.Left);

        //    dockPanel.Controls.Add(arrowMargin);
        //    dockPanel.Controls.Add(myValueControl);
        //}

        //public DropdownControl()
        //    : this(typeof(DropdownLabel), Activator.CreateInstance(typeof(DropdownLabel)) as IInteractiveContentPresenter)
        //{
        //}

        //void myDropdown_Click(object sender, EventArgs e)
        //{
        //    Content = myDropdown.CurrentSelection;
        //    IsEditing = false;
        //}

        //protected override bool HasEditFocus
        //{
        //    get
        //    {
        //        return base.HasEditFocus || FindFocusedControl(myScrollHost) != null;
        //    }
        //}

        //void myScrollHost_WindowlessLostFocus(Control sender)
        //{
        //    if (!HasEditFocus && IsEditing)
        //    {
        //        IsEditing = false;
        //    }
        //}

        //class ArrowButton : WindowlessPaintControl
        //{
        //    protected override void OnPaint(WindowlessPaintEventArgs e)
        //    {
        //        base.OnPaint(e);
        //        e.Graphics.FillPolygon(new SolidBrush(SystemColors.ControlDarkDark), new Point[] { new Point(e.Origin.X, e.Origin.Y), new Point(e.Origin.X + ClientWidth, e.Origin.Y), new Point((int)Math.Round((double)e.Origin.X + (double)ClientWidth / 2.0), e.Origin.Y + ClientHeight) });
        //    }
        //}

        //public object Content
        //{
        //    get
        //    {
        //        return myValueControl.Content;
        //    }
        //    set
        //    {
        //        myValueControl.Content = value;
        //    }
        //}

        //PropertyListener myFocusListener;
        //protected override void OnWindowlessGotFocus(WindowlessControlHost sender, EventArgs e)
        //{
        //    base.OnWindowlessGotFocus(sender, e);

        //    if (myFocusListener == null)
        //    {
        //        myFocusListener = new PropertyListener();
        //        myFocusListener.StartListen();
        //        if (myValueControl.FocusedStyle != null)
        //            myValueControl.FocusedStyle.Apply(myValueControl);
        //        myFocusListener.StopListen();
        //    }
        //}

        //protected override void OnWindowlessLostFocus(WindowlessControlHost sender, EventArgs e)
        //{
        //    base.OnWindowlessLostFocus(sender, e);

        //    if (FindFocusedControl(this) == null && myFocusListener != null)
        //    {
        //        myFocusListener.Undo();
        //        myFocusListener = null;
        //    }
        //}

        //ArrowButton myArrowButton = new ArrowButton();
        //public IWindowlessControl ArrowControl
        //{
        //    get
        //    {
        //        return myArrowButton;
        //    }
        //}
        //LayoutMeasurement myArrowButtonMeasurement = new LayoutMeasurement(0, LayoutUnit.Star);

        //public LayoutMeasurement ArrowButtonMeasurement
        //{
        //    get { return myArrowButtonMeasurement; }
        //    set { myArrowButtonMeasurement = value; }
        //}
    }
}