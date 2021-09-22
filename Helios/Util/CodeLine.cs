// Copyright 2020 Ammo Goettsch
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
// 

using Newtonsoft.Json;

namespace GadrocsWorkshop.Helios.Util
{
    public enum CodeLineEnding
    {
        None,
        DOS,
        Unix
    }

    public class CodeLine
    {
        public CodeLine(string line)
        {
            if (line.EndsWith("\r\n"))
            {
                LineEnding = CodeLineEnding.DOS;
                Text = line.Substring(0, line.Length-2);
                return;
            }

            if (line.EndsWith("\n"))
            {
                LineEnding = CodeLineEnding.Unix;
                Text = line.Substring(0, line.Length - 1);
                return;
            }

            LineEnding = CodeLineEnding.None;
            Text = line;
        }

        public CodeLine(string text, CodeLineEnding lineEnding)
        {
            Text = text;
            LineEnding = lineEnding;
        }

        // conversion from string
        public static implicit operator CodeLine(string line) => new CodeLine(line);

        [JsonProperty("text")]
        public string Text { get; }

        [JsonProperty("lineEnding")]
        public CodeLineEnding LineEnding { get; }
    }
}