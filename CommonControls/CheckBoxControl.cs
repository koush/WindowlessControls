using System;
using System.Drawing;
using System.Collections.Generic;
using System.Text;

namespace WindowlessControls.CommonControls
{
    public abstract class ImageCheckBoxPresenter : OverlayPanel, ICheckBoxPresenter
    {
        WindowlessImage myBackground = new WindowlessImage();
        WindowlessImage myCheck = new WindowlessImage();

        public abstract PlatformBitmap CheckBitmap
        {
            get;
        }

        public abstract PlatformBitmap UnfocusedBitmap
        {
            get;
        }

        public abstract PlatformBitmap FocusedBitmap
        {
            get;
        }

        public ImageCheckBoxPresenter()
        {
            myCheck.PaintSelf = false;
            myCheck.Bitmap = CheckBitmap;
            myBackground.Bitmap = UnfocusedBitmap;
            myBackground.HorizontalAlignment = myCheck.HorizontalAlignment = HorizontalAlignment.Center;
            myBackground.VerticalAlignment = myCheck.VerticalAlignment = VerticalAlignment.Center;
            Controls.Add(myBackground);
            Controls.Add(myCheck);
        }

        #region ICheckBoxPresenter Members
        public bool Checked
        {
            get
            {
                return myCheck.PaintSelf;
            }
            set
            {
                if (value != myCheck.PaintSelf)
                {
                    myCheck.PaintSelf = value;
                    if (CheckedChanged != null)
                        CheckedChanged(this);
                }
            }
        }

        public string Text
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public event CheckedChangedEventHandler CheckedChanged;
        #endregion

        #region IInteractiveStyleControl Members
        public void ApplyFocusedStyle()
        {
            myBackground.Bitmap = FocusedBitmap;
        }

        public void ApplyClickedStyle()
        {
        }

        #endregion
    }

    public class FourStateImageCheckBoxPresenter : WindowlessImage, ICheckBoxPresenter
    {
        public FourStateImageCheckBoxPresenter(PlatformBitmap uncheckedBitmap, PlatformBitmap checkedBitmap, PlatformBitmap selectedUncheckedBitmap, PlatformBitmap selectedCheckedBitmap)
        {
            myCheckedBitmap = checkedBitmap;
            myUncheckedBitmap = uncheckedBitmap;
            mySelectedCheckedBitmap = selectedCheckedBitmap;
            mySelectedUncheckedBitmap = selectedUncheckedBitmap;

            Bitmap = myUncheckedBitmap;

            myIsSelected = new DependencyPropertyStorage<bool>(this, false, new DependencyPropertyChangedEvent((o,e)=>
                {
                    RefreshImage();
                }
            ));
        }

        void RefreshImage()
        {
            if (myIsSelected)
            {
                Bitmap = Checked ? mySelectedCheckedBitmap : mySelectedUncheckedBitmap;
            }
            else
            {
                Bitmap = Checked ? myCheckedBitmap : myUncheckedBitmap;
            }
        }

        PlatformBitmap mySelectedCheckedBitmap;
        PlatformBitmap myCheckedBitmap;
        PlatformBitmap mySelectedUncheckedBitmap;
        PlatformBitmap myUncheckedBitmap;

        DependencyPropertyStorage<bool> myIsSelected;
        #region ICheckBoxPresenter Members

        bool myChecked;
        public bool Checked
        {
            get
            {
                return myChecked;
            }
            set
            {
                if (myChecked != value)
                {
                    myChecked = value;
                    RefreshImage();
                    if (CheckedChanged != null)
                        CheckedChanged(this);
                }
            }
        }

        public string Text
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public event CheckedChangedEventHandler CheckedChanged;

        #endregion

        #region IInteractiveStyleControl Members

        public void ApplyFocusedStyle()
        {
            myIsSelected.Value = true;
        }

        public void ApplyClickedStyle()
        {
        }

        #endregion
    }

    public class TogglingTextCheckBoxPresenter : WindowlessLabel, ICheckBoxPresenter
    {
        public TogglingTextCheckBoxPresenter(string uncheckedText, string checkedText)
        {
            myCheckedText = checkedText;
            myUncheckedText = uncheckedText;
            RefreshText();
        }

        void RefreshText()
        {
            Text = Checked ? CheckedText : UncheckedText;
        }

        string myCheckedText;
        public string CheckedText
        {
            get
            {
                return myCheckedText;
            }
            set
            {
                myCheckedText = value;
                RefreshText();
            }
        }

        string myUncheckedText;
        public string UncheckedText
        {
            get
            {
                return myUncheckedText;
            }
            set
            {
                myUncheckedText = value;
                RefreshText();
            }
        }
        #region ICheckBoxPresenter Members

        public bool Checked
        {
            get
            {
                return Text == CheckedText;
            }
            set
            {
                if (Checked != value)
                {
                    Text = value ? CheckedText : UncheckedText;
                    if (CheckedChanged != null)
                        CheckedChanged(this);
                }
            }
        }

        public event CheckedChangedEventHandler CheckedChanged;

        #endregion

        #region IInteractiveStyleControl Members

        public void ApplyFocusedStyle()
        {
        }

        public void ApplyClickedStyle()
        {
        }

        #endregion
    }

