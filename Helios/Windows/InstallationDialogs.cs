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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using GadrocsWorkshop.Helios.Windows.ViewModel;

namespace GadrocsWorkshop.Helios.Windows
{
    public class InstallationDialogs : IInstallationCallbacks
    {
        private readonly IInputElement _host;

        public InstallationDialogs(IInputElement host)
        {
            _host = host;
        }

        public InstallationPromptResult DangerPrompt(string title, string message, IList<StatusReportItem> details)
        {
            // show a custom dialog to explore the details
            InstallationDangerPromptModel dangerPrompt = new InstallationDangerPromptModel
            {
                Title = title,
                Message = message,
                Details = details.Select(item => new InterfaceStatusViewItem(item)).ToList()
            };
            Dialog.ShowModalCommand.Execute(new ShowModalParameter
            {
                Content = dangerPrompt
            }, _host);

            return dangerPrompt.Result;
        }

        public void Failure(string title, string message, IList<StatusReportItem> details)
        {
            // show a custom dialog to explore the details
            Dialog.ShowModalCommand.Execute(new ShowModalParameter
            {
                Content = new InstallationFailureModel
                {
                    Title = title,
                    Message = message,
                    Details = details.Select(item => new InterfaceStatusViewItem(item)).ToList()
                }
            }, _host);
        }

        public void Success(string title, string message, IList<StatusReportItem> details)
        {
            if (!ConfigManager.SettingsManager.LoadSetting("ProfileEditor", "DetailedViewOnConfigurationSuccess", false))
            {
                // don't present results for success
                return;
            }

            // show a custom dialog to explore the details
            Dialog.ShowModalCommand.Execute(new ShowModalParameter
            {
                Content = new InstallationSuccessModel
                {
                    Title = title,
                    Message = message,
                    Details = details.Select(item => new InterfaceStatusViewItem(item)).ToList()
                }
            }, _host);
        }
    }
}