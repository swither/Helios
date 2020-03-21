using System.Collections.Generic;

namespace GadrocsWorkshop.Helios.Patching
{
    class PatchList: List<Patch>
    {
        public IEnumerable<StatusReportItem> SimulateApply(IPatchDestination destination)
        {
            return DoApply(destination, true);
        }

        public IEnumerable<StatusReportItem> Apply(IPatchDestination destination)
        {
            return DoApply(destination, false);
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
            foreach (Patch patch in this)
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
                if (patch.IsApplied(source, out StatusReportItem appliedStatus))
                {
                    // already applied, go to next patch
                    yield return appliedStatus;
                    continue;
                }
                if (!patch.TryApply(source, out string patched, out StatusReportItem failureStatus)) 
                {
                    // could not patch; fatal
                    yield return failureStatus;
                    destination.TryUnlock();
                    yield break;
                }
                if (simulate)
                {
                    continue;
                }
                if (!destination.TryWritePatched(patch.TargetPath, patched))
                {
                    yield return new StatusReportItem
                    {
                        Status = $"{patch.TargetPath} could not be written to target destination after patch",
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
