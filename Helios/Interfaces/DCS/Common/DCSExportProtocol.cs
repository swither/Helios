using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using GadrocsWorkshop.Helios.ProfileAwareInterface;
using GadrocsWorkshop.Helios.UDPInterface;

namespace GadrocsWorkshop.Helios.Interfaces.DCS.Common
{
    public class DCSExportProtocol
    {
        private HeliosProfile _profile;
        private RetriedRequest _requestExport;
        private string _requestedExports;

        public class RetriedRequest
        {
            private HeliosProfile _profile;
            private BaseUDPInterface _udp;

            private string _request;
            private Timer _timer = new Timer();
            private int _retries = 0;
            private string _description = "";

            private int _retryLimit;

            public RetriedRequest(UDPInterface.BaseUDPInterface udp, HeliosProfile profile)
            {
                _profile = profile;
                _udp = udp;

                // REVISIT: configurable?
                _retryLimit = 10;
                _timer.Interval = 1000;

                _timer.Elapsed += Timer_Elapsed;
            }

            public void Send(string request, string description)
            {
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
                    ConfigManager.LogManager.LogDebug($"cannot send {_description} because UDP transport is not ready or does not know remote endpoint");
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
                _profile?.Dispatcher.Invoke((Action)OnRetry, System.Windows.Threading.DispatcherPriority.Normal);
            }

            private void OnRetry()
            {
                if (_retries >= _retryLimit)
                {
                    // no answer after max retries; the export script is either not there or does not support the command
                    // we are using (normal case if some other Export script is used)
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
                        ConfigManager.LogManager.LogDebug($"cannot retry {_description} because UDP transport is not ready or does not know remote endpoint");
                    }
                }
            }
        }

        public DCSExportProtocol(UDPInterface.BaseUDPInterface udp, HeliosProfile profile)
        {
            _profile = profile;
            _requestExport = new RetriedRequest(udp, profile);
        }

        public void SendDriverRequest(string driverShortName)
        {
            _requestedExports = driverShortName;
            _requestExport.Send($"D{driverShortName}", $"request to install export driver {driverShortName}");
        }

        public void SendModuleRequest()
        {
            _requestedExports = null;
            _requestExport.Send($"M", $"request to install export module for current aircraft");
        }

        public void OnDriverStatus(string driverShortName)
        {
            if (_requestedExports == driverShortName)
            {
                // this acknowledges our attempt to load this driver, if the name matches what we are trying to load
                // cancel retries of "D" command
                _requestExport.Stop();
            }
        }

        public void OnModuleStatus()
        {
            if (_requestedExports == null)
            {
                // this acknowledges our attempt to load a module
                // cancel retries of "M" command
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
