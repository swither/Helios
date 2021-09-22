//  Copyright 2014 Craig Courtney
//  Copyright 2020 Helios Contributors
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

using GadrocsWorkshop.Helios.Util;
using System.Collections.Generic;
using System.Diagnostics;

namespace GadrocsWorkshop.Helios
{
    using GadrocsWorkshop.Helios.ComponentModel;
    using System;
    using System.IO;
    using System.Reflection;
    using System.Windows.Media;

    /// <summary>
    /// Class to initialize the Helios Runtime.
    /// </summary>
    public static class HeliosInit
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// initialize only the document path and logging functionality, so that Helios classes that log can be used, without starting Helios
        /// </summary>
        /// <param name="docPath"></param>
        /// <param name="logLevel"></param>
        /// <param name="application">optional Application specification that can customize features of Helios system</param>
        public static void InitializeLogging(string docPath, LogLevel logLevel, HeliosApplication application = null)
        {
            // Create documents directory if it does not exist
            ConfigManager.DocumentPath = docPath;

            if (application?.AllowLogging ?? true)
            {
                // start up logging
                InitializeNLog(logLevel);
            }
            else
            {
                DisableLogging();
            }

            // create this legacy interface for other DLLs that don't have access to NLog
            ConfigManager.LogManager = new LogManager(logLevel);
        }

        private static void DisableLogging()
        {
            NLog.LogManager.Configuration = new NLog.Config.LoggingConfiguration();
            NLog.LogManager.Flush();
        }

        /// <summary>
        /// Start Helios global objects in ConfigManager
        /// </summary>
        /// <param name="docPath"></param>
        /// <param name="logFileNameIgnored">is ignored in this implementation, as log name is from the process name of the running process</param>
        /// <param name="logLevel"></param>
        /// <param name="application"></param>
        public static void Initialize(string docPath, string logFileNameIgnored, LogLevel logLevel, HeliosApplication application = null)
        {
            // check OS and build
            CheckPlatform();

            // initialize logging and documents folder
            InitializeLogging(docPath, logLevel, application);

            // previous versions of Helios got their log file name from here, now it is based on the process name of the running process
            _ = logFileNameIgnored;

            // initialize the rest of Helios
            Logger.Info("Helios Version " + RunningVersion.FromHeliosAssembly());

            ConfigManager.Application = application ?? new HeliosApplication();
            bool applicationSettingsAreWritable = ConfigManager.Application.SettingsAreWritable;
            InitializeSettings(applicationSettingsAreWritable);
            ConfigManager.VersionChecker = new VersionChecker(ConfigManager.SettingsManager, applicationSettingsAreWritable);
            ConfigManager.UndoManager = new UndoManager();
            ConfigManager.ProfileManager = new ProfileManager();
            ConfigManager.FontManager = new FontManager();
            ConfigManager.FontManager.LoadInstalledPrivateFonts();
            ConfigManager.ImageManager = new ImageManager(ConfigManager.ImagePath);
            ConfigManager.DisplayManager = new DisplayManager();
            ConfigManager.ModuleManager = new ModuleManager(ConfigManager.ApplicationPath);
            ConfigManager.TemplateManager = new TemplateManager(ConfigManager.TemplatePath);
            
            Logger.Debug("Searching for Helios modules in libraries");

            Assembly execAssembly = Assembly.GetExecutingAssembly();
            LoadModule(execAssembly);

            LoadPlugins(execAssembly);

            if (RenderCapability.Tier == 0)
            {
                Logger.Warn("Hardware rendering is not available on this machine.  Helios will consume large amounts of CPU.");
            }
        }

        private static void LoadPlugins(Assembly execAssembly)
        {
            if (!ConfigManager.Application.AllowPlugins)
            {
                Logger.Debug("Plugins disabled on this installation");
                return;
            }

            string pluginsFolder = Path.Combine(Path.GetDirectoryName(execAssembly.Location) ?? "", "Plugins");
            if (Directory.Exists(pluginsFolder))
            {
                foreach (string pluginPath in Directory.EnumerateFiles(pluginsFolder, "*.dll", SearchOption.AllDirectories))
                {
                    LoadModule(pluginPath);
                }
            }

            // REVISIT: move this to plugins folder and get rid of special case if we can add the check for system libraries
            // to HeliosModuleAttribute.  This would be relevant if we create other plugins for DCSFlightPanels and the like
            String phidgetsDllPath = Path.Combine(Environment.SystemDirectory, "phidget21.dll");
            if (File.Exists("Phidgets.dll") && File.Exists(phidgetsDllPath))
            {
                LoadModule("Phidgets.dll");
            }
        }

