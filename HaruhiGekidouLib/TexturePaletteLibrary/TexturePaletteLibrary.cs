using HaruhiGekidouLib.Util;
using SkiaSharp;

namespace HaruhiGekidouLib.TexturePalleteLibrary;



public class TexturePaletteLibrary
{
    public const uint Version = 0x20AF30;
    
    public List<Image>  Images { get; set; } = [];
    
    
    public TexturePaletteLibrary(byte[] data)
    {
        if (IO.ReadInt(data, 0x00) != Version)
        {
            throw new Exception("Not a valid texture palette library");
        }
        int numImages = IO.ReadInt(data, 0x04);
        int imageTableOffset = IO.ReadInt(data, 0x08);
        
        Dictionary<int, int> ImageOffsetTable = new Dictionary<int, int>();
        
        int imageTableEntryOffset = imageTableOffset;
        for (int i = 0; i < numImages; i++)
        {
            int imageHeaderOffset = IO.ReadInt(data, imageTableEntryOffset);
            int palleteHeaderOffset = IO.ReadInt(data, imageTableEntryOffset+0x04);
            ImageOffsetTable.Add(imageHeaderOffset, palleteHeaderOffset);
            imageTableEntryOffset += 0x08;

            Images.Add(new Image(data,imageHeaderOffset, palleteHeaderOffset));
        }
        
    }

    public byte[] toPNG(int imageIndex)
    {
        SKImage image = SKImage.FromBitmap(Images[imageIndex].bitmap);
        return image.Encode(SKEncodedImageFormat.Png, 100).ToArray();
    }
}

public class Image
{
    ColorPalette colorPalette;
    public SKBitmap bitmap;

    public Image(byte[] data, int imageHeaderOffset, int palleteHeaderOffset)
    {
        int paletteEntryCount = IO.ReadShort(data, palleteHeaderOffset);
        int paletteFormat = IO.ReadInt(data, palleteHeaderOffset + 0x04);
        int palleteDataAddress = IO.ReadInt(data, palleteHeaderOffset + 0x08);
        colorPalette = new ColorPalette(data, palleteDataAddress, paletteFormat, paletteEntryCount);


        int height = IO.ReadUShort(data, imageHeaderOffset);
        int width = IO.ReadUShort(data, imageHeaderOffset + 0x02);
        int imageFormat = IO.ReadInt(data, imageHeaderOffset + 0x04);
        int imageDataaddress = IO.ReadInt(data, imageHeaderOffset + 0x08);
        int wrapS = IO.ReadInt(data, imageHeaderOffset + 0x0C);
        int wrapT = IO.ReadInt(data, imageHeaderOffset + 0x10);
        int minFilter = IO.ReadInt(data, imageHeaderOffset + 0x14);
        int maxFilter = IO.ReadInt(data, imageHeaderOffset + 0x18);
        float lodBias = IO.ReadFloat(data, imageHeaderOffset + 0x1C);
        int edgeLodEnable = data[imageHeaderOffset + 0x20];
        int minLod = data[imageHeaderOffset + 0x21];
        int maxLod = data[imageHeaderOffset + 0x22];
        int imageUnpacked = data[imageHeaderOffset + 0x23];


        if (imageFormat == 9) //c8
        {
            bitmap = new SKBitmap(width, height);

            int ci8Index = imageDataaddress;
            for (int y = 0; y < height; y += 4)
            {
                int widthMod = (8 - (width % 8)) == 8 ? 0 : 8 - (width % 8);
                for (int x = 0; x < width + widthMod; x += 8)
                {
                    for (int row = 0; row < 4; row++)
                    {
                        for (int col = 0; col < 8; col++)
                        {
                            if (ci8Index + 1 >= data.Length || x + col >= width || y + row >= height)
                            {
                                ci8Index += 1;
                                continue;
                            }

                            byte colorByte = data[ci8Index];
                            ci8Index += 1; 
                            int colIndex = colorByte;
                            SKColor color = colorPalette.Colors[colIndex];

                            bitmap.SetPixel(x + col, y + row, color);
                        }
                    }
                }
            }
        }
    }
}
    
