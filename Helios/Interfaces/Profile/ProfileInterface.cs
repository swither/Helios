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

            RegexOptions options = RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace;
            MatchCollection _matches;
            // Find out if we're launching a common executable for this machine
            _matches = Regex.Matches(e.Value.StringValue, "(" + Environment.GetEnvironmentVariable("PATHEXT").Replace(";", "|").Replace(".", "\\.") + ")", options);
            bool _exe = _matches.Count > 0 ? true : false;

            string _expandedPath = e.Value.StringValue;
            string _expandedArgs = "";
            if (_exe)
            {
                // For executables launches, we resolve environment variables
                Logger.Debug("Launch type determined to be an executable based on %PATHEXT% \"" + e.Value.StringValue + "\")");

                _matches = Regex.Matches(e.Value.StringValue, "(?:\\\")([^\"]*)(?:\\\")");  // Extract anything which is enclosed in escaped double quotes
                int _blank = e.Value.StringValue.IndexOf(" ");
                if (_matches.Count == 0)
                {
                    //  There is nothing enclosed in double-quotes, so we assume the executable is before the first space and any arguments follow the first space

                    if (_blank > 0)
                    {
                        _expandedPath = Environment.ExpandEnvironmentVariables(e.Value.StringValue.Substring(0, _blank));
                        _expandedArgs = Environment.ExpandEnvironmentVariables(e.Value.StringValue.Substring(_blank + 1));
                    }
                    else
                    {
                        _expandedPath = Environment.ExpandEnvironmentVariables(e.Value.StringValue);
                        _expandedArgs = "";
                    }
                }
                else
                {
                    int _matchCursor = 0;
                    foreach (Match _matchItem in _matches)
                    {
                        if (_matchItem.Equals(_matches[0]))
                        {
                            if (_matchItem.Index == 0)
                            {
                                _expandedPath = Environment.ExpandEnvironmentVariables(_matchItem.Groups[1].ToString());
                                _matchCursor = _matchItem.Length;
                            }
                            else
                            {
                                _expandedPath = Environment.ExpandEnvironmentVariables(e.Value.StringValue.Substring(0, _blank));
                                _expandedArgs = Environment.ExpandEnvironmentVariables(e.Value.StringValue.Substring(_blank, _matchItem.Index - _blank));
                                _expandedArgs += " " + Environment.ExpandEnvironmentVariables(_matchItem.ToString());
                                _matchCursor = _matchItem.Index + _matchItem.Length;
                            }
                        }
                        else
                        {
                            if (_matchItem.Index == _matchCursor)
                            {
                                _expandedArgs += " " + Environment.ExpandEnvironmentVariables(_matchItem.ToString());
                            }
                            else
                            {
                                _expandedArgs += Environment.ExpandEnvironmentVariables(e.Value.StringValue.Substring(_matchCursor, _matchItem.Index - _matchCursor));
                                _matchCursor = _matchItem.Index + _matchItem.Length;
                                _expandedArgs += " " + Environment.ExpandEnvironmentVariables(_matchItem.ToString());
                            }
                        }
                        Logger.Debug("Double Quoted item found \"" + _matchItem.ToString() + "\"");
                    }
                }
            }
            try
            {
                if (_exe)
                {
                    ProcessStartInfo _psi = new ProcessStartInfo();
                    _psi.FileName = System.IO.Path.GetFileName(_expandedPath);
                    _psi.WorkingDirectory = System.IO.Path.GetDirectoryName(_expandedPath);
                    _psi.Arguments = _expandedArgs.Trim();
                    _psi.UseShellExecute = true;
                    _psi.RedirectStandardOutput = false;
                    Process.Start(_psi);
                }
                else
                {
                    Logger.Debug("Launch type determined to be non-executable based on %PATHEXT% \"" + e.Value.StringValue + "\")");
                    Process.Start(e.Value.StringValue);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex,"Error caught launching external application (path=\"" + _expandedPath + "\") Exception " + ex.Message);
            }
        }

        void KillApplication_Execute(object action, HeliosActionEventArgs e)
        {
            try
            {
                Process[] _localProcessesByName = Process.GetProcessesByName(e.Value.StringValue);
                foreach (Process _proc in _localProcessesByName)
                {
                    Logger.Info("Killing process image name \"" + e.Value.StringValue + "\"");
                    _proc.Kill();
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error caught during kill processing for process image name \"" + e.Value.StringValue + "\")");
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
            foreach(HeliosBinding _binding in OutputBindings)
            {
                if(_binding.Action.ActionID == "launch application")
                {
                    if (_binding.ValueSource == BindingValueSources.StaticValue && ValidateExecPath(_binding.Value))
                    {
                        newReport.Add(new StatusReportItem
                        {
                            Status = $"Item or application found (or cannot be checked) and is probably viable: '{_binding.OutputDescription}'",
                            Recommendation = "No action necessary.",
                            Severity = StatusReportItem.SeverityCode.Info,
                            Flags = StatusReportItem.StatusFlags.ConfigurationUpToDate | StatusReportItem.StatusFlags.Verbose
                        });
                    }
                    else
                    {
                        newReport.Add(new StatusReportItem
                        {
                            Status = $"Item or application not found on this machine and may not be launchable from '{_binding.OutputDescription}'",
                            Recommendation = "Install missing application or local file then reload profile.",
                            Severity = StatusReportItem.SeverityCode.Warning,
                            Flags = StatusReportItem.StatusFlags.ConfigurationUpToDate | StatusReportItem.StatusFlags.Verbose
                        });

                    }
                } else if(_binding.Action.ActionID == "kill application")
                {
                    newReport.Add(new StatusReportItem
                    {
                        Status = $"A kill action is contained in this profile for process image name '{_binding.Value}'",
                        Recommendation = "Ensure that the process being killed is expected and acceptable.",
                        Severity = StatusReportItem.SeverityCode.Info,
                        Flags = StatusReportItem.StatusFlags.ConfigurationUpToDate | StatusReportItem.StatusFlags.Verbose
                    });
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

        private bool ValidateExecPath(string _proposedPath){
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
            _proposedPath = Environment.ExpandEnvironmentVariables(_proposedPath);

            RegexOptions options = RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace;
            MatchCollection _matches;

            _matches = Regex.Matches(_proposedPath, "(?:\\\")([^\"]*)(?:\\\")",options);  // Extract anything which is enclosed in escaped double quotes
            int _blankPosition = _proposedPath.IndexOf(" ");                              // For uncomplicated launches, if there is a blank, it will denote the start of the argument(s) 
            if (_matches.Count == 0 & _blankPosition <= 0)
            {
                // nothing enclosed in escaped double quotes
                //
                // parse assuming not a URI
                try
                {
                    _proposedPath = Path.GetFullPath(_proposedPath);
                }
                catch (ArgumentException Ex)
                {
                    Logger.Debug("GetFullPath Parsing for \"" + _proposedPath + "\" threw exception \"" + Ex.ToString() + "\"");
                    return false;
                }
                catch (NotSupportedException Ex)
                {
                    // If we have a URI and attempt to get the full path, then this is probably what caused us to be here
                    Logger.Debug("GetFullPath Parsing for \"" + _proposedPath + "\" threw exception \"" + Ex.Message + "\"");
                }
                try
                {
                    Uri _uriCheck = new Uri(_proposedPath);
                    if (_uriCheck.IsFile) return File.Exists(_proposedPath) ? true : false;
                }
                catch (Exception Ex)
                {
                    Logger.Debug("URI Parsing for \"" + _proposedPath + "\" threw exception \"" + Ex.ToString() + "\"");
                    return false;
                }
            } else if (_matches.Count == 0)
            {
                //  There is nothing enclosed in double-quotes, so we assume the file to check is before the first blank
                //
                //  parse assuming not URI
                try
                {
                    _proposedPath = Path.GetFullPath(_proposedPath.Substring(0, _blankPosition));
                }
                catch (ArgumentException Ex)
                {
                    Logger.Debug("GetFullPath Parsing for \"" + _proposedPath.Substring(0, _blankPosition) + "\" threw exception \"" + Ex.ToString() + "\"");
                    return false;
                }
                catch (NotSupportedException Ex)
                {
                    // If we have a URI and attempt to get the full path, then this is probably what caused us to be here
                    Logger.Debug("GetFullPath Parsing for \"" + _proposedPath.Substring(0, _blankPosition) + "\" threw exception \"" + Ex.Message + "\"");
                }
                try
                {
                    Uri _uriCheck = new Uri(Path.GetFullPath(_proposedPath));
                    if (_uriCheck.IsFile) return File.Exists(_proposedPath) ? true : false;
                }
                catch (Exception Ex)
                {
                    Logger.Debug("URI Parsing for \"" + _proposedPath + "\" threw exception \"" + Ex.ToString() + "\"");
                    return false;
                }
            }
            else
            {
                // There is text enclosed in escaped double quotes and there are blank characters
                //
                // parse assuming not URI
                try
                {
                    _proposedPath = Path.GetFullPath(_matches[0].Groups[1].ToString());
                }
                catch (ArgumentException Ex)
                {
                    Logger.Debug("GetFullPath Parsing for \"" + _matches[0].Groups[1].ToString() + "\" threw exception \"" + Ex.Message + "\"");
                    return false;
                }
                catch(NotSupportedException Ex)
                {
                    // If we have a URI and attempt to get the full path, then this is probably what caused us to be here
                    Logger.Debug("GetFullPath Parsing for \"" + _matches[0].Groups[1].ToString() + "\" threw exception \"" + Ex.Message + "\"");
                }
                try
                {
                    Uri _uriCheck = new Uri(_proposedPath);
                    if (_uriCheck.IsFile) return File.Exists(_proposedPath) ? true : false;
                }
                catch (Exception Ex)
                {
                    Logger.Debug("URI Parsing for \"" + _matches[0].Groups[1].ToString() + "\" threw exception \""+ Ex.ToString() + "\"");
                    return false;
                }
            }
            return true;
        }
    }
}
