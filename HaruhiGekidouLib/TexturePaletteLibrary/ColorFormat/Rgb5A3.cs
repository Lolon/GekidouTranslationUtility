using HaruhiGekidouLib.Util;
using SkiaSharp;

namespace HaruhiGekidouLib.TexturePalleteLibrary.ColorFormat;

public class Rgb5A3 : ColorFormatBase
{
    public override SKColor GetColorFrom(byte[] data, int offset)
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

    public override byte[] TurnColorInto(SKColor color)
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
}