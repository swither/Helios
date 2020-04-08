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

using CommandLine;
using CommandLine.Text;

namespace GadrocsWorkshop.Helios.ControlCenter
{
    using Microsoft.Shell;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Threading;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application, ISingleInstanceApp
    {
        private const string InstanceUniqueName = "HeliosApplicationInstanceMutex";
        private string _startupProfile = null;

        /// <summary>
        /// Application Entry Point.
        /// </summary>
        [System.STAThreadAttribute()]
        public static void Main()
        {
            if (SingleInstance<App>.InitializeAsFirstInstance(InstanceUniqueName))
            {
                //SplashScreen splashScreen = null;

                //splashScreen = new SplashScreen("splash_logo.png");
                //splashScreen.Show(false);
                GadrocsWorkshop.Helios.ControlCenter.App app = new GadrocsWorkshop.Helios.ControlCenter.App();
                app.InitializeComponent();
                //Thread.Sleep(1000);
                //splashScreen.Close(TimeSpan.FromMilliseconds(500));
                app.Run();
                SingleInstance<App>.Cleanup();
            }
        }

        #region Properties

        public string StartupProfile
        {
            get { return _startupProfile; }
            private set { _startupProfile = value; }
        }

        #endregion

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            CommandLineOptions options = Util.CommandLineOptions.Parse(new CommandLineOptions(), e.Args, out int exitCode);

            // react to options or defaults
            if (options.Profiles != null && options.Profiles.Any())
            {
                StartupProfile = options.Profiles.Last();
            }

            // start up Helios
            string documentPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), options.DocumentPath);
            HeliosInit.Initialize(documentPath, "ControlCenter.log", options.LogLevel, new HeliosApplication(false));

            // need to defer exit until after we initialize Helios or our main window will crash
            if (exitCode < 0)
            {
                Current.Shutdown();
            }
        }

        public bool SignalExternalCommandLineArgs(IList<string> args)
        {
            CommandLineOptions options = Util.CommandLineOptions.Parse(new CommandLineOptions(), args.ToArray(), out int exitCode);
            if (options.Exit)
            {
                ApplicationCommands.Close.Execute(null, Current.MainWindow);
            }
            else if (options.Profiles != null && options.Profiles.Any() &&
                     File.Exists(options.Profiles.Last()))
            {
                ControlCenterCommands.RunProfile.Execute(options.Profiles.Last(),
                    Current.MainWindow);
            }
            return exitCode == 0;
        }

        // XXX re-enable this and test
        void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            System.Windows.MessageBox.Show(string.Format("An error occured: {0}", e.Exception.Message), "Error");
            e.Handled = true;
            //ConfigManager.LogManager.LogError("Unhandled Exception", e.Exception);
            //e.Handled = false;
        }
    }
}
