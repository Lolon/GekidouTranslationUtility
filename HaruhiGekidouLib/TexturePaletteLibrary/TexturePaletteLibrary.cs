using HaruhiGekidouLib.Util;
using SkiaSharp;

namespace HaruhiGekidouLib.TexturePalleteLibrary;



public class TexturePaletteLibrary
{
    private const uint Version = 0x20AF30;

    private List<ColorPalette> Palettes { get; set; } = [];
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
            int paletteHeaderOffset = IO.ReadInt(data, imageTableEntryOffset+0x04);
            ImageOffsetTable.Add(imageHeaderOffset, paletteHeaderOffset);
            imageTableEntryOffset += 0x08;

            if (paletteHeaderOffset != 0)
            {
                //palette header, can be moved into class
                
                Palettes.Add(new ColorPalette(data,paletteHeaderOffset));
                Images.Add(new Image(data,imageHeaderOffset, Palettes[i]));
            }
            else
            {
                Images.Add(new Image(data, imageHeaderOffset));
            }
        }
        
    }

    public byte[] ToPng(int imageIndex)
    {
        SKImage image = SKImage.FromBitmap(Images[imageIndex].Bitmap);
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
       
       List<byte> paletteHD = [];
       List<byte> imageHD = [];
       int imagePointer = bytes.Count+(Images.Count * 8);
       for (int i = 0; i < Images.Count; i++)
       {
           if (Palettes.Count > 0)
           {
               paletteHD.AddRange(Palettes[i].GetBytes()); //palette header and data
           }
           imageHD.AddRange(Images[i].GetBytes(imagePointer+paletteHD.Count)); //image header and data
           bytes.AddRange(IO.GetIntBytes(imagePointer+paletteHD.Count)); //offset to image header
           bytes.AddRange(Palettes.Count > 0 ? IO.GetIntBytes(imagePointer) : IO.GetIntBytes(0)); //offset to palette header
           imagePointer += paletteHD.Count+imageHD.Count;
       }
        //palette and image data
       bytes.AddRange(paletteHD);
       bytes.AddRange(imageHD);
       
       
        return[..bytes];
    }

    public void ImportPng(string path)
    {
        SKBitmap replacement = SKBitmap.Decode(path);
        Images[0].ReplaceImageData(replacement); 
    }
}
