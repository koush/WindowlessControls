//////////////////////////////////////////////////////////////
// Koushik Dutta - 9/1/2007
//////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Collections;
using WindowlessControls.CommonControls;

namespace WindowlessControls
{
    public class WindowlessLabel : WindowlessPaintControl, IContentPresenter
    {
        struct LineBreak
        {
            string text;
            int myIndex;

            public int Index
            {
                get { return myIndex; }
                set { myIndex = value; }
            }

            public string Text
            {
                get { return text; }
                set { text = value; }
            }
            SizeF mySize;

            public SizeF Size
            {
                get { return mySize; }
                set { mySize = value; }
            }

            public float Width
            {
                get
                {
                    return mySize.Width;
                }
                set
                {
                    mySize.Width = value;
                }
            }
            public float Height
            {
                get
                {
                    return mySize.Height;
                }
                set
                {
                    mySize.Height = value;
                }
            }
        }

        static Bitmap myMeasureBitmap = new System.Drawing.Bitmap(1, 1, System.Drawing.Imaging.PixelFormat.Format16bppRgb555);
        static Graphics myGraphics;
        static Font myDefaultFont = new Font(FontFamily.GenericSerif, 9.3f, FontStyle.Regular);
        static Font myDefaultBoldFont = new Font(FontFamily.GenericSerif, 9.3f, FontStyle.Bold);
        DependencyPropertyStorage<object> myContent;
        DependencyPropertyStorage<Font> myFont;
        DependencyPropertyStorage<Color> myForeColor;
        DependencyPropertyStorage<HorizontalAlignment> myHorizontalTextAlign;
        DependencyPropertyStorage<VerticalAlignment> myVerticalTextAlign;
        DependencyPropertyStorage<Color> myBackColor;
        DependencyPropertyStorage<bool> myAutoEllipsis;
        DependencyPropertyStorage<bool> myMultiline;
        List<LineBreak> myLineBreaks = new List<LineBreak>();
        float myTotalHeight = 0;
        SolidBrush myForeBrush = new SolidBrush(Color.Black);
        SolidBrush myBackBrush = new SolidBrush(Color.Transparent);

        static WindowlessLabel()
        {
            myGraphics = Graphics.FromImage(myMeasureBitmap);
        }

        public WindowlessLabel()
        {
            myBackColor = new DependencyPropertyStorage<Color>(this, Color.Transparent, new DependencyPropertyChangedEvent(BackColorChanged));
            myForeColor = new DependencyPropertyStorage<Color>(this, Color.Black, new DependencyPropertyChangedEvent(ForeColorChanged));
            myFont = new DependencyPropertyStorage<Font>(this, myDefaultFont, new DependencyPropertyChangedEvent(FontChanged));
            myContent = new DependencyPropertyStorage<object>(this, string.Empty, new DependencyPropertyChangedEvent(TextChanged));
            myHorizontalTextAlign = new DependencyPropertyStorage<HorizontalAlignment>(this, HorizontalAlignment.Left, new DependencyPropertyChangedEvent(HorizontalTextAlignmentChanged));
            myVerticalTextAlign = new DependencyPropertyStorage<VerticalAlignment>(this, VerticalAlignment.Top, new DependencyPropertyChangedEvent(VerticalTextAlignmentChanged));
            myAutoEllipsis = new DependencyPropertyStorage<bool>(this, true, new DependencyPropertyChangedEvent(AutoEllipsisChanged));
            myMultiline = new DependencyPropertyStorage<bool>(this, true, new DependencyPropertyChangedEvent(MultilineChanged));
        }

        public WindowlessLabel(string text)
            : this()
        {
            Text = text;
        }

        public WindowlessLabel(string text, Font font)
            : this(text)
        {
            Font = font;
        }

        public WindowlessLabel(string text, Font font, Thickness margin)
            : this(text, font)
        {
            Margin = margin;
        }

        public WindowlessLabel(string text, Font font, Thickness margin, Color foreColor)
            : this(text, font, margin)
        {
            ForeColor = foreColor;
        }

        void AutoEllipsisChanged(object sender, DependencyPropertyEventArgs e)
        {
            WindowlessControlHost.WindowlessInvalidate(this);
        }

        void MultilineChanged(object sender, DependencyPropertyEventArgs e)
        {
            WindowlessControlHost.WindowlessInvalidate(this);
        }

        void HorizontalTextAlignmentChanged(object sender, DependencyPropertyEventArgs e)
        {
            WindowlessControlHost.WindowlessInvalidate(this);
        }

        void VerticalTextAlignmentChanged(object sender, DependencyPropertyEventArgs e)
        {
            WindowlessControlHost.WindowlessInvalidate(this);
        }


