using HaruhiGekidouLib.TexturePalleteLibrary;
using HaruhiGekidouLib.Util;
using SkiaSharp;

namespace HaruhiGekidouLib.TexturePaletteLibrary.ImageFormat;

public class CI8 : ImageFormatBase
{
    public CI8(byte[] data, int startAddress, ColorPalette colPalette, SKBitmap image) : base(data, startAddress, image, colPalette)
    {
        blockWidth = 8;
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
                        if (Index + 1 >= data.Length || x + col >= width || y + row >= height)
                        {
                            Index += 1;
                            continue;
                        }

                        byte colorByte = data[Index];
                        int colIndex = colorByte;
                        SKColor color = colPalette.Colors[colIndex];

                        image.SetPixel(x + col, y + row, color);
                        Index += 1; 
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
        
        int Index = 0;
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
                            Index += 1;
                            continue;
                        }

                        SKColor color = bitmap.GetPixel(x + col, y + row);
                        int iPaletteIndex = colorPalette.Colors.IndexOf(color);
                        if (iPaletteIndex == -1)
                        {
                            PnnQuantizer quantizer = new PnnQuantizer();
                            iPaletteIndex = quantizer.DitherColorIndex(colorPalette.Colors.ToArray(), (uint)color,
                                Index);
                            if (iPaletteIndex == -1)
                            {
                                throw new InvalidDataException(
                                    "no color found for " + color.ToString() + " in palette!");
                            }
                        }

                        byte paletteindex = Convert.ToByte(iPaletteIndex);
                        bytes.Add(paletteindex);

                        //Data[ia8Index] = color.Alpha;
                        // Data[ia8Index + 1] = (byte)((color.Red / 3) + (color.Blue / 3) + (color.Green / 3));
                        Index += 1;
                    }
                }
            }
        }   
        return bytes.ToArray();
    }
}