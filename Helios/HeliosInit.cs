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

namespace GadrocsWorkshop.Helios
{
    using GadrocsWorkshop.Helios.ComponentModel;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Windows.Media;

    /// <summary>
    /// Class to initialize the Helios Runtime.
    /// </summary>
    public static class HeliosInit
    {
        public static void Initialize(string docPath, string logFileName, LogLevel logLevel, HeliosApplication application = null)
        {
            // Create documents directory if it does not exist
            ConfigManager.DocumentPath = docPath;

            string logPath = Path.Combine(ConfigManager.DocumentPath, logFileName);

            string backupName = logPath + ".bak";
            if (File.Exists(backupName))
            {
                File.Delete(backupName);
            }

            if (File.Exists(logPath))
            {
                File.Move(logPath, backupName);
            }

            ConfigManager.LogManager = new LogManager(logPath, logLevel);

            Assembly execAssembly = Assembly.GetExecutingAssembly();
            ConfigManager.LogManager.Log("Helios Version " + execAssembly.GetName().Version);

            ConfigManager.SettingsManager = new SettingsManager(Path.Combine(ConfigManager.DocumentPath, "HeliosSettings.xml"));
            ConfigManager.UndoManager = new UndoManager();
            ConfigManager.ProfileManager = new ProfileManager();
            ConfigManager.ImageManager = new ImageManager(ConfigManager.ImagePath);
            ConfigManager.DisplayManager = new DisplayManager();

            ConfigManager.ModuleManager = new ModuleManager(ConfigManager.ApplicationPath);
            ConfigManager.TemplateManager = new TemplateManager(ConfigManager.TemplatePath, ConfigManager.PanelTemplatePath);
            ConfigManager.Application = application ?? new HeliosApplication();
            
            ConfigManager.LogManager.LogDebug("Searching for Helios modules in libraries");

            LoadModule(Assembly.GetExecutingAssembly());

            if (ConfigManager.Application.AllowPlugins)
            {
                string pluginsFolder = Path.Combine("Plugins");
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

            if (RenderCapability.Tier == 0)
            {
                ConfigManager.LogManager.LogWarning("Hardware rendering is not available on this machine.  Helios will consume large amounts of CPU.");
            }
        }

        private static void LoadModule(string moduleFileName)
        {
            if (File.Exists(moduleFileName))
            {
                ConfigManager.LogManager.LogDebug($"Loading library: '{moduleFileName}'");
                try
                {
                    Assembly asm = Assembly.LoadFrom(moduleFileName);
                    if (asm != null)
                    {
                        LoadModule(asm);
                    }
                    else
                    {
                        ConfigManager.LogManager.LogWarning($"Failed to load library '{moduleFileName}'");
                    }
                }
                catch (Exception e)
                {
                    ConfigManager.LogManager.LogError($"Failed to load library '{moduleFileName}' due to error", e);
                }
            }
        }

        private static void LoadModule(Assembly asm)
        {
            string moduleName = asm.GetName().Name;
            ConfigManager.LogManager.LogDebug($"Helios is searching for Helios components in library '{moduleName}'");
            ((ModuleManager)ConfigManager.ModuleManager).RegisterModule(asm);

            string directoryName = moduleName;
            HeliosModuleAttribute[] moduleAttributes = (HeliosModuleAttribute[])asm.GetCustomAttributes(typeof(HeliosModuleAttribute), false);
            if (moduleAttributes != null && moduleAttributes.Length > 0)
            {
                directoryName = moduleAttributes[0].Directory;
            }
            else
            {
                ConfigManager.LogManager.LogDebug($"No Helios-specific module attribute in library '{directoryName}'; using default path based on library name");
            }

            ((TemplateManager)ConfigManager.TemplateManager).LoadModuleTemplates(directoryName);
        }
    }
}