        void TextChanged(object sender, DependencyPropertyEventArgs e)
        {
            WindowlessControlHost.WindowlessInvalidate(this);
            Remeasure();
            WindowlessControlHost.WindowlessInvalidate(this);
        }

        protected virtual void FontChanged(object sender, DependencyPropertyEventArgs e)
        {
            WindowlessControlHost.WindowlessInvalidate(this);
            Remeasure();
            WindowlessControlHost.WindowlessInvalidate(this);
        }

        void BackColorChanged(object sender, DependencyPropertyEventArgs e)
        {
            myBackBrush.Color = BackColor;
            WindowlessControlHost.WindowlessInvalidate(this);
        }

        void ForeColorChanged(object sender, DependencyPropertyEventArgs e)
        {
            myForeBrush.Color = ForeColor;
            WindowlessControlHost.WindowlessInvalidate(this);
        }

        public static Font DefaultFont
        {
            get { return WindowlessLabel.myDefaultFont; }
            set { WindowlessLabel.myDefaultFont = value; }
        }

        public static Font DefaultBoldFont
        {
            get { return WindowlessLabel.myDefaultBoldFont; }
            set { WindowlessLabel.myDefaultBoldFont = value; }
        }

        public SmartColor BackColor
        {
            get { return myBackColor.Value; }
            set
            {
                myBackColor.Value = value;
            }
        }

        public bool AutoEllipsis
        {
            get { return myAutoEllipsis; }
            set
            {
                myAutoEllipsis.Value = value;
            }
        }

        public bool Multiline
        {
            get { return myMultiline; }
            set
            {
                myMultiline.Value = value;
            }
        }

        public HorizontalAlignment HorizontalTextAlignment
        {
            get { return myHorizontalTextAlign; }
            set
            {
                myHorizontalTextAlign.Value = value;
            }
        }

        public VerticalAlignment VerticalTextAlignment
        {
            get { return myVerticalTextAlign; }
            set
            {
                myVerticalTextAlign.Value = value;
            }
        }

        public string Text
        {
            get
            {
                if (myContent.Value == null)
                    return string.Empty;
                return myContent.Value.ToString();
            }
            set
            {
                myContent.Value = value;
            }
        }
        
        [System.Xml.Serialization.XmlIgnore]
        public Font Font
        {
            get { return myFont; }
            set
            {
                myFont.Value = value;
            }
        }

        public SmartColor ForeColor
        {
            get { return myForeColor.Value; }
            set
            {
                myForeColor.Value = value;
            }
        }

        static int FindWhitespace(string text, int startIndex)
        {
            int space = text.IndexOf(' ', startIndex);
            int linebreak = text.IndexOf('\n', startIndex);
            if (space == -1)
                return linebreak;
            else if (linebreak == -1)
                return space;
            else
                return Math.Min(space, linebreak);
        }

        LineBreak FitString(string text, int startIndex, int width)
        {
            LineBreak lineBreak = new LineBreak();
            int currentIndex = FindWhitespace(text, startIndex);
            if (currentIndex == -1)
                currentIndex = text.Length;
            int lastIndex = startIndex;
            SizeF dims = new SizeF(0, 0);

            lineBreak.Size = new SizeF(0, 0);
            while (currentIndex != -1 && (dims = myGraphics.MeasureString(text.Substring(startIndex, currentIndex - startIndex), Font)).Width <= width)
            {
                // record the width/height while succesfully fit
                lineBreak.Width = dims.Width;
                lineBreak.Height = dims.Height;

                // done
                if (currentIndex == text.Length)
                {
                    lastIndex = currentIndex;
                    currentIndex = -1;
                }
                else if (text[currentIndex] == '\n')
                {
                    lastIndex = currentIndex + 1;
                    currentIndex = -1;
                }
                else
                {
                    // get next word
                    lastIndex = currentIndex + 1;
                    currentIndex = FindWhitespace(text, lastIndex);
                    // end of string
                    if (currentIndex == -1)
                        currentIndex = text.Length;
                }
            }

            if (lastIndex == startIndex)
            {
                // the string was either too long or we are at the end of the string
                if (currentIndex == -1)
                {
                    throw new Exception("Somehow executing unreachable code while drawing text.");
                }

                currentIndex = lastIndex + 1;
                while ((dims = myGraphics.MeasureString(text.Substring(startIndex, currentIndex - startIndex), Font)).Width <= width)
                {
                    lineBreak.Size = new SizeF(dims.Width, dims.Height);
                    currentIndex++;
                }
                lineBreak.Size = new SizeF(dims.Width, dims.Height);
                lineBreak.Width = Math.Min(lineBreak.Width, width);
                lineBreak.Text = text.Substring(startIndex, currentIndex - startIndex);
                lineBreak.Index = currentIndex;
                return lineBreak;
            }
            else
            {
                // return the index we're painting to
                lineBreak.Text = text.Substring(startIndex, lastIndex - startIndex);
                lineBreak.Index = lastIndex;
                return lineBreak;
            }
        }

