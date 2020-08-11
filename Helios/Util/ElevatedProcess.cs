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
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using Newtonsoft.Json;

namespace GadrocsWorkshop.Helios.Util
{
    public class OneMessageStream : Stream
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private NamedPipeClientStream _connection;
        private bool _didReadSomething;

        public OneMessageStream(NamedPipeClientStream connection)
        {
            _connection = connection;
        }

        public override void Flush()
        {
            // no op
        }

        public override long Seek(long offset, SeekOrigin origin) => throw new InvalidOperationException();

        public override void SetLength(long value)
        {
            throw new InvalidOperationException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_connection.IsMessageComplete && _didReadSomething)
            {
                // end of stream
                Logger.Debug("complete message received; ending stream");
                return 0;
            }

            int result =_connection.Read(buffer, offset, count);
            if (result > 0)
            {
                Logger.Debug($"received {result} bytes for total of {offset + result}");
                _didReadSomething = true;
            }
            else
            {
                Logger.Debug($"ending stream on read result of {result}");
            }
            return result;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new InvalidOperationException();
        }

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new InvalidOperationException();
        public override long Position
        {
            get => throw new InvalidOperationException(); 
            set => throw new InvalidOperationException();
        }
    }

    public class ElevatedProcess: IDisposable
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        // wait at most three seconds to start process (not to finish patching)
        private const int ConnectTimeout = 3000;

        private readonly string _executablePath;
        private IList<string> _args;
        private NamedPipeClientStream _client;
        private Process _process;

        public ElevatedProcess(string executablePath, IEnumerable<string> args)
        {
            _executablePath = executablePath;
            _args = args.ToList();
        }

        public bool TryExecute()
        {
            // get a unique pipe name
            string pipeName = Guid.NewGuid().ToString();

            // work around MS problem described in https://stackoverflow.com/questions/32739224/c-sharp-unauthorizedaccessexception-when-enabling-messagemode-for-read-only-name
            _client = new NamedPipeClientStream(".",
                pipeName,
                PipeAccessRights.ReadData | PipeAccessRights.WriteAttributes,
                PipeOptions.None,
                System.Security.Principal.TokenImpersonationLevel.None,
                HandleInheritability.None);

            // check for existing process
            if (!File.Exists(_executablePath))
            {
                // NOTE: we will also report this when called for status
                Logger.Error($"Elevation of privileges failed because the executable to be run as administrator does not exist: {Anonymizer.Anonymize(_executablePath)}");
                return false;
            }

            // create elevated process
            string arguments = string.Join(" ", new[] {"-o", pipeName}.Concat(_args));
            ProcessStartInfo startInfo = new ProcessStartInfo(_executablePath, arguments)
            {
                Verb = "runas",
                UseShellExecute = true
            };
            try
            {
                _process = Process.Start(startInfo);
                return true;
            }
            catch (Win32Exception)
            {
                // NOTE: this is a normal case, don't log the exception
                Logger.Error("Elevation of privileges to install patches failed or was canceled by the user");
                return false;
            }
        }

        public IList<StatusReportItem> ReadResults()
        {
            if (_client == null)
            {
                throw new Exception("tried to read results from elevated process before executing it");
            }
            if (_process == null)
            {
                return ReportError($"The executable {Anonymizer.Anonymize(_executablePath)} to be run as administrator was not found");
            }
            try
            {
                _client.Connect(ConnectTimeout);
            }
            catch (TimeoutException)
            {
                Logger.Error("start up of elevated process {Path} timed out", _executablePath);
                return ReportError($"The elevated process {_executablePath} failed to respond within the permissible timeout");
            }

            try
            {
                _client.ReadMode = PipeTransmissionMode.Message;
                using (OneMessageStream oneMessage = new OneMessageStream(_client))
                using (StreamReader reader = new StreamReader(oneMessage))
                using (JsonTextReader jsonReader = new JsonTextReader(reader))
                {
                    JsonSerializer json = new JsonSerializer();
                    return json.Deserialize<StatusReportItem[]>(jsonReader) ?? 
                           ReportError($"The elevated process {_executablePath} failed to return any result");
                }
            }
            catch (InvalidOperationException ex)
            {
                Logger.Error(ex, "connection to elevated process {Path} failed", _executablePath);
                return ReportError($"The elevated process {_executablePath} failed to return a valid status report");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "unexpected exception from IPC with elevated process {Path}", _executablePath);
                return ReportError($"The elevated process {_executablePath} failed in an unexpected way");
            }
        }

        private IList<StatusReportItem> ReportError(string message)
        {
            return new StatusReportItem
            {
                Status = message,
                Recommendation = "Please file a bug",
                Severity = StatusReportItem.SeverityCode.Error
            }.AsReport();
        }

        public void Dispose()
        {
            _client?.Dispose();
            _client = null;

            _process?.Dispose();
            _process = null;
        }
    }
}