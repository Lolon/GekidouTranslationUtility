using System.Diagnostics;
using SkiaSharp;
using HaruhiGekidouLib.Util;


namespace HaruhiGekidouLib.TexturePalleteLibrary;


public enum ImageFormat
{
    I4=0,
    I8=1,
    IA4=2,
    IA8=3,
    RGB565=4,
    RGB5A3=5,
    RGBA32=6,
    CI4=8,
    CI8=9,  //implemented
    C14X2=10,
    CMPR=14 //implemented, needs testing
    
}

public class Image
{
    public SKBitmap bitmap;
    public ColorPalette assignedPalette;
    public int width;
    public int height;
    public ImageFormat format;
    public uint imageDataaddress;
    int wrapS;
    int wrapT;
    int minFilter;
    int maxFilter;
    float lodBias;
    int edgeLodEnable;
    int minLod;
    int maxLod;
    int imageUnpacked;



    public Image(byte[] data, int imageHeaderOffset, ColorPalette colPalette)
    {
        assignedPalette = colPalette;
        height = IO.ReadUShort(data, imageHeaderOffset);
        width = IO.ReadUShort(data, imageHeaderOffset + 0x02);
        format = (ImageFormat)IO.ReadInt(data, imageHeaderOffset + 0x04);
        imageDataaddress = IO.ReadUInt(data, imageHeaderOffset + 0x08);
        wrapS = IO.ReadInt(data, imageHeaderOffset + 0x0C);
        wrapT = IO.ReadInt(data, imageHeaderOffset + 0x10);
        minFilter = IO.ReadInt(data, imageHeaderOffset + 0x14);
        maxFilter = IO.ReadInt(data, imageHeaderOffset + 0x18);
        lodBias = IO.ReadFloat(data, imageHeaderOffset + 0x1C);
        edgeLodEnable = data[imageHeaderOffset + 0x20];
        minLod = data[imageHeaderOffset + 0x21];
        maxLod = data[imageHeaderOffset + 0x22];
        imageUnpacked = data[imageHeaderOffset + 0x23];


        bitmap = new SKBitmap((int)width, (int)height);

        switch (format)
        {
            case ImageFormat.CI8:
            {
                int ci8Index = (int)imageDataaddress;
                for (int y = 0; y < height; y += 4) //each pixel row
                {
                    int widthMod = (8 - (width % 8)) == 8 ? 0 : 8 - (width % 8);
                    for (int x = 0; x < width + widthMod; x += 8)   //each pixel column
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
                                int colIndex = colorByte;
                                SKColor color = colPalette.Colors[colIndex];

                            bitmap.SetPixel(x + col, y + row, color);
                                ci8Index += 1; 
                            }
                        }
                    }
                }

                break;
            }
            default:
                throw new Exception("Image format "+ format.ToString() + " not supported");
        }
    }

    public byte[] GetBytes(int startAdress)
    {
        List<byte> bytes = new List<byte>();
        
        bytes.AddRange(IO.GetUShortBytes((ushort)height));
        bytes.AddRange(IO.GetUShortBytes((ushort)width));
        bytes.AddRange(IO.GetUIntBytes((uint)format));
        
        List<byte> data = new List<byte>();
        switch (format)
        {
            case ImageFormat.CI8:
            {
                int ci8Index = 0;
                int heightMod = (4 - (height % 4)) == 4 ? 0 : 4 - (width % 4);
                for (int y = 0; y < height+ heightMod; y += 4) //each pixel row
                {
                    int widthMod = (8 - (width % 8)) == 8 ? 0 : 8 - (width % 8);
                    for (int x = 0; x < width + widthMod; x += 8)   //each pixel column
                    {
                        for (int row = 0; row < 4; row++)
                        {
                            for (int col = 0; col < 8; col++)
                            {
                                if (x + col >= width || y + row >= height)
                                {
                                    ci8Index += 1;
                                    continue;
                                }

                                SKColor color = bitmap.GetPixel(x + col, y + row);
                                int iPaletteIndex = assignedPalette.Colors.IndexOf(color);
                                if (iPaletteIndex == -1)
                                {
                                    PnnQuantizer quantizer = new PnnQuantizer();
                                    iPaletteIndex = quantizer.DitherColorIndex(assignedPalette.Colors.ToArray(), (uint)color, ci8Index);
                                    if (iPaletteIndex == -1)
                                    {
                                        throw new InvalidDataException("no color found for " + color.ToString() +
                                                                       " in palette!");
                                    }
                                }
                                byte paletteindex = Convert.ToByte(iPaletteIndex);
                                data.Add(paletteindex);

                                //Data[ia8Index] = color.Alpha;
                                // Data[ia8Index + 1] = (byte)((color.Red / 3) + (color.Blue / 3) + (color.Green / 3));
                                ci8Index += 1;
                            }
                        }
                    }
                }

                break;
            }
            default:
                throw new Exception("Image format "+ format.ToString() + " not supported");
        }

        List<byte> restOfHeader = new List<byte>();
        
        restOfHeader.AddRange(IO.GetIntBytes(wrapS));
        restOfHeader.AddRange(IO.GetIntBytes(wrapT));
        restOfHeader.AddRange(IO.GetIntBytes(minFilter));
        restOfHeader.AddRange(IO.GetIntBytes(maxFilter));
        restOfHeader.AddRange(IO.GetFloatBytes(lodBias));
        restOfHeader.AddRange(IO.GetUShortBytes((ushort)minLod));
        restOfHeader.AddRange(IO.GetUShortBytes((ushort)maxLod));
        restOfHeader.AddRange(new byte[(32 - ((startAdress+bytes.Count + restOfHeader.Count+2) % 32)-2)]); //pads to the next 32 bytes?
        
        bytes.AddRange(IO.GetIntBytes(startAdress+bytes.Count+restOfHeader.Count+4));
        bytes.AddRange(restOfHeader.ToArray());
        
        bytes.AddRange(data.ToArray());
        return [..bytes];
    }

    public byte[] ReplaceImageData(SKBitmap newBitmap)
    {
       //quick check to make sure the dimensions are the same
       if (bitmap.Width != width && bitmap.Height != height)
       {
           throw new Exception("replacement image dimensions do not match, should be "  + width + ", " + height);
           return null;
       }
       bitmap = newBitmap;
       
       return [];
    }
}