using Newtonsoft.Json;

namespace ToolsCommon
{
    /// <summary>
    /// This is a loader for Jabbers' JSON format for viewport configuration information.  It must be kept compatible with 
    /// https://github.com/jeffboulanger/DCS-Alternative-Launcher/blob/master/DCS.Alternative.Launcher/DomainObjects/Viewport.cs
    /// </summary>
    public class Viewport
    {
        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("viewportName")]
        public string ViewportName { get; set; }

        /*
        [JsonProperty("monitorId")]
        public string MonitorId { get; set; }

        [JsonProperty("seatIndex")]
        public int SeatIndex { get; set; }
        */

        [JsonProperty("relativeInitFilePath")]
        public string RelativeInitFilePath { get; set; }

        [JsonProperty("originalDisplayWidth")]
        public int OriginalDisplayWidth { get; set; }

        [JsonProperty("originalDisplayHeight")]
        public int OriginalDisplayHeight { get; set; }

        [JsonProperty("x")]
        public int X { get; set; }

        [JsonProperty("y")]
        public int Y { get; set; }

        [JsonProperty("width")]
        public int Width { get; set; }

        [JsonProperty("height")]
        public int Height { get; set; }

        [JsonIgnore]
        public bool IsValid => (RelativeInitFilePath != null) && (RelativeInitFilePath.Length > 0);
    }
}
