using System;

namespace Shuriken.Rendering.Gvr
{
    //Modified from GVRTool
    abstract class GvrPaletteDataFormat : PaletteDataFormat
    {
        public GvrPaletteDataFormat(ushort paletteEntryCount) : base(paletteEntryCount)
        {

        }

        public static GvrPaletteDataFormat Get(ushort paletteEntryCount, GvrPixelFormat format)
        {
            switch (format)
            {
                case GvrPixelFormat.Rgb5a3:
                    return new GvrPaletteA8R8G8B8(paletteEntryCount);
                case GvrPixelFormat.Rgb565:
                    return new GvrPaletteR8G8B8(paletteEntryCount);
                default:
                    throw new NotImplementedException($"Unsupported GVR palette data format: {format}.");
            }
        }
    }
}