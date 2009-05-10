//////////////////////////////////////////////////////////////
// Koushik Dutta - 9/1/2007
//////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Collections;
using System.Drawing.Imaging;
using System.IO;
using WindowlessControls;
using System.Runtime.InteropServices;
using System.Xml.Serialization;


namespace WindowlessControls
{
    public partial class WindowlessControlHost : ScrollableControl, IWindowlessPaintControl
    {
        Size myDirtyRegionSize = Size.Empty;
        Region myDirtyRegion;
        Bitmap myBackBuffer;
        Graphics myGraphics;
        bool myDoubleBuffer = false;
        Size myOldSize;

        static Bitmap ourBackBuffer;
        static Graphics ourGraphics;

        public Region DirtyRegion
        {
            get { return myDirtyRegion; }
        }

        public Bitmap BackBuffer
        {
            get { return myBackBuffer; }
        }

        public bool DoubleBuffer
        {
            get { return myDoubleBuffer; }
            set
            {
                myDoubleBuffer = value;
                if (!myDoubleBuffer)
                    DisposeBackBuffer();
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            // we are only going to call OnPaint
            // otherwise get awesome screen flickering. 
            return;
        }

        void DisposeBackBuffer()
        {
            if (myBackBuffer != null)
            {
                myBackBuffer.Dispose();
                myBackBuffer = null;
            }
            if (myGraphics != null)
            {
                myGraphics.Dispose();
                myGraphics = null;
            }
        }

        void PrepareBackBuffer(Rectangle clipRectangle)
        {
            if (DoubleBuffer)
            {
                if (myBackBuffer == null || myBackBuffer.Width != Width || myBackBuffer.Height != Height)
                {
                    DisposeBackBuffer();
                    //myDirtyRegion = new Region(new Rectangle(0, 0, Width, Height));
                    PixelFormat format = (PixelFormat)925707;
                    if (Environment.OSVersion.Platform == PlatformID.WinCE)
                        format = PixelFormat.Format16bppRgb565;
                    myBackBuffer = new Bitmap(Math.Max(Width, 1), Math.Max(Height, 1), format);
                    myGraphics = Graphics.FromImage(myBackBuffer);
                }
            }
            else
            {
                Size size = Size;
                if (ourBackBuffer == null || ourBackBuffer.Width < clipRectangle.Width || ourBackBuffer.Height < clipRectangle.Height)
                {
                    if (ourBackBuffer != null)
                    {
                        ourBackBuffer.Dispose();
                        ourBackBuffer = null;
                    }
                    if (ourGraphics != null)
                    {
                        ourGraphics.Dispose();
                        ourGraphics = null;
                    }

                    PixelFormat format = (PixelFormat)925707;
                    if (Environment.OSVersion.Platform == PlatformID.WinCE)
                        format = PixelFormat.Format16bppRgb565;
                    ourBackBuffer = new Bitmap(Math.Max(Width, 1), Math.Max(Height, 1), format);
                    ourGraphics = Graphics.FromImage(ourBackBuffer);
                }
            }
        }

        public void WindowlessPaint(Graphics graphics, Point origin, Rectangle clipRectangle)
        {
            WindowlessPaintHost(this, graphics, origin, clipRectangle);
        }

        Region myDummyRegion = new Region();
        WindowlessPaintEventArgs myPaintEventArgs = new WindowlessPaintEventArgs(null, Rectangle.Empty, Point.Empty);
        void WindowlessPaintHost(IWindowlessControl control, Graphics graphics, Point origin, Rectangle destRect)
        {
            if (!control.Visible)
                return;
            // paint everything in a clip rectangle belonging to a host tree

            IWindowlessPaintControl paintControl = control as IWindowlessPaintControl;
            if (paintControl != null)
            {
                if (paintControl.PaintSelf)
                {
                    WindowlessPaintEventArgs we = myPaintEventArgs;
                    we.Graphics = graphics;
                    we.ClipRectangle = destRect;
                    we.Origin = origin;
                    Rectangle clip = we.ClipRectangle;
                    clip.X += we.Origin.X;
                    clip.Y += we.Origin.Y;

                    if (paintControl.ClipToBounds)
                    {
                        myDummyRegion.MakeEmpty();
                        myDummyRegion.Union(clip);
                        myDummyRegion.Intersect(graphics.Clip);
                    }
                    else
                    {
                        myDummyRegion.MakeInfinite();
                    }
                    Region oldClip = we.Graphics.Clip;
                    we.Graphics.Clip = myDummyRegion;

                    paintControl.OnWindowlessPaint(we);

                    we.Graphics.Clip = oldClip;
                }
                if (!paintControl.PaintChildren)
                    return;
            }

            if (control.Controls == null)
                return;
            foreach (IWindowlessControl child in control.Controls)
            {
                if (child is WindowlessControlHost)
                    continue;
                Point childOrigin = new Point(origin.X + child.Left, origin.Y + child.Top);
                Rectangle childRect = new Rectangle(child.Left, child.Top, child.Width, child.Height);
                Rectangle clipRect = childRect;
                clipRect.Location = childOrigin;
                if (!graphics.Clip.IsVisible(clipRect))
                    continue;
                childRect.Intersect(destRect);
                if (childRect.Width <= 0 || childRect.Height <= 0)
                    continue;
                childRect.X -= child.Left;
                childRect.Y -= child.Top;
                WindowlessPaintHost(child, graphics, childOrigin, childRect);
            }
        }

        //public void WindowlessValidate()
        //{
        //    // this function is used to paint the back buffer for alternate use if the control
        //    // is not parented and thus not receiving paint events.
        //    WindowlessValidate(new Rectangle(0, 0, Width, Height));
        //}

        //public void WindowlessValidate(Rectangle rect)
        //{
        //    // this function is used to paint the back buffer for alternate use if the control
        //    // is not parented and thus not receiving paint events.
        //    if (!DoubleBuffer)
        //        throw new Exception("This control is not double buffered.");
        //    OnWindowlessPaint(this, null, Point.Empty, rect);
        //}

        void PrepareDirtyRegion()
        {
            if (myDirtyRegionSize != Size || myDirtyRegion == null)
            {
                myDirtyRegionSize = Size;
                myDirtyRegion = new Region(new Rectangle(0, 0, Width, Height));
            }
        }

        protected void OnWindowlessPaint(WindowlessControlHost sender, Graphics graphics, Point origin, Rectangle clipRectangle)
        {
            SyncWindows();

            Rectangle rect = clipRectangle;
            Graphics g = graphics;
            Point o = origin;
            Bitmap backBuffer = null;
            Region oldClip = null;
            if (DoubleBuffer || sender == this)
            {
                PrepareBackBuffer(clipRectangle);
                o = Point.Empty;
                if (DoubleBuffer)
                {
                    g = myGraphics;
                    backBuffer = myBackBuffer;
                    PrepareDirtyRegion();

                    if (myDirtyRegion.IsVisible(clipRectangle))
                    {
                        RectangleF regionBounds = myDirtyRegion.GetBounds(myGraphics);
                        rect = new Rectangle((int)Math.Round(regionBounds.X, 0), (int)Math.Round(regionBounds.Y, 0), (int)Math.Round(regionBounds.Width, 0), (int)Math.Round(regionBounds.Height, 0));
                        rect.Intersect(clipRectangle);
                        myDirtyRegion.Exclude(rect);
                    }
                    else
                        rect = Rectangle.Empty;
                }
                else
                {
                    o = new Point(-clipRectangle.X, -clipRectangle.Y);
                    clipRectangle.X = 0;
                    clipRectangle.Y = 0;
                    g = ourGraphics;
                    backBuffer = ourBackBuffer;
                }

                oldClip = g.Clip;
                g.Clip = myDummyRegion;
                oldClip.MakeEmpty();
                oldClip.Union(clipRectangle);
                if (DoubleBuffer)
                    oldClip.Intersect(myDirtyRegion);
                g.Clip = oldClip;
            }

            if (rect.Width != 0 && rect.Height != 0)
            {
                if (BackColor == Color.Transparent)
                {
                    WindowlessControlHost host = Parent as WindowlessControlHost;
                    if (host != null)
                    {
                        Rectangle parentRect = rect;
                        parentRect.X += Left;
                        parentRect.Y += Top;
                        host.OnWindowlessPaint(sender, g, new Point(o.X - Left, o.Y - Top), parentRect);
                    }
                    else
                        g.FillRectangle(PlatformBitmap.TransparentBrush, origin.X + clipRectangle.X, origin.Y + clipRectangle.Y, clipRectangle.Width, clipRectangle.Height);
                }
                WindowlessPaintHost(this, g, o, rect);
            }

            if (DoubleBuffer || sender == this)
            {
                Rectangle sourceRectangle = clipRectangle;
                if (sender == this)
                {
                    clipRectangle.X = -o.X;
                    clipRectangle.Y = -o.Y;
                    sourceRectangle.X = 0;
                    sourceRectangle.Y = 0;
                }
                g.Clip = oldClip;
                if (graphics != null)
                    graphics.DrawImage(backBuffer, new Rectangle(origin.X + clipRectangle.Left, origin.Y + clipRectangle.Top, clipRectangle.Width, clipRectangle.Height), sourceRectangle, GraphicsUnit.Pixel);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            OnWindowlessPaint(this, e.Graphics, Point.Empty, e.ClipRectangle);
        }

        public override void Refresh()
        {
            base.Refresh();

            // workaround for windows ce bug where refresh doesn't actually refresh
            // child controls
            if (Environment.OSVersion.Platform == PlatformID.WinCE)
            {
                foreach (Control control in Controls)
                    control.Refresh();
            }
        }

        Color myBackColor = Color.White;
        //public override Color BackColor
        //{
        //    get
        //    {
        //        return myBackColor;
        //    }
        //    set
        //    {
        //        myBackColor = value;
        //    }
        //}

        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public new SmartColor BackColor
        {
            get
            {
                return myBackColor;
            }
            set
            {
                myBackColor = value;
            }
        }

        public void WindowlessInvalidate()
        {
            WindowlessInvalidate(new Rectangle(0, 0, ClientSize.Width, ClientSize.Height));
        }

        public static void WindowlessInvalidate(IWindowlessControl sender)
        {
            Point origin = Point.Empty;
            IWindowlessControl parent = sender;
            while (!(parent is WindowlessControlHost) && parent != null)
            {
                origin.X += parent.Left;
                origin.Y += parent.Top;
                parent = parent.Parent;
            }
            WindowlessControlHost host = parent as WindowlessControlHost;
            if (host != null)
                host.WindowlessInvalidate(new Rectangle(origin.X, origin.Y, sender.Width, sender.Height));
        }

        void WindowlessInvalidate(Rectangle rect)
        {
            PrepareDirtyRegion();
            myDirtyRegion.Union(rect);
            Invalidate(rect);

            // tell all child controls that are transparent and in this rect that they need to repaint
            foreach (Control control in Controls)
            {
                WindowlessControlHost wc = control as WindowlessControlHost;
                if (wc == null || wc.BackColor != Color.Transparent)
                    continue;
                Rectangle intersect = new Rectangle(wc.Left, wc.Top, wc.Width, wc.Height);
                intersect.Intersect(rect);
                if (intersect.Width == 0 || intersect.Height == 0)
                    continue;
                intersect.X -= wc.Left;
                intersect.Y -= wc.Top;
                wc.WindowlessInvalidate(intersect);
            }
        }

        //public void WindowlessDirty(Rectangle rect)
        //{
        //    WindowlessDirty(this, rect, false);
        //}

        //public void WindowlessDirty()
        //{
        //    Size clientSize = ClientSize;
        //    WindowlessDirty(new Rectangle(0, 0, clientSize.Width, clientSize.Height));
        //}

        //public void WindowlessInvalidate(Rectangle rect)
        //{
        //    WindowlessInvalidate(rect, true);
        //}


        void PaintSelfChanged(object sender, DependencyPropertyEventArgs e)
        {
            WindowlessControlHost.WindowlessInvalidate(this);
        }
        void PaintChildrenChanged(object sender, DependencyPropertyEventArgs e)
        {
            WindowlessControlHost.WindowlessInvalidate(this);
        }
        void ClipToBoundsChanged(object sender, DependencyPropertyEventArgs e)
        {
            WindowlessControlHost.WindowlessInvalidate(this);
        }

        #region IWindowlessPaintControl Members

        public virtual void OnWindowlessPaint(WindowlessPaintEventArgs e)
        {
            if (BackColor != Color.Transparent)
                e.Graphics.FillRectangle(new SolidBrush(BackColor), e.Origin.X + e.ClipRectangle.X, e.Origin.Y + e.ClipRectangle.Y, e.ClipRectangle.Width, e.ClipRectangle.Height);
        }

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
        #endregion
    }

    public delegate void WindowlessInvalidateEvent(WindowlessControlHost sender, Rectangle rectangle);

    public interface IWindowlessPaintControl : IWindowlessControl
    {
        bool PaintSelf
        {
            get;
            set;
        }
        bool PaintChildren
        {
            get;
            set;
        }
        bool ClipToBounds
        {
            get;
            set;
        }
        void OnWindowlessPaint(WindowlessPaintEventArgs e);
    }

    public class WindowlessPaintEventArgs
    {
        Graphics myGraphics;
        Point myOrigin;
        Rectangle myClipRectangle;

        public Rectangle ClipRectangle
        {
            get { return myClipRectangle; }
            set { myClipRectangle = value; }
        }

        public Graphics Graphics
        {
            get { return myGraphics; }
            set { myGraphics = value; }
        }

        /// <summary>
        /// Specifies the top left painting corner. Note that ClipRectangle is the property 
        /// that specifies what portion of the entire rectangle actually needs to be updated.
        /// Origin and ClipRectangle together define the location of the control on the Graphics object
        /// and what portion of the control needs to be drawn.
        /// </summary>
        public Point Origin
        {
            get { return myOrigin; }
            set { myOrigin = value; }
        }
        public WindowlessPaintEventArgs(Graphics graphics, Rectangle clipRectangle, Point origin)
        {
            myGraphics = graphics;
            myClipRectangle = clipRectangle;
            myOrigin = origin;
        }
    }
}