//  Copyright 2014 Craig Courtney
//    
//  Helios is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  Helios is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Linq;

namespace GadrocsWorkshop.Helios.Interfaces.DCS.Common
{
    using GadrocsWorkshop.Helios.ProfileAwareInterface;
    using GadrocsWorkshop.Helios.UDPInterface;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Windows;
    using System.Xml;

    public class DCSInterface : BaseUDPInterface, IProfileAwareInterface, IReadyCheck, IStatusReportNotify
    {
        private const string SETTINGS_GROUP = "DCSInterface";

        // do we expect the Export.lua to use a module we did not write?
        // exported via UsesExportModule property with IPropertyNotification
        protected bool _usesExportModule;
        private static readonly bool DEFAULT_MODULE_USE = false;

        // current state of the Export script, as far as we know
        protected string _currentDriver = "";
        protected bool _currentDriverIsModule = false;

        // phantom monitor fix 
        private DCSPhantomMonitorFix _phantomFix;

        // export configuration generation and checking
        protected DCSExportConfiguration _configuration;

        // vehicle impersonation configuration
        protected DCSVehicleImpersonation _vehicleImpersonation;

        // protocol to talk to DCS Export script (control messages)
        protected DCSExportProtocol _protocol;

        // observers of our status report that need to be told when we change status
        private readonly HashSet<IStatusReportObserver> _observers = new HashSet<IStatusReportObserver>();

        /// <summary>
        /// backing field for ImpersonatedVehicleName
        /// </summary>
        private string _impersonatedVehicleName;

        public DCSInterface(string name, string exportDeviceName, string exportFunctionsPath)
            : base(name)
        {
            VehicleName = exportDeviceName;
            ExportFunctionsPath = exportFunctionsPath;
            _usesExportModule = DEFAULT_MODULE_USE;

            // make sure we keep our list up to date and don't typo on the name of an export device
            Debug.Assert(DCSVehicleImpersonation.KnownVehicles.Contains(exportDeviceName));

            // create handling for DCS export meta information we handle ourselves
            NetworkTriggerValue activeVehicle = new NetworkTriggerValue(this, "ACTIVE_VEHICLE", "ActiveVehicle", "Vehicle currently inhabited in DCS.", "Short name of vehicle");
            AddFunction(activeVehicle);
            activeVehicle.ValueReceived += ActiveVehicle_ValueReceived;
            NetworkTriggerValue activeDriver = new NetworkTriggerValue(this, "ACTIVE_DRIVER", "ActiveDriver", "Export driver running on DCS.", "Short name of driver");
            AddFunction(activeDriver);
            activeDriver.ValueReceived += ActiveDriver_ValueReceived;
            NetworkTriggerValue activeModule = new NetworkTriggerValue(this, "ACTIVE_MODULE", "ActiveModule", "Export module running on DCS.", "Short name of module");
            AddFunction(activeModule);
            activeModule.ValueReceived += ActiveModule_ValueReceived;
            AddFunction(new NetworkTrigger(this, "ALIVE", "Heartbeat", "Received periodically if there is no other data received"));
        }

        #region Events
        // this event indicates that the interface received an indication that a profile that 
        // matches the specified hint should be loaded
        public event EventHandler<ProfileHint> ProfileHintReceived;

        // this event indicates that the interface received an indication that the specified
        // exports are loaded on the other side of the interface
        public event EventHandler<DriverStatus> DriverStatusReceived;
        #endregion

        #region Properties

        // WARNING: there is currently no UI for this feature, because that UI is in a different development branch.
        // this value will be set manually in the XML for testing in this branch of the code
        public bool UsesExportModule
        {
            get => _usesExportModule;
            set
            {
                if (_usesExportModule.Equals(value)) return;
                bool oldValue = _usesExportModule;
                _usesExportModule = value;
                OnPropertyChanged("UsesExportModule", oldValue, value, false);
            }
        }

        /// <summary>
        /// The vehicle (usually an aircraft) that DCS will report in LoGetSelfData when we are using this interface.
        /// </summary>
        public string VehicleName { get; private set; }

