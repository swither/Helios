using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ToolsCommon
{
    public class StandardViewports
    {
        public static IList<ViewportTemplate> Known { get; private set; }

        static StandardViewports()
        {
            string json = File.ReadAllText("Data/Viewports/ExistingViewportTemplates.json");
            Known = JsonConvert.DeserializeObject<ToolsCommon.ViewportTemplate[]>(json).ToList();
        }
    }
}
