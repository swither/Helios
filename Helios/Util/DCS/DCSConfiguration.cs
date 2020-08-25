using System;
using System.Collections.Generic;
using GadrocsWorkshop.Helios.ComponentModel;
using GadrocsWorkshop.Helios.Interfaces.Capabilities;

namespace GadrocsWorkshop.Helios.Util.DCS
{
    /// <summary>
    /// a base class for a configuration object that writes configuration to a number of
    /// selected DCS installation locations shared with all other such objects
    /// </summary>
    public abstract class DCSConfiguration : NotificationObject, IReadyCheck, IInstallation, IDisposable
    {
        /// <summary>
        /// utility accessor for pathing in UI, such as: Locations.IsRemote
        /// </summary>
        public InstallationLocations Locations => InstallationLocations.Singleton;

        protected void SubscribeToLocationChanges()
        {
            // if DCS locations are added, removed, enabled, or disabled, we need to check if the resulting set of locations is configured
            InstallationLocations locations = InstallationLocations.Singleton;
            locations.Added += Location_Added;
            locations.Removed += Location_Removed;
            locations.Enabled += Location_Enabled;
            locations.Disabled += Location_Disabled;
            locations.RemoteChanged += Locations_RemoteChanged;
        }

        public virtual void Dispose()
        {
            InstallationLocations locations = InstallationLocations.Singleton;
            locations.Added -= Location_Added;
            locations.Removed -= Location_Removed;
            locations.Enabled -= Location_Enabled;
            locations.Disabled -= Location_Disabled;
            locations.RemoteChanged -= Locations_RemoteChanged;
        }

        protected virtual void Location_Added(object sender, InstallationLocations.LocationEvent e)
        {
            Update();
        }

        protected virtual void Location_Removed(object sender, InstallationLocations.LocationEvent e)
        {
            Update();
        }

        protected virtual void Location_Enabled(object sender, InstallationLocations.LocationEvent e)
        {
            Update();
        }

        protected virtual void Location_Disabled(object sender, InstallationLocations.LocationEvent e)
        {
            Update();
        }

        protected virtual void Locations_RemoteChanged(object sender, InstallationLocations.RemoteChangeEvent e)
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