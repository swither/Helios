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

namespace GadrocsWorkshop.Helios.Util
{
    using System;
    using System.IO;
    using System.Windows;

    /// <summary>
    /// Utility class to copy resources out of assemblies into the files system.
    /// </summary>
    public static class Resources
    {
        public static void CopyResourceFile(string resourceUri, string destinationPath)
        {
            using (StreamWriter outputFile = File.CreateText(destinationPath))
            {
                CopyResourceFile(resourceUri, outputFile);
            }
        }

        public static void CopyResourceFile(string resourceUri, TextWriter destination)
        {
            using (Stream resourceStream = CreateResourceStream(resourceUri))
            using (StreamReader reader = new StreamReader(resourceStream))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    destination.WriteLine(line);
                }
            }
        }

        public static string ReadResourceFile(string resourceUri)
        {
            using (Stream resourceStream = CreateResourceStream(resourceUri))
            using (StreamReader reader = new StreamReader(resourceStream))
            {
                return reader.ReadToEnd();
            }
        }

        private static Stream CreateResourceStream(string resourceUri)
        {
            // WARNING: creating a pack URI sometimes fails with an "invalid port" error when running in a minimal environment
            Uri uri = new Uri(resourceUri, UriKind.Absolute);
            return Application.GetResourceStream(uri)?.Stream;
        }
    }
}
