using HaruhiGekidouLib.TexturePalleteLibrary;
using HaruhiGekidouLib.Util;
using SkiaSharp;

namespace HaruhiGekidouLib.TexturePaletteLibrary.ImageFormat;

public class CI4 : ImageFormatBase
{
    public CI4(byte[] data, int startAddress, ColorPalette colPalette, SKBitmap image) : base(data, startAddress, image, colPalette)
    {
        BlockWidth = 8;
        BlockHeight = 8;
        
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

            byte colorByte = data[scanIndex];
            int colIndex = colorByte;
            SKColor color = colPalette.Colors[colIndex];

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
                scanIndex += ValueSize;
                continue;
            }

            SKColor color = bitmap.GetPixel(x + blockColumn, y + blockRow);
            int iPaletteIndex = colorPalette.Colors.IndexOf(color);
            if (iPaletteIndex == -1)
            {
                PnnQuantizer quantizer = new PnnQuantizer();
                iPaletteIndex = quantizer.DitherColorIndex(colorPalette.Colors.ToArray(), (uint)color,
                    scanIndex);
                if (iPaletteIndex == -1)
                {
                    throw new InvalidDataException(
                        "no color found for " + color.ToString() + " in palette!");
                }
            }

            byte paletteindex = Convert.ToByte(iPaletteIndex);
            bytes.Add(paletteindex);
                        
            scanIndex += ValueSize;
        }
        return bytes.ToArray();
    }
}