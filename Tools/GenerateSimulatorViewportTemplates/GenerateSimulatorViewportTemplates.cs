using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;

namespace GenerateSimulatorViewportTemplates
{
    class GenerateSimulatorViewportTemplates
    {
        static void Main(string[] args)
        {
            if (args.Length < 3)
            {
                throw new System.Exception("JSON path, output path, and usesPatches boolean parameters are required");
            }
            string json = File.ReadAllText(args[0]);
            List<ToolsCommon.ViewportTemplate> templates = JsonConvert.DeserializeObject<ToolsCommon.ViewportTemplate[]>(json).ToList();
            Generate(templates, args[1], args[2]=="true");
        }

        private static readonly string[] _colors = new string[]
        {
            "DB2929",
            "D92762",
            "9545D8",
            "4C4CE0",
            "2F9FE0",
            "20BAA3",
            "1FC06F",
            "81D42F",
            "E3A322",
            "E05B2B",
            "598296",
            "596387"
        };

        private static void Generate(IList<ToolsCommon.ViewportTemplate> templates, string templatePath, bool usesPatches = false)
        {
            foreach (ToolsCommon.ViewportTemplate template in templates)
            {
                // assign a stable color based on the UI name of this viewport template
                int colorIndex = System.Math.Abs(template.TemplateDisplayName.GetHashCode()) % _colors.Length;

                // generate all valid viewports as templates
                foreach (ToolsCommon.Viewport viewport in template.Viewports.Where(v => v.IsValid || !usesPatches))
                {
                    List<string> lines = new List<string>();
                    string viewportName = viewport.ViewportName;
                    string category = "Simulator Viewports";
                    if (usesPatches)
                    {
                        viewportName = $"{template.ViewportPrefix}_{viewport.ViewportName}";
                        category = $"{template.TemplateCategory} Simulator Viewports";
                    }
                    lines.Add("<ControlTemplate>");
                    lines.Add($"    <Name>{template.DisplayName(viewport)}</Name>");
                    lines.Add($"    <Category>{category}</Category>");
                    lines.Add("    <TypeIdentifier>Helios.Base.ViewportExtent</TypeIdentifier>");
                    lines.Add("    <Template>");
                    lines.Add("        <TemplateValues>");
                    lines.Add("            <FillBackground>True</FillBackground>");
                    lines.Add($"            <BackgroundColor>#80{_colors[colorIndex]}</BackgroundColor>");
                    lines.Add("            <FontColor>#FFFFFFFF</FontColor>");
                    lines.Add("            <Font>");
                    lines.Add("                <FontFamily>Franklin Gothic</FontFamily>");
                    lines.Add("                <FontStyle>Normal</FontStyle>");
                    lines.Add("                <FontWeight>Normal</FontWeight>");
                    lines.Add("                <FontSize>12</FontSize>");
                    lines.Add("                <HorizontalAlignment>Center</HorizontalAlignment>");
                    lines.Add("                <VerticalAlignment>Center</VerticalAlignment>");
                    lines.Add("                <Padding>");
                    lines.Add("                    <Left>0</Left>");
                    lines.Add("                    <Top>0</Top>");
                    lines.Add("                    <Right>0</Right>");
                    lines.Add("                    <Bottom>0</Bottom>");
                    lines.Add("                </Padding>");
                    lines.Add("            </Font>");
                    lines.Add($"            <Text>{viewportName}</Text>");
                    lines.Add($"            <Location>{viewport.X},{viewport.Y}</Location>");
                    int width = viewport.Width;
                    if (width < 1)
                    {
                        width = 200;
                    }
                    int height = viewport.Height;
                    if (height < 1)
                    {
                        height = 200;
                    }
                    lines.Add($"            <Size>{width},{height}</Size>");
                    lines.Add("            <Hidden>False</Hidden>");
                    lines.Add($"            <ViewportName>{viewportName}</ViewportName>");
                    if (usesPatches)
                    {
                        lines.Add("            <RequiresPatches>true</RequiresPatches>");
                    }
                    lines.Add("        </TemplateValues>");
                    lines.Add("    </Template>");
                    lines.Add("</ControlTemplate>");

                    string outputDirectoryPath = Path.Combine(templatePath, usesPatches ? "Additional Simulator Viewports" : "Simulator Viewports");
                    if (!Directory.Exists(outputDirectoryPath))
                    {
                        Directory.CreateDirectory(outputDirectoryPath);
                    }
                    File.WriteAllLines(Path.Combine(outputDirectoryPath, $"{viewportName}.htpl"), lines);
                }
            }
        }
    }
}
