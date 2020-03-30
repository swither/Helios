using GadrocsWorkshop.Helios.ComponentModel;
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
        private HashSet<IStatusReportObserver> _observers = new HashSet<IStatusReportObserver>();

        public AdditionalViewports() : base("DCS Additional Viewports")
        {
        }

        public bool IsViewportAvailable(string viewportName)
        {
            // For now, this assumes all patches are either included or not (just checks for presence of interface.)        
            return true;
        }

        public IEnumerable<StatusReportItem> PerformReadyCheck()
        {
            // check if all our patches are installed
            List<IPatchDestination> destinations = InstallationLocations.Singleton.Items
                .Where(item => item.IsEnabled)
                .Select(location => new DCSPatchDestination(location) as IPatchDestination)
                .ToList();
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

        public void Subscribe(IStatusReportObserver observer)
        {
            _observers.Add(observer);
        }

        public void Unsubscribe(IStatusReportObserver observer)
        {
            _observers.Remove(observer);
        }

        internal void InvalidateStatus()
        {
            if (_observers.Count < 1)
            {
                return;
            }
            IEnumerable<StatusReportItem> newReport = PerformReadyCheck();
            foreach (IStatusReportObserver observer in _observers)
            {
                observer.ReceiveStatusReport(newReport);
            }
        }
    }
}
