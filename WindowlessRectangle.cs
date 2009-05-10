using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Runtime.InteropServices;

namespace WindowlessControls
{
    public class NativeObject : IDisposable
    {
        protected NativeObject(IntPtr handle)
        {
            myHandle = handle;
        }

        IntPtr myHandle;

        public IntPtr Handle
        {
            get { return myHandle; }
        }

        [DllImport("coredll")]
        static extern bool DeleteObject(IntPtr hObject);

        [DllImport("coredll")]
        internal static extern IntPtr SelectObject(IntPtr hdc, IntPtr hObject);

        internal IntPtr Select(IntPtr hdc)
        {
            return SelectObject(hdc, myHandle);
        }

        #region IDisposable Members

        public void Dispose()
        {
            DeleteObject(myHandle);
        }

        #endregion
    }

    public class NativePen : NativeObject
    {
        [DllImport("coredll")]
        static extern IntPtr CreatePen(int fnPenStyle, int nWidth, int crColor);

        //#define PS_SOLID            0
        //#define PS_DASH             1
        //#define PS_NULL             5
        public NativePen(Color color, int width)
            : base(CreatePen(0, width, color.ToArgb()))
        {
        }
    }

    public class NativeBrush : NativeObject
    {
        [DllImport("coredll")]
        static extern IntPtr CreateSolidBrush(int crColor);

        public NativeBrush(Color color)
            : base(CreateSolidBrush(color.ToArgb()))
        {
        }
    }

    public enum GradientDirection
    {
        Horizontal = 0,
        Vertical = 1
    }

    public class WindowlessGradientRectangle : WindowlessPaintControl
    {
        DependencyPropertyStorage<Color> myTopLeftColor;
        DependencyPropertyStorage<Color> myBottomRightColor;
        DependencyPropertyStorage<GradientDirection> myGradientDirection;

        public WindowlessGradientRectangle()
        {
            myTopLeftColor = new DependencyPropertyStorage<Color>(this, Color.Transparent, new DependencyPropertyChangedEvent(TopLeftColorChanged));
            myBottomRightColor = new DependencyPropertyStorage<Color>(this, Color.Transparent, new DependencyPropertyChangedEvent(BottomRightColorChanged));
            myGradientDirection = new DependencyPropertyStorage<GradientDirection>(this, GradientDirection.Vertical, new DependencyPropertyChangedEvent(GradientDirectionChanged));
        }
        public GradientDirection GradientDirection
        {
            get
            {
                return myGradientDirection.Value;
            }
            set
            {
                myGradientDirection.Value = value;
            }
        }

        public SmartColor TopLeftColor
        {
            get
            {
                return myTopLeftColor.Value;
            }
            set
            {
                myTopLeftColor.Value = value;
            }
        }

        public SmartColor BottomRightColor
        {
            get
            {
                return myBottomRightColor.Value;
            }
            set
            {
                myBottomRightColor.Value = value;
            }
        }

        void GradientDirectionChanged(object sender, DependencyPropertyEventArgs e)
        {
            WindowlessControlHost.WindowlessInvalidate(this);
        }

        void TopLeftColorChanged(object sender, DependencyPropertyEventArgs e)
        {
            myVertices[0].Red = (ushort)(myTopLeftColor.Value.R << 8);
            myVertices[0].Green = (ushort)(myTopLeftColor.Value.G << 8);
            myVertices[0].Blue = (ushort)(myTopLeftColor.Value.B << 8);
            myVertices[0].Alpha = (ushort)(myTopLeftColor.Value.A << 8);
            WindowlessControlHost.WindowlessInvalidate(this);
        }

        void BottomRightColorChanged(object sender, DependencyPropertyEventArgs e)
        {
            myVertices[1].Red = (ushort)(myBottomRightColor.Value.R << 8);
            myVertices[1].Green = (ushort)(myBottomRightColor.Value.G << 8);
            myVertices[1].Blue = (ushort)(myBottomRightColor.Value.B << 8);
            myVertices[1].Alpha = (ushort)(myBottomRightColor.Value.A << 8);
            WindowlessControlHost.WindowlessInvalidate(this);
        }

        struct TRIVERTEX
        {
            public int X;
            public int Y;
            public ushort Red;
            public ushort Green;
            public ushort Blue;
            public ushort Alpha;
        };
        static readonly int[] myRectIndices = new int[2] { 0, 1 };

        TRIVERTEX[] myVertices = new TRIVERTEX[2];

        [DllImport("coredll")]
        extern static bool GradientFill(IntPtr hdc, TRIVERTEX[] pVertex, uint nVertex, int[] pMesh, int nCount, int ulMode);

        protected override void OnPaint(WindowlessPaintEventArgs e)
        {
            myVertices[0].X = e.Origin.X;
            myVertices[0].Y = e.Origin.Y;
            myVertices[1].X = e.Origin.X + ClientWidth;
            myVertices[1].Y = e.Origin.Y + ClientHeight;

            IntPtr hdc = e.Graphics.GetHdc();
            GradientFill(hdc, myVertices, 2, myRectIndices, 1, (int)myGradientDirection.Value);
            e.Graphics.ReleaseHdc(hdc);
        }
    }

    public class WindowlessRoundRectangle : WindowlessPaintControl
    {
        DependencyPropertyStorage<NativePen> myPen;
        DependencyPropertyStorage<NativeBrush> myBrush;
        DependencyPropertyStorage<int> myEllipseWidth;
        DependencyPropertyStorage<int> myEllipseHeight;

        static NativePen myDefaultPen = new NativePen(Color.White, 1);
        static NativeBrush myDefaultBrush = new NativeBrush(Color.White);

