using DirectXTexNet;
using System.Drawing;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows;

public static class BitmapConverter
{
    public static BitmapSource FromBitmap(Bitmap bitmap)
    {
        BitmapSource i = Imaging.CreateBitmapSourceFromHBitmap(
                       bitmap.GetHbitmap(),
                       nint.Zero,
                       Int32Rect.Empty,
                       BitmapSizeOptions.FromEmptyOptions());
        return i;
    }

    public static Bitmap FromTextureImage(ScratchImage img, System.Drawing.Imaging.PixelFormat format)
    {
        return new Bitmap(img.GetImage(0).Width, img.GetImage(0).Height,
            (int)img.GetImage(0).RowPitch, format, img.GetImage(0).Pixels);
    }
}