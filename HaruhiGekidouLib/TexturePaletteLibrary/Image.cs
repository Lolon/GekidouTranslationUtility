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
    IA8=3,  //implemented
    RGB565=4,
    RGB5A3=5,
    RGBA32=6,
    CI4=8,  //implemented, needs testing
    CI8=9,  //implemented
    C14X2=10,
    CMPR=14 
    
}

public class Image
{
    public SKBitmap Bitmap;
    public ColorPalette AssignedPalette;
    public ImageFormatBase ImageFormat;
    public int Width;
    public int Height;
    public EImageFormat Format;
    public uint ImageDataAddress;
    public int WrapS;
    public int WrapT;
    public int MinFilter;
    public int MaxFilter;
    public float LodBias;
    public int EdgeLodEnable;
    public int MinLod;
    public int MaxLod;
    public int ImageUnpacked;

    


    public Image(byte[] data, int imageHeaderOffset, ColorPalette? colPalette = null)
    {
        AssignedPalette = colPalette;
        Height = IO.ReadUShort(data, imageHeaderOffset);
        Width = IO.ReadUShort(data, imageHeaderOffset + 0x02);
        Format = (EImageFormat)IO.ReadInt(data, imageHeaderOffset + 0x04);
        ImageDataAddress = IO.ReadUInt(data, imageHeaderOffset + 0x08);
        WrapS = IO.ReadInt(data, imageHeaderOffset + 0x0C);
        WrapT = IO.ReadInt(data, imageHeaderOffset + 0x10);
        MinFilter = IO.ReadInt(data, imageHeaderOffset + 0x14);
        MaxFilter = IO.ReadInt(data, imageHeaderOffset + 0x18);
        LodBias = IO.ReadFloat(data, imageHeaderOffset + 0x1C);
        EdgeLodEnable = data[imageHeaderOffset + 0x20];
        MinLod = data[imageHeaderOffset + 0x21];
        MaxLod = data[imageHeaderOffset + 0x22];
        ImageUnpacked = data[imageHeaderOffset + 0x23];


        Bitmap = new SKBitmap((int)Width, (int)Height);

        switch (Format)
        {
            case EImageFormat.IA8:
            {
                ImageFormat = new IA8(data,(int)ImageDataAddress,Bitmap);
                break;
            }
            case EImageFormat.CI8:
            {
                ImageFormat = new CI8(data, (int)ImageDataAddress, AssignedPalette,Bitmap);
                break;
            }
            case EImageFormat.CI4:
            {
                ImageFormat = new CI4(data, (int)ImageDataAddress, AssignedPalette,Bitmap);
                break;
            }
            default:
                throw new Exception("Image format "+ Format.ToString() + " not supported");
        }
    }

    public byte[] GetBytes(int startAdress)
    {
        List<byte> bytes = new List<byte>();
        
        bytes.AddRange(IO.GetUShortBytes((ushort)Height));
        bytes.AddRange(IO.GetUShortBytes((ushort)Width));
        bytes.AddRange(IO.GetUIntBytes((uint)Format));
        
        List<byte> data = new List<byte>();

        byte[] formatBytes = ImageFormat.GetBytes(Bitmap, AssignedPalette);
        data.AddRange(formatBytes);

        List<byte> restOfHeader = new List<byte>();
        
        restOfHeader.AddRange(IO.GetIntBytes(WrapS));
        restOfHeader.AddRange(IO.GetIntBytes(WrapT));
        restOfHeader.AddRange(IO.GetIntBytes(MinFilter));
        restOfHeader.AddRange(IO.GetIntBytes(MaxFilter));
        restOfHeader.AddRange(IO.GetFloatBytes(LodBias));
        restOfHeader.AddRange(IO.GetUShortBytes((ushort)MinLod));
        restOfHeader.AddRange(IO.GetUShortBytes((ushort)MaxLod));
        restOfHeader.AddRange(new byte[(32 - ((startAdress+bytes.Count + restOfHeader.Count+2) % 32)-2)]); //pads to the next 32 bytes?
        
        bytes.AddRange(IO.GetIntBytes(startAdress+bytes.Count+restOfHeader.Count+4));
        bytes.AddRange(restOfHeader.ToArray());
        
        bytes.AddRange(data.ToArray());
        return [..bytes];
    }

    public byte[] ReplaceImageData(SKBitmap newBitmap)
    {
       //quick check to make sure the dimensions are the same
       if (Bitmap.Width != Width && Bitmap.Height != Height)
       {
           throw new Exception("replacement image dimensions do not match, should be "  + Width + ", " + Height);
           return null;
       }
       Bitmap = newBitmap;
       return [];
    }
}