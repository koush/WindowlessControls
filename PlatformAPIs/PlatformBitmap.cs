using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using WindowlessControls;
using System.Runtime.InteropServices;
using System.IO;
using System.Drawing.Imaging;
using System.Reflection;

namespace WindowlessControls
{
    public abstract class PlatformBitmap : IDisposable
    {
        public abstract int Width
        {
            get;
        }

        public abstract int Height
        {
            get;
        }

        public abstract PlatformBitmap GetThumbnail(int width, int height);
        public abstract void Draw(Graphics g, Rectangle dstRect, Rectangle srcRect);

        internal static IImagingFactory myImagingFactory;
        public static readonly Color TransparentColor = Color.Transparent;
        public static readonly SolidBrush TransparentBrush = new SolidBrush(Color.Transparent);
        public static readonly ImageAttributes TransparentImageAttributes = new ImageAttributes();

        static PlatformBitmap()
        {
            if (Environment.OSVersion.Platform == PlatformID.WinCE)
            {
                TransparentColor = Color.FromArgb(100, 100, 100);
                TransparentBrush = new SolidBrush(TransparentColor);
                TransparentImageAttributes.SetColorKey(TransparentColor, TransparentColor);
            }
        }

        public static Bitmap OptimizeBitmap(Stream bitmapStream)
        {
            Bitmap bitmap = new Bitmap(bitmapStream);
            if (Environment.OSVersion.Platform != PlatformID.WinCE)
                return bitmap;
            
            using (bitmap)
            {
                Bitmap ret = new Bitmap(bitmap.Width, bitmap.Height, PixelFormat.Format16bppRgb565);
                using (Graphics g = Graphics.FromImage(ret))
                {
                    g.DrawImage(bitmap, 0, 0);
                    return ret;
                }
            }

            //if (Environment.OSVersion.Platform == PlatformID.WinCE)
            //{
            //    if (myImagingFactory == null)
            //        myImagingFactory = (IImagingFactory)Activator.CreateInstance(Type.GetTypeFromCLSID(new Guid("327ABDA8-072B-11D3-9D7B-0000F81EF32E")));
            //    byte[] bytes = new byte[bitmapStream.Length];
            //    bitmapStream.Read(bytes, 0, (int)bitmapStream.Length);
            //    IImage imagingResource;
            //    uint hresult = myImagingFactory.CreateImageFromBuffer(bytes, (uint)bitmapStream.Length, BufferDisposalFlag.BufferDisposalFlagNone, out imagingResource);
            //    ImageInfo imageInfo = new ImageInfo();
            //    hresult = imagingResource.GetImageInfo(out imageInfo);
            //    Bitmap ret = new Bitmap((int)imageInfo.Width, (int)imageInfo.Height, PixelFormat.Format16bppRgb565);
            //    using (Graphics g = Graphics.FromImage(ret))
            //    {
            //        g.Clear(myTransparentColor);
            //        RECT dstRect = new RECT(0, 0, (int)imageInfo.Width, (int)imageInfo.Height);
            //        imagingResource.Draw(g.GetHdc(), ref dstRect, IntPtr.Zero);
            //        Marshal.ReleaseComObject(imagingResource);
            //        return ret;
            //    }
            //}
            //else
            //{
            //    PixelFormat format = (PixelFormat)925707;
            //    using (Bitmap bitmap = new Bitmap(bitmapStream))
            //    {
            //        Bitmap ret = new Bitmap(bitmap.Width, bitmap.Height, format);
            //        using (Graphics g = Graphics.FromImage(ret))
            //        {
            //            g.DrawImage(bitmap, 0, 0);
            //            return ret;
            //        }
            //    }
            //}
        }

