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

        if (format == ImageFormat.CI8)
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
        }
        else if (format == ImageFormat.CMPR)
        {
            int cmprIndex = (int)imageDataaddress;
            for (int y = 0; y < height; y += 8)
            {
                int widthMod = (8 - (width % 8)) == 8 ? 0 : 8 - (width % 8);
                for (int x = 0; x < width + widthMod; x += 8)
                {
                    // 8x8 bytes, 4x4 sub-blocks
                    for (int row = 0; row < 8; row += 4)
                    {
                        for (int col = 0; col < 8; col += 4)
                        {
                            if (cmprIndex >= data.Length)
                            {
                                break;
                            }

                            ushort[] paletteData =
                            [
                                BitConverter.ToUInt16(data, cmprIndex),
                                BitConverter.ToUInt16(data, cmprIndex + 2),
                            ];
                            var palette = new SKColor[4];
                            for (int i = 0; i < paletteData.Length; i++)
                            {
                                palette[i] = new((byte)(((paletteData[i] >> 11) & 0x1F) * 0x08),
                                    (byte)(((paletteData[i] >> 5) & 0x3F) * 0x04),
                                    (byte)((paletteData[i] & 0x1F) * 0x08), 0xFF);
                            }

                            if (paletteData[0] > paletteData[1])
                            {
                                palette[2] = new((byte)((palette[0].Red * 2 + palette[1].Red) / 3),
                                    (byte)((palette[0].Green * 2 + palette[1].Green) / 3),
                                    (byte)((palette[0].Blue * 2 + palette[1].Blue) / 3), 0xFF);
                                palette[3] = new((byte)((palette[0].Red + palette[1].Red * 2) / 3),
                                    (byte)((palette[0].Green + palette[1].Green * 2) / 3),
                                    (byte)((palette[0].Blue + palette[1].Blue * 2) / 3), 0xFF);
                            }
                            else
                            {
                                palette[2] = new((byte)((palette[0].Red + palette[1].Red) / 2),
                                    (byte)((palette[0].Green + palette[1].Green) / 2),
                                    (byte)((palette[0].Blue + palette[1].Blue) / 2), 0xFF);
                                palette[3] = SKColors.Transparent;
                            }

                            cmprIndex += 4;
                            for (int subRow = 0; subRow < 4; subRow++)
                            {
                                byte pixelData = data[cmprIndex++];
                                for (int subCol = 0; subCol < 4; subCol++)
                                {
                                    if (x + col + subCol >= width || y + row + subRow >= height)
                                    {
                                        continue;
                                    }

                                    bitmap.SetPixel(x + col + subCol, y + row + subRow,
                                        palette[(pixelData >> (2 * (3 - subCol))) & 0x03]);
                                }
                            }
                        }
                    }
                }
            }
        }
        else
        {
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
        if (format == ImageFormat.CI8)
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
                            int iPaletteIndex = int.Clamp(assignedPalette.Colors.IndexOf(color), 0, 255);
                            byte paletteindex = Convert.ToByte(iPaletteIndex);
                            data.Add(paletteindex);

                            //Data[ia8Index] = color.Alpha;
                           // Data[ia8Index + 1] = (byte)((color.Red / 3) + (color.Blue / 3) + (color.Green / 3));
                           ci8Index += 1;
                        }
                    }
                }
            }
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