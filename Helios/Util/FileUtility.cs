// Copyright 2021 Ammo Goettsch
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

using System.IO;
using System.Text;

namespace GadrocsWorkshop.Helios.Util
{
    public class FileUtility
    {
        /// <summary>
        /// replacement for ReadAllText that does not process or alter the text in the file in any way, so that
        /// files that are written by WriteFile can actually be read back to an identical string
        /// </summary>
        /// <param name="inputPath"></param>
        /// <returns></returns>
        public static string ReadFile(string inputPath)
        {
            using (StreamReader streamReader = new StreamReader(inputPath, new UTF8Encoding(false)))
            {
                return streamReader.ReadToEnd();
            }
        }

        /// <summary>
        /// replacement for WriteAllText that does not process or alter the text in the file in any way, so that
        /// files that are written by WriteFile can actually be read back to an identical string
        /// </summary>
        /// <param name="outputPath"></param>
        /// <param name="text"></param>
        public static void WriteFile(string outputPath, string text)
        {
            using (StreamWriter streamWriter = new StreamWriter(outputPath, false, new UTF8Encoding(false)))
            {
                streamWriter.Write(text);
            }
        }
    }
}