        /// <summary>
        /// If not null, the this interface instance is configured to impersonate the specified vehicle name.  This means
        /// that Helios should select it for the given vehicle, instead of the one that the interface natively supports.
        /// </summary>
        public string ImpersonatedVehicleName
        {
            get { return _impersonatedVehicleName; }
            set
            {
                _impersonatedVehicleName = value;
                foreach (IStatusReportObserver observer in _observers)
                {
                    string newName = StatusName;
                    observer.ReceiveNameChange(newName);
                }
            }
        }

        // we only support selection based on which vehicle this interface supports
        public IEnumerable<string> Tags => new string[] { ImpersonatedVehicleName ?? VehicleName };

        /// <summary>
        /// vehicle-specific file resource to include
        /// </summary>
        public string ExportFunctionsPath { get; }

        public DCSExportConfiguration Configuration => _configuration;

        public DCSVehicleImpersonation VehicleImpersonation => _vehicleImpersonation;

        public string StatusName => (ImpersonatedVehicleName != null) ? $"{Name} impersonating {ImpersonatedVehicleName}" : Name;
        #endregion

        internal string LoadSetting(string key, string defaultValue)
        {
            if (ConfigManager.SettingsManager.IsSettingAvailable(SETTINGS_GROUP, key))
            {
                // get from shared location
                return ConfigManager.SettingsManager.LoadSetting(SETTINGS_GROUP, key, defaultValue);
            }

            // get from legacy location
            return ConfigManager.SettingsManager.LoadSetting(Name, key, defaultValue);
        }

        internal T LoadSetting<T>(string key, T defaultValue)
        {
            if (ConfigManager.SettingsManager.IsSettingAvailable(SETTINGS_GROUP, key))
            {
                // get from shared location, using LoadSetting<T>
                return ConfigManager.SettingsManager.LoadSetting(SETTINGS_GROUP, key, defaultValue);
            }

            // get from legacy location, using LoadSetting<T>
            return ConfigManager.SettingsManager.LoadSetting(Name, key, defaultValue);
        }

        internal void SaveSetting(string key, string value)
        {
            ConfigManager.SettingsManager.SaveSetting(SETTINGS_GROUP, key, value);
        }

        internal void SaveSetting<T>(string key, T value)
        {
            ConfigManager.SettingsManager.SaveSetting(SETTINGS_GROUP, key, value);
        }

        protected override void OnProfileChanged(HeliosProfile oldProfile)
        {
            base.OnProfileChanged(oldProfile);

            if (oldProfile != null)
            {
                // deinitialize on main thread
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Deinit(oldProfile);
                });
            }

