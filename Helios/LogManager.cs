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
        private string _logFile;
        private LogLevel _level = LogLevel.Info;
        private HashSet<ILogConsumer> _consumers = new HashSet<ILogConsumer>();
        private System.Object _lock = new System.Object();

        public LogManager(string path, LogLevel level)
        {
            _logFile = path;
            _level = level;
        }

        #region Properties

        public LogLevel LogLevel
        {
            get { return _level; }
            set { _level = value; }
        }

        #endregion

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

        public void RegisterConsumer(ILogConsumer consumer)
        {
            lock (_lock)
            {
                _consumers.Add(consumer);
            }
        }

        public void DeregisterConsumer(ILogConsumer consumer)
        {
            lock (_lock)
            {
                _consumers.Remove(consumer);
            }
        }

        private void WriteLogMessage(LogLevel level, string message, Exception exception)
        {
#if DEBUG
            Console.WriteLine($"{createTimeStamp()} {level.ToString()}: {message}");
#endif
            string timeStamp = null;
            if (_level >= level)
            {
                timeStamp = WriteToFile(level, message, exception);
            } 
 
            // after writing to file, also send all non-debug messages to consumers
            if (level < LogLevel.Debug)
            {
                // use timestamp from file for correlation, if available
                DispatchToConsumers(timeStamp ?? createTimeStamp(), level, message, exception);
            }
        }

        private static string createTimeStamp()
        {
            return DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt");
        }

        private string WriteToFile(LogLevel level, string message, Exception exception)
        {
            lock (_lock)
            {
                try
                {
                    FileInfo errorFile = new FileInfo(_logFile);

                    StreamWriter errorWriter;

                    if (errorFile.Exists)
                    {
                        errorWriter = errorFile.AppendText();
                    }
                    else
                    {
                        errorWriter = errorFile.CreateText();
                    }

                    using (errorWriter)
                    {
                        // because we get this timestamp under lock, messages will be in order in the file
                        string timeStamp = createTimeStamp();

                        // write to file
                        errorWriter.Write(timeStamp);
                        errorWriter.Write(" - ");
                        errorWriter.Write(level.ToString());
                        errorWriter.Write(" - ");
                        errorWriter.WriteLine(message);

                        if (exception != null)
                        {
                            WriteException(errorWriter, exception);
                        }
                        return timeStamp;
                    }
                }
                catch (Exception)
                {
                    // Nothing to do but go on.
                    return null;
                }
            }
        }

        private void DispatchToConsumers(string timeStamp, LogLevel level, string message, Exception exception)
        {
            // all we do under lock here is schedule async code
            lock (_lock)
            {
                // no exception handling here, since messages aren't handled on this stack
                foreach (ILogConsumer consumer in _consumers)
                {
                    // note: exceptions here will bring down the application
                    consumer.Dispatcher.InvokeAsync(() =>
                    {
                        try
                        {
                            consumer.WriteLogMessage(timeStamp, level, message, exception);
                        }
                        catch (Exception ex)
                        {
                            WriteToFile(LogLevel.Error, "log consumer failed to write message", ex);
                        }
                    }, DispatcherPriority.ApplicationIdle);
                }
            }
        }

        private void WriteException(StreamWriter writer, Exception exception)
        {
            if (exception.Source != null && exception.Source.Length > 0)
            {
                writer.WriteLine("Exception Source:" + exception.Source);
            }
            writer.WriteLine("Exception Message:" + exception.Message);
            writer.WriteLine("Stack Trace:");
            writer.WriteLine(exception.StackTrace);

            ReflectionTypeLoadException le = exception as ReflectionTypeLoadException;
            if (le != null)
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
    }
}
