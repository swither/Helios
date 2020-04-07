using GadrocsWorkshop.Helios.ComponentModel;
using GadrocsWorkshop.Helios.Util.DCS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace GadrocsWorkshop.Helios.Patching.DCS
{
    [HeliosInterface("Patching.DCS.AdditionalViewports", "DCS Additional Viewports", typeof(AdditionalViewportsEditor), Factory = typeof(UniqueHeliosInterfaceFactory))]
    public class AdditionalViewports : HeliosInterface, IReadyCheck, IViewportProvider, IStatusReportNotify
    {
        public const string PATCH_SET = "Viewports";
        private readonly HashSet<IStatusReportObserver> _observers = new HashSet<IStatusReportObserver>();

        public AdditionalViewports() : base("DCS Additional Viewports")
        {
        }

        protected override void OnProfileChanged(HeliosProfile oldProfile)
        {
            base.OnProfileChanged(oldProfile);
            if (Profile != null)
            {
                // initialization
                InstallationLocations locations = InstallationLocations.Singleton;
                locations.Added += Locations_Changed;
                locations.Removed += Locations_Changed;
                locations.Enabled += Locations_Changed;
                locations.Disabled += Locations_Changed;
            }
            else
            {
                // deinit
                InstallationLocations locations = InstallationLocations.Singleton;
                locations.Added -= Locations_Changed;
                locations.Removed -= Locations_Changed;
                locations.Enabled -= Locations_Changed;
                locations.Disabled -= Locations_Changed;
            }
        }

        private void Locations_Changed(object sender, InstallationLocations.LocationEvent e)
        {
            InvalidateStatusReport();
        }

        public bool IsViewportAvailable(string viewportName)
        {
            // For now, this assumes all patches are either included or not (just checks for presence of interface.)        
            return true;
        }

        public IEnumerable<StatusReportItem> PerformReadyCheck()
        {
            // check if DCS install folders are configured
            IList<InstallationLocation> locations = InstallationLocations.Singleton.Active;
            if (!locations.Any())
            {
                yield return new StatusReportItem
                {
                    Status = $"No DCS installation locations are configured for viewport patch installation",
                    Recommendation = $"Using Helios Profile Editor, configure any DCS installation directories you use",
                    Severity = StatusReportItem.SeverityCode.Error
                };
                yield break;
            }

            // check if all our patches are installed
            foreach (IPatchDestination destination in locations
                .Select(location => new PatchDestination(location) as IPatchDestination))
            {
                PatchList patches = PatchList.LoadPatches(destination, "Viewports");
                if (patches.Count < 1)
                {
                    yield return new StatusReportItem
                    {
                        Status = $"No Viewport patches compatible with {destination.Description} found in installation",
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

        public void Subscribe(IStatusReportObserver observer)
        {
            _observers.Add(observer);
        }

        public void Unsubscribe(IStatusReportObserver observer)
        {
            _observers.Remove(observer);
        }

        public void InvalidateStatusReport()
        {
            if (_observers.Count < 1)
            {
                return;
            }
            IEnumerable<StatusReportItem> newReport = PerformReadyCheck();
            PublishStatusReport(newReport);
        }

        public void PublishStatusReport(IEnumerable<StatusReportItem> statusReport)
        {
            IEnumerable<StatusReportItem> statusReportItems = statusReport.ToList();
            foreach (IStatusReportObserver observer in _observers)
            {
                observer.ReceiveStatusReport(statusReportItems);
            }
        }
    }
}
