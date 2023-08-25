// Copyright 2023 Helios Contributors
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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using GadrocsWorkshop.Helios.Util;
using GadrocsWorkshop.Helios.Util.DCS;
using System.Xml;
using System.ComponentModel;

namespace GadrocsWorkshop.Helios.Patching.DCS
{
    internal class IrisConfiguration : DCSConfiguration
    {

        #region Private

        private readonly MonitorSetup _parent;

        private bool _isOpen = false;
        private bool _isRemote = false; 
        private string _configName;
        private string _irisPath;
        private string _tempPath;
        private string _backupPath;
        private string _remoteHost;
        private double _portNumber = 9091;
        private bool _hasBackground = true;
        private Rect _backgroundRect = new Rect(0,0,1920,1080);
        private TextWriter _writer;
        private XmlWriterSettings _settings;
        private XmlWriter _xmlWriter;

        private string _networkAddress = "";
        #endregion

        public IrisConfiguration(MonitorSetup parent)
        {
            _parent = parent;
            SubscribeToLocationChanges();
            _isRemote = ConfigManager.SettingsManager.LoadSetting("DCSInstallation", "IsRemote", false);
            _networkAddress = ConfigManager.SettingsManager.LoadSetting("DCSInterface", "IPAddress", System.Net.IPAddress.Loopback.ToString());
            _portNumber = ConfigManager.SettingsManager.LoadSetting("DCSInterface", "Port", 9089+5);
        }

        public bool Open(string configName)
        {
            try
            {
                _configName = Path.ChangeExtension(configName, "xml");
                _irisPath = Path.Combine(Directory.GetParent(Path.GetDirectoryName(_parent.Profile.Path)).FullName, "Iris");
                _tempPath = Path.Combine(_irisPath, Path.ChangeExtension(_configName, "tmp"));
                _backupPath = Path.Combine(_irisPath, Path.ChangeExtension(_configName, "bak"));
                _remoteHost = _networkAddress;


                if (!Directory.Exists(_irisPath))
                {
                    Directory.CreateDirectory(_irisPath);
                }
                // Delete tmp file if exists
                if (File.Exists(_tempPath))
                {
                    File.Delete(_tempPath);
                }
                _writer = new StreamWriter(_tempPath, false);
                TypeConverter boolConverter = TypeDescriptor.GetConverter(typeof(bool));

                _settings = new XmlWriterSettings();
                _settings.Indent = true;

                _xmlWriter = XmlWriter.Create(_writer, _settings);
                _xmlWriter.WriteStartElement("IrisConfig");
                _xmlWriter.WriteAttributeString("xmlns", "xsi", null, "http://www.w3.org/2001/XMLSchema-instance");
                _xmlWriter.WriteAttributeString("xmlns", "xsd", null, "http://www.w3.org/2001/XMLSchema");
                _xmlWriter.WriteStartElement("Viewports");
                _isOpen = true;
            } catch (Exception ex)
            {
                string message = $"Iris Configuration failure:  '{ex.Message}'";
                ConfigManager.LogManager.LogError(message);
                _isOpen = false;
            }
            return _isOpen;
        }

        public bool WriteViewport(KeyValuePair<string, Rect> viewport)
        {
            if (_isOpen)
            {
                Rect viewportRect = viewport.Value;
                viewportRect.Intersect(_parent.Rendered);
                if (viewportRect.Width < viewport.Value.Width || viewportRect.Height < viewport.Value.Height)
                {
                    // viewports that aren't entire rendered do not work
                    string message = $"viewport '{viewport.Key}' not included in IRIS configuration because it is not entirely contained in rendered resolution";
                    ConfigManager.LogManager.LogInfo(message);
                    return false;
                }
                else
                {

                    _xmlWriter.WriteStartElement("Viewport");
                    ConvertToDCS(ref viewportRect);
                    _xmlWriter.WriteElementString("Name", viewport.Key);
                    _xmlWriter.WriteElementString("Description", $"Viewport for {viewport.Key}");
                    _xmlWriter.WriteElementString("Host", _remoteHost);
                    _xmlWriter.WriteElementString("Port", $"{_portNumber++}");
                    _xmlWriter.WriteElementString("ScreenCaptureX", $"{(!_isRemote ? viewportRect.Left : 0)}");
                    _xmlWriter.WriteElementString("ScreenCaptureY", $"{(!_isRemote ? viewportRect.Top : 0)}");
                    _xmlWriter.WriteElementString("SizeX", viewportRect.Width.ToString());
                    _xmlWriter.WriteElementString("SizeY", viewportRect.Height.ToString());
                    _xmlWriter.WriteElementString("ScreenPositionX", $"{(_isRemote ? viewportRect.Left : 0)}");
                    _xmlWriter.WriteElementString("ScreenPositionY", $"{(_isRemote ? viewportRect.Top : 0)}");
                    _xmlWriter.WriteEndElement();  // viewport
                    return true;
                }
            }else
            {
                return false;
            }
        }

