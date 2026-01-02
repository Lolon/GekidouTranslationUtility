using HaruhiGekidouLib.Util;
using SkiaSharp;

namespace HaruhiGekidouLib.TexturePalleteLibrary;



public class TexturePaletteLibrary
{
    public const uint Version = 0x20AF30;

    public List<ColorPalette> Palettes { get; set; } = [];
    public List<Image>  Images { get; set; } = [];
    
    
    public TexturePaletteLibrary(byte[] data)
    {
        //header
        if (IO.ReadInt(data, 0x00) != Version)
        {
            throw new Exception("Not a valid texture palette library");
        }
        int numImages = IO.ReadInt(data, 0x04);
        int imageTableOffset = IO.ReadInt(data, 0x08);
        
        //image offset table
        Dictionary<int, int> ImageOffsetTable = new Dictionary<int, int>();
        
        int imageTableEntryOffset = imageTableOffset;
        for (int i = 0; i < numImages; i++)
        {
            int imageHeaderOffset = IO.ReadInt(data, imageTableEntryOffset);
            int palleteHeaderOffset = IO.ReadInt(data, imageTableEntryOffset+0x04);
            ImageOffsetTable.Add(imageHeaderOffset, palleteHeaderOffset);
            imageTableEntryOffset += 0x08;
            
            //palette header, can be moved into class
            int paletteEntryCount = IO.ReadShort(data, palleteHeaderOffset);
            int paletteFormat = IO.ReadInt(data, palleteHeaderOffset + 0x04);
            int palleteDataAddress = IO.ReadInt(data, palleteHeaderOffset + 0x08);
            Palettes.Add(new ColorPalette(data, palleteDataAddress, paletteFormat, paletteEntryCount));

            Images.Add(new Image(data,imageHeaderOffset, Palettes[i]));
        }
        
    }

    public byte[] toPNG(int imageIndex)
    {
        SKImage image = SKImage.FromBitmap(Images[imageIndex].bitmap);
        return image.Encode(SKEncodedImageFormat.Png, 100).ToArray();
    }

    public byte[] GetBytes()
    {
        //tpl header
       List<byte> bytes = new List<byte>();
       bytes.AddRange(IO.GetUIntBytes(Version));
       bytes.AddRange(IO.GetIntBytes(Images.Count));
       bytes.AddRange(new byte[3]);
       bytes.Add(0x0c); //the table comes after the header, so 0c is assumed

       //image offset table
       List<byte> PaletteHD = new List<byte>();
       List<byte> ImageHD = new List<byte>();
       int ImagePointer = bytes.Count+(Images.Count * 8);
       for (int i = 0; i < Images.Count; i++)
       {
           PaletteHD.AddRange(Palettes[i].GetBytes());   //palette header and data
           ImageHD.AddRange(Images[i].GetBytes(ImagePointer+PaletteHD.Count)); //image header and data
           bytes.AddRange(IO.GetIntBytes(ImagePointer+PaletteHD.Count)); //offset to image header
           bytes.AddRange(IO.GetIntBytes(ImagePointer));    //offset to palette header
           ImagePointer += PaletteHD.Count+ImageHD.Count;
       }
        //palette and image data
       bytes.AddRange(PaletteHD);
       bytes.AddRange(ImageHD);
       
       
        return[..bytes];
    }

    public void ImportPng(string path)
    {
        SKBitmap replacement = SKBitmap.Decode(path);
        Images[0].ReplaceImageData(replacement); 
        
    }
}
