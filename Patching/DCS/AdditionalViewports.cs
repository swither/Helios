// Copyright 2020 Ammo Goettsch
// 
// Helios is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// Helios is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
// 

using System.Collections.Generic;
using System.Linq;
using System.Xml;
using GadrocsWorkshop.Helios.ComponentModel;
using GadrocsWorkshop.Helios.Interfaces.Capabilities;
using GadrocsWorkshop.Helios.Util.DCS;

namespace GadrocsWorkshop.Helios.Patching.DCS
{
    [HeliosInterface("Patching.DCS.AdditionalViewports", "DCS Additional Viewports", typeof(AdditionalViewportsEditor),
        Factory = typeof(UniqueHeliosInterfaceFactory))]
    public class AdditionalViewports : HeliosInterface, IReadyCheck, IViewportProvider, IStatusReportNotify,
        IExtendedDescription
    {
        #region Constant

        public const string PATCH_SET = "Viewports";

        #endregion

        #region Private

        private readonly HashSet<IStatusReportObserver> _observers = new HashSet<IStatusReportObserver>();

        #endregion

        public AdditionalViewports() : base("DCS Additional Viewports")
        {
        }

        /// <summary>
        /// set up the application of patches to a specific location in DCS-specific way
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        internal static PatchApplication CreatePatchDestination(InstallationLocation location) =>
            new PatchApplication(
                new PatchDestination(location),
                location.IsEnabled,
                !location.Writable,
                PATCH_SET,
                // load user-provided patches from documents folder
                System.IO.Path.Combine(ConfigManager.DocumentPath, "Patches", "DCS"),
                // then load pre-installed patches from Helios installation folder
                System.IO.Path.Combine(ConfigManager.ApplicationPath, "Plugins", "Patches", "DCS"));

        protected override void AttachToProfileOnMainThread()
        {
            base.AttachToProfileOnMainThread();
            InstallationLocations locations = InstallationLocations.Singleton;
            locations.Added += Locations_Changed;
            locations.Removed += Locations_Changed;
            locations.Enabled += Locations_Changed;
            locations.Disabled += Locations_Changed;
        }

        protected override void DetachFromProfileOnMainThread(HeliosProfile oldProfile)
        {
            base.DetachFromProfileOnMainThread(oldProfile);
            InstallationLocations locations = InstallationLocations.Singleton;
            locations.Added -= Locations_Changed;
            locations.Removed -= Locations_Changed;
            locations.Enabled -= Locations_Changed;
            locations.Disabled -= Locations_Changed;
        }

        private void Locations_Changed(object sender, InstallationLocations.LocationEvent e)
        {
            InvalidateStatusReport();
        }

        public override void ReadXml(XmlReader reader)
        {
            // no code
        }

        public override void WriteXml(XmlWriter writer)
        {
            // no code
        }

        #region IExtendedDescription

        public string Description =>
            "Utility interface that applies patches to DCS installation files to create additional exported viewports.";

        public string RemovalNarrative =>
            "Delete this interface to no longer let Helios manage viewport patches in DCS.";

        #endregion

        #region IReadyCheck

        public IEnumerable<StatusReportItem> PerformReadyCheck()
        {
            // check if DCS install folders are configured
            IList<InstallationLocation> locations = InstallationLocations.Singleton.Active;
            if (!locations.Any())
            {
                yield return new StatusReportItem
                {
                    Status = "No DCS installation locations are configured for viewport patch installation",
                    Recommendation = "Configure any DCS installation directories you use",
                    Link = StatusReportItem.ProfileEditor,
                    Severity = StatusReportItem.SeverityCode.Error
                };
                yield break;
            }

            // check if all our patches are installed
            foreach (PatchApplication item in locations
                .Select(CreatePatchDestination))
            {
                if (item.SelectedVersion == null)
                {
                    yield return new StatusReportItem
                    {
                        Status = $"No Viewport patches compatible with {item.Destination.Description} found",
                        Recommendation =
                            "Please reinstall Helios to install these patches or provide them in documents folder",
                        Severity = StatusReportItem.SeverityCode.Error
                    };
                }

                foreach (StatusReportItem result in item.Patches.Verify(item.Destination))
                {
                    // return detailed results instead of just "up to date or not"
                    yield return result;
                }
            }

            yield return new StatusReportItem
            {
                Status = "Helios is managing DCS patches for viewports and drawing settings",
                Recommendation = "Do not also install viewport mods manually or via a mod manager like OVGME",
                Flags = StatusReportItem.StatusFlags.Verbose | StatusReportItem.StatusFlags.ConfigurationUpToDate
            };
        }

        #endregion

        #region IStatusReportNotify

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

            IList<StatusReportItem> newReport = PerformReadyCheck().ToList();
            PublishStatusReport(newReport);
        }

        public void PublishStatusReport(IList<StatusReportItem> statusReport)
        {
            foreach (IStatusReportObserver observer in _observers)
            {
                observer.ReceiveStatusReport(Name,
                    Description,
                    statusReport);
            }
        }

        #endregion

        #region IViewportProvider

        // For now, this assumes all patches are either included or not (just checks for presence of interface.)        
        public bool IsViewportAvailable(string viewportName) => true;

        #endregion
    }
}