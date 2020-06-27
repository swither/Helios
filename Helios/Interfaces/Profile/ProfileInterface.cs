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
    using System.IO;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text.RegularExpressions;
    using System.Windows;

    [HeliosInterface("Helios.Base.ProfileInterface", "Profile", null, typeof(UniqueHeliosInterfaceFactory), AutoAdd = true)]
    public class ProfileInterface : HeliosInterface, IStatusReportNotify, IResetMonitorsObserver, IReadyCheck
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        // failed image load paths we know about, so that we can recognize when a load success changes the status
        private HashSet<string> _failedImagePaths;

        // our implementation of IStatusReportNotify
        private readonly StatusReportNotifyAsyncOnce _statusReportNotify;

        // definition for profile related triggers
        private HeliosTrigger _profileStartedTrigger;
        private HeliosTrigger _profileResetTrigger;
        private HeliosTrigger _profileStoppedTrigger;

        private static Regex _rxDoubleQuotes = new Regex("(?:\\\")([^\"]*)(?:\\\")", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
        private static string executableExtensions = Environment.GetEnvironmentVariable("PATHEXT") ?? ".COM;.EXE;.BAT;.CMD;.VBS;.VBE;.JS;.JSE;.WSF;.WSH;.MSC;";
        private static Regex _rxExe = new Regex("(" + executableExtensions.Replace(";", "|").Replace(".", "\\.") + ")", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);

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

            HeliosAction killApplication = new HeliosAction(this, "", "", "kill application", "Kills an external process", "Process Image name of the process to be killed.", BindingValueUnits.Text);
            killApplication.Execute += KillApplication_Execute;
            Actions.Add(killApplication);

            _profileStartedTrigger = new HeliosTrigger(this, "", "", "Started", "Fired when a profile is started.");
            Triggers.Add(_profileStartedTrigger);

           _profileResetTrigger = new HeliosTrigger(this, "", "", "Reset", "Fired when a profile has been reset.");
            Triggers.Add(_profileResetTrigger);

            _profileStoppedTrigger = new HeliosTrigger(this, "", "", "Stopped", "Fired when a profile is stopped.");
            Triggers.Add(_profileStoppedTrigger);

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
            _profileStartedTrigger.FireTrigger(BindingValue.Empty);
        }

        private void Profile_ProfileStopped(object sender, EventArgs e)
        {
            _profileStoppedTrigger.FireTrigger(BindingValue.Empty);
        }

        public override void Reset()
        {
            _profileResetTrigger.FireTrigger(BindingValue.Empty);
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
            // Arguments need to be split off from the incoming string, and both the
            // application full path and the arguments could contain spaces.  
            // The first un-double-quoted space is assumed to be the delimiter between 
            // executable and argument.  
            // 
            // Once arguments have been split off, both the executable and arguments are 
            // processed to expand environment variables which might introduce spaces.
            //
            // This process might break backward compatibility because existing launch actions
            // could contain blanks and not be enclosed in double quotes
            //

            MatchCollection matches;
            // Find out if we're launching a common executable for this machine
            matches = _rxExe.Matches(e.Value.StringValue);
            bool exe = matches.Count > 0 ? true : false;

            string expandedPath = e.Value.StringValue;
            string expandedArgs = "";
            if (exe)
            {
                // For executables launches, we resolve environment variables
                Logger.Debug("Launch type determined to be an executable based on %PATHEXT% and value {ActionValue}", e.Value.StringValue);

                matches = _rxDoubleQuotes.Matches(e.Value.StringValue);  // Extract anything which is enclosed in escaped double quotes
                int blank = e.Value.StringValue.IndexOf(" ");
                try
                {
                    if (matches.Count == 0)
                    {
                        //  There is nothing enclosed in double-quotes, so we assume the executable is before the first space and any arguments follow the first space

                        if (blank > 0)
                        {
                            expandedPath = Environment.ExpandEnvironmentVariables(e.Value.StringValue.Substring(0, blank));
                            expandedArgs = Environment.ExpandEnvironmentVariables(e.Value.StringValue.Substring(blank + 1));
                        }
                        else
                        {
                            expandedPath = Environment.ExpandEnvironmentVariables(e.Value.StringValue);
                            expandedArgs = "";
                        }
                    }
                    else
                    {
                        int matchCursor = 0;
                        foreach (Match matchItem in matches)
                        {
                            if (matchItem.Equals(matches[0]))
                            {
                                if (matchItem.Index == 0)
                                {
                                    expandedPath = Environment.ExpandEnvironmentVariables(matchItem.Groups[1].ToString());
                                    matchCursor = matchItem.Length;
                                }
                                else
                                {
                                    expandedPath = Environment.ExpandEnvironmentVariables(e.Value.StringValue.Substring(0, blank));
                                    expandedArgs = Environment.ExpandEnvironmentVariables(e.Value.StringValue.Substring(blank, matchItem.Index - blank));
                                    expandedArgs += " " + Environment.ExpandEnvironmentVariables(matchItem.ToString());
                                    matchCursor = matchItem.Index + matchItem.Length;
                                }
                            }
                            else
                            {
                                if (matchItem.Index == matchCursor)
                                {
                                    expandedArgs += " " + Environment.ExpandEnvironmentVariables(matchItem.ToString());
                                }
                                else
                                {
                                    expandedArgs += Environment.ExpandEnvironmentVariables(e.Value.StringValue.Substring(matchCursor, matchItem.Index - matchCursor));
                                    matchCursor = matchItem.Index + matchItem.Length;
                                    expandedArgs += " " + Environment.ExpandEnvironmentVariables(matchItem.ToString());
                                }
                            }
                            Logger.Debug("Double Quoted item found {ItemText}", matchItem);
                        }
                    }
                }
                catch (ArgumentNullException ex)
                {
                    Logger.Error(ex, "Error caught preparing to identify launch program or arguments for an external application (path={ExpandedPath})", expandedPath);
                }
            }
            try
            {
                if (exe)
                {
                    ProcessStartInfo psi = new ProcessStartInfo();
                    psi.FileName = System.IO.Path.GetFileName(expandedPath);
                    psi.WorkingDirectory = System.IO.Path.GetDirectoryName(expandedPath);
                    psi.Arguments = expandedArgs.Trim();
                    psi.UseShellExecute = true;
                    psi.RedirectStandardOutput = false;
                    Process.Start(psi);
                }
                else
                {
                    Logger.Debug("Launch type determined to be non-executable based on %PATHEXT% {ProcessName}", e.Value.StringValue);
                    Process.Start(e.Value.StringValue);
                }
            }
            catch (Exception ex)
            {
                // this is written as info because it is very likely to be a user error and we do not want to see problem reports from this.
                Logger.Info(ex,"Error caught launching external application (path={ExpandedPath}).  This is very likely to be due to a missing program, missing file, or incorrect permissions on what has been specified on the lauinch action.", expandedPath);
            }
        }

        void KillApplication_Execute(object action, HeliosActionEventArgs e)
        {
            try
            {
                Process[] localProcessesByName = Process.GetProcessesByName(e.Value.StringValue);
                foreach (Process proc in localProcessesByName)
                {
                    Logger.Info("Killing process image name {ProcessImageName}", e.Value.StringValue);
                    proc.Kill();
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error caught during kill processing for process image name {ProcessImageName}", e.Value.StringValue);
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
                    // TODO: Provide information about where the missing image is used
                    Status = $"An image used in this Profile's controls could not be loaded from '{path}'",
                    Recommendation = "Install missing images or correct image paths in controls, then reload profile.",
                    Severity = StatusReportItem.SeverityCode.Warning,
                    Flags = StatusReportItem.StatusFlags.ConfigurationUpToDate | StatusReportItem.StatusFlags.Verbose
                });
            }
        }

        private void CheckResetMonitors(IList<StatusReportItem> newReport)
        {
            if (Profile != null)
            {
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
                        // TODO handle reporting tablet orientated incorrectly for the profile - ie profile width = screen height and profile height = screen width - the action would be to rotate the device rather than perform a monitor reset
                    });
                }
            }
        }

        /// <summary>
        /// check configuration related to launching executables
        /// </summary>
        /// <param name="newReport"></param>
        private void CheckExecutablePaths(IList<StatusReportItem> newReport)
        {
            foreach(HeliosBinding binding in OutputBindings)
            {
                switch (binding.Action.ActionID)
                {
                    case "launch application":
                        if (binding.ValueSource == BindingValueSources.StaticValue && ValidateExecPath(binding.Value))
                        {
                            newReport.Add(new StatusReportItem
                            {
                                Status = $"Item or application found (or cannot be checked) and is probably viable: '{binding.OutputDescription}'",
                                Recommendation = "No action necessary.",
                                Severity = StatusReportItem.SeverityCode.Info,
                                Flags = StatusReportItem.StatusFlags.ConfigurationUpToDate | StatusReportItem.StatusFlags.Verbose
                            });
                        }
                        else
                        {
                            newReport.Add(new StatusReportItem
                            {
                                Status = $"Argument Error - Item or application not found on this machine or contains incorrect symbols and is not launchable from '{binding.OutputDescription}'",
                                Recommendation = "Install missing application, local file then reload profile.",
                                Severity = StatusReportItem.SeverityCode.Error,
                                Flags = StatusReportItem.StatusFlags.Verbose
                            });
                        }
                        break;
                    case "kill application":
                        newReport.Add(new StatusReportItem
                        {
                            Status = $"A kill action is contained in this profile for process image name '{binding.Value}'",
                            Recommendation = "Ensure that the process being killed is expected and acceptable.",
                            Severity = StatusReportItem.SeverityCode.Info,
                            Flags = StatusReportItem.StatusFlags.ConfigurationUpToDate | StatusReportItem.StatusFlags.Verbose
                        });
                        break;
                    default:
                        break;
                }
            }
        }

        public void PublishStatusReport(IList<StatusReportItem> statusReport)
        {
            _statusReportNotify.PublishStatusReport(statusReport);
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

        private bool ValidateExecPath(string proposedPath){
            //
            // This attempts to validate the text which will eventually form part of a launch
            // operation.
            //
            // The launch is shell based so in addition to an executable, the input could 
            // also be a data file, arguments. It could also
            // contain environment variables or be a UNC, rooted or relative path.
            // We could also have received a URI just to complicate matters.
            //
            // The pre-flight does not attempt to validate arguments, but it takes a stab at
            // validating the first file or URI
            //

            // Resolve environment variables
            proposedPath = Environment.ExpandEnvironmentVariables(proposedPath);

            MatchCollection matches;

            matches = _rxDoubleQuotes.Matches(proposedPath);                            // Extract anything which is enclosed in escaped double quotes
            int blankPosition = proposedPath.IndexOf(" ");                              // For uncomplicated launches, if there is a blank, it will denote the start of the argument(s) 
            if (matches.Count == 0 & blankPosition <= 0)
            {
                // nothing enclosed in escaped double quotes
                return ExtractPathCheck(proposedPath);
            }
            else if (matches.Count == 0)
            {
                //  There is nothing enclosed in double-quotes, so we assume the file to check is before the first blank
                return ExtractPathCheck(proposedPath.Substring(0, blankPosition));
            }
            else
            {
                // There is text enclosed in escaped double quotes and there are blank characters
                return ExtractPathCheck(matches[0].Groups[1].ToString());
            }
        }

        private static bool ExtractURICheck(string proposedPath)
        {
            try
            {
                Uri uriCheck = new Uri(proposedPath);
                switch (uriCheck.Scheme)
                {
                    case "file":
                        if (uriCheck.IsFile) return File.Exists(proposedPath);
                        break;
                    case "http":
                    case "https":
                        return true;
                        break;
                    default:
                        return true;
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.Debug(ex, "URI Parsing for {ProposedPath} threw exception", proposedPath);
                return false;
            }
            return true;
        }

        private static bool ExtractPathCheck(string proposedPath)
        {
            // parse assuming not a URI
            try
            {
                proposedPath = Path.GetFullPath(proposedPath);
            }
            catch (ArgumentException ex)
            {
                Logger.Debug(ex, "GetFullPath Parsing for {ProposedPath} threw exception", proposedPath);
                return false;
            }
            catch (NotSupportedException ex)
            {
                // If we have a URI and attempt to get the full path, then this is probably what caused us to be here
                // this is not necessarily a problem so we do not fail and carry on to do URI checks
                Logger.Debug(ex, "GetFullPath Parsing for {ProposedPath} threw exception", proposedPath);
            }
            // Possibly a URI
            return ExtractURICheck(proposedPath);
        }
    }
}
