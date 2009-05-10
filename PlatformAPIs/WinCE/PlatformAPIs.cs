using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace WindowlessControls
{
    // These structures, enumerations and p/invoke signatures come from
    // wingdi.h in the Windows Mobile 5.0 Pocket PC SDK

    struct BlendFunction
    {
        public byte BlendOp;
        public byte BlendFlags;
        public byte SourceConstantAlpha;
        public byte AlphaFormat;
    }

    enum BlendOperation : byte
    {
        AC_SRC_OVER = 0x00
    }

    enum BlendFlags : byte
    {
        Zero = 0x00
    }

    enum SourceConstantAlpha : byte
    {
        Transparent = 0x00,
        Opaque = 0xFF
    }

    enum AlphaFormat : byte
    {
        AC_SRC_ALPHA = 0x01
    }

    class PlatformAPIs
    {
        [DllImport("coredll.dll")]
        extern public static Int32 AlphaBlend(IntPtr hdcDest, Int32 xDest, Int32 yDest, Int32 cxDest, Int32 cyDest, IntPtr hdcSrc, Int32 xSrc, Int32 ySrc, Int32 cxSrc, Int32 cySrc, BlendFunction blendFunction);
    }
}
