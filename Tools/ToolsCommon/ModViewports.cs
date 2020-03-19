using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ToolsCommon
{
    public class ModViewports
    {
        public static IList<ViewportTemplate> Known { get; private set; }

        static ModViewports()
        {
            string json = File.ReadAllText("Data/Viewports/ViewportTemplates.json");
            Known = JsonConvert.DeserializeObject<ToolsCommon.ViewportTemplate[]>(json).ToList();
        }
    }
}
