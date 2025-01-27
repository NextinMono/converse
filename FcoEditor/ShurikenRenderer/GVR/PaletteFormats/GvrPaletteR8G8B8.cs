using Shuriken.Rendering.Gvr;

namespace Shuriken.Rendering.Gvr
{
    class GvrPaletteR8G8B8 : GvrPaletteDataFormat
    {
        public override uint DecodedDataLength => (uint)(PaletteEntryCount * 3);
        public override uint EncodedDataLength => (uint)(PaletteEntryCount * 2);


        public GvrPaletteR8G8B8(ushort paletteEntryCount) : base(paletteEntryCount)
        {

        }

        public override byte[] Decode(byte[] input)
        {
            byte[] output = new byte[DecodedDataLength];
            int paletteOffset = 0;

            for (int p = 0; p < output.Length; p += 3)
            {
                ushort entry = (ushort)((input[paletteOffset] << 8) | input[paletteOffset + 1]);
                paletteOffset += 2;

                output[p + 2] = (byte)(((entry >> 11) & 0b0000_0000_0001_1111) * (255 / 31));
                output[p + 1] = (byte)(((entry >> 05) & 0b0000_0000_0011_1111) * (255 / 63));
                output[p + 0] = (byte)(((entry >> 00) & 0b0000_0000_0001_1111) * (255 / 31));
            }

            return output;
        }

        public override byte[] Encode(byte[] input)
        {
            byte[] output = new byte[EncodedDataLength];
            int offset = 0;

            for (int p = 0; p < output.Length; p += 2)
            {
                ushort color = 0x0;
                color |= (ushort)((input[offset + 2] >> 3) << 11);
                color |= (ushort)((input[offset + 1] >> 2) << 5);
                color |= (ushort)((input[offset + 0] >> 3) << 0);

                output[p + 0] = (byte)(color >> 8);
                output[p + 1] = (byte)(color & 0b0000_0000_1111_1111);

                offset += 3;
            }

            return output;
        }
    }
}