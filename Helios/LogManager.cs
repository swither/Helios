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

using System.Text.RegularExpressions;

namespace GadrocsWorkshop.Helios
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Windows.Threading;

    public interface ILogConsumer
    {
        /// <summary>
        /// write calls will be asynchronously scheduled on this
        /// </summary>
        Dispatcher Dispatcher { get; }

        /// <summary>
        /// Callback scheduled on dispatcher provided.  Debug messages
        /// are not scheduled at all.
        /// </summary>
        /// <param name="timeStamp">the same timestamp used in the primary log file</param>
        /// <param name="level">always LogLevel.Info or higher</param>
        /// <param name="message"></param>
        /// <param name="exception">an exception that is being logged or null</param>
        void WriteLogMessage(string timeStamp, LogLevel level, string message, Exception exception);
    }

    public class LogManager
    {
        private System.Object _lock = new System.Object();

        public LogManager(LogLevel level)
        {
        }

        public void LogDebug(string message)
        {
            WriteLogMessage(Helios.LogLevel.Debug, message, null);
        }

        public void Log(string message)
        {
            WriteLogMessage(Helios.LogLevel.All, message, null);
        }

        public void LogWarning(string message)
        {
            WriteLogMessage(LogLevel.Warning, message, null);
        }

        public void LogWarning(string message, Exception exception)
        {
            WriteLogMessage(LogLevel.Warning, message, exception);
        }

        public void LogError(string message)
        {
            WriteLogMessage(LogLevel.Error, message, null);
        }

        public void LogError(string message, Exception exception)
        {
            WriteLogMessage(LogLevel.Error, message, exception);
        }

        public void LogInfo(string message)
        {
            WriteLogMessage(LogLevel.Info, message, null);
        }

        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private void WriteLogMessage(LogLevel level, string message, Exception exception)
        {
            switch (level)
            {
                case LogLevel.All:
                    Logger.Info(message);
                    break;
                case LogLevel.Error:
                    if (exception != null)
                    {
                        Logger.Error(exception, message);
                    }
                    else
                    {
                        Logger.Error(message);
                    }
                    break;
                case LogLevel.Warning:
                    Logger.Warn(message);
                    break;
                case LogLevel.Info:
                    Logger.Info(message);
                    break;
                case LogLevel.Debug:
                    Logger.Debug(message);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(level), level, null);
            }
        }

        private static string CreateTimeStamp()
        {
            return DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt");
        }

        private void WriteException(StreamWriter writer, Exception exception)
        {
            if (!string.IsNullOrEmpty(exception.Source))
            {
                writer.WriteLine("Exception Source:" + exception.Source);
            }
            writer.WriteLine("Exception Message:" + exception.Message);
            writer.WriteLine("Stack Trace:");
            WriteStackTrace(writer, exception);

            if (exception is ReflectionTypeLoadException le)
            {
                foreach (Exception e2 in le.LoaderExceptions)
                {
                    WriteException(writer, e2);
                }
            }

            if (exception.InnerException != null)
            {
                writer.WriteLine();
                WriteException(writer, exception.InnerException);
            }
        }

        private static void WriteStackTrace(StreamWriter writer, Exception exception)
        {
            Regex buidPathExpression = new Regex("[A-Z]:\\\\.*\\\\Helios\\\\");
            string trace = exception.StackTrace;
            Match buildPathMatch = buidPathExpression.Match(trace);
            if (buildPathMatch.Success)
            {
                writer.WriteLine(trace.Replace(buildPathMatch.Groups[0].Value, ""));
            }
            else
            {
                writer.WriteLine(trace);
            }
        }
    }
}
