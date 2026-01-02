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

    public PaletteFormat Format;
    public short NumColors;
    public List<SKColor>  Colors { get; set; } = [];
    
    public ColorPalette(byte[] data, int paletteDataAddress, int format, int numColors)
    {
        NumColors = Convert.ToInt16(numColors);
        Format= (PaletteFormat)format;
        if (Format == PaletteFormat.RGB5A3)
        {
            for (int i = 0; i < NumColors; i++)
            {
                Colors.Add(getColourFromRgb5A3(data,paletteDataAddress + (i * 2)));
            }
        }
        else
        {
            throw new Exception("Palette format "+Format.ToString()+" not supported");
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

    public byte[] getRgb5A3FromColor(SKColor color)
    {
        List<byte> output = new List<byte>();
        if (color.Alpha != 255)
        {
            byte alphaComponent = (byte)(color.Alpha / 0x20);
            byte redComponent = (byte)(color.Red / 0x11);
            byte greenComponent = (byte)(color.Green / 0x11);
            byte blueComponent = (byte)(color.Blue / 0x11);

            output.Add((byte)((alphaComponent << 4) | redComponent));
            output.Add((byte)((greenComponent << 4) | blueComponent));
        }
        else
        {
            byte topBit = 0x80;
            byte redComponent = (byte)(color.Red / 0x08);
            byte greenComponent = (byte)(color.Green / 0x08);
            byte blueComponent = (byte)(color.Blue / 0x08);

            output.Add((byte)(topBit | (redComponent << 2) | (greenComponent >> 3)));
            output.Add((byte)((greenComponent & 0x07) << 5 | blueComponent));
        }
        return [..output];
    }

    public byte[] GetBytes()
    {
        List<byte> bytes = new List<byte>();
        bytes.AddRange(IO.GetShortBytes(NumColors));
        bytes.AddRange(new byte[0x02]);
        bytes.AddRange(IO.GetIntBytes((int)Format));
        bytes.AddRange(IO.GetIntBytes(32)); //unsure what this is rn

        foreach (SKColor c in Colors)
        {
            if (Format == PaletteFormat.RGB5A3)
            {
                bytes.AddRange(getRgb5A3FromColor(c));
            }
        }
        
        return [..bytes];
    }
}