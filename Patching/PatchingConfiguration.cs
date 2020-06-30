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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using GadrocsWorkshop.Helios.Util;

namespace GadrocsWorkshop.Helios.Patching
{
    // REVISIT: reimplement as a HeliosViewModel and a DCSConfiguration
    public class PatchingConfiguration : DependencyObject, IInstallation
    {
        private readonly Dictionary<string, PatchApplication> _destinations;
        private string _patchSet;
        private readonly string _patchSetDescription;

        public PatchingConfiguration(Dictionary<string, PatchApplication> destinations, string patchSet,
            string patchSetDescription)
        {
            _destinations = destinations;
            _patchSet = patchSet;
            _patchSetDescription = patchSetDescription;

            // check if all selected patches are installed
            foreach (PatchApplication status in _destinations.Values)
            {
                status.CheckApplied();
            }

            // update overall status
            UpdateStatus();
        }

        public void OnEnabled(string key)
        {
            PatchApplication destinationPatches = _destinations[key];
            destinationPatches.Enabled = true;
            destinationPatches.CheckApplied();
            UpdateStatus();
        }

        public void OnDisabled(string key)
        {
            _destinations[key].Enabled = false;
            UpdateStatus();
        }

        public void OnRemoved(string key)
        {
            _destinations.Remove(key);
            UpdateStatus();
        }

        public void OnAdded(string key, PatchApplication destinationPatches)
        {
            destinationPatches.CheckApplied();
            _destinations[key] = destinationPatches;
            UpdateStatus();
        }

        private void UpdateStatus()
        {
            StatusCodes newStatus = StatusCodes.UpToDate;
            if (!_destinations.Values.Any(d => d.Enabled))
            {
                newStatus = StatusCodes.NoLocations;
            }

            foreach (PatchApplication status in _destinations.Values)
            {
                if (!status.Enabled)
                {
                    // does not count
                    continue;
                }

                switch (status.Status)
                {
                    case StatusCodes.Unknown:
                    case StatusCodes.OutOfDate:
                        newStatus = StatusCodes.OutOfDate;
                        // don't end iteration, need to check for failures
                        break;
                    case StatusCodes.UpToDate:
                        break;
                    case StatusCodes.Incompatible:
                        // any destination being incompatible counts
                        Status = StatusCodes.Incompatible;
                        return;
                    case StatusCodes.ResetRequired:
                        // any destination being dirty counts
                        Status = StatusCodes.ResetRequired;
                        return;
                    case StatusCodes.NoLocations:
                    case StatusCodes.ResetMonitorsRequired:
                    case StatusCodes.ProfileSaveRequired:
                    default:
                        throw new ArgumentOutOfRangeException(nameof(status.Status), status.Status, null);
                }
            }

            Status = newStatus;
        }

