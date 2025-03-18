using System.Drawing;
using HaruhiGekidouLib.Util;
using SkiaSharp;

namespace HaruhiGekidouLib.TexturePalleteLibrary;

public enum PaletteFormat
{
    IA8 = 0,
    RGB565 = 1,
    RGB5A3 = 2, //implemented
}


public class ColorPalette
{
    
    public List<SKColor>  Colors { get; set; } = [];
    
    public ColorPalette(byte[] data, int paletteDataAddress, int format, int numColors)
    {
        PaletteFormat paletteFormat = (PaletteFormat)format;
        if (paletteFormat == PaletteFormat.RGB5A3)
        {
            for (int i = 0; i < numColors; i++)
            {
                Colors.Add(getColourFromRgb5A3(data,paletteDataAddress + (i * 2)));
            }
        }
        
    }
    
    public SKColor getColourFromRgb5A3(byte[] data, int offset)
    {
        SKColor color = new SKColor();
        
        ushort colorData = IO.ReadUShort(data, offset);

        if (colorData >> 15 == 0)
        {
            color = new((byte)(((colorData >> 8) & 0x0F) * 0x11), (byte)(((colorData >> 4) & 0x0F) * 0x11), (byte)((colorData & 0x0F) * 0x11), (byte)(((colorData >> 12) & 0x07) * 0x20));
        }
        else
        {
            color = new((byte)(((colorData >> 10) & 0x1F) * 0x08), (byte)(((colorData >> 5) & 0x1F) * 0x08), (byte)((colorData & 0x1F) * 0x08), 0xFF);
        }
        
        return color;
    }
}