using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using GadrocsWorkshop.Helios.Interfaces.Capabilities;

namespace GadrocsWorkshop.Helios.Interfaces.Common
{
    public class InterfaceStatusScanner : NotificationObject, IStatusReportObserver
    {
        public event EventHandler Triggered;

        #region Private
        private static readonly string CONFIGURATION_GROUP = "ConfigurationCheck";

        /// <summary>
        /// backing field for property InterfaceStatuses, contains
        /// a collection of interface status objects for the interfaces we observe
        /// </summary>
        private ObservableCollection<InterfaceStatus> _interfaceStatuses = new ObservableCollection<InterfaceStatus>();

        /// <summary>
        /// backing field for property TriggerThreshold, contains
        /// the minimum severity that a result item must have to trigger displaying the
        /// check list
        /// </summary>
        private StatusReportItem.SeverityCode _triggerThreshold;

        /// <summary>
        /// backing field for property DisplayThreshold, contains
        /// the minimum severity that a result item must have to be shown in details
        /// </summary>
        private StatusReportItem.SeverityCode _displayThreshold;

        private HeliosProfile _profile;

        #endregion

        public InterfaceStatusScanner()
        {
            _triggerThreshold = ConfigManager.SettingsManager.LoadSetting(CONFIGURATION_GROUP, "TriggerThreshold",
                StatusReportItem.SeverityCode.Warning);
            _displayThreshold = ConfigManager.SettingsManager.LoadSetting(CONFIGURATION_GROUP, "DisplayThreshold",
                StatusReportItem.SeverityCode.Info);
        }

        public void Reload(HeliosProfile profile)
        {
            if (_profile != null)
            {
                // disconnect
                _profile.Interfaces.CollectionChanged -= Profile_InterfacesChanged;
                _profile = null;
            }

            // signal all interfaces removed so that any running view models / editors can refresh
            InterfaceStatuses?.Clear();

            // add back new interface status objects for new profile
            foreach (HeliosInterface heliosInterface in profile.Interfaces)
            {
                AddInterfaceIfSupported(heliosInterface);
            }

            PerformChecks();
            profile.Interfaces.CollectionChanged += Profile_InterfacesChanged;
            _profile = profile;
        }

        private void Profile_InterfacesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                List<InterfaceStatus> remove = new List<InterfaceStatus>();
                foreach (HeliosInterface heliosInterface in e.OldItems)
                {
                    foreach (InterfaceStatus interfaceStatus in InterfaceStatuses)
                    {
                        if (interfaceStatus.Interface == heliosInterface)
                        {
                            remove.Add(interfaceStatus);
                        }
                    }
                }

                // now that we are done iterating this collection, remove all the marked items
                foreach (InterfaceStatus section in remove)
                {
                    RemoveInterface(section);
                }
            }

            if (e.NewItems != null)
            {
                foreach (HeliosInterface heliosInterface in e.NewItems)
                {
                    AddInterfaceIfSupported(heliosInterface);
                }
            }

            PerformChecks();
        }

        private void AddInterfaceIfSupported(HeliosInterface heliosInterface)
        {
            if (InterfaceStatus.TryManage(heliosInterface, out InterfaceStatus interfaceStatus))
            {
                InterfaceStatuses.Add(interfaceStatus);
                interfaceStatus.Subscription?.Subscribe(this);
            }
        }

        public void PerformChecks()
        {
            foreach (InterfaceStatus interfaceStatus in InterfaceStatuses)
            {
                interfaceStatus.PerformCheck();
            }
        }

        public void RemoveInterface(InterfaceStatus interfaceStatus)
        {
            // clean up
            interfaceStatus.Subscription?.Unsubscribe(this);
            InterfaceStatuses.Remove(interfaceStatus);
            interfaceStatus.Dispose();
        }

        #region Properties

        /// <summary>
        /// the minimum severity that a result item must have to trigger displaying the
        /// check list
        /// </summary>
        public StatusReportItem.SeverityCode TriggerThreshold
        {
            get => _triggerThreshold;
            set
            {
                if (_triggerThreshold == value)
                {
                    return;
                }

                StatusReportItem.SeverityCode oldValue = _triggerThreshold;
                _triggerThreshold = value;
                ConfigManager.SettingsManager.SaveSetting(CONFIGURATION_GROUP, "TriggerThreshold", value);
                OnPropertyChanged("TriggerThreshold", oldValue, value, true);
            }
        }

        /// <summary>
        /// the minimum severity that a result item must have to be shown in details
        /// </summary>
        public StatusReportItem.SeverityCode DisplayThreshold
        {
            get => _displayThreshold;
            set
            {
                if (_displayThreshold == value)
                {
                    return;
                }

                StatusReportItem.SeverityCode oldValue = _displayThreshold;
                _displayThreshold = value;
                ConfigManager.SettingsManager.SaveSetting(CONFIGURATION_GROUP, "DisplayThreshold", value);
                OnPropertyChanged("DisplayThreshold", oldValue, value, true);
            }
        }

        /// <summary>
        /// a collection of interface status objects for the interfaces we observe
        /// </summary>
        public ObservableCollection<InterfaceStatus> InterfaceStatuses
        {
            get => _interfaceStatuses;
            set
            {
                if (_interfaceStatuses == value)
                {
                    return;
                }

                ObservableCollection<InterfaceStatus> oldValue = _interfaceStatuses;
                _interfaceStatuses = value;
                OnPropertyChanged("InterfaceStatuses", oldValue, value, true);
            }
        }

        #endregion

        #region IStatusReportObserver

        public void ReceiveStatusReport(string name, string description, IList<StatusReportItem> statusReport)
        {
            _ = name;
            _ = description;

            if (TriggerThreshold == StatusReportItem.SeverityCode.None)
            {
                // don't bother scanning
                return;
            }

            if (statusReport.Any(item => item.Severity >= TriggerThreshold))
            {
                Triggered?.Invoke(this, EventArgs.Empty);
            }
        }

        #endregion
    }
}