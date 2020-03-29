using GadrocsWorkshop.Helios.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace GadrocsWorkshop.Helios.Patching.DCS
{
    [HeliosInterface("Patching.DCS.AdditionalViewports", "DCS Additional Viewports", typeof(AdditionalViewportsEditor), Factory = typeof(UniqueHeliosInterfaceFactory))]
    public class AdditionalViewports : HeliosInterface, IReadyCheck
    {
        public const string PATCH_SET = "Viewports";

        public AdditionalViewports() : base("DCS Additional Viewports")
        {
        }

        public IEnumerable<StatusReportItem> PerformReadyCheck()
        {
            // check if all our patches are installed
            List<IPatchDestination> destinations = InstallationLocations.Singleton.Items.Select(
                location => new DCSPatchDestination(location) as IPatchDestination).ToList();
            foreach (IPatchDestination destination in destinations)
            {
                PatchList patches = PatchList.LoadPatches(destination, "Viewports");
                if (patches.Count < 1)
                {
                    yield return new StatusReportItem
                    {
                        Status = "DCS Viewport patches not found in installation",
                        Recommendation = $"Please reinstall Helios to install these patches or provide them in documents folder",
                        Severity = StatusReportItem.SeverityCode.Error
                    };
                }
                IEnumerable<StatusReportItem> results = patches.Verify(destination);
                foreach (StatusReportItem result in results)
                {
                    yield return result;
                }
            }
        }

        public override void ReadXml(XmlReader reader)
        {
            // no code
        }

        public override void WriteXml(XmlWriter writer)
        {
            // no code
        }
    }
}
