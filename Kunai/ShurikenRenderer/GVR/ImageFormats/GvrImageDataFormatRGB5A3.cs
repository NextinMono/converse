namespace Shuriken.Rendering.Gvr
{
    class GvrImageDataFormatRGB5A3 : GvrImageDataFormat
    {
        public override uint BitsPerPixel => 16;

        public override uint DecodedDataLength => (uint)((Width * Height) << 2);
        public override uint EncodedDataLength => (uint)((Width * Height) << 1);

        public override byte TgaAlphaChannelBits => 8;

        public GvrImageDataFormatRGB5A3(ushort width, ushort height) : base(width, height)
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
                            ushort pixel = (ushort)((input[offset] << 8) | input[offset + 1]);
                            offset += 2;

                            if ((pixel & 0b1000_0000_0000_0000) == 0) // Argb3444
                            {
                                output[((((y + y2) * Width) + (x + x2)) * 4) + 3] = (byte)(((pixel >> 12) & 0x07) * 0xFF / 0x07);
                                output[((((y + y2) * Width) + (x + x2)) * 4) + 2] = (byte)(((pixel >> 8) & 0x0F) * 0xFF / 0x0F);
                                output[((((y + y2) * Width) + (x + x2)) * 4) + 1] = (byte)(((pixel >> 4) & 0x0F) * 0xFF / 0x0F);
                                output[((((y + y2) * Width) + (x + x2)) * 4) + 0] = (byte)(((pixel >> 0) & 0x0F) * 0xFF / 0x0F);
                            }
                            else // Rgb555
                            {
                                output[((((y + y2) * Width) + (x + x2)) * 4) + 3] = 0xFF;
                                output[((((y + y2) * Width) + (x + x2)) * 4) + 2] = (byte)(((pixel >> 10) & 0x1F) * 0xFF / 0x1F);
                                output[((((y + y2) * Width) + (x + x2)) * 4) + 1] = (byte)(((pixel >> 5) & 0x1F) * 0xFF / 0x1F);
                                output[((((y + y2) * Width) + (x + x2)) * 4) + 0] = (byte)(((pixel >> 0) & 0x1F) * 0xFF / 0x1F);
                            }
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

            for (int y = 0; y < Height; y += 4)
            {
                for (int x = 0; x < Width; x += 4)
                {
                    for (int y2 = 0; y2 < 4; y2++)
                    {
                        for (int x2 = 0; x2 < 4; x2++)
                        {
                            ushort pixel = 0x0000;

                            if (input[((((y + y2) * Width) + (x + x2)) * 4) + 3] <= 0xDA) // Argb3444
                            {
                                pixel |= (ushort)((input[((((y + y2) * Width) + (x + x2)) * 4) + 3] >> 5) << 12);
                                pixel |= (ushort)((input[((((y + y2) * Width) + (x + x2)) * 4) + 2] >> 4) << 8);
                                pixel |= (ushort)((input[((((y + y2) * Width) + (x + x2)) * 4) + 1] >> 4) << 4);
                                pixel |= (ushort)((input[((((y + y2) * Width) + (x + x2)) * 4) + 0] >> 4) << 0);
                            }
                            else // Rgb555
                            {
                                pixel |= 0x8000;
                                pixel |= (ushort)((input[((((y + y2) * Width) + (x + x2)) * 4) + 2] >> 3) << 10);
                                pixel |= (ushort)((input[((((y + y2) * Width) + (x + x2)) * 4) + 1] >> 3) << 5);
                                pixel |= (ushort)((input[((((y + y2) * Width) + (x + x2)) * 4) + 0] >> 3) << 0);
                            }

                            output[offset + 0] = (byte)(pixel >> 8);
                            output[offset + 1] = (byte)(pixel & 0xFF);
                            offset += 2;
                        }
                    }
                }
            }

            return output;
        }
    }
}