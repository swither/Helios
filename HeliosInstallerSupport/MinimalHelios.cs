using GadrocsWorkshop.Helios;
using GadrocsWorkshop.Helios.Util;
using System;
using System.IO;
using System.IO.Packaging;

namespace HeliosInstallerSupport
{
    class MinimalHelios: IDisposable
    {
        public MinimalHelios()
        {
            HeliosInit.Initialize(FindHeliosDocuments(), "", LogLevel.Error, new HeliosApplication
            {
                AllowPlugins = false,
                ConnectToServers = false,
                SettingsAreWritable = false,
                ShowDesignTimeControls = false,
                AllowLogging = false
            });

            // we are not creating an Application object, so we have to at least static initialize this class 
            // so that pack URIs will work
            _ = PackUriHelper.UriSchemePack;
        }

        public static bool CanStart(out string message)
        {
            string requiredPath = FindHeliosDocuments();
            bool result = Directory.Exists(requiredPath);
            message = result ? null : $"could not find Helios folder '{requiredPath}'";
            return result;
        }

        public static string FindHeliosDocuments()
        {
            // get defaults, without an actual command line
            CommonCommandLineOptions options = new CommonCommandLineOptions();
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) ?? "",
                RunningVersion.IsDevelopmentPrototype ? options.DevDocumentsName: options.DocumentsName);
        }

        public void Dispose()
        {
            HeliosInit.OnShutdown();
        }
    }
}
