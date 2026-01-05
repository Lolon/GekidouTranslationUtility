using HaruhiGekidouLib.TexturePalleteLibrary;
using SkiaSharp;

namespace HaruhiGekidouLib.TexturePaletteLibrary.ImageFormat;

public class ImageFormatBase
{
    public int blockWidth;
    public int blockHeight;

    public ImageFormatBase(byte[] data, int startAddress, SKBitmap image, ColorPalette? colPalette = null)
    {
        
    }

    public virtual byte[] GetBytes(SKBitmap bitmap, ColorPalette? colorPalette = null)
    {
        return null;
    }

}