    public class TextCheckBoxPresenter<T> : WrapPanel, ICheckBoxPresenter where T : ICheckBoxPresenter, new()
    {
        T myCheckBox = new T();

        WindowlessLabel myLabel = new WindowlessLabel();

        public WindowlessLabel Label
        {
            get { return myLabel; }
        }

        public TextCheckBoxPresenter()
        {
            myCheckBox.CheckedChanged += new CheckedChangedEventHandler(myCheckBox_CheckedChanged);
            StackPanel stack = new StackPanel();
            stack.Controls.Add(myCheckBox);
            myLabel.VerticalAlignment = stack.VerticalAlignment = VerticalAlignment.Center;
            myLabel.Margin = new Thickness(2, 0, 0, 0);
            Controls.Add(stack);
            Controls.Add(myLabel);
        }

        void myCheckBox_CheckedChanged(ICheckBoxPresenter sender)
        {
            if (CheckedChanged != null)
                CheckedChanged(this);
        }

        #region IInteractiveStyleControl Members
        public void ApplyFocusedStyle()
        {
            CheckBox.ApplyFocusedStyle();
        }

        public void ApplyClickedStyle()
        {
        }
        #endregion

        #region ICheckBoxPresenter Members
        public bool Checked
        {
            get { return myCheckBox.Checked; }
            set { myCheckBox.Checked = value; }
        }

        public string Text
        {
            get
            {
                return myLabel.Text;
            }
            set
            {
                myLabel.Text = value;
            }
        }
        public event CheckedChangedEventHandler CheckedChanged;
        #endregion
        
        public T CheckBox
        {
            get { return myCheckBox; }
        }
    }

    public class CheckBoxPresenter : WindowlessRectangle, IInteractiveStyleControl, ICheckBoxPresenter
    {
        public CheckBoxPresenter()
        {
            MaxWidth = 20;
            MaxHeight = 20;
            HorizontalAlignment = HorizontalAlignment.Stretch;
            VerticalAlignment = VerticalAlignment.Stretch;
            Filled = false;
            myChecked = new DependencyPropertyStorage<bool>(this, false, new DependencyPropertyChangedEvent(OnCheckedChanged));
            RectangleWidth = 3;
            Color = SystemColors.ControlDarkDark;
        }

        void OnCheckedChanged(object sender, DependencyPropertyEventArgs e)
        {
            WindowlessControlHost.WindowlessInvalidate(this);
            if (CheckedChanged != null)
                CheckedChanged(this);
        }


        protected override void OnPaint(WindowlessPaintEventArgs e)
        {
            if (Checked)
            {
                e.Graphics.DrawLine(new Pen(SystemColors.ControlDarkDark, 2), e.Origin.X, e.Origin.Y, e.Origin.X + ClientWidth, e.Origin.Y + ClientHeight);
                e.Graphics.DrawLine(new Pen(SystemColors.ControlDarkDark, 2), e.Origin.X + ClientWidth, e.Origin.Y, e.Origin.X, e.Origin.Y + ClientHeight);
            }
            base.OnPaint(e);
        }

        #region IInteractiveStyleControl Members
        public void ApplyFocusedStyle()
        {
            Color = SystemColors.ActiveBorder;
        }

        public void ApplyClickedStyle()
        {
        }

        #endregion

        #region ICheckBoxPresenter Members
        DependencyPropertyStorage<bool> myChecked;
        public bool Checked
        {
            get
            {
                return myChecked.Value;
            }
            set
            {
                myChecked.Value = value;
            }
        }

        public string Text
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public event CheckedChangedEventHandler CheckedChanged;
        #endregion
    }

    public interface ICheckBoxPresenter : IInteractiveStyleControl
    {
        bool Checked
        {
            get;
            set;
        }
        string Text
        {
            get;
            set;
        }
        event CheckedChangedEventHandler CheckedChanged;
    }

    public class CheckBoxControlBase : ButtonBase
    {
        public new ICheckBoxPresenter Control
        {
            get
            {
                return base.Control as ICheckBoxPresenter;
            }
            set
            {
                base.Control = value;
            }
        }

        public bool Checked
        {
            get { return Control.Checked; }
            set { Control.Checked = value; }
        }

        protected override void OnWindowlessClick(WindowlessControlHost sender, WindowlessMouseEventArgs e)
        {
            base.OnWindowlessClick(sender, e);
            if (!e.Handled)
                Checked = !Checked;
        }

        public event CheckedChangedEventHandler CheckedChanged
        {
            add
            {
                Control.CheckedChanged += value;
            }
            remove
            {
                Control.CheckedChanged -= value;
            }
        }

        public override string Text
        {
            get
            {
                return Control.Text;
            }
            set
            {
                base.Text = Control.Text = value;
            }
        }
    }

    public class CheckBoxControlBase<T> : CheckBoxControlBase where T : class, ICheckBoxPresenter, new()
    {
        public CheckBoxControlBase()
        {
            base.Control = new T();
        }

        public new T Control
        {
            get
            {
                return base.Control as T;
            }
        }
    }

    public class CheckBoxControl : CheckBoxControlBase<TextCheckBoxPresenter<CheckBoxPresenter>>
    {
    }
    public delegate void CheckedChangedEventHandler(ICheckBoxPresenter sender);
}