        public bool IsOpen { get => _isOpen; }
        public bool HasBackground { get => _hasBackground; set => _hasBackground = value; }

        public Rect BackgroundRectangle { 
            get => _backgroundRect; 
            
            set {
                Rect oldValue = _backgroundRect;
                if (!oldValue.Equals(value)) {
                    _backgroundRect = value;
                }
            } }
        public void Close()
        {
            if (_isOpen)
            {
                if (_hasBackground)
                {
                    _xmlWriter.WriteStartElement("Viewport");
                    _xmlWriter.WriteElementString("Name", "Background");
                    _xmlWriter.WriteElementString("Description", "This is the full screen background for the back of the screen");
                    _xmlWriter.WriteElementString("Host", _remoteHost);
                    _xmlWriter.WriteElementString("Port", $"{_portNumber++}");
                    _xmlWriter.WriteElementString("ScreenCaptureX", "0");
                    _xmlWriter.WriteElementString("ScreenCaptureY", "0");
                    _xmlWriter.WriteElementString("SizeX", _backgroundRect.Width.ToString());
                    _xmlWriter.WriteElementString("SizeY", _backgroundRect.Height.ToString());
                    _xmlWriter.WriteElementString("ScreenPositionX", _backgroundRect.X.ToString());
                    _xmlWriter.WriteElementString("ScreenPositionY", _backgroundRect.Y.ToString());
                    _xmlWriter.WriteEndElement();  // background viewport
                }
                _xmlWriter.WriteEndElement();  // Viewports
                _xmlWriter.WriteElementString("PollingInterval", "100");
                _xmlWriter.WriteEndElement(); // IrisConfig

                _xmlWriter.Close();
                _writer.Close();
                _isOpen = false;
                // Delete existing backup
                if (File.Exists(_backupPath))
                {
                    File.Delete(_backupPath);
                }
                _configName = Path.Combine(_irisPath, _configName);
                // backup existing file
                if (File.Exists(_configName))
                {
                    File.Move(_configName, _backupPath);
                }

                // Rename .tmp to actual
                File.Move(_tempPath, _configName);
            }
        }

        protected override void Update()
        {
        }

        internal void ConvertToDCS(ref Rect windowsRect)
        {
            windowsRect.Offset(-_parent.Rendered.TopLeft.X, -_parent.Rendered.TopLeft.Y);
            windowsRect.Scale(ConfigManager.DisplayManager.PixelsPerDip, ConfigManager.DisplayManager.PixelsPerDip);
        }

        private string GenerateSavedGamesPath(string savedGamesName) =>
            Path.Combine(KnownFolders.SavedGames, savedGamesName, "Config", "MonitorSetup");

        /// <summary>
        /// attempt to write the monitor setup file to all configured Saved Games folders
        /// and also persist our viewports information
        /// </summary>
        /// <param name="callbacks"></param>
        /// <returns></returns>
        public override InstallationResult Install(IInstallationCallbacks callbacks)
        {
            try
            {
                return InstallationResult.Success;
            }
            catch (Exception ex)
            {
                return InstallationResult.Fatal;
            }
        }

        public override IEnumerable<StatusReportItem> PerformReadyCheck()
        {
            return null;
        }

    }
}