using System.IO;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media.Imaging;
using DirectXTexNet;
using Shuriken.Rendering.Gvr;
using System.Windows.Media;
using System.Drawing;
using System.Windows.Interop;
using OpenTK.Graphics.OpenGL;

namespace Shuriken.Rendering
{    
    public class Texture
    {
        public string Name { get; }
        public string FullName { get; }
        public int Width { get; private set; }
        public int Height { get; private set; }

        public BitmapSource ImageSource { get; private set; }
        internal GLTexture GlTex { get; private set; }
        public ObservableCollection<int> Sprites { get; set; }

        private void CreateTexture(ScratchImage img)
        {
            if (TexHelper.Instance.IsCompressed(img.GetMetadata().Format))
                img = img.Decompress(DXGI_FORMAT.B8G8R8A8_UNORM);

            else if (img.GetMetadata().Format != DXGI_FORMAT.B8G8R8A8_UNORM)
                img = img.Convert(DXGI_FORMAT.B8G8R8A8_UNORM, TEX_FILTER_FLAGS.DEFAULT, 0.5f);

            Width = img.GetImage(0).Width;
            Height = img.GetImage(0).Height;

            GlTex = new GLTexture(img.FlipRotate(TEX_FR_FLAGS.FLIP_VERTICAL).GetImage(0).Pixels, Width, Height);

            CreateBitmap(img);

            img.Dispose();
        }

        public void Destroy()
        {
            if(GlTex != null)
            GL.DeleteTexture(GlTex.ID);
        }
        /// <summary>
        /// Used for GVR textures for GNCPs, converts GVR's to BitmapSource and output a pixel array for the GL WPF Control
        /// </summary>
        /// <param name="in_Gvr">GVR Texture</param>
        /// <param name="out_Pixels">Pixel array output for GL</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if the GVR pixel array is null</exception>
        public static BitmapSource LoadTga(GVRFile in_Gvr, ref byte[] out_Pixels)
        {
            if (in_Gvr.Pixels == null) throw new ArgumentNullException("GVR Image might be invalid, pixel array is null.");
            var pixelFormat = PixelFormats.Bgr32; //temporary!!!!!!

            ///FIX PLEASE THIS IS HORRIBLE AND MAKES THIS NOT FULLY FUNCTIONAL
            if (in_Gvr.DataFormat == GvrDataFormat.Index4 || in_Gvr.DataFormat == GvrDataFormat.Index8)
            {
                var bitmap2 = new WriteableBitmap(
                in_Gvr.Width, in_Gvr.Height,
                96, 96,
                pixelFormat,
                null
                );
                return bitmap2;
            }
            int bytesPerPixel = pixelFormat.BitsPerPixel / 8;
            int stride = in_Gvr.Width * bytesPerPixel;

            var bitmap = new WriteableBitmap(
                in_Gvr.Width, in_Gvr.Height,
                96, 96,
                pixelFormat,
                null
            );

            bitmap.WritePixels(
                new Int32Rect(0, 0, in_Gvr.Width, in_Gvr.Height),
                in_Gvr.Pixels,
                stride,
                0
            );
            //Flip vertically
            TransformedBitmap transformedBitmap = new TransformedBitmap();
            WriteableBitmap bmpClone = bitmap.Clone();
            transformedBitmap.BeginInit();
            transformedBitmap.Source = bmpClone;
            ScaleTransform transform = new ScaleTransform(1, -1, 0, 0);
            transformedBitmap.Transform = transform;
            transformedBitmap.EndInit();

            out_Pixels = new byte[stride * transformedBitmap.PixelHeight];

            transformedBitmap.CopyPixels(out_Pixels, stride, 0);

            return bitmap;
        }
        private unsafe void CreateTextureGvr(GVRFile gvr)
        {
            Width = gvr.Width;
            Height = gvr.Height;

            byte[] forGlTex = null;
            var bmp = LoadTga(gvr, ref forGlTex);
            if (bmp == null)
                return;

            fixed (byte* pBytes = forGlTex)
                GlTex = new GLTexture((nint)pBytes, Width, Height);
            ImageSource = bmp;
        }

        private unsafe void CreateTexture(byte[] bytes)
        {
            fixed (byte* pBytes = bytes)
                CreateTexture(TexHelper.Instance.LoadFromDDSMemory((nint)pBytes, bytes.Length, DDS_FLAGS.NONE));
        }

        private void CreateTexture(string filename)
        {
            CreateTexture(TexHelper.Instance.LoadFromDDSFile(filename, DDS_FLAGS.NONE));
        }

        private void CreateBitmap(ScratchImage img)
        {
            var bmp = BitmapConverter.FromTextureImage(img, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            ImageSource = BitmapConverter.FromBitmap(bmp);

            img.Dispose();
            bmp.Dispose();
        }

        public Texture(string filename, bool gvrTex = false) : this()
        {
            FullName = filename;
            if(string.IsNullOrEmpty(filename))
            {
                return;
            }
            Name = Path.GetFileNameWithoutExtension(filename);
            if (gvrTex)
            {
                GVRFile gVR = new GVRFile();
                gVR.LoadFromGvrFile(filename.ToLower());
                CreateTextureGvr(gVR);
            }
            else
                CreateTexture(filename);
        }

        public Texture(string name, byte[] bytes) : this()
        {
            FullName = name;
            Name = name;
            CreateTexture(bytes);
        }

        public Texture()
        {
            Name = FullName = "";
            Width = Height = 0;
            ImageSource = null;
            GlTex = null;

            Sprites = new ObservableCollection<int>();
        }
    }
}
