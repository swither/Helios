using System;
using System.Collections.Generic;
using System.Linq;

namespace GadrocsWorkshop.Helios.Patching
{
    public class PatchList : List<PatchFile>
    {
        public static PatchList LoadPatches(IPatchDestination destination, string patchSet)
        {
            // load user-provided patches from documents folder
            string userPatchesPath = System.IO.Path.Combine(ConfigManager.DocumentPath, "Patches", "DCS");
            PatchList patches = destination.SelectPatches(userPatchesPath, patchSet);

            // load pre-installed patches from Helios installation folder
            string installedPatchesPath = System.IO.Path.Combine(ConfigManager.ApplicationPath, "Patches", "DCS");
            PatchList patches2 = destination.SelectPatches(installedPatchesPath, patchSet);

            // index patches by target path
            HashSet<string> existing = new HashSet<string>(patches.Select(p => p.TargetPath));

            // merge those preinstalled patches that are not replaced by a file for the same target path
            patches.AddRange(patches2.Where(p => !existing.Contains(p.TargetPath)));
            return patches;
        }

        public IEnumerable<StatusReportItem> SimulateApply(IPatchDestination destination)
        {
            return DoApply(destination, true);
        }

        public IEnumerable<StatusReportItem> Apply(IPatchDestination destination)
        {
            return DoApply(destination, false);
        }

        public IEnumerable<StatusReportItem> Verify(IPatchDestination destination)
        {
            if (!destination.TryLock())
            {
                yield return new StatusReportItem
                {
                    Status = $"cannot acquire lock on {destination.Description} to verify patches",
                    Recommendation = $"close any programs that are holding a lock on this location",
                    Severity = StatusReportItem.SeverityCode.Error
                };
                yield break;
            }
            foreach (PatchFile patch in this)
            {
                if (!destination.TryGetSource(patch.TargetPath, out string source))
                {
                    ConfigManager.LogManager.LogDebug($"{patch.TargetPath} does not exist in target destination; patch does not apply");
                    continue;
                }
                if (!patch.IsApplied(source, out string appliedStatus))
                {
                    yield return new StatusReportItem
                    {
                        Status = appliedStatus,
                        Recommendation = "using Helios Profile Editor, apply patches",
                        Severity = StatusReportItem.SeverityCode.Error
                    };
                }
            }
            if (!destination.TryUnlock())
            {
                ConfigManager.LogManager.LogError($"cannot release lock on {destination.Description} after verifying patches");
            }
            yield break;
        }

        private IEnumerable<StatusReportItem> DoApply(IPatchDestination destination, bool simulate)
        {
            string verb = simulate ? "simulate" : "apply";
            string verbing = simulate ? "simulating" : "applying";
            if (!destination.TryLock())
            {
                yield return new StatusReportItem
                {
                    Status = $"cannot acquire lock on {destination.Description} to {verb} patches",
                    Recommendation = $"close any programs that are holding a lock on this location",
                    Severity = StatusReportItem.SeverityCode.Error
                };
                yield break;
            }
            foreach (PatchFile patch in this)
            {
                if (!destination.TryGetSource(patch.TargetPath, out string source))
                {
                    yield return new StatusReportItem
                    {
                        Status = $"{patch.TargetPath} does not exist in target destination; ignoring patch",
                        Severity = StatusReportItem.SeverityCode.Info
                    };
                    continue;
                }
                if (patch.IsApplied(source, out string appliedStatus))
                {
                    // already applied, go to next patch
                    yield return new StatusReportItem
                    {
                        Status = appliedStatus
                    };
                    continue;
                }
                if (!patch.TryApply(source, out string patched, out string failureStatus))
                {
                    // could not patch; fatal
                    yield return new StatusReportItem
                    {
                        Status = $"{destination.Description} {failureStatus}",
                        Recommendation = "please install a newer Helios distribution or patches with support for this DCS version",
                        Severity = StatusReportItem.SeverityCode.Error
                    };
                    destination.TryUnlock();
                    yield break;
                }
                if (simulate)
                {
                    continue;
                }
                if (destination.TryWritePatched(patch.TargetPath, patched))
                {
                    yield return new StatusReportItem
                    {
                        Status = $"{patch.TargetPath} successfully patched"
                    };
                }
                else
                {
                    yield return new StatusReportItem
                    {
                        Status = $"{destination.Description} {patch.TargetPath} could not be written to target destination after patch",
                        Severity = StatusReportItem.SeverityCode.Error,
                        Recommendation = "please ensure you have write permission to all the files in the target location"
                    };
                    destination.TryUnlock();
                    yield break;
                }
            }
            if (!destination.TryUnlock())
            {
                yield return new StatusReportItem
                {
                    Status = $"cannot release lock on {destination.Description} after {verbing} patches",
                    Recommendation = $"please restart and try the patch process again",
                    Severity = StatusReportItem.SeverityCode.Error
                };
            }
            yield break;
        }
    }
}
