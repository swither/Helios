using GadrocsWorkshop.Helios;
using GadrocsWorkshop.Helios.Patching.DCS;
using GadrocsWorkshop.Helios.Util.DCS;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ToolsCommon;

namespace EditViewports
{
    class EditViewports
    {
        private static Regex _viewportHandling = new Regex("dofile.*ViewportHandling.lua", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static Regex _tryFindAssignedViewport = new Regex("try_find_assigned_viewport\\(\"([^\"]*)\"(?:, *\"(.*)\")?\\)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        static void Main(string[] args)
        {
            string jsonPath;
            if (args.Length < 1)
            {
                jsonPath = FileSystem.FindNearestDirectory("ToolsCommon\\Data\\Viewports") + "ViewportTemplates.json";
            }
            else
            {
                jsonPath = args[0];
            }
            string json = File.ReadAllText(jsonPath);
            List<ToolsCommon.ViewportTemplate> templates = JsonConvert.DeserializeObject<ToolsCommon.ViewportTemplate[]>(json).ToList();

            // get DCS location from the Helios utility that manages DCS install locations (have to use Profile Editor to configure it, either running dev build or start with --documents HeliosDev)
            string documentPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "HeliosDev");
            HeliosInit.Initialize(documentPath, "EditViewports.log", LogLevel.Debug);

            InstallationLocations locations = InstallationLocations.Singleton;
            if (!locations.Active.Any())
            {
                throw new System.Exception("at least one DCS install location must be configured and enabled");
            }
            foreach (InstallationLocation location in locations.Items)
            {
                PatchDestination destination = new PatchDestination(location);
                EditFilesInDestination(templates, destination);
            }
        }

        private static void EditFilesInDestination(List<ViewportTemplate> templates, PatchDestination destination)
        {
            if (!destination.TryLock())
            {
                throw new System.Exception($"cannot acquire lock on {destination.LongDescription} to edit viewports");
            }
            try
            {
                foreach (ToolsCommon.ViewportTemplate template in templates)
                {
                    foreach (ToolsCommon.Viewport viewport in template.Viewports.Where(v => v.IsValid))
                    {
                        if (null == viewport.RelativeInitFilePath)
                        {
                            Debug.WriteLine($"viewport {viewport.ViewportName} has no file path; ignoring");
                            continue;
                        }
                        if (!destination.TryGetSource(viewport.RelativeInitFilePath, out string source))
                        {
                            Debug.WriteLine($"'{viewport.RelativeInitFilePath}' does not exist in target destination; ignoring patch");
                            continue;
                        }

                        string patched = EditCode(template, viewport, source);
                        if (source == patched)
                        {
                            Debug.WriteLine($"'{viewport.RelativeInitFilePath}' is unchanged");
                            continue;
                        }

                        Debug.WriteLine("---------------------------------------------------------");
                        Debug.WriteLine(patched);
                        Debug.WriteLine("=========================================================");

                        if (!destination.TryWritePatched(viewport.RelativeInitFilePath, patched))
                        {
                            throw new System.Exception($"'{viewport.RelativeInitFilePath}' could not be written to target destination after edit");
                        }
                    }
                }
            }
            finally
            {
                if (!destination.TryUnlock())
                {
                    Debug.Fail($"cannot release lock on {destination.LongDescription} after editing viewports");
                }
            }
        }

        private static string EditCode(ViewportTemplate template, Viewport viewport, string source)
        {
            string patched = source;
            if (!_viewportHandling.IsMatch(source))
            {
                Debug.WriteLine($"adding viewport handling to '{viewport.RelativeInitFilePath}'");
                EnsureCrLf(ref patched);
                patched += $"dofile(LockOn_Options.common_script_path..\"ViewportHandling.lua\")\r\n";
            }

            // XXX this is wrong.  if there are multiple viewports defined in the same file, we need to 
            // XXX use original viewport name to disambiguate and edit each separately
            Match assigned = _tryFindAssignedViewport.Match(source);
            if (!assigned.Success)
            {
                Debug.WriteLine($"adding viewport selection to '{viewport.RelativeInitFilePath}'");
                EnsureCrLf(ref patched);
                patched += $"try_find_assigned_viewport(\"{template.ViewportPrefix}_{viewport.ViewportName}\", \"{viewport.ViewportName}\")\r\n";
            }
            else
            {
                Debug.WriteLine($"changing viewport selection in '{viewport.RelativeInitFilePath}'");
                string abstractName;
                if (assigned.Groups[2].Success)
                {
                    // use existing abstract name
                    abstractName = assigned.Groups[2].Value;
                }
                else
                {
                    // use existing exact name as abstract name
                    abstractName = assigned.Groups[1].Value;
                }
                patched = patched.Replace(assigned.Groups[0].Value, $"try_find_assigned_viewport(\"{template.ViewportPrefix}_{viewport.ViewportName}\", \"{abstractName}\")");
            }

            return patched;
        }

        private static void EnsureCrLf(ref string patched)
        {
            if (!patched.EndsWith("\r\n"))
            {
                patched += "\r\n";
            }
        }
    }
}
