using System.Diagnostics;
using HaruhiGekidouLib.TexturePalleteLibrary;
using HaruhiGekidouLib.Util;
using SkiaSharp;

namespace HaruhiGekidouLib.TexturePaletteLibrary.ImageFormat;

public class IA4 : ImageFormatBase
{
    public IA4(byte[] data, int startAddress, SKBitmap image) : base(data, startAddress, image)
    {
        BlockWidth = 8;
        BlockHeight = 4;
        ValueSize = 1;
        
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
            ushort value = Convert.ToUInt16(data[scanIndex]);
            if (value != 0x0F)
            {
                Debug.WriteLine("here!");
            }
            byte intensity = (byte)((value & 0xF)*0x11);
            byte alpha = (byte)((value >> 4)*0x11);
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
                continue;
            }

            SKColor color = bitmap.GetPixel(x + blockColumn, y + blockRow);
            //alpha is the first half of the byte
            //color intensity is the second half of the byte
            byte intensity = (byte) (color.Red/0x11);
            byte alpha = (byte)((color.Alpha*0x11)<<4);
            byte result = (byte) (intensity + alpha);
            bytes.Add(result);
        }
        return bytes.ToArray();
    }
}
