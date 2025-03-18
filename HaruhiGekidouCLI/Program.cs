using Mono.Options;

namespace HaruhiGekidouCLI;

class Program
{
    static void Main(string[] args)
    {
        CommandSet commands = new("HaruhiGekidouCLI")
        {
            new ArcCommand(),
            new AdvScriptCommand(),
            new TplCommand(),
        };

        commands.Run(args);
    }
}