//  Copyright 2014 Craig Courtney
//    
//  Helios is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  Helios is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.


// assign an alias to this namespace so we can refer to it without the space is the assembly name

using GadrocsWorkshop.Helios.Windows;
using System.Windows.Markup;

// NOTE: URL used to resolve XAML references without breaking on our assembly name that has spaces in it
[assembly: XmlnsDefinition("http://Helios.local/ProfileEditor", "GadrocsWorkshop.Helios.ProfileEditor")]
namespace GadrocsWorkshop.Helios.ProfileEditor
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Windows;
    using System.Windows.Threading;

    /// <summary>
    /// Application class for the Profile Editor, instantiated before even the main window
    /// </summary>
    public partial class App : Application
    {
        #region Properties

        public string StartupFile { get; private set; }

        #endregion

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            Current.Dispatcher.UnhandledException += App_DispatcherUnhandledException;

            CommandLineOptions options = Util.CommandLineOptions.Parse(new CommandLineOptions(), e.Args, out int exitCode);

            // react to options or defaults
            if (options.Profiles.Any())
            {
                StartupFile = options.Profiles.Last();
            }

            // start up Helios
            string documentPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                RunningVersion.IsDevelopmentPrototype ? options.DevDocumentsName : options.DocumentsName);
            HeliosInit.Initialize(documentPath, "ProfileEditor.log", options.LogLevel);
            if (options.GenerateInterfaceSchema)
            {
                Json.InterfaceExport exporter = new Json.InterfaceExport();
                exporter.GenerateInterfaceSchema();
            }
            if (options.GenerateInterfaceJson)
            {
                Json.InterfaceExport exporter = new Json.InterfaceExport();
                exporter.GenerateInterfaceJson();
            }
            if (options.GenerateInterfaceJson || options.GenerateInterfaceSchema)
            {
                Current.Shutdown();
            }
            ConfigManager.LogManager.LogInfo("Starting Editor");
        }

        protected override void OnExit(ExitEventArgs e)
        {
            HeliosInit.OnShutdown();
            base.OnExit(e);
        }

        /// <summary>
        /// last resort exception sink, does not assume the application is operable other than logging
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            ExceptionViewer.DisplayUnhandledException(e);
        }
    }
}