            if (Profile != null)
            {
                // initialize on main thread
                Application.Current.Dispatcher.Invoke(Init);
            }
        }

        private void Init()
        {
            Profile.ProfileTick += Profile_Tick;
            _configuration = new DCSExportConfiguration(this);
            _vehicleImpersonation = new DCSVehicleImpersonation(this);
        }

        private void Deinit(HeliosProfile oldProfile)
        {
            oldProfile.ProfileTick -= Profile_Tick;
            _configuration.Dispose();
            _configuration = null;
            _vehicleImpersonation.Dispose();
            _vehicleImpersonation = null;
        }

        private void Profile_Tick(object sender, EventArgs e)
        {
            _phantomFix?.Profile_Tick(sender, e);
        }

        private void ActiveDriver_ValueReceived(object sender, NetworkTriggerValue.Value e)
        {
            _currentDriver = e.Text;
            _currentDriverIsModule = false;
            _protocol?.OnDriverStatus(e.Text);
            DriverStatusReceived?.Invoke(this, new DriverStatus() { ExportDriver = e.Text });
        }

        private void ActiveModule_ValueReceived(object sender, NetworkTriggerValue.Value e)
        {
            _currentDriver = e.Text;
            _currentDriverIsModule = true;
            _protocol?.OnModuleStatus();
            DriverStatusReceived?.Invoke(this, new DriverStatus() { ExportDriver = e.Text });
        }

        private void ActiveVehicle_ValueReceived(object sender, NetworkTriggerValue.Value e)
        {
            ProfileHintReceived?.Invoke(this, new ProfileHint() { Tag = e.Text });
        }

        public void RequestDriver(string name)
        {
            // NOTE: we don't have per-profile drivers, so we ignore the short name of the 
            // profile provided and instead just request support for the correct vehicle

            // the interface is supposed to have called OnProfileStarted before this is called,
            // so don't check for null; we want this to crash if this breaks in the future
            if (_usesExportModule)
            {
                if (_currentDriverIsModule)
                {
                    // we let the Export script make sure it is the right one for the vehicle
                    // but we let our UI know
                    DriverStatusReceived?.Invoke(this, new DriverStatus() { ExportDriver = _currentDriver });
                    return;
                }
                _protocol.SendModuleRequest();
            }
            else
            {
                if ((!_currentDriverIsModule) && (VehicleName == _currentDriver))
                {
                    // already in correct state
                    // but we let our UI know
                    DriverStatusReceived?.Invoke(this, new DriverStatus() { ExportDriver = _currentDriver });
                    return;
                }
                _protocol.SendDriverRequest(VehicleName);
            }
        }

        public override void Reset()
        {
            base.Reset();
            _protocol?.Reset();
        }

        protected override void OnProfileStarted()
        {
            // these parts are only used at run time (i.e. not in the Profile Editor)
            _protocol = new DCSExportProtocol(this, Profile.Dispatcher);
            _phantomFix = new DCSPhantomMonitorFix(this);
        }

        protected override void OnProfileStopped()
        {
            _protocol.Stop();
            _protocol = null;
        }

        protected override void OnClientChanged(string fromValue, string toValue)
        {
            base.OnClientChanged(fromValue, toValue);
            
            // protocol needs to know
            _protocol.OnClientChanged();

            // our information is now out of date
            _currentDriver = "";
            _currentDriverIsModule = false;
        }

        public override void ReadXml(XmlReader reader)
        {
            base.ReadXml(reader);
            TypeConverter bc = TypeDescriptor.GetConverter(typeof(bool));
            while (reader.NodeType == XmlNodeType.Element)
            {
                switch (reader.Name)
                {
                    case "UsesExportModule":
                        _usesExportModule = (bool)bc.ConvertFromInvariantString(reader.ReadElementString("UsesExportModule"));
                        break;
                    case "ImpersonatedVehicleName":
                        ImpersonatedVehicleName = reader.ReadElementString("ImpersonatedVehicleName");
                        break;
                    default:
                        string discard = reader.ReadElementString(reader.Name);
                        ConfigManager.LogManager.LogWarning($"Ignored unsupported DCS Interface setting '{reader.Name}' with value '{discard}'");
                        break;
                }
            }
        }

        public override void WriteXml(XmlWriter writer)
        {
            base.WriteXml(writer);
            TypeConverter bc = TypeDescriptor.GetConverter(typeof(bool));
            if (_usesExportModule != DEFAULT_MODULE_USE)
            {
                // write new Xml only if configured, because it may break previous versions
                writer.WriteElementString("UsesExportModule", bc.ConvertToInvariantString(_usesExportModule));
            }
            if (ImpersonatedVehicleName != null)
            {
                writer.WriteElementString("ImpersonatedVehicleName", ImpersonatedVehicleName);
            }
        }

        public IEnumerable<StatusReportItem> PerformReadyCheck()
        {
            // XXX check on our health

            // check on the health of our exports
            foreach (StatusReportItem item in _configuration.PerformReadyCheck())
            {
                yield return item;
            }
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
            IList<StatusReportItem> newReport = _configuration.CheckConfig();
            PublishStatusReport(newReport);
        }

        public void PublishStatusReport(IEnumerable<StatusReportItem> statusReport)
        {
            IList<StatusReportItem> statusReportItems = statusReport.ToList();
            foreach (IStatusReportObserver observer in _observers)
            {
                observer.ReceiveStatusReport(statusReportItems);
            }
        }
    }
}
