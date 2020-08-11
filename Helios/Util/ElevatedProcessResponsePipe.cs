using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipes;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading;
using Newtonsoft.Json;

namespace GadrocsWorkshop.Helios.Util
{
    /// <summary>
    /// this class implements the server side of a named pipe to be used as the other end of
    /// ElevatedProcess or similar child process utilities
    ///
    /// it serializes a status report through the named pipe
    /// </summary>
    public class ElevatedProcessResponsePipe: IDisposable
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private NamedPipeServerStream _server;
        private readonly object _lock = new object();
        private IList<StatusReportItem> _report;
        private bool _connected;
        private ManualResetEvent _reportSent = new ManualResetEvent(false);

        // how long we will wait for our caller to retrieve our report
        private const int ReportDeliveryTimeout = 3000;

        public ElevatedProcessResponsePipe(string pipeName)
        {
            // create a pipe that can be connected to by a non-elevated process running as any authenticated user
            PipeSecurity pipeSecurity = new PipeSecurity();
            PipeAccessRule aceClients = new PipeAccessRule(
                new SecurityIdentifier(WellKnownSidType.AuthenticatedUserSid, null),
                PipeAccessRights.ReadWrite,
                AccessControlType.Allow);
            pipeSecurity.AddAccessRule(aceClients);

            // full permissions for us
            WindowsIdentity windowsIdentity = WindowsIdentity.GetCurrent();
            if (windowsIdentity.Owner == null)
            {
                throw new Exception(
                    "unsupported platform or program error: the current windows identity has no owner, so we cannot correctly configure permissions");
            }
            PipeAccessRule aceOwner = new PipeAccessRule(
                    windowsIdentity.Owner,
                    PipeAccessRights.FullControl,
                    AccessControlType.Allow);
            pipeSecurity.AddAccessRule(aceOwner);

            // create the pipe atomically with its security settings, so client does not try to
            // talk to it before we set security
            _server = new NamedPipeServerStream(pipeName,
                PipeDirection.Out,
                1,
                PipeTransmissionMode.Message,
                PipeOptions.Asynchronous,
                0,
                4096,
                pipeSecurity,
                System.IO.HandleInheritability.None,
                PipeAccessRights.ChangePermissions);

            // async connect
            _server.BeginWaitForConnection(OnConnection, null);
        }

        private void OnConnection(IAsyncResult ar)
        {
            if (_server == null)
            {
                Debug.WriteLine("ignoring late connection");
                return;
            }
            _server.EndWaitForConnection(ar);
            IList<StatusReportItem> report;
            lock (_lock)
            {
                _connected = true;
                report = _report;
            }

            if (report != null)
            {
                Debug.WriteLine("report already available; sending on connect");
                DoSendReport(report);
            }
        }

        public void SendReport(IList<StatusReportItem> report)
        {
            bool connected;
            lock (_lock)
            {
                connected = _connected;
                _report = report;
            }

            if (connected)
            {
                Debug.WriteLine("already connected; sending report immediately");
                DoSendReport(report);
            }
        }

        private void DoSendReport(IList<StatusReportItem> report)
        {
            string text = JsonConvert.SerializeObject(report);
            Logger.Debug($"Sending result report:\n{text}");
            byte[] buffer = new UTF8Encoding().GetBytes(text);
            _server.BeginWrite(buffer, 0, buffer.Length, OnWrite, null);
        }

        private void OnWrite(IAsyncResult ar)
        {
            if (_server == null)
            {
                Debug.WriteLine("ignoring late write");
                return;
            }
            Debug.WriteLine("write completed");
            _server.EndWrite(ar);
            _server.Close();
            Debug.WriteLine("signaling report sent");
            _reportSent.Set();
        }

        public void Dispose()
        {
            _server?.Dispose();
            _server = null;
        }

        public void WaitToDeliver()
        {
            bool result = _reportSent.WaitOne(ReportDeliveryTimeout);
            if (result)
            {
                Debug.WriteLine("report was sent");
            }
        }
    }
}