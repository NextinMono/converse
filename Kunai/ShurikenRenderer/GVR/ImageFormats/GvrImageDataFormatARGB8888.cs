namespace Shuriken.Rendering.Gvr
{
    class GvrImageDataFormatARGB8888 : GvrImageDataFormat
    {
        public override uint BitsPerPixel => 32;

        public override uint DecodedDataLength => (uint)((Width * Height) << 2);
        public override uint EncodedDataLength => (uint)((Width * Height) << 2);

        public override byte TgaAlphaChannelBits => 8;

        public GvrImageDataFormatARGB8888(ushort width, ushort height) : base(width, height)
        {

        }

        public override byte[] Decode(byte[] input)
        {
            byte[] output = new byte[DecodedDataLength];
            int offset = 0;

            for (int y = 0; y < Height; y += 4)
            {
                for (int x = 0; x < Width; x += 4)
                {
                    for (int y2 = 0; y2 < 4; y2++)
                    {
                        for (int x2 = 0; x2 < 4; x2++)
                        {
                            output[((((y + y2) * Width) + (x + x2)) * 4) + 3] = input[offset + 0];
                            output[((((y + y2) * Width) + (x + x2)) * 4) + 2] = input[offset + 1];
                            output[((((y + y2) * Width) + (x + x2)) * 4) + 1] = input[offset + 32];
                            output[((((y + y2) * Width) + (x + x2)) * 4) + 0] = input[offset + 33];

                            offset += 2;
                        }
                    }

                    offset += 32;
                }
            }

            return output;
        }

        public override byte[] Encode(byte[] input)
        {
            byte[] output = new byte[EncodedDataLength];
            int offset = 0;

            for (int y = 0; y < Height; y += 4)
            {
                for (int x = 0; x < Width; x += 4)
                {
                    for (int y2 = 0; y2 < 4; y2++)
                    {
                        for (int x2 = 0; x2 < 4; x2++)
                        {
                            output[offset + 00] = input[((((y + y2) * Width) + (x + x2)) * 4) + 3];
                            output[offset + 01] = input[((((y + y2) * Width) + (x + x2)) * 4) + 2];
                            output[offset + 32] = input[((((y + y2) * Width) + (x + x2)) * 4) + 1];
                            output[offset + 33] = input[((((y + y2) * Width) + (x + x2)) * 4) + 0];

                            offset += 2;
                        }
                    }

                    offset += 32;
                }
            }

            return output;
        }
    }
}