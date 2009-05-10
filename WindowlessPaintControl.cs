using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Xml.Serialization;
using System.Xml.Schema;
using System.Xml;
using System.Runtime.InteropServices;

namespace WindowlessControls
{
    public enum HorizontalAlignment
    {
        Left,
        Right,
        Center,
        Stretch
    }

    public enum VerticalAlignment
    {
        Top,
        Bottom,
        Center,
        Stretch
    }

    public struct Thickness : IXmlSerializable
    {
        const int LOGPIXELSXIndex = 88;
        const int LOGPIXELSYIndex = 90;

        [DllImport("coredll")]
        extern static IntPtr GetDC(IntPtr hwnd);

        [DllImport("coredll")]
        extern static int GetDeviceCaps(IntPtr hdc, int index);

        public static readonly int DpiX;
        public static readonly int DpiY;

        static Thickness()
        {
            IntPtr hdc = GetDC(IntPtr.Zero);
            DpiX = GetDeviceCaps(hdc, LOGPIXELSXIndex);
            DpiY = GetDeviceCaps(hdc, LOGPIXELSYIndex);
        }

        public Thickness(int left, int top, int right, int bottom)
        {
            Left = left;
            Top = top;
            Bottom = bottom;
            Right = right;
        }

        public static Thickness From(float leftInches, float topInches, float rightInches, float bottomInches)
        {
            Thickness ret = new Thickness();
            ret.Left = (int)Math.Round(leftInches * DpiX);
            ret.Right = (int)Math.Round(rightInches * DpiX);
            ret.Top = (int)Math.Round(topInches * DpiY);
            ret.Bottom = (int)Math.Round(bottomInches * DpiY);
            return ret;
        }

        public int Left;
        public int Top;
        public int Right;
        public int Bottom;

        public static Thickness operator +(Thickness one, Thickness two)
        {
            return new Thickness(one.Left + two.Left, one.Top + two.Top, one.Right + two.Right, one.Bottom + two.Bottom);
        }

        public static readonly Thickness Empty = new Thickness();

        #region IXmlSerializable Members

        public XmlSchema GetSchema()
        {
            return null;
        }

        void Parse(string s)
        {
            string[] splits = s.Split(',');
            Left = Int32.Parse(splits[0]);
            Top = Int32.Parse(splits[1]);
            Right = Int32.Parse(splits[2]);
            Bottom = Int32.Parse(splits[3]);
        }

        public void ReadXml(XmlReader reader)
        {
            Parse(reader.ReadElementContentAsString());
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteString(string.Format("{0},{1},{2},{3}", Left, Top, Right, Bottom));
        }

        #endregion
    }

    public class WindowlessPaintControl : WindowlessControl, IWindowlessPaintControl
    {
        DependencyPropertyStorage<bool> myPaintSelf;
        DependencyPropertyStorage<bool> myPaintChildren;
        DependencyPropertyStorage<bool> myClipToBounds;
        public WindowlessPaintControl()
        {
            myPaintSelf = new DependencyPropertyStorage<bool>(this, true, new DependencyPropertyChangedEvent(PaintSelfChanged));
            myPaintChildren = new DependencyPropertyStorage<bool>(this, true, new DependencyPropertyChangedEvent(PaintChildrenChanged));
            myClipToBounds = new DependencyPropertyStorage<bool>(this, true, new DependencyPropertyChangedEvent(ClipToBoundsChanged));
        }

        void ClipToBoundsChanged(object sender, DependencyPropertyEventArgs e)
        {
            WindowlessControlHost.WindowlessInvalidate(this);
        }
        void PaintSelfChanged(object sender, DependencyPropertyEventArgs e)
        {
            WindowlessControlHost.WindowlessInvalidate(this);
        }
        void PaintChildrenChanged(object sender, DependencyPropertyEventArgs e)
        {
            WindowlessControlHost.WindowlessInvalidate(this);
        }

        protected virtual void OnPaint(WindowlessPaintEventArgs e)
        {
        }

        #region IWindowlessPaintControl Members

        [XmlAttribute]
        public bool ClipToBounds
        {
            get
            {
                return myClipToBounds;
            }
            set
            {
                myClipToBounds.Value = value;
            }
        }

        [XmlAttribute]
        public bool PaintSelf
        {
            get
            {
                return myPaintSelf;
            }
            set
            {
                myPaintSelf.Value = value;
            }
        }

        [XmlAttribute]
        public bool PaintChildren
        {
            get
            {
                return myPaintChildren;
            }
            set
            {
                myPaintChildren.Value = value;
            }
        }

        public void OnWindowlessPaint(WindowlessPaintEventArgs e)
        {
            Point adjustedOrigin = new Point(e.Origin.X + Margin.Left, e.Origin.Y + Margin.Top);
            Rectangle newClip = e.ClipRectangle;
            newClip.X += Margin.Left;
            newClip.Y += Margin.Top;
            newClip.Width -= (Margin.Left + Margin.Right);
            newClip.Height -= (Margin.Top + Margin.Bottom);
            e.ClipRectangle = newClip;
            e.Origin = adjustedOrigin;
            OnPaint(e);
        }

        #endregion
    }
}
