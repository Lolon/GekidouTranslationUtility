using HaruhiGekidouLib.TexturePalleteLibrary;
using HaruhiGekidouLib.Util;
using SkiaSharp;

namespace HaruhiGekidouLib.TexturePaletteLibrary.ImageFormat;

public class IA8 : ImageFormatBase
{
    public IA8(byte[] data, int startAddress, SKBitmap image) : base(data, startAddress, image)
    {
        blockWidth = 4;
        blockHeight = 4;
        
        int width = image.Width;
        int height = image.Height;
        
        int Index = startAddress;
        int heightMod = (blockHeight - (height % blockHeight)) == blockHeight ? 0 : blockHeight - (width % blockHeight);
        for (int y = 0; y < height+heightMod; y += blockHeight) //each pixel row
        {
            int widthMod = (blockWidth - (width % blockWidth)) == blockWidth ? 0 : blockWidth - (width % blockWidth);
            for (int x = 0; x < width + widthMod; x += blockWidth)   //each pixel column
            {
                for (int row = 0; row < blockHeight; row++)
                {
                    for (int col = 0; col < blockWidth; col++)
                    {
                        if (Index + 2 >= data.Length || x + col >= width || y + row >= height)
                        {
                            Index += 2;
                            continue;
                        }

                        //byte colorByte = data[Index];
                        byte intensity = (byte) data[Index + 1];
                        byte alpha = (byte)data[Index];
                        SKColor color = new SKColor(intensity, intensity, intensity, alpha);

                        image.SetPixel(x + col, y + row, color);
                        Index += 2; 
                    }
                }
            }
        }
    }

    public override byte[] GetBytes(SKBitmap bitmap, ColorPalette? colorPalette)
    {
        List<byte> bytes = new List<byte>();
        int width = bitmap.Width;
        int height = bitmap.Height;
        
        int heightMod = (blockHeight - (height % blockHeight)) == blockHeight ? 0 : blockHeight - (width % blockHeight);
        for (int y = 0; y < height +heightMod; y += blockHeight) //each pixel row
        {
            int widthMod = (blockWidth - (width % blockWidth)) == blockWidth ? 0 : blockWidth - (width % blockWidth);
            for (int x = 0; x < width + widthMod; x += blockWidth)   //each pixel column
            {
                for (int row = 0; row < blockHeight; row++)
                {
                    for (int col = 0; col < blockWidth; col++)
                    {
                        if (x + col >= width || y + row >= height)
                        {
                            bytes.Add(0);
                            bytes.Add(0);
                            continue;
                        }

                        SKColor color = bitmap.GetPixel(x + col, y + row);
                        //alpha is the first byte
                        //color intensity is the second
                        byte intensity = (byte) (color.Red);
                        byte alpha = (byte)(color.Alpha);
                        bytes.Add(alpha);
                        bytes.Add(intensity);
                    }
                }
            }
        }   
        return bytes.ToArray();
    }
}