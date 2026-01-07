using System.Diagnostics;
using HaruhiGekidouLib.TexturePalleteLibrary;
using HaruhiGekidouLib.Util;
using SkiaSharp;

namespace HaruhiGekidouLib.TexturePaletteLibrary.ImageFormat;

public class CI8 : ImageFormatBase
{
    public CI8(byte[] data, int startAddress, ColorPalette colPalette, SKBitmap image) : base(data, startAddress, image, colPalette)
    {
        BlockWidth = 8;
        BlockHeight = 4;
        
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

    
    public override byte[] GetBytes(SKBitmap bitmap, ColorPalette? colorPalette = null)
    {
        List<byte> bytes = [];
        int width = bitmap.Width;
        int height = bitmap.Height;
        
        int heightMod = (BlockHeight - (height % BlockHeight)) == BlockHeight ? 0 : BlockHeight - (width % BlockHeight);
        while (LinearProcess(width, height))
        {
            if (x + blockColumn >= width || y + blockRow >= height)
            {
                bytes.Add(0);
                scanIndex += ValueSize;
                continue;
            }

            SKColor color = bitmap.GetPixel(x + blockColumn, y + blockRow);
            if (colorPalette != null)
            {
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

                byte paletteIndex = Convert.ToByte(iPaletteIndex);
                bytes.Add(paletteIndex);
            }

            scanIndex += ValueSize;
        }
        return bytes.ToArray();
    }
}

