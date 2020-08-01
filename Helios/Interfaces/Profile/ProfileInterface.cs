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

using System.Runtime.CompilerServices;
using System.Windows.Threading;
using GadrocsWorkshop.Helios.Interfaces.Capabilities;
using GadrocsWorkshop.Helios.Interfaces.Common;

namespace GadrocsWorkshop.Helios.Interfaces.Profile
{
    using GadrocsWorkshop.Helios.ComponentModel;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Windows;

    [HeliosInterface("Helios.Base.ProfileInterface", "Profile", null, typeof(UniqueHeliosInterfaceFactory), AutoAdd = true)]
    public class ProfileInterface : HeliosInterface, IStatusReportNotify, IResetMonitorsObserver, IReadyCheck
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        // failed image load paths we know about, so that we can recognize when a load success changes the status
        private HashSet<string> _failedImagePaths;

        // our implementation of IStatusReportNotify
        private readonly StatusReportNotifyAsyncOnce _statusReportNotify;

        public ProfileInterface()
            : base("Profile")
        {
            HeliosAction resetAction = new HeliosAction(this, "", "", "reset", "Resets the profile to default state.");
            resetAction.Execute += new HeliosActionHandler(ResetAction_Execute);
            Actions.Add(resetAction);

            HeliosAction stopAction = new HeliosAction(this, "", "", "stop", "Stops the profile from running.");
            stopAction.Execute += new HeliosActionHandler(StopAction_Execute);
            Actions.Add(stopAction);

            HeliosAction showControlCenter = new HeliosAction(this, "", "", "show control center", "Shows the control center.");
            showControlCenter.Execute += new HeliosActionHandler(ShowAction_Execute);
            Actions.Add(showControlCenter);

            HeliosAction hideControlCenter = new HeliosAction(this, "", "", "hide control center", "Shows the control center.");
            hideControlCenter.Execute += new HeliosActionHandler(HideAction_Execute);
            Actions.Add(hideControlCenter);

            HeliosAction launchApplication = new HeliosAction(this, "", "", "launch application", "Launches an external application", "Full path to appliation or document you want to launch or URL to a web page.", BindingValueUnits.Text);
            launchApplication.Execute += LaunchApplication_Execute;
            Actions.Add(launchApplication);

            // TODO declare triggers

            _statusReportNotify = new StatusReportNotifyAsyncOnce(CreateStatusReport, () => "Profile Interface", () => "Interface to Helios Control Center itself.");
        }

        protected override void AttachToProfileOnMainThread()
        {
            base.AttachToProfileOnMainThread();
            if (!(ConfigManager.ImageManager is IImageManager2 images))
            {
                return;
            }

            _failedImagePaths = new HashSet<string>();
            images.ReplayCurrentFailures(Images_ImageLoadFailure);

            images.ImageLoadSuccess += Images_ImageLoadSuccess;
            images.ImageLoadFailure += Images_ImageLoadFailure;
            Profile.ProfileStarted += Profile_ProfileStarted;
            Profile.ProfileStopped += Profile_ProfileStopped;
        }

        private void Profile_ProfileStarted(object sender, EventArgs e)
        {
            // TODO start trigger
        }

        private void Profile_ProfileStopped(object sender, EventArgs e)
        {
            // TODO stop trigger
        }

        public override void Reset()
        {
            // TODO reset trigger
            base.Reset();
        }

        private void Images_ImageLoadFailure(object sender, ImageLoadEventArgs e)
        {
            if (_failedImagePaths == null)
            {
                // this is normal state during profile loading
                return;
            }
            _failedImagePaths.Add(e.Path);
            InvalidateStatusReport();
        }

        private void Images_ImageLoadSuccess(object sender, ImageLoadEventArgs e)
        {
            if (_failedImagePaths?.Remove(e.Path) ?? false)
            {
                InvalidateStatusReport();
            }
        }

        protected override void DetachFromProfileOnMainThread(HeliosProfile oldProfile)
        {
            base.DetachFromProfileOnMainThread(oldProfile);
            if (!(ConfigManager.ImageManager is IImageManager2 images))
            {
                return;
            }

            oldProfile.ProfileStopped -= Profile_ProfileStopped;
            oldProfile.ProfileStarted -= Profile_ProfileStarted;
            images.ImageLoadSuccess -= Images_ImageLoadSuccess;
            images.ImageLoadFailure -= Images_ImageLoadFailure;
        }

        void LaunchApplication_Execute(object action, HeliosActionEventArgs e)
        {
            try
            {
                Process.Start(e.Value.StringValue);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error caught launching external application (path=\"" + e.Value.StringValue + "\")");
            }
        }

