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

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using GadrocsWorkshop.Helios.Windows.ViewModel;

namespace GadrocsWorkshop.Helios.Windows
{
    public class InstallationDialogs : IInstallationCallbacks2
    {
        private readonly IInputElement _host;

        public InstallationDialogs(IInputElement host)
        {
            _host = host;
        }

        public InstallationPromptResult DangerPrompt(string title, string message, IList<StructuredInfo> info, IList<StatusReportItem> details)
        {
            // show a custom dialog to explore the details
            InstallationDangerPromptModel dangerPrompt = new InstallationDangerPromptModel
            {
                Title = title,
                Message = message,
                Info = info,
                Details = details.Select(item => new InterfaceStatusViewItem(item)).ToList()
            };
            Dialog.ShowModalCommand.Execute(new ShowModalParameter
            {
                Content = dangerPrompt
            }, _host);

            return dangerPrompt.Result;
        }

        public void Failure(string title, string message, IList<StructuredInfo> info, IList<StatusReportItem> details)
        {
            // show a custom dialog to explore the details
            Dialog.ShowModalCommand.Execute(new ShowModalParameter
            {
                Content = new InstallationFailureModel
                {
                    Title = title,
                    Message = message,
                    Info = info,
                    Details = details.Select(item => new InterfaceStatusViewItem(item)).ToList()
                }
            }, _host);
        }

        public bool Success(string title, string message, IList<StructuredInfo> info, IList<StatusReportItem> details)
        {
            if (!ConfigManager.SettingsManager.LoadSetting("ProfileEditor", "DetailedViewOnConfigurationSuccess", false)
            )
            {
                // don't present results for success
                return false;
            }

            // show a custom dialog to explore the details
            Dialog.ShowModalCommand.Execute(new ShowModalParameter
            {
                Content = new InstallationSuccessModel
                {
                    Title = title,
                    Message = message,
                    Info = info,
                    Details = details.Select(item => new InterfaceStatusViewItem(item)).ToList()
                }
            }, _host);
            return true;
        }

        public void ImportantMessage(string title, string message, IList<StructuredInfo> info)
        {
            Dialog.ShowModalCommand.Execute(new ShowModalParameter
            {
                Content = new InstallationMessageModel
                {
                    Title = title,
                    Message = message,
                    Info = info
                }
            }, _host);
        }

        public InstallationPromptResult DangerPrompt(string title, string message, IList<StatusReportItem> status)
        {
            return DangerPrompt(title, message, null, status);
        }

        public void Failure(string title, string message, IList<StatusReportItem> status)
        {
            Failure(title, message, null, status);
        }

        public bool Success(string title, string message, IList<StatusReportItem> status)
        {
            return Success(title, message, null, status);
        }

        public void ImportantMessage(string title, string message)
        { 
            ImportantMessage(title, message, null);
        }
    }
}