using CommandLine;
using CommandLine.Text;
using System.Collections.Generic;
using System.Windows;

namespace GadrocsWorkshop.Helios.Util
{
    /// <summary>
    /// command line options common to Helios programs
    /// </summary>
    public class CommandLineOptions
    {
        [Option('l', "loglevel", Required = false, Default = LogLevel.Info,
            HelpText = "Set log level [Debug, Info, Warning, Error].")]
        public LogLevel LogLevel { get; set; } = LogLevel.Info;

        [Option('d', "documents", Required = false, Default = "Helios", HelpText = "Set the Documents folder name to use.")]
        public string DocumentPath { get; set; } = "Helios";

        [Option('e', "devdocuments", Required = false, Default = "HeliosDev", HelpText = "Set the Documents folder name to use for a Development Prototype build.")]
        public string DevDocumentPath { get; set; } = "HeliosDev";

        public static T Parse<T> (T defaults, string[] args, out int exitCode) where T: CommandLineOptions
        {
            T options = defaults;

            // parse command line: we can't use the simple form of this because we need to display
            // help ourselves as the library just prints help to the console which we don't see
            ParserResult<T> result = Parser.Default.ParseArguments<T>(args);
            exitCode = result.MapResult((success) =>
                {
                    options = success;
                    return 0;
                },
                (errors) => HandleCommandLineErrors(result, errors));
            return options;
        }

        public static int HandleCommandLineErrors<T>(ParserResult<T> result, IEnumerable<Error> _) where T : CommandLineOptions
        {
            // WARNING: this is broken in the command line utility.  It prints help automatically
            // to a console we can't see and then returns a parser error instead of a IsHelp item
            // so we just treat all parser errors the same and just show the help
            MessageBox.Show(HelpText.AutoBuild(result), "Command Line Options");
            return -1;
        }
    }
}