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
        
        int Index = startAddress;
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

    public override byte[] GetBytes(SKBitmap bitmap, ColorPalette? colorPalette = null)
    {
        List<byte> bytes = [];
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
                        if (colorPalette != null)
                        {
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

                            byte paletteIndex = Convert.ToByte(iPaletteIndex);
                            bytes.Add(paletteIndex);
                        }

                        index += 1;
                    }
                }
            }
        }   
        return bytes.ToArray();
    }
}