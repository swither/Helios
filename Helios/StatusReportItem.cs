// Copyright 2020 Helios Contributors
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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using GadrocsWorkshop.Helios.Util;
using Newtonsoft.Json;

namespace GadrocsWorkshop.Helios
{
    /// <summary>
    /// this is a structured log item, with the additional restriction
    /// that it is intended for human consumption, so long messages
    /// can be truncated and implementation-related messages are discouraged
    /// </summary>
    public class StatusReportItem
    {
        /// <summary>
        /// This severity code mirrors the log levels where names are the same.
        /// Note: It should be a nested class of StatusReportItem, but WPF bindings require it to be
        /// a top-level type.
        /// </summary>
        public enum SeverityCode
        {
            /// <summary>
            /// Informational messages, to be displayed with no indication that this is a problem.
            /// Usually, these items have no Recommendation, but if the Recommendation is present, then this
            /// indicates an action the use should take but does not represent an unusual or dangerous
            /// problem
            /// </summary>
            Info,

            /// <summary>
            /// Warning indicates a problem that the user should correct.  It will be displayed in a disruptive
            /// manner in the UI, but the program can continue to operate.
            /// </summary>
            Warning,

            /// <summary>
            /// Error indicates a problem that must be corrected for correct operation.  If the user decides to
            /// continue anyway, things may break.
            /// </summary>
            Error,

            /// <summary>
            /// no messages should be created at this level, in it used to filter out all messages
            /// </summary>
            None
        }

        // Uri components used in Links
        public const string HELIOS_SCHEME = "helios";
        public const string PROFILE_EDITOR_HOST = "profileeditor";
        public const string CONTROL_CENTER_HOST = "controlcenter";

        /// <summary>
        /// optional time stamp or null
        /// </summary>
        [JsonProperty("timeStamp", NullValueHandling = NullValueHandling.Ignore)]
        public string TimeStamp { get; set; } = CreateTimeStamp(DateTime.Now);

        [JsonProperty("severity", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public SeverityCode Severity { get; set; } = SeverityCode.Info;

        /// <summary>
        /// the status message, which may be more than one line long
        /// but should generally be short
        /// </summary>
        [JsonProperty("status")]
        public string Status { get; set; }

        /// <summary>
        /// Additional information related to this status, expressed as a set of
        /// flags that may be set to communicate different facts.
        /// </summary>
        [Flags]
        public enum StatusFlags
        {
            /// <summary>
            /// no flags 
            /// </summary>
            None = 0,

            /// <summary>
            /// this status may be numerous or verbose and should be filtered in small status displays
            /// </summary>
            Verbose = 1,

            /// <summary>
            /// this status indicates that some checked configuration item was up to date and does not need to regenerated
            /// </summary>
            ConfigurationUpToDate = 2,

            /// <summary>
            /// the timestamp in this item is from a log, so correlation can be done
            /// otherwise the time stamp might be off by some milliseconds from related log entries
            /// </summary>
            TimeStampIsPrecise = 4,

            /// <summary>
            /// when a warning or error item with this flag is reported, do not trigger distruptive UI interaction, such as popping up
            /// a dialog, because it will happen during normal operation
            /// </summary>
            DoNotDisturb = 8
        }

        /// <summary>
        /// Any flags set in this value (combined via binary 'or') indicate that
        /// the corresponding fact is true about this status report item.
        /// </summary>
        [JsonProperty("flags", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public StatusFlags Flags { get; set; }

        /// <summary>
        /// a recommendation to the user or null
        /// </summary>
        [JsonProperty("recommendation", NullValueHandling = NullValueHandling.Ignore)]
        public string Recommendation { get; set; }

        /// <summary>
        /// in the event that the status message was shortened or cleaned up, this
        /// property will have the original full text, which is not safe for UI display
        /// and should only be used for logging or sharing purposes
        /// </summary>
        [JsonProperty("fullText", NullValueHandling = NullValueHandling.Ignore)]
        public string FullText { get; set; }

        /// <summary>
        /// If this is not null and a consumer of this data has the capability of generating
        /// references or links, this value should be parsed and presented as a link to the
        /// specified entities.
        /// </summary>
        [JsonIgnore] public Uri Link;

        [JsonProperty("code", NullValueHandling = NullValueHandling.Ignore)]
        public IList<CodeLine> CodeLines { get; private set; }

        [JsonIgnore]
        public string Code
        {
            // parse into code lines
            set
            {
                if (value == null)
                {
                    CodeLines = null;
                    return;
                }
                CodeLines = Regex.Split(value, "(?<=\r?\n)")
                    .Where(text => !string.IsNullOrEmpty(text))
                    .Select(text => new CodeLine(text))
                    .ToList();
            }
        }

        /// <summary>
        /// utility to create a link to the profile editor without referring to any specific UI component
        /// </summary>
        public static Uri ProfileEditor => new Uri($"{HELIOS_SCHEME}://{PROFILE_EDITOR_HOST}/");

        /// <summary>
        /// utility to create a link to the control center without referring to any specific UI component
        /// </summary>
        public static Uri ControlCenter => new Uri($"{HELIOS_SCHEME}://{CONTROL_CENTER_HOST}/");

        /// <summary>
        /// log this result
        /// </summary>
        /// <param name="logManager"></param>
        public void Log(LogManager logManager)
        {
            switch (Severity)
            {
                case SeverityCode.None:
                    throw new Exception(
                        "Severity 'None' must not be used for any status report instances; implementation error");
                case SeverityCode.Info:
                    logManager.LogInfo(Status);
                    if (Recommendation != null)
                    {
                        logManager.LogInfo(Recommendation);
                    }

                    break;
                case SeverityCode.Warning:
                    // status of warning or error severity is still correct program behavior, so we log as info
                    logManager.LogInfo(Status);
                    if (Recommendation != null)
                    {
                        logManager.LogInfo(Recommendation);
                    }

                    break;
                case SeverityCode.Error:
                    // status of warning or error severity is still correct program behavior, so we log as info
                    logManager.LogInfo(Status);
                    if (Recommendation != null)
                    {
                        logManager.LogInfo(Recommendation);
                    }
                    break;
            }
        }

        public static string CreateTimeStamp(DateTime dateTime) => dateTime.ToString("MM/dd/yyyy hh:mm:ss.fff tt", CultureInfo.InvariantCulture);

        // utility to encapsulate this as a status report when it is the only item in the report
        public IList<StatusReportItem> AsReport()
        {
            return new List<StatusReportItem> { this };
        }
    }
}