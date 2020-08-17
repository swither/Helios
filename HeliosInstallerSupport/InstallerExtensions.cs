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
