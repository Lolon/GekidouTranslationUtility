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
        
        int index = startAddress;
        int heightMod = (BlockHeight - (height % BlockHeight)) == BlockHeight ? 0 : BlockHeight - (width % BlockHeight);
        for (int y = 0; y < height+heightMod; y += BlockHeight) //each pixel row
        {
            int widthMod = (BlockWidth - (width % BlockWidth)) == BlockWidth ? 0 : BlockWidth - (width % BlockWidth);
            for (int x = 0; x < width + widthMod; x += BlockWidth)   //each pixel column
            {
                for (int row = 0; row < BlockHeight; row++)
                {
                    for (int col = 0; col < BlockWidth; col++)
                    {
                        if (index + 1 >= data.Length || x + col >= width || y + row >= height)
                        {
                            index += 1;
                            continue;
                        }

                        byte colorByte = data[index];
                        int colIndex = colorByte;
                        SKColor color = colPalette.Colors[colIndex];

                        image.SetPixel(x + col, y + row, color);
                        index += 1; 
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
        
        int index = 0;
        int heightMod = (BlockHeight - (height % BlockHeight)) == BlockHeight ? 0 : BlockHeight - (width % BlockHeight);
        for (int y = 0; y < height +heightMod; y += BlockHeight) //each pixel row
        {
            int widthMod = (BlockWidth - (width % BlockWidth)) == BlockWidth ? 0 : BlockWidth - (width % BlockWidth);
            for (int x = 0; x < width + widthMod; x += BlockWidth)   //each pixel column
            {
                for (int row = 0; row < BlockHeight; row++)
                {
                    for (int col = 0; col < BlockWidth; col++)
                    {
                        if (x + col >= width || y + row >= height)
                        {
                            bytes.Add(0);
                            index += 1;
                            continue;
                        }

                        SKColor color = bitmap.GetPixel(x + col, y + row);
                        int iPaletteIndex = colorPalette.Colors.IndexOf(color);
                        if (iPaletteIndex == -1)
                        {
                            PnnQuantizer quantizer = new PnnQuantizer();
                            iPaletteIndex = quantizer.DitherColorIndex(colorPalette.Colors.ToArray(), (uint)color,
                                index);
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
                        index += 1;
                    }
                }
            }
        }   
        return bytes.ToArray();
    }
}