        public static void InitializeSettings(bool settingsAreWritable)
        {
            ConfigManager.SettingsManager =
                new SettingsManager(Path.Combine(ConfigManager.DocumentPath, "HeliosSettings.xml"))
                {
                    Writable = settingsAreWritable
                };
        }

        [Conditional("HELIOS_64BIT")]
        private static void CheckPlatform()
        {
            if (!Environment.Is64BitOperatingSystem)
            {
                throw new PlatformNotSupportedException("Helios must be executed on a 64-bit Windows OS");
            }

            if (!Environment.Is64BitProcess)
            {
                throw new PlatformNotSupportedException("Helios must be executed as a 64-bit application");
            }
        }

        private static void InitializeNLog(LogLevel logLevel)
        {
            // tell NLog config where to log
            NLog.GlobalDiagnosticsContext.Set("documentsPath", ConfigManager.DocumentPath);

            // tell NLog config the log level
            NLog.LogLevel nlogLevel;
            bool badLogLevel = false;
            switch (logLevel)
            {
                case LogLevel.All:
                case LogLevel.Error:
                    nlogLevel = NLog.LogLevel.Error;
                    break;
                case LogLevel.Warning:
                    nlogLevel = NLog.LogLevel.Warn;
                    break;
                case LogLevel.Info:
                    nlogLevel = NLog.LogLevel.Info;
                    break;
                case LogLevel.Debug:
                    nlogLevel = NLog.LogLevel.Debug;
                    break;
                default:
                    nlogLevel = NLog.LogLevel.Info;
                    badLogLevel = true;
                    break;
            }

            NLog.GlobalDiagnosticsContext.Set("logLevel", nlogLevel.ToString());

            // now that we are configured, we can log this
            if (badLogLevel)
            {
                Logger.Warn("Log level {Level} is not valid for command line selection; Info level used instead", logLevel);
            }

            NLog.LogManager.Configuration = NLog.LogManager.Configuration?.Reload();
            NLog.LogManager.Flush();
        }

        // special cases of libraries that we fail to properly ignore otherwise
        private static readonly HashSet<string> ForbiddenPlugins = new HashSet<string>
        {
            "Phidget22.NET.dll",
            "phidget22.dll"
        };

        private static void LoadModule(string moduleFileName)
        {
            if (!File.Exists(moduleFileName))
            {
                return;
            }

            if (ForbiddenPlugins.Contains(Path.GetFileName(moduleFileName)))
            {
                Logger.Info($"Ignoring library that is known to not be one of our plugins: '{moduleFileName}'");
                return;
            }

            if (!ModuleUtility.IsProbablyAssembly(moduleFileName))
            {
                Logger.Info($"Ignoring library that does not appear to be a .NET assembly: '{moduleFileName}'");
                return;
            }

            Logger.Debug($"Loading library: '{moduleFileName}'");
            try
            {
                Assembly asm = Assembly.LoadFrom(moduleFileName);
                LoadModule(asm);
            }
            catch (BadImageFormatException)
            {
                // not a .NET assembly we can load
                Logger.Info($"Ignoring CLR library that could not be loaded as a .NET assembly: '{moduleFileName}'");
            }
            catch (Exception e)
            {
                Logger.Error(e, $"Failed to load library '{moduleFileName}' due to error");
            }
        }

        private static void LoadModule(Assembly asm)
        {
            string moduleName = asm.GetName().Name;
            Logger.Debug($"Helios is searching for Helios components in library '{moduleName}'");
            ((IModuleManagerWritable)ConfigManager.ModuleManager).RegisterModule(asm);

            string directoryName = moduleName;
            HeliosModuleAttribute[] moduleAttributes = (HeliosModuleAttribute[])asm.GetCustomAttributes(typeof(HeliosModuleAttribute), false);
            if (moduleAttributes.Length > 0)
            {
                directoryName = moduleAttributes[0].Directory;
            }
            else
            {
                Logger.Debug($"No Helios-specific module attribute in library '{directoryName}'; using default path based on library name");
            }

            ((TemplateManager)ConfigManager.TemplateManager).LoadModuleTemplates(directoryName);
        }

        /// <summary>
        /// flush logs and do anything else Helios wants to do before exit
        ///
        /// WARNING: this method is not guaranteed to get called since there are many ways to
        /// exit an application, some of them not allowing us time to call it.  It may also
        /// be called more than once.
        /// </summary>
        public static void OnShutdown()
        {
            NLog.LogManager.Shutdown();
        }
    }
}
