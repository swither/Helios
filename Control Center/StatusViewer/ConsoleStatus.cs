using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GadrocsWorkshop.Helios.ControlCenter.StatusViewer
{
    public class ConsoleStatus
    {
        [JsonProperty("Name")] 
        public string Name => "Control Center Console";

        [JsonProperty("Report")]
        public IEnumerable<StatusReportItem> Report;

        public ConsoleStatus(IEnumerable<StatusReportItem> report)
        {
            Report = report;
        }
    }
}
