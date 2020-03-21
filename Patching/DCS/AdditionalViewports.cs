using System.Collections.Generic;
using System.Xml;
using GadrocsWorkshop.Helios.ComponentModel;
using GadrocsWorkshop.Helios.Patching;

namespace GadrocsWorkshop.Helios.Patching.DCS
{
    // XXX this needs an interface editor where multiple DCS installations can be configured and selected/deselected for patch installation
    [HeliosInterface("Patching.DCS.AdditionalViewports", "DCS Additional Viewports", null, Factory = typeof(UniqueHeliosInterfaceFactory))]
    public class AdditionalViewports : HeliosInterface, IReadyCheck
    {
        private PatchList _patches = new PatchList();
        private List<IPatchDestination> _destinations = new List<IPatchDestination>();

        public AdditionalViewports() : base("DCS Additional Viewports")
        {
        }

        public IEnumerable<StatusReportItem> PerformReadyCheck()
        {
            // do the check if all our patches are installed
            foreach (IPatchDestination destination in _destinations)
            {
                IEnumerable<StatusReportItem> results = _patches.SimulateApply(destination);
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
