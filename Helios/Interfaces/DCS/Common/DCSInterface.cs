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

namespace GadrocsWorkshop.Helios.Interfaces.DCS.Common
{
    using GadrocsWorkshop.Helios.ProfileAwareInterface;
    using GadrocsWorkshop.Helios.UDPInterface;
    using Microsoft.Win32;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Xml;

    public class DCSInterface : BaseUDPInterface, IProfileAwareInterface
    {
        protected string _dcsPath;
        protected bool _phantomFix;
        protected int _phantomLeft;
        protected int _phantomTop;
        protected long _nextCheck = 0;

        protected string _exportDeviceName;

        protected bool _usesExportModule;
        private static readonly bool DEFAULT_MODULE_USE = false;

        // current state of the Export script, as far as we know
        protected string _currentDriver = "";
        protected bool _currentDriverIsModule = false;

        // protocol to talk to DCS Export script (control messages)
        protected DCSExportProtocol _protocol;

        public DCSInterface(string name, string exportDeviceName)
            : base(name)
        {
            _exportDeviceName = exportDeviceName;
            _usesExportModule = DEFAULT_MODULE_USE;

            // XXX temp until we get rid of alternate names
            AlternateName = exportDeviceName;

            DCSConfigurator config = new DCSConfigurator(name, DCSPath);
            Port = config.Port;
            _phantomFix = config.PhantomFix;
            _phantomLeft = config.PhantomFixLeft;
            _phantomTop = config.PhantomFixTop;

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
        [field: NonSerialized]
        public event EventHandler<ProfileHint> ProfileHintReceived;

        // this event indicates that the interface received an indication that the specified
        // profile name is loaded on the other side of the interface
        [field: NonSerialized]
        public event EventHandler<DriverStatus> DriverStatusReceived;
        #endregion

        #region Properties

        // WARNING: there is currently no UI for this feature, because that UI is in a different development branch.
        // this value will be set manually in the XML for testing in this branch of the code
        public bool UsesExportModule
        {
            get
            {
                return _usesExportModule;
            }
            set
            {
                if (!_usesExportModule.Equals(value))
                {
                    bool oldValue = _usesExportModule;
                    _usesExportModule = value;
                    OnPropertyChanged("UsesExportModule", oldValue, value, false);
                }
            }
        }

        // WARNING: there is currently no UI for this feature, because that UI is in a different development branch.
        // this value will be set manually in the XML for testing in this branch of the code
        /// <summary>
        /// If not null, the this interface instance is configured to impersonate the specified vehicle name.  This means
        /// that Helios should select it for the given vehicle, instead of the one that the interface natively supports.
        /// </summary>
        public string ImpersonatedVehicleName { get; internal set; }

        #endregion

        private string DCSPath
        {
            get
            {
                if (_dcsPath == null)
                {
                    RegistryKey pathKey = Registry.CurrentUser.OpenSubKey(@"Software\Eagle Dynamics\DCS World");
                    if (pathKey != null)
                    {
                        _dcsPath = (string)pathKey.GetValue("Path");
                        pathKey.Close();
                        ConfigManager.LogManager.LogDebug($"{Name} Interface Editor - Found DCS Path (Path=\"" + _dcsPath + "\")");
                    }
                    else
                    {
                        _dcsPath = "";
                    }
                }
                return _dcsPath;
            }
        }

        // we only support selection based on which vehicle this interface supports
        public IEnumerable<string> Tags
        {
            get
            {
                return new string[] { ImpersonatedVehicleName ?? _exportDeviceName };
            }
        }

        protected override void OnProfileChanged(HeliosProfile oldProfile)
        {
            base.OnProfileChanged(oldProfile);

            if (oldProfile != null)
            {
                oldProfile.ProfileTick -= Profile_Tick;
            }

            if (Profile != null)
            {
                Profile.ProfileTick += Profile_Tick;
            }
        }

        void Profile_Tick(object sender, EventArgs e)
        {
            if (_phantomFix && System.Environment.TickCount - _nextCheck >= 0)
            {
                System.Diagnostics.Process[] dcs = System.Diagnostics.Process.GetProcessesByName("DCS");
                if (dcs.Length == 1)
                {
                    IntPtr hWnd = dcs[0].MainWindowHandle;
                    NativeMethods.Rect dcsRect;
                    NativeMethods.GetWindowRect(hWnd, out dcsRect);

                    if (dcsRect.Width > 640 && (dcsRect.Left != _phantomLeft || dcsRect.Top != _phantomTop))
                    {
                        NativeMethods.MoveWindow(hWnd, _phantomLeft, _phantomTop, dcsRect.Width, dcsRect.Height, true);
                    }
                }
                _nextCheck = System.Environment.TickCount + 5000;
            }
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
                if ((!_currentDriverIsModule) && (_exportDeviceName == _currentDriver))
                {
                    // already in correct state
                    // but we let our UI know
                    DriverStatusReceived?.Invoke(this, new DriverStatus() { ExportDriver = _currentDriver });
                    return;
                }
                _protocol.SendDriverRequest(_exportDeviceName);
            }
        }

        public override void Reset()
        {
            base.Reset();
            _protocol?.Reset();
        }

        protected override void OnProfileStarted()
        {
            _protocol = new DCSExportProtocol(this, Profile);
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
    }
}