        public InstallationResult Install(IInstallationCallbacks callbacks)
        {
            List<StatusReportItem> results = new List<StatusReportItem>();
            bool failed = false;
            bool imprecise = false;
            string message = "";

            // simulate patches and collect any errors
            foreach (PatchApplication item in _destinations.Values)
            {
                if (!item.Enabled)
                {
                    continue;
                }

                foreach (StatusReportItem result in item.Patches.SimulateApply(item.Destination))
                {
                    result.Log(ConfigManager.LogManager);
                    results.Add(result);
                    if (result.Severity >= StatusReportItem.SeverityCode.Error)
                    {
                        if (!failed)
                        {
                            // keep first message
                            message =
                                $"Patching {item.Destination.Description} would fail\n{result.Status}\n{result.Recommendation}";
                        }

                        failed = true;
                        item.Status = StatusCodes.Incompatible;
                    }

                    if (result.Severity >= StatusReportItem.SeverityCode.Warning)
                    {
                        imprecise = true;
                    }
                }
            }

            if (failed)
            {
                UpdateStatus();
                callbacks.Failure($"{_patchSetDescription} installation would fail", message, results);
                return InstallationResult.Fatal;
            }

            if (imprecise)
            {
                InstallationPromptResult response = callbacks.DangerPrompt(
                    $"{_patchSetDescription} installation may have risks",
                    $"{_patchSetDescription} installation can continue, but some target files have changed since these patches were created.",
                    results);
                if (response == InstallationPromptResult.Cancel)
                {
                    return InstallationResult.Canceled;
                }
            }


            // apply patches
            foreach (PatchApplication item in _destinations.Values)
            {
                if (!item.Enabled)
                {
                    continue;
                }

                // apply patches directly or via elevated process, as appropriate
                IList<StatusReportItem> applyResults = item.UseRemote ? item.RemoteApply() : item.Apply();
                foreach (StatusReportItem result in applyResults)
                {
                    result.Log(ConfigManager.LogManager);
                    results.Add(result);
                    if (result.Severity < StatusReportItem.SeverityCode.Error)
                    {
                        continue;
                    }

                    if (!failed)
                    {
                        // keep first message
                        message = $"Patching {item.Destination.Description} failed\n{result.Status}\n{result.Recommendation}";
                    }
                    failed = true;
                }
            }

            UpdateStatus();
            if (failed)
            {
                // XXX need to revert any patches that were installed, if we can, and add to result report
                callbacks.Failure($"{_patchSetDescription} installation failed", message, results);
                return InstallationResult.Fatal;
            }

            if (Status != StatusCodes.UpToDate)
            {
                // installation completed, but not everything was done
                callbacks.Failure($"{_patchSetDescription} installation incomplete", "Not all patches were installed", results);
                return InstallationResult.Canceled;
            }

            callbacks.Success($"{_patchSetDescription} installation success", "All patches installed successfully",
                results);
            return InstallationResult.Success;
        }


        public InstallationResult Uninstall(IInstallationCallbacks callbacks)
        {
            List<StatusReportItem> results = new List<StatusReportItem>();
            bool failed = false;
            string message = "";

            // revert patches, by either restoring original file or reverse patching
            foreach (PatchApplication item in _destinations.Values)
            {
                if (!item.Enabled)
                {
                    continue;
                }

                // revert directly or via elevated process, as appropriate
                IList<StatusReportItem> revertResults = item.UseRemote ? item.RemoteRevert() : item.Revert();
                foreach (StatusReportItem result in revertResults)
                {
                    result.Log(ConfigManager.LogManager);
                    results.Add(result);
                    if (result.Severity < StatusReportItem.SeverityCode.Error)
                    {
                        continue;
                    }

                    if (!failed)
                    {
                        // keep first message
                        message = $"Reverting patches in {item.Destination.Description} failed\n{result.Status}\n{result.Recommendation}";
                    }
                    failed = true;
                }

                // ReSharper disable once InvertIf early exit on error condition should have a block
                if (failed)
                {
                    // give up and just direct the user to fix one installation this time, meaning they may have to do this multiple times
                    message +=
                        $"\nPlease execute 'bin\\dcs_updater.exe repair' in your {item.Destination.LongDescription} to restore to original files";
                    break;
                }
            }

            UpdateStatus();

            if (failed)
            {
                callbacks.Failure($"{_patchSetDescription} revert failed", message, results);
                return InstallationResult.Fatal;
            }

            // special check that is not handled by UpdateStatus, because we are technically in a state that just means
            // we have to reinstall some patches, but this still means something went wrong in reverting patches, such as
            // user cancellation
            if (_destinations.Values.Any(d => d.Status != StatusCodes.OutOfDate))
            {
                // revert completed, but not everything was done
                callbacks.Failure($"{_patchSetDescription} removal incomplete", "Some patches were not reverted", results);
                return InstallationResult.Canceled;
            }

            callbacks.Success($"{_patchSetDescription} removal success", "All patches reverted successfully",
                results);
            return InstallationResult.Success;
        }

        #region Properties

        /// <summary>
        /// Patch installation status
        /// </summary>
        public StatusCodes Status
        {
            get => (StatusCodes) GetValue(StatusProperty);
            set => SetValue(StatusProperty, value);
        }

        public static readonly DependencyProperty StatusProperty =
            DependencyProperty.Register("Status", typeof(StatusCodes), typeof(PatchingConfiguration),
                new PropertyMetadata(StatusCodes.OutOfDate));

        #endregion
    }
}