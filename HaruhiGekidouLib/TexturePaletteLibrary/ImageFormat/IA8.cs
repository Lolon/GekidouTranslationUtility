using HaruhiGekidouLib.TexturePalleteLibrary;
using HaruhiGekidouLib.Util;
using SkiaSharp;

namespace HaruhiGekidouLib.TexturePaletteLibrary.ImageFormat;

public class IA8 : ImageFormatBase
{
    public IA8(byte[] data, int startAddress, SKBitmap image) : base(data, startAddress, image)
    {
        BlockWidth = 4;
        BlockHeight = 4;
        ValueSize = 2;
        
        int width = image.Width;
        int height = image.Height;
        
        scanIndex = startAddress;
        while (LinearProcess(width, height))
        {
            if (scanIndex + ValueSize >= data.Length || x + blockColumn >= width || y + blockRow >= height)
            {
                scanIndex += ValueSize;
                continue;
            }

            //byte colorByte = data[Index];
            byte intensity = (byte) data[scanIndex + 1];
            byte alpha = (byte)data[scanIndex];
            SKColor color = new SKColor(intensity, intensity, intensity, alpha);

            image.SetPixel(x + blockColumn, y + blockRow, color);
            scanIndex += ValueSize; 
        }
    }
    

    public override byte[] GetBytes(SKBitmap bitmap, ColorPalette? colorPalette)
    {
        List<byte> bytes = new List<byte>();
        int width = bitmap.Width;
        int height = bitmap.Height;

        while (LinearProcess(width, height))
        {
            if (x + blockColumn >= width || y + blockRow >= height)
            {
                bytes.Add(0);
                bytes.Add(0);
                continue;
            }

            SKColor color = bitmap.GetPixel(x + blockColumn, y + blockRow);
            //alpha is the first byte
            //color intensity is the second
            byte intensity = (byte) (color.Red);
            byte alpha = (byte)(color.Alpha);
            bytes.Add(alpha);
            bytes.Add(intensity);
        }
        return bytes.ToArray();
    }
}
