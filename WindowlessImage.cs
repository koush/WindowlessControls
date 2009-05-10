using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using WindowlessControls;

namespace WindowlessControls
{
    public enum Stretch
    {
        None,
        Uniform,
        Normal
    }

    public class WindowlessImage : WindowlessPaintControl
    {
        DependencyPropertyStorage<PlatformBitmap> myBitmap;
        Stretch myStretch = Stretch.None;

        public WindowlessImage()
        {
            myBitmap = new DependencyPropertyStorage<PlatformBitmap>(this, null, new DependencyPropertyChangedEvent(BitmapChanged));
        }

        public WindowlessImage(PlatformBitmap bitmap)
            :this()
        {
            myBitmap.Value = bitmap;
        }

        public WindowlessImage(PlatformBitmap bitmap, Stretch stretch)
            : this(bitmap)
        {
            myStretch = stretch;
        }

        public WindowlessImage(PlatformBitmap bitmap, Stretch stretch, Thickness margin)
            : this(bitmap, stretch)
        {
            Margin = margin;
        }

        void BitmapChanged(object sender, DependencyPropertyEventArgs e)
        {
            PlatformBitmap oldBitmap = e.OldValue as PlatformBitmap;
            PlatformBitmap newBitmap = e.NewValue as PlatformBitmap;
            Size originalSize = Size.Empty;
            if (oldBitmap != null)
                originalSize = new Size(oldBitmap.Width, oldBitmap.Height);
            Size newSize = Size.Empty;
            if (newBitmap != null)
                newSize = new Size(newBitmap.Width, newBitmap.Height);
            if (originalSize != newSize)
                Remeasure();
            WindowlessControlHost.WindowlessInvalidate(this);
        }
        
        public Stretch Stretch
        {
            get { return myStretch; }
            set { myStretch = value; }
        }

        public PlatformBitmap Bitmap
        {
            get { return myBitmap; }
            set
            {
                myBitmap.Value = value;
            }
        }

        public override bool MeasureUnpadded(Size bounds, bool boundsChange)
        {
            PlatformBitmap bitmap = myBitmap.Value;
            if (bitmap == null)
            {
                ClientWidth = ClientHeight = 0;
                return false;
            }

            // no stretch, unless it won't fit
            if (Stretch == Stretch.None && bounds.Width > bitmap.Width && bounds.Height > bitmap.Height)
            {
                ClientWidth = bitmap.Width;
                ClientHeight = bitmap.Height;
                return false;
            }
            else if (Stretch == Stretch.Normal && bounds.Width != Int32.MaxValue && bounds.Height != Int32.MaxValue)
            {
                ClientWidth = bounds.Width;
                ClientHeight = bounds.Height;
                return false;
            }


            // uniform stretch/uniform shrink
            float ratio = (float)bitmap.Width / (float)bitmap.Height;

            if (bounds.Width / ratio > bounds.Height)
            {
                ClientHeight = bounds.Height;
                ClientWidth = (int)Math.Round(bounds.Height * ratio, 0);
            }
            else
            {
                ClientWidth = bounds.Width;
                ClientHeight = (int)Math.Round(bounds.Width / ratio, 0);
            }
            return false;
        }

        protected override void OnPaint(WindowlessPaintEventArgs e)
        {
            PlatformBitmap bitmap = myBitmap.Value;
            if (bitmap != null)
            {
                bitmap.Draw(e.Graphics, new Rectangle(e.Origin.X, e.Origin.Y, ClientWidth, ClientHeight), new Rectangle(0, 0, bitmap.Width, bitmap.Height));


                //bitmap.Draw(e.Graphics, e.ClipRectangle,


                //if (Transparent)
                //    e.Graphics.DrawImage(bitmap, new Rectangle(e.Origin.X, e.Origin.Y, ClientWidth, ClientHeight), 0, 0, bitmap.Width, bitmap.Height, GraphicsUnit.Pixel, WindowlessControlHost.TransparentImageAttributes);
                //else
                //    e.Graphics.DrawImage(bitmap, new Rectangle(e.Origin.X, e.Origin.Y, ClientWidth, ClientHeight), new Rectangle(0, 0, bitmap.Width, bitmap.Height), GraphicsUnit.Pixel);
            }
        }
    }
}
