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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using GadrocsWorkshop.Helios.Windows;
using GadrocsWorkshop.Helios.Windows.ViewModel;

namespace GadrocsWorkshop.Helios.Patching
{
    /// <summary>
    /// configuration model for a patched path and its filtering status
    /// </summary>
    public class PatchedPath: NotificationObject
    {
        /// <summary>
        /// backing field for property Path, contains
        /// relative path within the patch destination
        /// </summary>
        private string _path;

        /// <summary>
        /// backing field for property Filtered, contains
        /// false if this path has been suppressed/filtered by the user, meaning we won't patch it
        /// </summary>
        private bool _allowed;

        /// <summary>
        /// backing field for property AllowedCommand, contains
        /// handler for interaction with the visual representation (such as checkbox) of the Allowed property
        /// </summary>
        private ICommand _allowedCommand;

        /// <summary>
        /// backing field for property IsWarningSuppressed which is used to limit the times that the 
        /// Advanced Operation Requested warning message is displayed 
        /// </summary>
        private bool _isWarningSuppressed = false;

        private const string SETTINGS_GROUP = "PatchExclusions";

        /// <summary>
        /// settings format for "allowed" encoded as an enum so that we can extend it later to support different modes
        /// </summary>
        public enum Exclusion
        {
            Excluded,
            Included
        }

        internal PatchedPath(string path)
        {
            _path = path;
            _allowed = Exclusion.Included == ConfigManager.SettingsManager.LoadSetting("PatchExclusions", Path, Exclusion.Included);
        }

        /// <summary>
        /// handler for interaction with the visual representation (such as checkbox) of the Allowed property
        /// </summary>
        public ICommand AllowedCommand
        {
            get
            {
                _allowedCommand = _allowedCommand ?? new RelayCommand(parameter =>
                {
                    CheckBox source = (CheckBox) parameter;
                    if (source.IsChecked ?? true)
                    {
                        // nothing special
                        return;
                    }

                    if (_isWarningSuppressed)
                    {
                        // display a warning
                        InstallationDangerPromptModel warningModel = new InstallationDangerPromptModel
                        {
                            Title = "Advanced Operation Requested",
                            Message = "You are about to take ownership of a file that is normally patched for you by Helios.  This means Helios will no "
                            + "longer patch this file.  Helios will no longer be able to ensure that your viewports are correctly named and you will be on your own."
                            + Environment.NewLine
                            + Environment.NewLine
                            + $"You will need to ensure that you also integrate any changes required by Helios into the file {Path} in each of your patched locations. "
                            + "Note that this file may not exist in all patched locations depending on the installed versions."
                        };
                        Dialog.ShowModalCommand.Execute(new ShowModalParameter
                        {
                            Content = warningModel
                        }, source);

                        if (warningModel.Result == InstallationPromptResult.Cancel)
                        {
                            // undo it
                            Allowed = true;
                        }
                    }
                });
                return _allowedCommand;
            }
        }

        /// <summary>
        /// relative path within the patch destination
        /// </summary>
        public string Path
        {
            get => _path;
            set
            {
                if (_path != null && _path == value) return;
                string oldValue = _path;
                _path = value;
                OnPropertyChanged("Path", oldValue, value, true);
            }
        }

        /// <summary>
        /// false if this path has been suppressed/filtered by the user, meaning we won't patch it
        /// </summary>
        public bool Allowed
        {
            get => _allowed;
            set
            {
                if (_allowed == value) return;
                bool oldValue = _allowed;
                _allowed = value;

                // we record the opposite of "value" and only if it is not default (usually there are no exclusions)
                if (value)
                {
                    if (ConfigManager.SettingsManager is ISettingsManager2 clean)
                    {
                        clean.DeleteSetting(SETTINGS_GROUP, Path);
                    }
                    else
                    {
                        ConfigManager.SettingsManager.SaveSetting(SETTINGS_GROUP, Path, Exclusion.Included);
                    }
                }
                else
                {
                    ConfigManager.SettingsManager.SaveSetting(SETTINGS_GROUP, Path, Exclusion.Excluded);
                }
                OnPropertyChanged("Allowed", oldValue, value, true);
            }
        }

        internal static IEnumerable<string> ExcludedPaths
        {
            get
            {
                if (!(ConfigManager.SettingsManager is ISettingsManager2 enumerable))
                {
                    return new string[0];
                }
                return enumerable.EnumerateSettingNames(SETTINGS_GROUP).Where(IsExcluded);
            }
        }

        internal static bool IsExcluded(string path)
        {
            return Exclusion.Excluded == ConfigManager.SettingsManager.LoadSetting(SETTINGS_GROUP, path, Exclusion.Included);
        }

        internal bool IsWarningSuppressed
        {
            get => _isWarningSuppressed;
            set => _isWarningSuppressed = value;
        }
    }
}