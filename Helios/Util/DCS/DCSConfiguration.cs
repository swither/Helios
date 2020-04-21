using System.Collections.Generic;
using GadrocsWorkshop.Helios.ComponentModel;
using GadrocsWorkshop.Helios.Interfaces.Capabilities;

namespace GadrocsWorkshop.Helios.Util.DCS
{
    /// <summary>
    /// a base class for a configuration object that writes configuration to a number of
    /// selected DCS installation locations shared with all other such objects
    /// </summary>
    public abstract class DCSConfiguration : NotificationObject, IReadyCheck, IInstallation
    {
        protected void SubscribeToLocationChanges()
        {
            // if DCS locations are added, removed, enabled, or disabled, we need to check if the resulting set of locations is configured
            InstallationLocations locations = InstallationLocations.Singleton;
            locations.Added += Locations_Changed;
            locations.Removed += Locations_Changed;
            locations.Enabled += Locations_Changed;
            locations.Disabled += Locations_Changed;
        }

        public virtual void Dispose()
        {
            InstallationLocations locations = InstallationLocations.Singleton;
            locations.Added -= Locations_Changed;
            locations.Removed -= Locations_Changed;
            locations.Enabled -= Locations_Changed;
            locations.Disabled -= Locations_Changed;
        }

        private void Locations_Changed(object sender, InstallationLocations.LocationEvent e)
        {
            Update();
        }

        protected override void OnPropertyChanged(PropertyNotificationEventArgs args)
        {
            base.OnPropertyChanged(args);
            Update();
        }

        /// <summary>
        /// called when some data on which we depend is changed and we should invalidate
        /// the current configuration and status report
        /// </summary>
        protected abstract void Update();

        public abstract InstallationResult Install(IInstallationCallbacks callbacks);

        public abstract IEnumerable<StatusReportItem> PerformReadyCheck();
    }
}