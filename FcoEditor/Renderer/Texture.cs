using DirectXTexNet;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Image = SixLabors.ImageSharp.Image;

namespace Converse.Rendering
{
    public class Texture
    {
        public string Name { get; }
        public string FullName { get; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public Vector2 Size { get { return new Vector2(Width, Height); } set { Width = (int)value.X; Height = (int)value.Y; } }

        public bool IsLoaded => GlTex != null;
        public BitmapSource ImageSource { get; private set; }
        internal GlTexture GlTex { get; private set; }
        public List<int> CropIndices { get; set; }

        private void CreateTexture(ScratchImage in_Img)
        {
            if (TexHelper.Instance.IsCompressed(in_Img.GetMetadata().Format))
                in_Img = in_Img.Decompress(DXGI_FORMAT.B8G8R8A8_UNORM);

            else if (in_Img.GetMetadata().Format != DXGI_FORMAT.B8G8R8A8_UNORM)
                in_Img = in_Img.Convert(DXGI_FORMAT.B8G8R8A8_UNORM, TEX_FILTER_FLAGS.DEFAULT, 0.5f);

            Width = in_Img.GetImage(0).Width;
            Height = in_Img.GetImage(0).Height;

            GlTex = new GlTexture(in_Img.FlipRotate(TEX_FR_FLAGS.FLIP_VERTICAL).GetImage(0).Pixels, Width, Height);

            CreateBitmap(in_Img);

            in_Img.Dispose();
        }

        public void Destroy()
        {
            if (GlTex != null)
            {
                GlTex.Dispose();
            }
            ImageSource = null;
        }
       
        private unsafe void CreateTexture(byte[] in_Bytes)
        {
            fixed (byte* pBytes = in_Bytes)
                CreateTexture(TexHelper.Instance.LoadFromDDSMemory((nint)pBytes, in_Bytes.Length, DDS_FLAGS.NONE));
        }

        private void CreateTextureDds(string in_Filename)
        {
            CreateTexture(TexHelper.Instance.LoadFromDDSFile(in_Filename, DDS_FLAGS.NONE));
        }

        private void CreateBitmap(ScratchImage in_Img)
        {
            var bmp = BitmapConverter.FromTextureImage(in_Img, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            ImageSource = BitmapConverter.FromBitmap(bmp);

            in_Img.Dispose();
            bmp.Dispose();
        }

        public Texture(string in_Filename) : this()
        {
            FullName = in_Filename;
            if (string.IsNullOrEmpty(in_Filename))
            {
                return;
            }
            Name = Path.GetFileNameWithoutExtension(in_Filename);

            if (File.Exists(in_Filename))
            {
                string ext = Path.GetExtension(in_Filename);
                
                if (ext == ".dds")
                {
                    CreateTextureDds(in_Filename);
                    return;
                }
                try
                {
                    CreateTextureUnknown(in_Filename);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Unknown file format.");
                }

            }
        }

        private void CreateTextureUnknown(string in_Filename)
        {
            Image<Bgra32> image = Image.Load<Bgra32>(in_Filename);

            image.Mutate(in_X => in_X.Flip(FlipMode.Vertical));
            Width = image.Width;
            Height = image.Height;
            byte[] pixelArray = new byte[(image.Width * image.Height) * 4];
            image.CopyPixelDataTo(pixelArray);
            unsafe
            {
                fixed (byte* pBytes = pixelArray)
                    GlTex = new GlTexture((nint)pBytes, Width, Height);
            }
        }

        public Texture(string in_Name, byte[] in_Bytes) : this()
        {
            FullName = in_Name;
            Name = in_Name;
            CreateTexture(in_Bytes);
        }

        public Texture()
        {
            Name = FullName = "";
            Width = Height = 0;
            ImageSource = null;
            GlTex = null;

            CropIndices = new List<int>();
        }
    }
}
