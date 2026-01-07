using HaruhiGekidouLib.TexturePalleteLibrary;
using SkiaSharp;

namespace HaruhiGekidouLib.TexturePaletteLibrary.ImageFormat;

public class ImageFormatBase
{
    public int BlockWidth;
    public int BlockHeight;
    public int ValueSize = 1;

    public ImageFormatBase(byte[] data, int startAddress, SKBitmap image, ColorPalette? colPalette = null)
    {
        
    }

    public virtual byte[] GetBytes(SKBitmap bitmap, ColorPalette? colorPalette = null)
    {
        return null;
    }

    public int x;
    public int y;
    public int blockColumn;
    public int blockRow;
    public int scanIndex;
    public bool LinearProcess(int width,int height)
    {
        int heightMod = (BlockHeight - (height % BlockHeight)) == BlockHeight ? 0 : BlockHeight - (width % BlockHeight);
        while (y < height + heightMod) //each pixel row
        {
            int widthMod = (BlockWidth - (width % BlockWidth)) == BlockWidth ? 0 : BlockWidth - (width % BlockWidth);
            while (x < width + widthMod)   //each pixel column
            {
                while ( blockRow < BlockHeight)
                {
                    if (blockColumn < BlockWidth)
                    {
                        blockColumn++;
                        return true;
                    }

                    blockColumn = 0;
                    blockRow++;
                }
                blockColumn = 0;
                blockRow = 0;
                x += BlockWidth;
            }
            blockColumn = 0;
            blockRow = 0;
            x = 0;
            y += BlockHeight;
        }
        
        x=0;
        y=0;
        blockColumn=0;
        blockRow=0;
        scanIndex=0;
        return false;
    }
    
    
}