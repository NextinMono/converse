using System.IO;

namespace Shuriken.Rendering.Gvr
{
    abstract class PaletteDataFormat
    {
        public ushort PaletteEntryCount { get; set; }

        public abstract uint DecodedDataLength { get; }
        public abstract uint EncodedDataLength { get; }

        public byte[] Decode(Stream inputStream)
        {
            byte[] input = new byte[EncodedDataLength];
            inputStream.Read(input, 0, input.Length);

            return Decode(input);
        }

        public byte[] Encode(Stream inputStream)
        {
            byte[] input = new byte[DecodedDataLength];
            inputStream.Read(input, 0, input.Length);

            return Encode(input);
        }

        public abstract byte[] Decode(byte[] input);
        public abstract byte[] Encode(byte[] input);

        public PaletteDataFormat(ushort paletteEntryCount)
        {
            PaletteEntryCount = paletteEntryCount;
        }
    }
}