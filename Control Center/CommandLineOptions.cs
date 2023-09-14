//  Copyright 2014 Craig Courtney
//    
//  Helios is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  Helios is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.

namespace GadrocsWorkshop.Helios.ControlCenter
{
    using CommandLine;
    using CommandLine.Text;
    using System.Collections.Generic;
    using System.Linq;

    class CommandLineOptions : Util.CommonCommandLineOptions
    {
        [Option('x', "exit", HelpText = "Kill a running Control Center")]
        public bool Exit { get; set; } = false;

        [Value(0, MetaName="[Profile]", HelpText = "Filename & path of the Profile to autostart")]
        public IEnumerable<string> Profiles
        {
            get;
            set;
        } = new List<string>();

        [Usage(ApplicationAlias = "\"Control Center.exe\"")] 
        public static IEnumerable<Example> Examples
        {
            get
            {
                yield return new Example("Running a Profile at startup", UnParserSettings.WithGroupSwitchesOnly(), new CommandLineOptions() { Profiles = new string[] { @"c:\temp\Helios Profiles\Wibble.hpf" } });
                yield return new Example("Running a Profile at startup from the active Helios Profiles directory", new CommandLineOptions() { Profiles = new string[] { @"Wobble.hpf" } });
                yield return new Example("Debug logging enabled", new CommandLineOptions() { LogLevel = LogLevel.Debug });
                yield return new Example("Using an alternate Helios data directory", new CommandLineOptions() { DocumentsName = "HeliosVR" });
            }
        }
    }
}
