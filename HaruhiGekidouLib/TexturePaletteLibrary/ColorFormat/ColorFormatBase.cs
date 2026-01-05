using SkiaSharp;

namespace HaruhiGekidouLib.TexturePalleteLibrary.ColorFormat;

public class ColorFormatBase
{
    public virtual SKColor GetColorFrom(byte[] data, int offset)
    {
        return new SKColor();
    }

    public virtual byte[] TurnColorInto(SKColor color)
    {
        return null;
    }
}