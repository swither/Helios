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

using GadrocsWorkshop.Helios.Util;
using GadrocsWorkshop.Helios.Util.DCS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GadrocsWorkshop.Helios.Windows.Controls.DCS
{
    /// <summary>
    /// Installation Locations section shared by multiple dialogs, currently includes all its view model code
    /// </summary>
    public partial class InstallationLocationsControl : ItemsControl
    {
        static InstallationLocationsControl()
        {
            System.Type ownerType = typeof(InstallationLocationsControl);
            CommandManager.RegisterClassCommandBinding(ownerType, new CommandBinding(AddInstallationLocationCommand, Add_Executed, CanExecuteAddRemove));
            CommandManager.RegisterClassCommandBinding(ownerType, new CommandBinding(RemoveInstallationLocationCommand, Remove_Executed, CanExecuteAddRemove));
        }

        private static void CanExecuteAddRemove(object target, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ((InstallationLocationsControl)target).CanConfigureLocations;
        }

        public InstallationLocationsControl()
        {
            DataContext = InstallationLocations.Singleton;
            InitializeComponent();
            UpdateStatus();
            if (Status == StatusCodes.NoLocations)
            {
                // this is not done via a style, because we don't want it to automatically collapse when the status changes
                GroupIsExpanded = true;
            }
            InstallationLocations.Singleton.Enabled += Locations_Changed;
            InstallationLocations.Singleton.Disabled += Locations_Changed;

            // we also need to register these, because another dialog could be editing this collection also
            InstallationLocations.Singleton.Added += Locations_Changed;
            InstallationLocations.Singleton.Removed += Locations_Changed;
            InstallationLocations.Singleton.RemoteChanged += Locations_Changed;
        }

        public void Dispose()
        {
            InstallationLocations.Singleton.Enabled -= Locations_Changed;
            InstallationLocations.Singleton.Disabled -= Locations_Changed;
            InstallationLocations.Singleton.Added -= Locations_Changed;
            InstallationLocations.Singleton.Removed -= Locations_Changed;
            InstallationLocations.Singleton.RemoteChanged -= Locations_Changed;
        }

        private void Locations_Changed(object sender, EventArgs e)
        {
            UpdateStatus();
        }

        private void UpdateStatus()
        {
            if (InstallationLocations.Singleton.IsRemote)
            {
                CanConfigureLocations = false;
                Status = StatusCodes.NotApplicable;
            }
            else
            {
                CanConfigureLocations = true;
                Status = InstallationLocations.Singleton.Active.Any() ? StatusCodes.UpToDate : StatusCodes.NoLocations;
            }
        }

        #region Commands
        public static readonly RoutedUICommand AddInstallationLocationCommand = new RoutedUICommand("Adds a new DCS installation location.", "AddInstallationLocation", typeof(InstallationLocationsControl));
        public static readonly RoutedUICommand RemoveInstallationLocationCommand = new RoutedUICommand("Removes a DCS installation location.", "RemoveInstallationLocation", typeof(InstallationLocationsControl));

        private static void Add_Executed(object target, ExecutedRoutedEventArgs e)
        {
            List<string> guesses = InstallationLocations.GenerateDcsRootDirectoryGuesses();

            string guessName = InstallationLocation.AUTO_UPDATE_CONFIG;
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = $"Navigate to DCS Installation and select {InstallationLocation.AUTO_UPDATE_CONFIG} or {InstallationLocation.DCS_EXE} file",
                InitialDirectory = "",
                FileName = guessName,
                DefaultExt = ".cfg",
                CheckPathExists = true,
                DereferenceLinks = true,
                Multiselect = false,
                ValidateNames = true,
                Filter = $"DCS|{InstallationLocation.AUTO_UPDATE_CONFIG};{InstallationLocation.DCS_EXE}",
                CheckFileExists = true
            };

            foreach (string guess in guesses)
            {
                if (openFileDialog.InitialDirectory == "")
                {
                    openFileDialog.InitialDirectory = guesses[0];
                }
                else
                {
                    openFileDialog.CustomPlaces.Add(new Microsoft.Win32.FileDialogCustomPlace(guess));
                }
            }

            if (openFileDialog.ShowDialog() == true)
            {
                if (InstallationLocation.TryBrowseLocation(openFileDialog.FileName, out InstallationLocation location))
                {
                    // silently ignore duplicates
                    _ = InstallationLocations.Singleton.TryAdd(location);
                }
            }
            ((InstallationLocationsControl)target).UpdateStatus();
        }

        private static void Remove_Executed(object target, ExecutedRoutedEventArgs e)
        {
            InstallationLocations.Singleton.TryRemove(e.Parameter as InstallationLocation);
            ((InstallationLocationsControl)target).UpdateStatus();
        }

        #endregion

        #region Properties
        public StatusCodes Status
        {
            get => (StatusCodes)GetValue(StatusProperty);
            set => SetValue(StatusProperty, value);
        }

        public bool GroupIsExpanded
        {
            get => (bool) GetValue(GroupIsExpandedProperty);
            set => SetValue(GroupIsExpandedProperty, value);
        }

        public bool CanConfigureLocations
        {
            get => (bool)GetValue(CanConfigureLocationsProperty);
            set => SetValue(CanConfigureLocationsProperty, value);
        }

        public static readonly DependencyProperty CanConfigureLocationsProperty =
            DependencyProperty.Register("CanConfigureLocations", typeof(bool), typeof(InstallationLocationsControl), new PropertyMetadata(true));

        public static readonly DependencyProperty StatusProperty =
            DependencyProperty.Register("Status", typeof(StatusCodes), typeof(InstallationLocationsControl), new PropertyMetadata(StatusCodes.Unknown));

        public static readonly DependencyProperty GroupIsExpandedProperty = 
            DependencyProperty.Register("GroupIsExpanded", typeof(bool), typeof(InstallationLocationsControl), new PropertyMetadata(false));

        #endregion
    }
}
