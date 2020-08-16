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

namespace GadrocsWorkshop.Helios
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.IO;
    using System.Xml;

    public class SettingsManager : ISettingsManager2
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public event EventHandler<EventArgs> Synchronized;

        private class Setting
        {
            public string Name;
            public string Value;
        }

        private class SettingsCollection : KeyedCollection<string, Setting>
        {
            public IEnumerable<string> Keys {
                get {
                    if (Dictionary == null) {
                        return new List<string>();
                    }
                    return Dictionary.Keys;
                }
            }

            protected override string GetKeyForItem(Setting item)
            {
                return item.Name;
            }
        }

        private class Group
        {
            private SettingsCollection _settings = new SettingsCollection();

            public string Name;
            public SettingsCollection Settings { get { return _settings; } }
        }

        private class GroupCollection : KeyedCollection<string, Group>
        {
            protected override string GetKeyForItem(Group item)
            {
                return item.Name;
            }

            internal Group GetOrCreateGroup(string groupName)
            {
                Group retValue;
                if (Contains(groupName))
                {
                    retValue = this[groupName];
                }
                else
                {
                    retValue = new Group();
                    retValue.Name = groupName;
                    Add(retValue);
                }
                return retValue;
            }
        }

        private string _settingsFile;
        private GroupCollection _settings = null;

        /// <summary>
        /// if set to false, any attempt to write the settings is an error
        /// </summary>
        public bool Writable { get; set; }

        public SettingsManager(string settingsFile)
        {
            Logger.Debug($"Helios will load settings from {Util.Anonymizer.Anonymize(settingsFile)}");
            _settingsFile = settingsFile;
        }

        private void LoadSettings()
        {
            if (_settings != null)
            {
                // only load once
                return;
            }

            try
            {
                _settings = LoadSettingsFile();
            } 
            catch (System.Exception ex)
            {
                Logger.Error(ex, $"the settings file '{_settingsFile}' cannot be read; all settings will be reset");
                // reset to defaults (empty settings) and let it overwrite the settings file when we next save
                _settings = new GroupCollection();
            }
        }

        private GroupCollection LoadSettingsFile()
        {
            // start with empty settings
            GroupCollection settingsCollection = new GroupCollection();
            if (!File.Exists(_settingsFile))
            {
                return settingsCollection;
            }

            XmlReaderSettings settings = new XmlReaderSettings {IgnoreComments = true, IgnoreWhitespace = true};
            using (TextReader reader = new StreamReader(_settingsFile))
            using (XmlReader xmlReader = XmlReader.Create(reader, settings))
            {
                try
                {
                    xmlReader.ReadStartElement("HeliosSettings");
                    if (!xmlReader.IsEmptyElement)
                    {
                        while (xmlReader.NodeType != XmlNodeType.EndElement)
                        {
                            xmlReader.ReadStartElement("Group");
                            string name = xmlReader.ReadElementString("Name");
                            Group group = settingsCollection.GetOrCreateGroup(name);

                            if (!xmlReader.IsEmptyElement)
                            {
                                xmlReader.ReadStartElement("Settings");
                                while (xmlReader.NodeType != XmlNodeType.EndElement)
                                {
                                    Setting setting = new Setting();

                                    xmlReader.ReadStartElement("Setting");
                                    setting.Name = xmlReader.ReadElementString("Name");
                                    setting.Value = xmlReader.ReadElementString("Value");
                                    xmlReader.ReadEndElement();
                                    @group.Settings.Add(setting);
                                }

                                xmlReader.ReadEndElement();
                            }
                            else
                            {
                                xmlReader.Read();
                            }

                            xmlReader.ReadEndElement();
                        }
                    }
                    else
                    {
                        xmlReader.Read();
                    }

                    xmlReader.ReadEndElement();
                }
                finally
                {
                    xmlReader.Close();
                    reader.Close();
                }
            }
            return settingsCollection;
        }

        private void SaveSettings()
        {
            LoadSettings();

            if (!Writable)
            {
                throw new AccessViolationException("Implementation error: attempt to save settings, but SettingsManager is not writable in this application");
            }

            // delete existing settings file
            if (File.Exists(_settingsFile))
            {
                // REVISIT one user has reported a crash here on permission denied trying to delete our settings file
                // it could either have been https://github.com/HeliosVirtualCockpit/Helios/issues/290
                // or otherwise Control Center still holds the lock on this file itself due to a hung/crashed Control Center
                // or multiple threads trying to save the settings?
                //
                // related: The "single instance" mechanism ensures we don't have two Control Center processes running, but that does not
                // apply to Profile Editor.  XXX So there could be two Profile Editor instances running clobbering each other's settings.
                // This does not explain the reported problem but it is a major flaw in this design.  Do we need to detect this situation
                // and tell the user not to make changes in both instances?
                File.Delete(_settingsFile);
            }

            using (TextWriter writer = new StreamWriter(_settingsFile, false))
            {
                using (XmlWriter xmlWriter = XmlWriter.Create(writer, new XmlWriterSettings {Indent = true}))
                {
                    xmlWriter.WriteStartElement("HeliosSettings");
                    foreach (Group group in _settings)
                    {
                        xmlWriter.WriteStartElement("Group");
                        xmlWriter.WriteElementString("Name", group.Name);
                        xmlWriter.WriteStartElement("Settings");
                        foreach (Setting setting in group.Settings)
                        {
                            xmlWriter.WriteStartElement("Setting");
                            xmlWriter.WriteElementString("Name", setting.Name);
                            xmlWriter.WriteElementString("Value", setting.Value); 
                            xmlWriter.WriteEndElement();
                        }
                        xmlWriter.WriteEndElement();
                        xmlWriter.WriteEndElement();
                    }
                    xmlWriter.WriteEndElement();
                    xmlWriter.Close();
                }
                writer.Close();
            }
        }

        public void SaveSetting(string group, string name, string value)
        {
            LoadSettings();

            if (!Writable)
            {
                throw new AccessViolationException("Implementation error: attempt to write settings, but SettingsManager is not writable in this application");
            }

            Group settingGroup = _settings.GetOrCreateGroup(group);
            Setting setting;
            if (settingGroup.Settings.Contains(name))
            {
                setting = settingGroup.Settings[name];
                setting.Value = value;
            }
            else
            {
                setting = new Setting();
                setting.Name = name;
                setting.Value = value;
                settingGroup.Settings.Add(setting);
            }

            SaveSettings();
        }

        public string LoadSetting(string group, string name, string defaultValue)
        {
            LoadSettings();

            Group settingGroup = _settings.GetOrCreateGroup(group);
            Setting setting;
            if (settingGroup.Settings.Contains(name))
            {
                setting = settingGroup.Settings[name];
                return setting.Value;
            }
            else
            {
                return defaultValue;
            }
        }

        public T LoadSetting<T>(string group, string name, T defaultValue)
        {
            LoadSettings();

            Group settingGroup = _settings.GetOrCreateGroup(group);
            Setting setting;
            if (settingGroup.Settings.Contains(name))
            {
                setting = settingGroup.Settings[name];
                TypeConverter conv = TypeDescriptor.GetConverter(typeof(T));
                return (T)conv.ConvertFromInvariantString(setting.Value);
            }
            else
            {
                return defaultValue;
            }
        }


        public void SaveSetting<T>(string group, string name, T value)
        {
            LoadSettings();

            Group settingGroup = _settings.GetOrCreateGroup(group);
            Setting setting;
            if (settingGroup.Settings.Contains(name))
            {
                setting = settingGroup.Settings[name];
            }
            else
            {
                setting = new Setting();
                setting.Name = name;
                settingGroup.Settings.Add(setting);
            }

            TypeConverter conv = TypeDescriptor.GetConverter(typeof(T));
            setting.Value = conv.ConvertToInvariantString(value);

            SaveSettings();
        }

        public bool IsSettingAvailable(string group, string name)
        {
            LoadSettings();

            Group settingGroup = _settings.GetOrCreateGroup(group);
            return settingGroup.Settings.Contains(name);
        }

        #region ISettingsManager2

        public IEnumerable<string> EnumerateSettingNames(string group)
        {
            LoadSettings();
            Group settingGroup = _settings.GetOrCreateGroup(group);
            return settingGroup.Settings.Keys;
        }

        public void DeleteSetting(string group, string name)
        {
            LoadSettings();
            Group settingGroup = _settings.GetOrCreateGroup(group);
            settingGroup.Settings.Remove(name);
            SaveSettings();
        }

        /// <summary>
        /// reset the settings to the state they have on disk, useful for process that does not write settings but needs current version
        /// </summary>
        /// <param name="since"></param>
        /// <returns></returns>
        public bool SynchronizeSettings(DateTime? since)
        {
            try
            {
                if (since.HasValue)
                {
                    if (File.GetLastWriteTime(_settingsFile) < since.Value)
                    {
                        return false;
                    }
                }
                _settings = LoadSettingsFile();
                Synchronized?.Invoke(this, EventArgs.Empty);
                return true;
            }
            catch (System.Exception ex)
            {
                Logger.Error(ex, $"the settings file '{_settingsFile}' has become corrupted; writing new file from settings we have in memory");
                SaveSettings();
                return false;
            }
        }

        #endregion
    }
}
