using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;

namespace AlligatorGh.Components.UI.PlugInManager.Wpf.Services
{
    /// <summary>
    /// Converts a GDI <see cref="Image"/> (the form the Grasshopper / category / generated icons
    /// arrive in) into a frozen WPF <see cref="BitmapSource"/> suitable for binding.
    /// </summary>
    /// <remarks>
    /// Uses a PNG stream round-trip rather than <c>Bitmap.GetHbitmap</c> + <c>DeleteObject</c>, so
    /// there is no GDI handle to leak. Icons are 16px, so the encode cost is negligible. The result
    /// is frozen (immutable, cross-thread safe) and meant to be converted once and cached on the row.
    /// </remarks>
    public static class ImageBridge
    {
        /// <summary>
        /// Converts a GDI image to a frozen WPF bitmap source.
        /// </summary>
        /// <param name="image">The source image; may be null.</param>
        /// <returns>A frozen <see cref="BitmapSource"/>, or null when the input is null.</returns>
        public static BitmapSource ToImageSource(Image image)
        {
            if (image == null)
            {
                return null;
            }

            using var stream = new MemoryStream();
            image.Save(stream, ImageFormat.Png);
            stream.Position = 0;

            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad; // fully load now so the stream can be disposed
            bitmap.StreamSource = stream;
            bitmap.EndInit();
            bitmap.Freeze();

            return bitmap;
        }
    }
}
