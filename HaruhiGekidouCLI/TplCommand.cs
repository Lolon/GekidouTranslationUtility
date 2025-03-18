using AuroraLib.Core.IO;
using HaruhiGekidouLib.TexturePalleteLibrary;
using Mono.Options;


namespace HaruhiGekidouCLI;

public class TplCommand : Command
{
    private string _input = string.Empty, _output = string.Empty, _imgIndex = string.Empty;
    private bool _extract, _replace;

    public TplCommand() : base("tpl", "Various functions to deal with texture palette libraries")
    {
        Options = new()
        {
            { "x|extract", "Extracts all the images inside a tpl as pngs", _ => _extract = true },
            { "r|replace", "replaces an image inside a tpl with the input", _ => _replace = true },
            { "i|input=", "The path to the input tpl or png", i => _input = i },
            { "o|output=", "The path to the output tpl, png, or CSV", o => _output = o },
            { "n|index=", "The index of the image inside the tpl file to replace", n => _imgIndex = n },
        };
    }

    public override int Invoke(IEnumerable<string> arguments)
    {
        Options.Parse(arguments);

        if (!Directory.Exists(Path.GetDirectoryName(_output)))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_output)!);
        }

        if (_extract)
        {
            TexturePaletteLibrary tpl = new(File.ReadAllBytes(_input));
            if (tpl.Images.Count >0)
            {
                File.WriteAllBytes( _output,tpl.toPNG(0));
            }
        }
        


        else
        {
            Options.WriteOptionDescriptions(CommandSet.Out);
        }

        return 0;
    }
}