using System;
using System.Collections;
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
                MessageBox.Show(failureMessage, "Helios uninstall helper will not run");
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
    }
}
