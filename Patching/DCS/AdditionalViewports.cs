using GadrocsWorkshop.Helios.ComponentModel;
using System.Collections.Generic;
using System.Xml;

namespace GadrocsWorkshop.Helios.Patching.DCS
{
    // XXX this needs an interface editor where multiple DCS installations can be configured and selected/deselected for patch installation
    [HeliosInterface("Patching.DCS.AdditionalViewports", "DCS Additional Viewports", typeof(AdditionalViewportsEditor), Factory = typeof(UniqueHeliosInterfaceFactory))]
    public class AdditionalViewports : HeliosInterface, IReadyCheck
    {
        private List<IPatchDestination> _destinations = new List<IPatchDestination>();

        public AdditionalViewports() : base("DCS Additional Viewports")
        {
        }

        public IEnumerable<StatusReportItem> PerformReadyCheck()
        {
            // do the check if all our patches are installed
            foreach (IPatchDestination destination in _destinations)
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

        protected override void OnProfileChanged(HeliosProfile oldProfile)
        {
            base.OnProfileChanged(oldProfile);
            if (Profile != null)
            {
                // XXX implement configuration of DCS installations
                _destinations.Clear();
                _destinations.Add(new PatchDestination());
            }
        }
    }
}
