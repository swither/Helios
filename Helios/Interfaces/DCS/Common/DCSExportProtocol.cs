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

using System.Timers;
using System.Windows;
using System.Windows.Threading;
using GadrocsWorkshop.Helios.UDPInterface;

namespace GadrocsWorkshop.Helios.Interfaces.DCS.Common
{
    public class DCSExportProtocol
    {
        private readonly RetriedRequest _requestExport;
        private DCSExportModuleFormat _requestedExports;
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public class RetriedRequest
        {
            private readonly BaseUDPInterface _udp;

            private string _request;
            private readonly Timer _timer = new Timer();
            private int _retries;
            private string _description = "";

            private readonly int _retryLimit;
            private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

            public RetriedRequest(BaseUDPInterface udp)
            {
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
                    Logger.Debug($"sending {_description}");
                    _udp.SendData(_request);
                }
                else
                {
                    Logger.Debug(
                        $"cannot send {_description} because UDP transport is not ready or does not know remote endpoint");
                }

                _timer.Start();
            }

            public void Restart()
            {
                Logger.Debug($"restarting {_description} with up to {_retryLimit} retries");
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
                Application.Current.Dispatcher.Invoke(OnRetry, DispatcherPriority.Normal);
            }

            private void OnRetry()
            {
                if (_retries >= _retryLimit)
                {
                    // no answer after max retries; the export script is either not there or does not support the command
                    // we are using (normal case if some other Export script is used, so not fatal)
                    // REVISIT: could have advanced setting to say this is entirely different script and just don't even try
                    Logger.Info($"giving up on {_description} after {_retries} attempts");
                    _timer.Stop();
                    return;
                }

                if (_request != null)
                {
                    if (_udp.CanSend)
                    {
                        Logger.Debug($"retrying {_description}");
                        _udp.SendData(_request);
                        _retries++;
                    }
                    else
                    {
                        Logger.Debug(
                            $"cannot retry {_description} because UDP transport is not ready or does not know remote endpoint");
                    }
                }
            }
        }

        public DCSExportProtocol(BaseUDPInterface udp)
        {
            _requestExport = new RetriedRequest(udp);
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