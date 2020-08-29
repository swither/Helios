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
using GadrocsWorkshop.Helios.Interfaces.Capabilities;
using GadrocsWorkshop.Helios.Util.DCS;

namespace GadrocsWorkshop.Helios.Patching.DCS
{
    /// <summary>
    /// the viewport patching functionality of DCS Additional Viewports
    ///
    /// NOTE: initially, this is the only capability of that interface
    /// </summary>
    public class DCSPatchInstallation : DCSConfiguration
    {
        /// <summary>
        /// our parent interface to which we indicate when the status report needs to be updated
        /// </summary>
        private readonly IStatusReportNotify _parent;

        internal DCSPatchInstallation(IStatusReportNotify parent, string patchSet, string patchSetShortName)
        {
            _parent = parent;
            PatchSet = patchSet;
            PatchSetShortName = patchSetShortName;
            Patching = new PatchInstallation(LoadDestinations(Locations),
                PatchSet,
                $"Helios {PatchSetShortName} patches");
            Patching.PatchesChanged += Patching_PatchesChanged;
            SubscribeToLocationChanges();
        }

        private void Patching_PatchesChanged(object sender, System.EventArgs e)
        {
            _parent?.InvalidateStatusReport();
        }

        /// <summary>
        /// the patchset name in the directory tree of Patches
        /// </summary>
        public string PatchSet { get; }

        /// <summary>
        /// a short, singular, lower case name
        /// </summary>
        public string PatchSetShortName { get; }

        /// <summary>
        /// UI access to state of generic patching implementation
        /// </summary>
        public PatchInstallation Patching { get; }

        /// <summary>
        /// set up the application of patches to a specific location in DCS-specific way
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        internal PatchApplication CreatePatchDestination(InstallationLocation location) =>
            new PatchApplication(
                new PatchDestination(location),
                location.IsEnabled,
                !location.Writable,
                PatchSet,
                // load user-provided patches from documents folder
                System.IO.Path.Combine(ConfigManager.DocumentPath, "Patches", "DCS"),
                // then load pre-installed patches from Helios installation folder
                System.IO.Path.Combine(ConfigManager.ApplicationPath, "Plugins", "Patches", "DCS"));


        private Dictionary<string, PatchApplication> LoadDestinations(InstallationLocations locations)
        {
            Dictionary<string, PatchApplication> destinations =
                new Dictionary<string, PatchApplication>();

            if (locations.IsRemote)
            {
                // just leave it blank
                return destinations;
            }

            foreach (InstallationLocation location in locations.Items)
            {
                destinations[location.Path] = CreatePatchDestination(location);
            }

            return destinations;
        }

        #region DCSConfiguration

        protected override void Update()
        {
            _parent?.InvalidateStatusReport();
        }

        protected override void Location_Added(object sender, InstallationLocations.LocationEvent e)
        {
            Patching?.OnAdded(e.Location.Path, CreatePatchDestination(e.Location));
            base.Location_Added(sender, e);
        }

        protected override void Location_Removed(object sender, InstallationLocations.LocationEvent e)
        {
            Patching?.OnRemoved(e.Location.Path);
            base.Location_Removed(sender, e);
        }

        protected override void Location_Enabled(object sender, InstallationLocations.LocationEvent e)
        {
            Patching?.OnEnabled(e.Location.Path);
            base.Location_Enabled(sender, e);
        }

        protected override void Location_Disabled(object sender, InstallationLocations.LocationEvent e)
        {
            Patching?.OnDisabled(e.Location.Path);
            base.Location_Disabled(sender, e);
        }

        protected override void Locations_RemoteChanged(object sender, InstallationLocations.RemoteChangeEvent e)
        {
            Patching?.Reload(LoadDestinations(InstallationLocations.Singleton));
            base.Locations_RemoteChanged(sender, e);
        }

        #endregion

        #region IInstallation

        public override InstallationResult Install(IInstallationCallbacks callbacks) =>
            // use the generic patch installation to implement this
            Patching.Install(callbacks);

        #endregion

        #region IReadyCheck

        public override IEnumerable<StatusReportItem> PerformReadyCheck()
        {
            // load patchExclusions from settings instead of caching them, so we don't have different code paths from elevated binary
            HashSet<string> patchExclusions = PatchInstallation.LoadPatchExclusions();

            // check if DCS install folders are configured
            IList<InstallationLocation> locations = InstallationLocations.Singleton.Active;
            if (!locations.Any())
            {
                yield return new StatusReportItem
                {
                    Status = $"No DCS installation locations are configured for {PatchSetShortName} patch installation",
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
                        Status = $"No {PatchSetShortName} patches compatible with {item.Destination.Description} found",
                        Recommendation =
                            "Please reinstall Helios to install these patches or provide them in documents folder",
                        Severity = StatusReportItem.SeverityCode.Error
                    };
                }

                foreach (StatusReportItem result in item.Patches.Verify(item.Destination, patchExclusions))
                {
                    // return detailed results instead of just "up to date or not"
                    yield return result;
                }
            }

            yield return new StatusReportItem
            {
                Status = $"Helios is managing DCS {PatchSetShortName} patches",
                Recommendation =
                    $"Do not also install {PatchSetShortName} mods manually or via a mod manager like OVGME",
                Flags = StatusReportItem.StatusFlags.Verbose | StatusReportItem.StatusFlags.ConfigurationUpToDate
            };
        }

        #endregion
    }
}