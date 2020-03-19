using Newtonsoft.Json;
using System.Collections.Generic;

namespace ToolsCommon
{
    /// <summary>
    /// This is a loader for Jabbers' JSON format for viewport templates.  It must be kep compatible with
    /// https://github.com/jeffboulanger/DCS-Alternative-Launcher/blob/master/DCS.Alternative.Launcher/DomainObjects/ModuleViewportTemplate.cs
    /// </summary>
    public class ViewportTemplate
    {
        private static readonly Dictionary<string, string> _moduleCategories = new Dictionary<string, string> {
            // make these fit in with template / control categories in Helios UI
            { "A-10C", "A-10" },
            { "F-16C", "F-16" },
            { "FA-18C", "F/A-18C" },
            { "HEATBLUR_F-14", "F-14" },
            { "M-2000C", "M2000C" },
            { "Mi-8MTV2", "Mi-8" },
            { "MIG-21bis", "MiG-21" }
        };

        [JsonProperty("templateName")]
        public string TemplateName { get; set; }

        [JsonProperty("isHeliosTemplate")]
        public bool IsHeliosTemplate { get; set; }

        [JsonProperty("moduleId")]
        public string ModuleId { get; set; }

        [JsonProperty("viewports")]
        public List<Viewport> Viewports { get; set; } = new List<Viewport>();

        [JsonIgnore]
        public string ViewportPrefix => ModuleId.Replace(" ", "_").Replace("-", "_");

        [JsonIgnore]
        public object TemplateCategory
        {
            get
            {
                if (_moduleCategories.TryGetValue(ModuleId, out string category))
                {
                    return category;
                }
                return ModuleId;
            }
        }

        public string DisplayName(Viewport viewport)
        {
            return $"{TemplateName.Replace("-", "").Replace("Helios ","")} {(viewport.Description ?? viewport.ViewportName).Replace("-", " ")}";
        }
    }
}
