using System.Drawing;
using HaruhiGekidouLib.TexturePalleteLibrary.ColorFormat;
using HaruhiGekidouLib.Util;
using SkiaSharp;

namespace HaruhiGekidouLib.TexturePalleteLibrary;

public enum EPaletteFormat
{
    IA8 = 0,
    RGB565 = 1,
    RGB5A3 = 2, //implemented
}


public class ColorPalette
{

    public EPaletteFormat Format;
    public ColorFormatBase ColorFormat;
    public short NumColors;
    public List<SKColor>  Colors { get; set; } = [];
    
    public ColorPalette(byte[] data, int paletteDataAddress, int format, int numColors)
    {
        NumColors = Convert.ToInt16(numColors);
        Format= (EPaletteFormat)format;
        switch (Format)
        {
            case EPaletteFormat.RGB5A3:
            {
                ColorFormat = new Rgb5A3();
                for (int i = 0; i < NumColors; i++)
                {
                    Colors.Add(this.ColorFormat.GetColorFrom(data,paletteDataAddress + (i * 2)));
                }

                break;
            }
            default:
                throw new Exception("Palette format "+Format.ToString()+" not supported");
        }
        
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
            switch (Format)
            {
                case EPaletteFormat.RGB5A3:
                    bytes.AddRange(this.ColorFormat.TurnColorInto(c));
                    break;
            
            default:
                throw new Exception("Palette format "+Format.ToString()+" not supported");
            }
        }
        
        return [..bytes];
    }
}