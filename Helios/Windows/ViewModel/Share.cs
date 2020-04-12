using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace GadrocsWorkshop.Helios.Windows.ViewModel
{
    /// <summary>
    /// a view model for sharing an IList of InterfaceStatus
    /// </summary>
    public class Share<T> : DependencyObject
    {
        /// <summary>
        /// backing field for property CopyCommand, contains
        /// handler for Share | Copy
        /// </summary>
        private ICommand _copyCommand;

        /// <summary>
        /// backing field for property FileBugCommand, contains
        /// handler for Share | FileBug
        /// </summary>
        private ICommand _fileBugCommand;

        private IList<T> _report;

        public string Text { get; }

        internal class EmptyStatus
        {
            public string Name { get; }
            public IList<StatusReportItem> Report { get; }

            internal EmptyStatus()
            {
                Name = "Share";
                Report = new List<StatusReportItem>
                {
                    new StatusReportItem
                    {
                        Status = "No installed interfaces support configuration check.",
                        Flags = StatusReportItem.StatusFlags.ConfigurationUpToDate
                    }
                };
            }
        }

        private static readonly IList<EmptyStatus> EmptyReport = new List<EmptyStatus>
        {
            new EmptyStatus()
        };

        /// <summary>
        /// string processor to remove user name from report and shorten some paths
        /// </summary>
        private class PrivacyConverter : JsonConverter<string>
        {
            public override string ReadJson(JsonReader reader, Type objectType, string existingValue,
                bool hasExistingValue, JsonSerializer serializer) => reader.ReadAsString();

            public override void WriteJson(JsonWriter writer, string value, JsonSerializer serializer)
            {
                writer.WriteValue(Util.Anonymizer.Anonymize(value));
            }
        }

        public Share(IList<T> report)
        {
            _report = report;

            // XXX enum converter
            // XXX flags converter
            if (_report.Count == 0)
            {
                Text = JsonConvert.SerializeObject(EmptyReport, Formatting.Indented);
            }
            else
            {
                Text = JsonConvert.SerializeObject(_report, new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    Converters = new List<JsonConverter>
                    {
                        new PrivacyConverter()
                    }
                });
            }
        }

        /// <summary>
        /// handler for Share | Copy
        /// </summary>
        public ICommand CopyCommand
        {
            get
            {
                _copyCommand = _copyCommand ?? new RelayCommand(parameter =>
                {
                    Clipboard.SetText(Text);
                    if (parameter is IInputElement inputElement)
                    {
                        // fire a close command up the tree from the source of our command so it finds the right window
                        SystemCommands.CloseWindowCommand.Execute(null, inputElement);
                    }
                });
                return _copyCommand;
            }
        }

        /// <summary>
        /// handler for Share | FileBug
        /// </summary>
        public ICommand FileBugCommand
        {
            get
            {
                _fileBugCommand = _fileBugCommand ?? new RelayCommand(FileBug);
                return _fileBugCommand;
            }
        }

        private void FileBug(object parameter)
        {
            string repo = ConfigManager.SettingsManager.LoadSetting("Helios", "Repo", null);

            if (repo == null)
            {
                repo = ConfigManager.SettingsManager.LoadSetting("Helios", "LastestGitHubDownloadUrl", null);
            }
            else
            {
                if (!repo.EndsWith("/"))
                {
                    repo += ('/');
                }
            }

            if (repo == null)
            {
                // last resort
                repo = "https://github.com/BlueFinBima/Helios/";
            }

            Match match = new Regex("^https://github.com/[a-zA-Z0-9_]+/[a-zA-Z0-9_]+/").Match(repo);
            if (match.Success)
            {
                // prepare the clipboard with a file reference
                string fileName = $"Helios_status_report_{System.DateTime.Now.ToFileTime()}.txt";
                string tempPath = Path.GetTempPath();
                string path = Path.Combine(tempPath, fileName);
                File.WriteAllText(path, Text);

                // NOTE: github claims to support paste for attachments, but it doesn't work so we have to drag and drop
                string explorer =
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "explorer.exe");
                System.Diagnostics.Process.Start(tempPath);

                // only take the part we validated
                repo = match.Groups[0].Value;
                string encoded = Uri.EscapeDataString($"[Helios has written the report to the file '{fileName}' and opened its containing folder in Explorer.  Please drag and drop it from Explorer to here and replace this message with a description of the problem you are reporting.]\n\n");

                // launch the default browser
                string url = $"{repo}issues/new?body={encoded}";
                System.Diagnostics.Process.Start(url);
            }

            // close dialog
            if (parameter is IInputElement inputElement)
            {
                // fire a close command up the tree from the source of our command so it finds the right window
                SystemCommands.CloseWindowCommand.Execute(null, inputElement);
            }
        }

        public string Title
        {
            get => (string) GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(Share<T>),
                new PropertyMetadata("Share anonymized Status Report"));
    }
}