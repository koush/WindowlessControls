using System;
using System.Collections.Generic;
using System.Text;
using WindowlessControls;
using System.Drawing;

namespace WindowlessControls.CommonControls
{
    public class HyperlinkButton : ButtonBase
    {
        public HyperlinkButton()
        {
            base.Control = new HyperlinkPresenter();
            Orientation = WindowlessControls.Orientation.Vertical;
            BackColor = Color.Transparent;
        }

        public HyperlinkButton(string text)
            : this()
        {
            Text = text;
        }

        public new HyperlinkPresenter Control
        {
            get
            {
                return base.Control as HyperlinkPresenter;
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
                Control.Text = value;
            }
        }
    }

    public class HyperlinkPresenter : WindowlessLabel, IInteractiveStyleControl
    {
        public HyperlinkPresenter()
        {
            ForeColor = Color.Blue;
            Font = new Font(DefaultFont.Name, DefaultFont.Size, FontStyle.Underline);
        }

        #region IInteractiveStyleControl Members
        public void ApplyFocusedStyle()
        {
            ForeColor = Color.Blue;
            Font = new Font(Font.Name, Font.Size, Font.Style | FontStyle.Bold);
        }

        public void ApplyClickedStyle()
        {
            ForeColor = Color.Red;
            Font = new Font(Font.Name, Font.Size, Font.Style | FontStyle.Bold);
        }

        #endregion
    }
}