        void HideAction_Execute(object action, HeliosActionEventArgs e)
        {
            if (Profile != null)
            {
                Profile.HideControlCenter();
            }
        }

        void ShowAction_Execute(object action, HeliosActionEventArgs e)
        {
            if (Profile != null)
            {
                Profile.ShowControlCenter();
            }
        }

        void StopAction_Execute(object action, HeliosActionEventArgs e)
        {
            if (Profile != null)
            {
                Profile.Stop();
            }
        }

        void ResetAction_Execute(object action, HeliosActionEventArgs e)
        {
            if (Profile != null)
            {
                Profile.Reset();
            }
        }

        public override void ReadXml(System.Xml.XmlReader reader)
        {
            // No-Op
        }

        public override void WriteXml(System.Xml.XmlWriter writer)
        {
            // No-Op
        }

        public void Subscribe(IStatusReportObserver observer)
        {
            _statusReportNotify.Subscribe(observer);
        }

        public void Unsubscribe(IStatusReportObserver observer)
        {
            _statusReportNotify.Unsubscribe(observer);
        }

        public void InvalidateStatusReport()
        {
            _statusReportNotify.InvalidateStatusReport();
        }

        private void CreateStatusReport()
        {
            IList<StatusReportItem> newReport = new List<StatusReportItem>();
            CheckResetMonitors(newReport);
            CheckMissingimages(newReport);
            CheckExecutablePaths(newReport);
            PublishStatusReport(newReport);
        }

        private void CheckMissingimages(IList<StatusReportItem> newReport)
        {
            foreach (string path in _failedImagePaths)
            {
                newReport.Add(new StatusReportItem
                {
                    Status = $"An image used in this Profile's controls could not be loaded from '{path}'",
                    Recommendation = "Install missing images or correct image paths in controls, then reload profile.",
                    Severity = StatusReportItem.SeverityCode.Warning,
                    Flags = StatusReportItem.StatusFlags.ConfigurationUpToDate | StatusReportItem.StatusFlags.Verbose
                });
            }
        }

        private void CheckResetMonitors(IList<StatusReportItem> newReport)
        {
            if (Profile == null)
            {
                return;
            }

            if (Profile.IsValidMonitorLayout)
            {
                newReport.Add(new StatusReportItem
                {
                    Status = "The monitor arrangement saved in this profile matches this computer",
                    Flags = StatusReportItem.StatusFlags.ConfigurationUpToDate | StatusReportItem.StatusFlags.Verbose
                });
            }
            else
            {
                newReport.Add(new StatusReportItem
                {
                    Status = "The monitor arrangement saved in this profile does not match this computer",
                    Recommendation = "Perform Reset Monitors from the Profile menu",
                    Severity = StatusReportItem.SeverityCode.Error,
                    Link = StatusReportItem.ProfileEditor
                });
                foreach (Monitor display in Profile.CheckedDisplays)
                {
                    newReport.Add(new StatusReportItem
                    {
                        Status = $"Windows reports an attached display of size {display.Width}x{display.Height} at ({display.Left}, {display.Top})",
                        Flags = StatusReportItem.StatusFlags.ConfigurationUpToDate | StatusReportItem.StatusFlags.Verbose
                    });
                }
            }

            foreach (Monitor monitor in Profile.Monitors)
            {
                newReport.Add(new StatusReportItem
                {
                    Status = $"The profile declares a monitor of size {monitor.Width}x{monitor.Height} at ({monitor.Left}, {monitor.Top})",
                    Flags = StatusReportItem.StatusFlags.ConfigurationUpToDate | StatusReportItem.StatusFlags.Verbose
                });
            }
        }

        /// <summary>
        /// check configuration related to launching or stopping executables
        /// </summary>
        /// <param name="newReport"></param>
        private void CheckExecutablePaths(IList<StatusReportItem> newReport)
        {
            // TODO if any launch or kill actions are configured, check paths and access
            // TODO if incorrect, report warning or error (to stop launch) in StatusReportItem
            // TODO if correct, report detail also in StatusReportItem
            _ = newReport;
        }

        public void PublishStatusReport(IList<StatusReportItem> statusReport)
        {
            _statusReportNotify.PublishStatusReport(statusReport);
        }

        public void NotifyResetMonitorsStarting()
        {
            // no code
        }

        public void NotifyResetMonitorsComplete()
        {
            InvalidateStatusReport();
        }

        public IEnumerable<StatusReportItem> PerformReadyCheck()
        {
            // NOTE: we don't have to check monitor reset because Control Center already does that
            // NOTE: we don't have to check missing images because they will log warnings 
            List<StatusReportItem> items = new List<StatusReportItem>();
            CheckExecutablePaths(items);
            return items;
        }
    }
}