        public WindowlessRoundRectangle()
        {
            myPen = new DependencyPropertyStorage<NativePen>(this, myDefaultPen, new DependencyPropertyChangedEvent(PenChanged));
            myBrush = new DependencyPropertyStorage<NativeBrush>(this, myDefaultBrush, new DependencyPropertyChangedEvent(BrushChanged));
            myEllipseWidth = new DependencyPropertyStorage<int>(this, 0, new DependencyPropertyChangedEvent(EllipseWidthChanged));
            myEllipseHeight = new DependencyPropertyStorage<int>(this, 0, new DependencyPropertyChangedEvent(EllipseHeightChanged));
        }
        public NativePen Pen
        {
            get
            {
                return myPen;
            }
            set
            {
                myPen.Value = value;
            }
        }
        public NativeBrush Brush
        {
            get
            {
                return myBrush;
            }
            set
            {
                myBrush.Value = value;
            }
        }
        public int EllipseWidth
        {
            get
            {
                return myEllipseWidth;
            }
            set
            {
                myEllipseWidth.Value = value;
            }
        }
        public int EllipseHeight
        {
            get
            {
                return myEllipseHeight;
            }
            set
            {
                myEllipseHeight.Value = value;
            }
        }
        void PenChanged(object sender, DependencyPropertyEventArgs e)
        {
            WindowlessControlHost.WindowlessInvalidate(this);
        }
        void BrushChanged(object sender, DependencyPropertyEventArgs e)
        {
            WindowlessControlHost.WindowlessInvalidate(this);
        }

        void EllipseWidthChanged(object sender, DependencyPropertyEventArgs e)
        {
            WindowlessControlHost.WindowlessInvalidate(this);
        }

        void EllipseHeightChanged(object sender, DependencyPropertyEventArgs e)
        {
            WindowlessControlHost.WindowlessInvalidate(this);
        }

        [DllImport("coredll")]
        static extern bool RoundRect(
        IntPtr hdc,
        int nLeftRect,
        int nTopRect,
        int nRightRect,
        int nBottomRect,
        int nWidth,
        int nHeight
        );

        protected override void OnPaint(WindowlessPaintEventArgs e)
        {
            IntPtr hdc = e.Graphics.GetHdc();
            IntPtr oldPen = myPen.Value.Select(hdc);
            IntPtr oldBrush = myBrush.Value.Select(hdc);
            RoundRect(hdc, e.Origin.X, e.Origin.Y, e.Origin.X + ClientWidth, e.Origin.Y + ClientHeight, myEllipseWidth, myEllipseHeight);
            NativeObject.SelectObject(hdc, oldBrush);
            NativeObject.SelectObject(hdc, oldPen);
            e.Graphics.ReleaseHdc(hdc);
        }
    }

    public class WindowlessRectangle : WindowlessPaintControl
    {
        DependencyPropertyStorage<Color> myColor;
        DependencyPropertyStorage<bool> myFilled;
        DependencyPropertyStorage<double> myRectangleWidth;
        SolidBrush myBrush = new SolidBrush(Color.Transparent);
        Pen myPen = new Pen(Color.Transparent);

        public Color Color
        {
            get { return myColor; }
            set
            {
                myColor.Value = value;
            }
        }

        public double RectangleWidth
        {
            get
            {
                return myRectangleWidth;
            }
            set
            {
                myRectangleWidth.Value = value;
            }
        }

        public bool Filled
        {
            get
            {
                return myFilled;
            }
            set
            {
                myFilled.Value = value;
            }
        }

        public WindowlessRectangle()
        {
            myColor = new DependencyPropertyStorage<Color>(this, Color.Transparent, new DependencyPropertyChangedEvent(ColorChanged));
            myFilled = new DependencyPropertyStorage<bool>(this, true, new DependencyPropertyChangedEvent(FilledChanged));
            myRectangleWidth = new DependencyPropertyStorage<double>(this, 1.0, new DependencyPropertyChangedEvent(RectangleWidthChanged));
        }

        public WindowlessRectangle(int maxWidth, int maxHeight, Color color)
            : this()
        {
            HorizontalAlignment = HorizontalAlignment.Stretch;
            VerticalAlignment = VerticalAlignment.Stretch;
            MaxWidth = maxWidth;
            MaxHeight = maxHeight;
            myColor.Value = color;
            myBrush = new SolidBrush(color);
            myPen = new Pen(color, (float)myRectangleWidth.Value);
        }

        void ColorChanged(object sender, DependencyPropertyEventArgs e)
        {
            myPen.Color = Color;
            myBrush.Color = Color;
            WindowlessControlHost.WindowlessInvalidate(this);
        }

        void FilledChanged(object sender, DependencyPropertyEventArgs e)
        {
            WindowlessControlHost.WindowlessInvalidate(this);
        }

        void RectangleWidthChanged(object sender, DependencyPropertyEventArgs e)
        {
            myPen.Width = (float)myRectangleWidth.Value;
            if (!myFilled.Value)
                WindowlessControlHost.WindowlessInvalidate(this);
        }

        protected override void OnPaint(WindowlessPaintEventArgs e)
        {
            if (myColor != Color.Transparent)
            {
                if (myFilled.Value)
                    e.Graphics.FillRectangle(myBrush, e.Origin.X, e.Origin.Y, ClientWidth, ClientHeight);
                else
                    e.Graphics.DrawRectangle(myPen, e.Origin.X, e.Origin.Y, ClientWidth - 1, ClientHeight - 1);
            }
        }
    }
}