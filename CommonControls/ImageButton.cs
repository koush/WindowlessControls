using System.Drawing;
using WindowlessControls;

namespace WindowlessControls.CommonControls
{
    public class ImageButton : ButtonBase
    {
        public ImageButton()
        {
            base.Control = new WindowlessImagePresenter();
        }

        public new WindowlessImagePresenter Control
        {
            get
            {
                return base.Control as WindowlessImagePresenter;
            }
        }

        public ImageButton(PlatformBitmap bitmap, Stretch stretch)
            : this()
        {
            Control.Bitmap = bitmap;
            Control.Stretch = stretch;
        }

        public ImageButton(PlatformBitmap bitmap, Stretch stretch, PlatformBitmap focusedBitmap)
            : this(bitmap, stretch)
        {
            Control.FocusedBitmap = focusedBitmap;
        }

        public ImageButton(PlatformBitmap bitmap, Stretch stretch, PlatformBitmap focusedBitmap, PlatformBitmap clickedBitmap)
            : this(bitmap, stretch, focusedBitmap)
        {
            Control.ClickedBitmap = clickedBitmap;
        }
    }
}