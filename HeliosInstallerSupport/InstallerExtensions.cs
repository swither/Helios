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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using GadrocsWorkshop.Helios.Interfaces.DCS.Common;

namespace HeliosInstallerSupport
{
    [RunInstaller(true)]
    public class InstallerExtensions : System.Configuration.Install.Installer
    {
        public static void RunHeliosUninstall()
        {
            using (new MinimalHelios())
            {
                DCSExportConfiguration.RemoveExportLuaHooks();
            }
        }

        public void TestRun()
        {
            OnBeforeUninstall(new Dictionary<string, object>());
        }

        // XXX for safety, we should also handle post install, so that if the helper fails to run we roll back
        // otherwise, we can get in a situation where we can install but not uninstall
        // WARNING: don't run the minimal Helios on install or we will create folders

        // REVISIT: make this helper conditional on a public property that can be set from command line to turn it off for such recovery cases
        // NOTE: command line can apparently only set properties that already exist in the table, and the setup project does not let us
        // create one so we have to do it in the VBS

        protected override void OnBeforeUninstall(IDictionary savedState)
        {
            if (!MinimalHelios.CanStart(out string failureMessage))
            {
                // don't try
                // MessageBox.Show(failureMessage, "Helios uninstall helper will not run");
                return;
            }

            try
            {
                RunHeliosUninstall();
            }
            catch (Exception ex)
            {
                string message = $"{ex}{Environment.NewLine}{ex.StackTrace}";
                MessageBox.Show(message, "Failed to execute minimal Helios from installer");
            }

            base.OnBeforeUninstall(savedState);
        }

#if false
using System.Linq;
using Microsoft.Win32;

        // testing only code
        // REVISIT: remove
        private static void DumpUserPathsFromRegistry()
        {
            RegistryKey key =
                Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Shell Folders");
            if (key == null)
            {
                MessageBox.Show("key not found", "testing");
            }
            else
            {
                IEnumerable<string> values =
                    key.GetValueNames().Select(valueName => $"{valueName} = {key.GetValue(valueName)}");
                MessageBox.Show($"{string.Join(Environment.NewLine, values)}", "testing");
            }
        }
#endif
    }
}