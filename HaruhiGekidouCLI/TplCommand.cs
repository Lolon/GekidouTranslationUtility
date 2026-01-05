using AuroraLib.Core.IO;
using HaruhiGekidouLib.TexturePalleteLibrary;
using Mono.Options;


namespace HaruhiGekidouCLI;

public class TplCommand : Command
{
    private string _input = string.Empty, _output = string.Empty, _png = string.Empty;
    private bool _extract, _replace;

    public TplCommand() : base("tpl", "Various functions to deal with texture palette libraries")
    {
        Options = new()
        {
            { "x|extract", "Extracts all the images inside a tpl as pngs", _ => _extract = true },
            { "r|replace", "replaces an image inside a tpl with the input", _ => _replace = true },
            { "i|input=", "The path to the input tpl", i => _input = i },
            { "o|output=", "The path to the output tpl or png", o => _output = o },
            { "png|png=", "The png you wish to use to replace", n => _png = n },
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
                File.WriteAllBytes( _output,tpl.ToPng(0));
            }
        }
        else if (_replace)
        {
            TexturePaletteLibrary tpl = new(File.ReadAllBytes(_input));
            tpl.ImportPng(_png);
            File.WriteAllBytes( _output,tpl.GetBytes());
        }

        else
        {
            Options.WriteOptionDescriptions(CommandSet.Out);
        }

        return 0;
    }
}