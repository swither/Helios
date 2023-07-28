// Copyright 2023 Helios Contributors

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

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;


namespace GenerateImageTemplates
{
    internal class GenerateImageTemplates
    {
        private static void Main(string[] args)
        {
            if (args.Length < 4)
            {
                throw new Exception("Input Image path, output template path, Resource path (e.g. {F-15E}), Template Category Name.");
            }
            Generate(args[0], args[1], args[2], args[3]);
        }

        private static void Generate(string imagePath, string templatePath, string resourcePath, string TemplateCategory)
        {
            foreach (string imageFilePath in Directory.EnumerateFiles(Path.GetDirectoryName(imagePath),"*.png", SearchOption.AllDirectories))
            {
                FileInfo fInfo = new FileInfo(imageFilePath);
                string imageName = Path.GetFileName(imageFilePath);
                Console.WriteLine($"reading {imageFilePath}");
                Bitmap img = new Bitmap(imageFilePath, true);
                Size imageSize = new Size(img.Width, img.Height);
                Console.WriteLine($"  generating {imageFilePath}");
                List<string> lines = new List<string>();
                lines.Add("<ControlTemplate>");
                lines.Add($"    <Name>{Path.GetFileNameWithoutExtension(imageFilePath).Replace('_',' ')} Image</Name>");
                lines.Add($"    <Category>{TemplateCategory}</Category>");
                lines.Add("    <TypeIdentifier>Helios.Base.Image</TypeIdentifier>");
                lines.Add("    <Template>");
                lines.Add("        <TemplateValues>");
                lines.Add($@"            <Image>{resourcePath}/{DirectoryDifference(Path.GetDirectoryName(imagePath), Path.GetDirectoryName(imageFilePath)).Replace('\\', '/')}{Path.GetFileName(imageFilePath)}</Image>");
                lines.Add("            <Alignment>Stretched</Alignment>"); 
                lines.Add("            <CornerRadius>0</CornerRadius>");
                lines.Add("            <Location>0,0</Location>");
                lines.Add($"            <Size>{imageSize.Width},{imageSize.Height}</Size>");
                lines.Add("            <Hidden>False</Hidden>");
                lines.Add("        </TemplateValues>");
                lines.Add("    </Template>");
                lines.Add("</ControlTemplate>");

                string outputDirectoryPath = templatePath;
                if (!Directory.Exists(outputDirectoryPath))
                {
                    Directory.CreateDirectory(outputDirectoryPath);
                }
                File.WriteAllLines(Path.Combine(outputDirectoryPath,Path.ChangeExtension(imageName,"htpl")), lines);
            }
        }
        private static string DirectoryDifference(string originalDirectoryPath, string fileDirectoryPath)
        {
            string result = "";
            string[] dirPieces = fileDirectoryPath.Split(Path.DirectorySeparatorChar);
            for(int i = originalDirectoryPath.Split(Path.DirectorySeparatorChar).Length;i< dirPieces.Length; i++)
            {
                result += dirPieces[i] + Path.DirectorySeparatorChar.ToString();
            }
            return result;
        }
    }
}