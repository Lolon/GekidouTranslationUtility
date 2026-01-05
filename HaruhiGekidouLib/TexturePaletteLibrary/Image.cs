using System.Diagnostics;
using HaruhiGekidouLib.TexturePaletteLibrary.ImageFormat;
using SkiaSharp;
using HaruhiGekidouLib.Util;


namespace HaruhiGekidouLib.TexturePalleteLibrary;


public enum EImageFormat
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
    public ImageFormatBase ImageFormat;
    public int width;
    public int height;
    public EImageFormat format;
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

    


    public Image(byte[] data, int imageHeaderOffset, ColorPalette? colPalette = null)
    {
        assignedPalette = colPalette;
        height = IO.ReadUShort(data, imageHeaderOffset);
        width = IO.ReadUShort(data, imageHeaderOffset + 0x02);
        format = (EImageFormat)IO.ReadInt(data, imageHeaderOffset + 0x04);
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
            case EImageFormat.IA8:
            {
                ImageFormat = new IA8(data,(int)imageDataaddress,bitmap);
                break;
            }
            case EImageFormat.CI8:
            {
                ImageFormat = new CI8(data, (int)imageDataaddress, assignedPalette,bitmap);
                break;
            }
            case EImageFormat.CI4:
            {
                ImageFormat = new CI4(data, (int)imageDataaddress, assignedPalette,bitmap);
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

        byte[] formatBytes = ImageFormat.GetBytes(bitmap, assignedPalette);
        data.AddRange(formatBytes);

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