        public override bool MeasureUnpadded(Size bounds, bool boundsChange)
        {
            myLineBreaks.Clear();

            ClientWidth = 0;
            ClientHeight = 0;
            myTotalHeight = 0;

            if (string.IsNullOrEmpty(Text))
                return false;

            // limit to one line. trim anything after line feed.
            string processingText = Text;
            if (!myMultiline)
            {
                int index = processingText.IndexOf('\n');
                if (index != -1)
                    processingText = processingText.Substring(0, index);
            }
            LineBreak lineBreak = FitString(processingText, 0, bounds.Width);
            // simulate nonmultiline by capping the bounds to the height of the first string... hackish but easy!
            if (!myMultiline)
            {
                bounds.Height = (int)Math.Ceiling(lineBreak.Height);
            }

            while (lineBreak.Index != processingText.Length)
            {
                LineBreak nextBreak = FitString(processingText, lineBreak.Index, bounds.Width);
                // see if this line needs ellipsis
                if (lineBreak.Height + nextBreak.Height + myTotalHeight > bounds.Height && AutoEllipsis)
                {
                    string text = lineBreak.Text;
                    int ellipsisStart = text.Length - 3;
                    if (ellipsisStart < 0)
                        ellipsisStart = 0;
                    text = text.Substring(0, ellipsisStart) + "...";
                    lineBreak.Width = myGraphics.MeasureString(text, Font).Width;
                    lineBreak.Text = text;
                    break;
                }

                myLineBreaks.Add(lineBreak);
                ClientWidth = (int)Math.Ceiling(Math.Max(ClientWidth, lineBreak.Width));
                myTotalHeight += lineBreak.Height;
                lineBreak = nextBreak;
            }
            myLineBreaks.Add(lineBreak);
            myTotalHeight += lineBreak.Height;

            if (HorizontalAlignment == HorizontalAlignment.Stretch && bounds.Width != Int32.MaxValue)
                ClientWidth = bounds.Width;
            else
                ClientWidth = (int)Math.Ceiling(Math.Max(ClientWidth, lineBreak.Width));
            if (VerticalAlignment == VerticalAlignment.Stretch && bounds.Height != Int32.MaxValue)
                ClientHeight = bounds.Height;
            else
                ClientHeight = (int)Math.Ceiling(Math.Min(myTotalHeight, bounds.Height));
            return false;
        }

        protected override void OnPaint(WindowlessPaintEventArgs e)
        {
            //SetupDefaultClip(g);
            float topAdjust = 0;
            switch (VerticalTextAlignment)
            {
                case VerticalAlignment.Top:
                    topAdjust = 0;
                    break;
                case VerticalAlignment.Bottom:
                    topAdjust = ClientHeight - myTotalHeight;
                    break;
                case VerticalAlignment.Center:
                    topAdjust = (ClientHeight - myTotalHeight) / 2;
                    break;
            }

            if (BackColor != Color.Transparent)
            {
                e.Graphics.FillRectangle(myBackBrush, e.Origin.X, e.Origin.Y, ClientWidth, ClientHeight);
            }

            foreach (LineBreak lineBreak in myLineBreaks)
            {
                float leftAdjust = 0;
                switch (HorizontalTextAlignment)
                {
                    case HorizontalAlignment.Left:
                        leftAdjust = 0;
                        break;
                    case HorizontalAlignment.Right:
                        leftAdjust = ClientWidth - lineBreak.Width;
                        break;
                    case HorizontalAlignment.Center:
                        leftAdjust = (ClientWidth - lineBreak.Width) / 2.0f;
                        break;
                }

                if (e.ClipRectangle.IntersectsWith(new Rectangle(Margin.Left + (int)leftAdjust, Margin.Top + (int)topAdjust, (int)lineBreak.Width, (int)lineBreak.Height)))
                    e.Graphics.DrawString(lineBreak.Text, Font, myForeBrush, e.Origin.X + leftAdjust, e.Origin.Y + topAdjust);
                topAdjust += lineBreak.Height;
            }
        }

        #region IContentPresenter Members

        public object Content
        {
            get
            {
                return myContent;
            }
            set
            {
                myContent.Value = value;
            }
        }

        #endregion
    }
}
