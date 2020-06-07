// Copyright 2020 Helios Contributors
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
using System.Windows;
using System.Windows.Input;

namespace GadrocsWorkshop.Helios.Windows.ViewModel
{
    public class VersionCheckViewModel : HeliosViewModel<VersionChecker.Check>
    {
        public static readonly DependencyProperty SelectedReminderProperty =
            DependencyProperty.Register("SelectedReminder", typeof(string), typeof(VersionCheckViewModel),
                new PropertyMetadata("0", OnSelectedReminderChange));

        public static readonly DependencyProperty DownloadNarrativeProperty =
            DependencyProperty.Register("DownloadNarrative", typeof(string), typeof(VersionCheckViewModel),
                new PropertyMetadata(default(string)));

        private static void OnSelectedReminderChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            string cbTag = e.NewValue.ToString();
            if (cbTag == "Next")
            {
                ConfigManager.VersionChecker.ReminderForNextRelease = true;
            }
            else
            {
                ConfigManager.VersionChecker.RemindInDays(Convert.ToInt32(cbTag));
            }
        }

        /// <summary>
        /// normal constructor from model
        /// </summary>
        /// <param name="data"></param>
        public VersionCheckViewModel(VersionChecker.Check data) : base(data)
        {
            if (Data.AvailableVersion == null)
            {
                DownloadNarrative = "No online version check has been performed";
                CloseNarrative = "Ignore result until specified reminder time";
            }
            else if (CanDownload(null))
            {
                DownloadNarrative = $"Download version {Data.AvailableVersion}";
                CloseNarrative = "Ignore new version and remind again at specified time";
            }
            else
            {
                DownloadNarrative = "No newer version is available";
                CloseNarrative = "Close this dialog and remind again at specified time";
            }
        }

        /// <summary>
        /// backing field for property DownloadCommand, contains
        /// handlers for Download command
        /// </summary>
        private ICommand _someCommand;

        public static readonly DependencyProperty CloseNarrativeProperty = DependencyProperty.Register("CloseNarrative",
            typeof(string), typeof(VersionCheckViewModel), new PropertyMetadata(default(string)));

        /// <summary>
        /// handlers for Download command
        /// </summary>
        public ICommand DownloadCommand
        {
            get
            {
                _someCommand = _someCommand ?? new RelayCommand(PerformDownload, CanDownload);
                return _someCommand;
            }
        }

        private bool CanDownload(object commandParameter) => Data.HasNewVersion;

        private void PerformDownload(object commandParameter)
        {
            CloseWindow(commandParameter);
            if (!IsValidUrl(Data.DownloadUrl))
            {
                // don't start process
                throw new Exception($"program error: version check found new version but did not determine a valid download URL: '{Data.DownloadUrl}'");
            }
            System.Diagnostics.Process.Start(Data.DownloadUrl);
            ConfigManager.VersionChecker.HandleDowloadStarted(Data.DownloadUrl);
        }

        private static bool IsValidUrl(string dataDownloadUrl) => dataDownloadUrl != null && Uri.TryCreate(dataDownloadUrl, UriKind.Absolute, out Uri _);

        /// <summary>
        /// backing field for property OpenInBrowserCommand, contains
        /// handlers for OpenInBrowser command
        /// </summary>
        private ICommand _openInBrowserCommand;

        /// <summary>
        /// handlers for OpenBrowser command
        /// </summary>
        public ICommand OpenInBrowserCommand
        {
            get
            {
                _openInBrowserCommand = _openInBrowserCommand ?? new RelayCommand(OpenNotes, CanOpenNotes);
                return _openInBrowserCommand;
            }
        }

        private bool CanOpenNotes(object obj)
        {
            return Data.NotesUrl != null;
        }

        private void OpenNotes(object obj)
        {
            if (!IsValidUrl(Data.NotesUrl))
            {
                // don't start process
                throw new Exception($"program error: version check found new version but did not determine a valid release notes URL: '{Data.NotesUrl}'");
            }
            System.Diagnostics.Process.Start(Data.NotesUrl);
        }

        /// <summary>
        /// backing field for property CloseCommand, contains
        /// handlers for Close button
        /// </summary>
        private ICommand _closeCommand;

        /// <summary>
        /// handlers for Close button
        /// </summary>
        public ICommand CloseCommand
        {
            get
            {
                _closeCommand = _closeCommand ?? new RelayCommand(CloseWindow);
                return _closeCommand;
            }
        }

        private static void CloseWindow(object commandParameter)
        {
            if (commandParameter is IInputElement inputElement)
            {
                // fire a close command up the tree from the source of our command so it finds the right window
                SystemCommands.CloseWindowCommand.Execute(null, inputElement);
            }
        }

        /// <summary>
        /// create design data
        /// </summary>
        public VersionCheckViewModel() : base(new VersionChecker.Check(new Version(1, 6, 1000, 0)))
        {
            Data.AvailableVersion = new Version(1, 6, 1001, 2);
        }

        public string SelectedReminder
        {
            get => (string) GetValue(SelectedReminderProperty);
            set => SetValue(SelectedReminderProperty, value);
        }

        public string DownloadNarrative
        {
            get => (string) GetValue(DownloadNarrativeProperty);
            set => SetValue(DownloadNarrativeProperty, value);
        }

        public string CloseNarrative
        {
            get => (string) GetValue(CloseNarrativeProperty);
            set => SetValue(CloseNarrativeProperty, value);
        }
    }
}