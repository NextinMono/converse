using System;

namespace Shuriken.Rendering.Gvr
{
    internal abstract class GvrImageDataFormat : ImageDataFormatBase
    {
        public GvrImageDataFormat(ushort width, ushort height) : base(width, height)
        {

        }

        public static GvrImageDataFormat Get(ushort width, ushort height, GvrDataFormat format)
        {
            switch (format)
            {
                case GvrDataFormat.Index4:
                    return new GvrImageDataFormatIndex4(width, height);
                case GvrDataFormat.Index8:
                    return new GvrImageDataFormatIndex8(width, height);
                case GvrDataFormat.Rgb5a3:
                    return new GvrImageDataFormatRGB5A3(width, height);
                case GvrDataFormat.Argb8888:
                    return new GvrImageDataFormatARGB8888(width, height);
                case GvrDataFormat.Dxt1:
                    return new GvrImageDataFormatDxt1(width, height);
                default:
                    throw new NotImplementedException($"Unsupported GVR image data format: {format}.");
            }
        }
    }
}