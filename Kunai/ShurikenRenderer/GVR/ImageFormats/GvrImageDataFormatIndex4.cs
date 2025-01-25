
namespace Shuriken.Rendering.Gvr
{
    class GvrImageDataFormatIndex4 : GvrImageDataFormat
    {
        public override uint BitsPerPixel => 4;

        public override uint DecodedDataLength => (uint)(Width * Height);
        public override uint EncodedDataLength => (uint)((Width * Height) >> 1);

        public override byte TgaAlphaChannelBits => 0;

        public GvrImageDataFormatIndex4(ushort width, ushort height) : base(width, height)
        {

        }

        public override byte[] Decode(byte[] input)
        {
            byte[] output = new byte[DecodedDataLength];
            int offset = 0;

            for (int y = 0; y < Height; y += 8)
            {
                for (int x = 0; x < Width; x += 8)
                {
                    for (int y2 = 0; y2 < 8; y2++)
                    {
                        for (int x2 = 0; x2 < 8; x2++)
                        {
                            byte entry = (byte)((input[offset] >> ((~x2 & 0x01) * 4)) & 0x0F);

                            output[(((y + y2) * Width) + (x + x2))] = entry;

                            if ((x2 & 0x01) != 0) offset++;
                        }
                    }
                }
            }

            return output;
        }

        public override byte[] Encode(byte[] input)
        {
            byte[] output = new byte[EncodedDataLength];
            int offset = 0;

            for (int y = 0; y < Height; y += 8)
            {
                for (int x = 0; x < Width; x += 8)
                {
                    for (int y2 = 0; y2 < 8; y2++)
                    {
                        for (int x2 = 0; x2 < 8; x2++)
                        {
                            byte entry = (byte)(input[((y + y2) * Width) + (x + x2)] & 0x0F);
                            entry = (byte)((output[offset] & (0x0F << (x2 & 0x01) * 4)) | (entry << ((~x2 & 0x01) * 4)));

                            output[offset] = entry;

                            if ((x2 & 0x01) != 0) offset++;
                        }
                    }
                }
            }

            return output;
        }
    }
}