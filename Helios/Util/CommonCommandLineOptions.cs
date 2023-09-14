// Copyright 2014 Craig Courtney
// Copyright 2020 Helios Contributors
// 
// Helios is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// Helios is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using CommandLine;

namespace GadrocsWorkshop.Helios.Util
{
    /// <summary>
    /// command line options common to Helios programs
    /// </summary>
    public class CommonCommandLineOptions: CommandLineOptions
    {
        [Option('l', "loglevel", Required = false, Default = LogLevel.Info,
            HelpText = "Set logging level [Debug, Info, Warning, Error].")]
        public LogLevel LogLevel { get; set; } = LogLevel.Info;

        [Option('d', "documents", Required = false, Default = "Helios",
            HelpText = "Set the Documents folder name to use.")]
        public string DocumentsName { get; set; } = "Helios";

        [Option('e', "devdocuments", Required = false, Default = "HeliosDev",
            HelpText = "Set the Documents folder name to use for a Development Prototype build.")]
        public string DevDocumentsName { get; set; } = "HeliosDev";

    }
}