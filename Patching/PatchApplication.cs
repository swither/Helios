// Copyright 2020 Ammo Goettsch
// 
// Helios is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// Helios is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
// 

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using GadrocsWorkshop.Helios.Util;

namespace GadrocsWorkshop.Helios.Patching
{
    /// <summary>
    /// context for the application a particular set of patches to a particular patch destination
    /// </summary>
    [DebuggerDisplay("Patches for {Destination} with status {Status}")]
    public class PatchApplication
    {
        public IPatchDestinationWritable Destination { get; }
        public PatchList Patches { get; }
        public string PatchSet { get; }
        public string[] PatchesRoots { get; }
        public string SelectedVersion { get; }
        public bool Enabled { get; internal set; }
        public bool UseRemote { get; }
        public StatusCodes Status { get; internal set; }

        public PatchApplication(IPatchDestinationWritable destination, bool enabled, bool useRemote, string patchSet,
            params string[] patchesRoots)
        {
            PatchSet = patchSet;
            PatchesRoots = patchesRoots;
            Destination = destination;
            Enabled = enabled;
            UseRemote = useRemote;

            PatchList patches = new PatchList();
            string selectedVersion = null;
            foreach (string patchesRoot in PatchesRoots)
            {
                PatchList loaded = Destination.SelectPatches(patchesRoot, ref selectedVersion, PatchSet);
                patches.Merge(loaded);
            }

            Patches = patches;
            SelectedVersion = selectedVersion;
            Status = StatusCodes.Unknown;
        }

        internal void CheckApplied()
        {
            bool notInstalled = false;
            bool installed = false;
            foreach (StatusReportItem result in Patches.Verify(Destination))
            {
                // don't log these results, because Verify considers being out of date to be an error
                if (result.Severity > StatusReportItem.SeverityCode.Warning)
                {
                    // errors indicate patch needs work
                    notInstalled = true;
                }
                else
                {
                    // any indication other than error means patch is installed
                    installed = true;
                }
            }

            if (installed && notInstalled)
            {
                Status = StatusCodes.ResetRequired;
            }
            else if (installed)
            {
                Status = StatusCodes.UpToDate;
            }
            else
            {
                Status = StatusCodes.OutOfDate;
            }
        }

        internal IList<StatusReportItem> RemoteApply()
        {
            if (string.IsNullOrEmpty(SelectedVersion))
            {
                return new List<StatusReportItem>
                {
                    new StatusReportItem
                    {
                        Status = $"No compatible patch set version available for {Destination.LongDescription}",
                        Recommendation =
                            $"Upgrade Helios to support {Destination.LongDescription} or install patches in user documents",
                        Severity = StatusReportItem.SeverityCode.Error
                    }
                };
            }

            IList<StatusReportItem> results = Destination.RemoteApply(PatchesRoots, SelectedVersion, PatchSet);
            return ScanResults(results);
        }

        internal IList<StatusReportItem> RemoteRevert()
        {
            if (string.IsNullOrEmpty(SelectedVersion))
            {
                return new List<StatusReportItem>
                {
                    new StatusReportItem
                    {
                        Status = $"Nothing to revert for for {Destination.LongDescription}",
                        Flags = StatusReportItem.StatusFlags.ConfigurationUpToDate
                    }
                };
            }

            IList<StatusReportItem> results = Destination.RemoteRevert(PatchesRoots, SelectedVersion, PatchSet);
            return ScanResults(results);
        }

        internal IList<StatusReportItem> Apply()
        {
            IList<StatusReportItem> results = Patches.Apply(Destination).ToList();
            return ScanResults(results);
        }

        internal IList<StatusReportItem> Revert()
        {
            IList<StatusReportItem> results = Patches.Revert(Destination).ToList();
            return ScanResults(results);
        }

        private IList<StatusReportItem> ScanResults(IList<StatusReportItem> results)
        {
            if (results.Any(result => result.Severity >= StatusReportItem.SeverityCode.Error))
            {
                // fatal error
                Status = StatusCodes.Incompatible;
                return results;
            }

            // nothing fatal found, check files to set new status
            CheckApplied();
            return results;
        }
    }
}