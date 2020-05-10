using System;
using System.Collections.Generic;
using System.Windows;
using Newtonsoft.Json;

namespace GadrocsWorkshop.Helios.Patching.DCS
{
    /// <summary>
    /// A collection of viewports that is stored to a file for each previously
    /// configured Profile that uses MonitorSetup.  Merged these files is used to
    /// collect viewports from many Profiles into one Combined Monitor Setup.
    /// </summary>
    public class ViewportSetupFile : NotificationObject
    {
        [JsonProperty("Viewports")] 
        public Dictionary<string, Rect> Viewports { get; } = new Dictionary<string, Rect>();

        internal IEnumerable<StatusReportItem> Merge(string name, ViewportSetupFile from)
        {
            foreach (KeyValuePair<string, Rect> viewport in from.Viewports)
            {
                if (!Viewports.TryGetValue(viewport.Key, out Rect existingRect))
                {
                    // just copy it
                    Viewports.Add(viewport.Key, viewport.Value);
                    continue;
                }

                if (existingRect.Equals(viewport.Value))
                {
                    // no problem
                    continue;
                }

                // overwrite and warn
                Viewports[viewport.Key] = viewport.Value;
                yield return new StatusReportItem
                {
                    Status = $"profile '{name}' defines the viewport '{viewport.Key}' at a different screen location",
                    Recommendation =
                        $"Resolve viewport conflicts or do not include profile '{name}' in the combined monitor setup",
                    Severity = StatusReportItem.SeverityCode.Warning,
                    Link = StatusReportItem.ProfileEditor,
                    Flags = StatusReportItem.StatusFlags.ConfigurationUpToDate
                };
            }
        }

        internal void Clear()
        {
            Viewports.Clear();
        }
    }
}