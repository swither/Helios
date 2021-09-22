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

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Newtonsoft.Json;

namespace GadrocsWorkshop.Helios.Windows.ViewModel
{
    public class InstallationMessageViewModelBase
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public IList<StructuredInfo> Info{ get; set; }
    }

    public class InstallationResultViewModel : InstallationMessageViewModelBase
    {
        private IList<InterfaceStatusViewItem> _details = new List<InterfaceStatusViewItem>();

        public class SingleReport
        {
            [JsonProperty("name")] public string Name => "Configuration Results";

            [JsonProperty("report")] public IEnumerable<StatusReportItem> Report;

            public SingleReport(IEnumerable<InterfaceStatusViewItem> details)
            {
                Report = details.Select(item => item.Data).ToList();
            }
        }

        public IList<InterfaceStatusViewItem> Details
        {
            get => _details;
            internal set
            {
                _details = value;
                Recommendations = new HashSet<string>(Details
                        .Where(item => item.HasRecommendation)
                        .Select(item => item.TextLine2))
                    .ToList();
            }
        }

        public IList<string> Recommendations { get; private set; } = new List<string>();

        /// <summary>
        /// backing field for property ShareCommand, contains
        /// handlers for status share command
        /// </summary>
        private ICommand _shareCommand;

        /// <summary>
        /// handlers for status share command
        /// </summary>
        public ICommand ShareCommand
        {
            get
            {
                _shareCommand = _shareCommand ?? new RelayCommand(parameter =>
                {
                    // execute a modal dialog to share the report
                    Dialog.ShowModalCommand.Execute(
                        new ShowModalParameter
                        {
                            Content = new ShareInstallationResults(new List<SingleReport>
                            {
                                new SingleReport(Messages.Concat(Details))
                            })
                        },
                        parameter as IInputElement);
                });
                return _shareCommand;
            }
        }

        private IEnumerable<InterfaceStatusViewItem> Messages
        {
            get
            {
                if (!string.IsNullOrEmpty(Title))
                {
                    yield return WrapMessage(Title);
                }
                if (!string.IsNullOrEmpty(Message))
                {
                    yield return WrapMessage(Message);
                }
            }
        }

        private static InterfaceStatusViewItem WrapMessage(string status) =>
            new InterfaceStatusViewItem(new StatusReportItem
            {
                Status = status
            });
    }

    public class InstallationDangerPromptModel : InstallationResultViewModel
    {
        public InstallationPromptResult Result { get; set; } = InstallationPromptResult.Cancel;

        /// <summary>
        /// backing field for property OkCommand, contains
        /// handler for Ok button
        /// </summary>
        private ICommand _okCommand;

        /// <summary>
        /// handler for Ok button
        /// </summary>
        public ICommand OkCommand
        {
            get
            {
                _okCommand = _okCommand ?? new RelayCommand(parameter =>
                {
                    Result = InstallationPromptResult.Ok;

                    // close dialog
                    if (parameter is IInputElement inputElement)
                    {
                        // fire a close command up the tree from the source of our command so it finds the right window
                        SystemCommands.CloseWindowCommand.Execute(null, inputElement);
                    }
                });
                return _okCommand;
            }
        }
    }

    public class InstallationFailureModel : InstallationResultViewModel
    {
        // no code, only separate for data template selection
    }

    public class InstallationSuccessModel : InstallationResultViewModel
    {
        // no code, only separate for data template selection
    }

    public class InstallationMessageModel : InstallationMessageViewModelBase
    {
        // no code, only separate for data template selection
    }
}