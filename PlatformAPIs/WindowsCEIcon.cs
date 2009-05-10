using System;
using WindowlessControls;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Runtime.InteropServices;

namespace WindowlessControls
{
    public class WindowsCEIcon : PlatformBitmap
    {
        [DllImport("coredll")]
        extern static IntPtr ExtractIconEx(string file, int iconIndex, out IntPtr largeIcon, out IntPtr smallIcon, uint nIcons);

        [DllImport("coredll")]
        extern static bool DrawIconEx(IntPtr hdc, int xLeft, int yTop, IntPtr hIcon, int width, int height, int animation, IntPtr flickerFree, uint drawFlags);

        Icon myIcon;
        public WindowsCEIcon(Icon icon)
        {
            myIcon = icon;
        }

        public override int Height
        {
            get
            {
                if (myIcon == null)
                    return 0;
                return myIcon.Height;
            }
        }

        public override int Width
        {
            get
            {
                if (myIcon == null)
                    return 0;
                return myIcon.Width;
            }
        }

        public override PlatformBitmap GetThumbnail(int width, int height)
        {
            throw new NotSupportedException();
        }

        public static Icon GetIcon(string path, int index)
        {
            IntPtr largeIcon;
            IntPtr smallIcon;
            IntPtr hIcon = ExtractIconEx(path, -Math.Abs(index), out largeIcon, out smallIcon, 1);
            if (hIcon == IntPtr.Zero)
                return null;
            return Icon.FromHandle(hIcon);
        }

        public override void Draw(Graphics g, Rectangle dstRect, Rectangle srcRect)
        {
            if (myIcon == null)
                return;
            IntPtr hdc = g.GetHdc();
            try
            {
                DrawIconEx(hdc, dstRect.X, dstRect.Y, myIcon.Handle, dstRect.Width, dstRect.Height, 0, IntPtr.Zero, 3);
            }
            finally
            {
                g.ReleaseHdc(hdc);
            }
        }
    }
}