        public static PlatformBitmap SafeFrom(string filename)
        {
            try
            {
                return From(filename);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static PlatformBitmap SafeFrom(Stream bitmapStream)
        {
            try
            {
                return From(bitmapStream);
            }
            catch (Exception)
            {
                return null;
            }
        }

        static List<string> myOpaqueBitmaps = new List<string>(new string[] { ".BMP", ".JPG" });
        public static PlatformBitmap From(string filename)
        {
            using (FileStream stream = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                return PlatformBitmap.From(stream, IsOpaque(filename));
            }
        }

        static bool IsOpaque(string filename)
        {
            return myOpaqueBitmaps.Contains(Path.GetExtension(filename).ToUpper());
        }

        public static PlatformBitmap FromResource(string name)
        {
            Assembly ass = Assembly.GetCallingAssembly();
            Stream stream = ass.GetManifestResourceStream(string.Format("{0}.{1}", ass.GetName().Name, name));
            if (stream == null)
                stream = ass.GetManifestResourceStream(name);
            return PlatformBitmap.From(stream, IsOpaque(name));
        }

        public static PlatformBitmap From(Stream bitmapStream)
        {
            return From(bitmapStream, false);
        }

        public static PlatformBitmap From(Stream bitmapStream, bool isOpaque)
        {
            if (Environment.OSVersion.Platform == PlatformID.WinCE && !isOpaque)
            {
                //return new StandardBitmap(OptimizeBitmap(bitmapStream));

                if (myImagingFactory == null)
                    myImagingFactory = (IImagingFactory)Activator.CreateInstance(Type.GetTypeFromCLSID(new Guid("327ABDA8-072B-11D3-9D7B-0000F81EF32E")));
                byte[] bytes = new byte[bitmapStream.Length];
                bitmapStream.Read(bytes, 0, (int)bitmapStream.Length);
                IImage imagingResource;
                uint hresult = myImagingFactory.CreateImageFromBuffer(bytes, (uint)bitmapStream.Length, BufferDisposalFlag.BufferDisposalFlagNone, out imagingResource);

                IBitmapImage bitmap;
                myImagingFactory.CreateBitmapFromImage(imagingResource, 0, 0, PixelFormatID.PixelFormat32bppARGB, InterpolationHint.InterpolationHintDefault, out bitmap);
                Marshal.FinalReleaseComObject(imagingResource);
                imagingResource = bitmap as IImage; 
                
                return new WindowsCEBitmap(imagingResource);
            }
            else
            {
                return new StandardBitmap(bitmapStream);
            }
        }

        #region IDisposable Members

        public virtual void Dispose()
        {
        }

        #endregion
    }

    public class StandardBitmap : PlatformBitmap, IDisposable
    {
        public StandardBitmap(int width, int height)
        {
            if (Environment.OSVersion.Platform == PlatformID.WinCE)
                myBitmap = new Bitmap(width, height, PixelFormat.Format16bppRgb565);
            else
                myBitmap = new Bitmap(width, height, (PixelFormat)925707);
        }

        public override PlatformBitmap GetThumbnail(int width, int height)
        {
            Bitmap bitmap;
            if (Environment.OSVersion.Platform == PlatformID.WinCE)
                bitmap = new Bitmap(width, height, PixelFormat.Format16bppRgb565);
            else
                bitmap = new Bitmap(width, height, (PixelFormat)925707);

            using (Graphics newGraphics = Graphics.FromImage(bitmap))
            {
                newGraphics.DrawImage(myBitmap, new Rectangle(0, 0, width, height), new Rectangle(0, 0, myBitmap.Width, myBitmap.Height), GraphicsUnit.Pixel);
            }
            return new StandardBitmap(bitmap);
        }

        public StandardBitmap(Bitmap bitmap)
        {
            myBitmap = bitmap;
        }

        public StandardBitmap(Stream stream)
        {
            myBitmap = new Bitmap(PlatformBitmap.OptimizeBitmap(stream));
        }

        public static new StandardBitmap From(string filename)
        {
            return new StandardBitmap(new Bitmap(filename));
        }

        public static new StandardBitmap SafeFrom(string filename)
        {
            try
            {
                return From(filename);
            }
            catch (Exception)
            {
                return null;
            }
        }

        Bitmap myBitmap;

        public Bitmap Bitmap
        {
            get { return myBitmap; }
            set { myBitmap = value; }
        }

        public override int Height
        {
            get { return myBitmap.Height; }
        }

        public override int Width
        {
            get { return myBitmap.Width; }
        }

        public override void Draw(Graphics g, Rectangle dstRect, Rectangle srcRect)
        {
            //g.DrawImage(myBitmap, dstRect, srcRect.X, srcRect.Y, srcRect.Width, srcRect.Height, GraphicsUnit.Pixel, TransparentImageAttributes);
            g.DrawImage(myBitmap, dstRect, srcRect, GraphicsUnit.Pixel);
        }

        public override void Dispose()
        {
            if (myBitmap != null)
            {
                myBitmap.Dispose();
                myBitmap = null;
            }
        }
    }

    class WindowsCEBitmap : PlatformBitmap
    {
        IImage myImage;
        ImageInfo myImageInfo = new ImageInfo();
        IntPtr myBuffer = IntPtr.Zero;
        double myScaleFactorX = 0;
        double myScaleFactorY = 0;

        public WindowsCEBitmap(IImage image)
        {
            myImage = image;
            myImage.GetImageInfo(out myImageInfo);
            myScaleFactorX = 1 / myImageInfo.Xdpi * 2540;
            myScaleFactorY = 1 / myImageInfo.Ydpi * 2540;
            myBuffer = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(RECT)));
        }

        public override int Height
        {
            get
            {
                return (int)myImageInfo.Height;
            }
        }

        public override int Width
        {
            get
            {
                return (int)myImageInfo.Width;
            }
        }

        public override PlatformBitmap GetThumbnail(int width, int height)
        {
            IBitmapImage bitmap;
            myImagingFactory.CreateBitmapFromImage(myImage, (uint)width, (uint)height, myImageInfo.pixelFormat, InterpolationHint.InterpolationHintDefault, out bitmap);
            return new WindowsCEBitmap(bitmap as IImage);
        }

        public override void Draw(Graphics g, Rectangle dstRect, Rectangle srcRect)
        {
            RECT dst = new RECT(dstRect);

            if (dstRect == srcRect)
            {
                IntPtr hdc = g.GetHdc();
                myImage.Draw(hdc, ref dst, IntPtr.Zero);
                g.ReleaseHdc(hdc);
            }
            else
            {
                RECT src = new RECT(srcRect);
                src.Left = (int)(src.Left * myScaleFactorX);
                src.Top = (int)(src.Top * myScaleFactorY);
                src.Right = (int)(src.Right * myScaleFactorX);
                src.Bottom = (int)(src.Bottom * myScaleFactorY);
                Marshal.StructureToPtr(src, myBuffer, false);
                IntPtr hdc = g.GetHdc();
                myImage.Draw(hdc, ref dst, myBuffer);
                g.ReleaseHdc(hdc);
            }
        }

        public override void Dispose()
        {
            if (myImage != null)
            {
                Marshal.FinalReleaseComObject(myImage);
                myImage = null;
            }
            if (myBuffer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(myBuffer);
            }
        }
    }
}
