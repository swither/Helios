using System.Timers;
using System.Windows.Threading;
using GadrocsWorkshop.Helios.UDPInterface;

namespace GadrocsWorkshop.Helios.Interfaces.DCS.Common
{
    public class DCSExportProtocol
    {
        private readonly Dispatcher _dispatcher;
        private readonly RetriedRequest _requestExport;
        private DCSExportModuleFormat _requestedExports;

        public class RetriedRequest
        {
            private readonly Dispatcher _dispatcher;
            private readonly BaseUDPInterface _udp;

            private string _request;
            private readonly Timer _timer = new Timer();
            private int _retries;
            private string _description = "";

            private readonly int _retryLimit;

            public RetriedRequest(BaseUDPInterface udp, Dispatcher dispatcher)
            {
                _dispatcher = dispatcher;
                _udp = udp;

                // REVISIT: configurable?
                _retryLimit = 10;
                _timer.Interval = 1000;

                _timer.Elapsed += Timer_Elapsed;
            }

            public void Send(string request, string description)
            {
                if (request == _request)
                {
                    OnRetry();
                    return;
                }

                _timer.Stop();
                _request = request;
                _retries = 0;
                _description = description;
                if (_udp.CanSend)
                {
                    ConfigManager.LogManager.LogDebug($"sending {_description}");
                    _udp.SendData(_request);
                }
                else
                {
                    ConfigManager.LogManager.LogDebug(
                        $"cannot send {_description} because UDP transport is not ready or does not know remote endpoint");
                }

                _timer.Start();
            }

            public void Restart()
            {
                ConfigManager.LogManager.LogDebug($"restarting {_description} with up to {_retryLimit} retries");
                _retries = 0;
                _timer.Start();
            }

            public void Stop()
            {
                _request = null;
                _timer.Stop();
            }

            // callback on timer worker thread
            private void Timer_Elapsed(object sender, ElapsedEventArgs e)
            {
                // if profile is unloaded, do nothing
                _dispatcher?.Invoke(OnRetry, DispatcherPriority.Normal);
            }

            private void OnRetry()
            {
                if (_retries >= _retryLimit)
                {
                    // no answer after max retries; the export script is either not there or does not support the command
                    // we are using (normal case if some other Export script is used, so not fatal)
                    // REVISIT: could have advanced setting to say this is entirely different script and just don't even try
                    ConfigManager.LogManager.LogWarning($"giving up on {_description} after {_retries} attempts");
                    _timer.Stop();
                    return;
                }

                if (_request != null)
                {
                    if (_udp.CanSend)
                    {
                        ConfigManager.LogManager.LogDebug($"retrying {_description}");
                        _udp.SendData(_request);
                        _retries++;
                    }
                    else
                    {
                        ConfigManager.LogManager.LogDebug(
                            $"cannot retry {_description} because UDP transport is not ready or does not know remote endpoint");
                    }
                }
            }
        }

        public DCSExportProtocol(BaseUDPInterface udp, Dispatcher dispatcher)
        {
            _dispatcher = dispatcher;
            _requestExport = new RetriedRequest(udp, _dispatcher);
        }

        public void SendDriverRequest(DCSExportModuleFormat driver)
        {
            _requestedExports = driver;
            _requestExport.Send($"D{driver}", $"request to install export module of type {driver}");
        }

        public void OnDriverStatus(DCSExportModuleFormat? driver)
        {
            if (_requestedExports == driver)
            {
                // this acknowledges our attempt to load this driver, if the name matches what we are trying to load
                // cancel retries of "D" command
                _requestExport.Stop();
            }
        }

        /// <summary>
        /// stop any current requests, but keep this object usable
        /// </summary>
        public void Stop()
        {
            _requestExport.Stop();
        }

        public void Reset()
        {
            _requestExport.Restart();
        }

        internal void OnClientChanged()
        {
            // this handles the cases where DCS starts or restarts and we don't have an active
            // retry any more
            _requestExport.Restart();
        }
    }
}