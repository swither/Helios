//  Copyright 2014 Craig Courtney
//  Copyright 2020 Helios Contributors
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

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace GadrocsWorkshop.Helios
{
    using System;
    using System.Runtime.InteropServices;

    public class DisplayManager
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private int _dpi;
        private List<Monitor> _simulatedMonitors;

        /// <summary>
        /// can be used to pretend that the local machine has the given monitors, to test reset monitors or to reset
        /// published profiles to a neutral "reference" configuration
        ///
        /// NOTE: monitors will be sorted according to normal sorting rules if they are not provided in sorted order
        /// </summary>
        /// <param name="monitors"></param>
        public void Simulate(IEnumerable<Monitor> monitors)
        {
            // deep copy
            _simulatedMonitors = monitors.Select(monitor => new Monitor(monitor)).ToList();
        }

        public void StopSimulating()
        {
            _simulatedMonitors = null;
        }

        #region Properties

        // XXX there is now a correct API for this: https://docs.microsoft.com/en-us/windows/win32/api/shellscalingapi/nf-shellscalingapi-getdpiformonitor?redirectedfrom=MSDN
        // XXX this really needs to be a property of a specific monitor
        public int DPI
        {
            get
            {
                if (_dpi != 0)
                {
                    return _dpi;
                }

                Logger.Debug("Helios is measuring screen DPI");
                IntPtr desktopHwnd = IntPtr.Zero;
                IntPtr desktopDC = NativeMethods.GetDC(desktopHwnd);
                _dpi = NativeMethods.GetDeviceCaps(desktopDC, 88 /*LOGPIXELSX*/);
                NativeMethods.ReleaseDC(desktopHwnd, desktopDC);
                Logger.Info($"Helios has determined screen DPI is {_dpi}");
                return _dpi;
            }
        }

        public double ConvertPixels(int pixels)
        {
            return Math.Round(pixels * 96.0 / DPI);
        }

        /// <summary>
        /// this value is required by some windows APIs such as FormattedText
        /// </summary>
        public double PixelsPerDip => ((double)DPI) / 96.0;

        /// <summary>
        /// Returns the number of Displays using the Win32 functions
        /// </summary>
        /// <returns>collection of Display Info</returns>
        public MonitorCollection Displays
        {
            get 
            {
                MonitorCollection displayCollection = new MonitorCollection();
                try
                {
                    IEnumerable<Monitor> monitors = CreateSimulatedMonitors ?? EnumerateDisplays()
                        .Select(LogDisplayDevice)
                        .Where(displayDevice => displayDevice.StateFlags.HasFlag(NativeMethods.DisplayDeviceStateFlags.AttachedToDesktop))
                        .Select(CreateHeliosMonitor)
                        // filter failed monitors
                        .Where(monitor => monitor != null);

                    displayCollection.AddRange(monitors
                        // sort to make consistent monitor list as long as same monitors are present
                        .OrderBy(monitor => monitor.Left)
                        .ThenBy(monitor => monitor.Top)
                    );
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Exception thrown enumerating display devices.");
                }

                return displayCollection;
            }
        }

        /// <summary>
        /// create monitors from stored simulated monitors, making copies because monitor objects will be changed by profile configuration
        /// </summary>
        private IEnumerable<Monitor> CreateSimulatedMonitors => _simulatedMonitors?.Select(monitor => new Monitor(monitor));

        private NativeMethods.DISPLAY_DEVICE LogDisplayDevice(NativeMethods.DISPLAY_DEVICE displayDevice)
        {
            Logger.Debug("Windows reporting display {Name} with id {Id} and key {Key} with string {String}",
                displayDevice.DeviceName,
                displayDevice.DeviceID,
                displayDevice.DeviceKey,
                displayDevice.DeviceString);
            return displayDevice;
        }

        private IEnumerable<NativeMethods.DISPLAY_DEVICE> EnumerateDisplays()
        {
            NativeMethods.DISPLAY_DEVICE displayDevice = new NativeMethods.DISPLAY_DEVICE();
            displayDevice.cb = Marshal.SizeOf(displayDevice);
            for (uint id = 0; NativeMethods.EnumDisplayDevices(null, id, ref displayDevice, 0); id++)
            {
                yield return displayDevice;
            }
        }

        private Monitor CreateHeliosMonitor(NativeMethods.DISPLAY_DEVICE displayDevice)
        {
            NativeMethods.DEVMODE deviceMode = new NativeMethods.DEVMODE();

            bool suc2 = NativeMethods.EnumDisplaySettings(displayDevice.DeviceName, NativeMethods.ENUM_CURRENT_SETTINGS,
                ref deviceMode);
            if (!suc2)
            {
                Logger.Error("failed to enumerate current settings for display {InternalName}", displayDevice.DeviceName);
                return null;
            }

            Monitor heliosMonitor = new Monitor(ConvertPixels(deviceMode.dmPositionX),
                ConvertPixels(deviceMode.dmPositionY),
                ConvertPixels(deviceMode.dmPelsWidth),
                ConvertPixels(deviceMode.dmPelsHeight),
                deviceMode.dmDisplayOrientation);

            Logger.Debug("found display {Name} of size {Width}x{Height} at {Left},{Top}",
                displayDevice.DeviceName,
                heliosMonitor.Width,
                heliosMonitor.Height,
                heliosMonitor.Left,
                heliosMonitor.Top);

            // check this assumption that we make throughout the code: we identify the main display by its coordinates being
            // 0,0 at the top left
            Debug.Assert(displayDevice.StateFlags.HasFlag(NativeMethods.DisplayDeviceStateFlags.PrimaryDevice) ==
                         heliosMonitor.IsPrimaryDisplay);
            return heliosMonitor;
        }

        private IEnumerable<Monitor> EnumerateMonitors(NativeMethods.DISPLAY_DEVICE displayDevice)
        {
            for (uint id = 0; NativeMethods.EnumDisplayDevices(null, id, ref displayDevice, 0); id++)
            {
                if (!displayDevice.StateFlags.HasFlag(NativeMethods.DisplayDeviceStateFlags.AttachedToDesktop))
                {
                    continue;
                }

                NativeMethods.DEVMODE ds = new NativeMethods.DEVMODE();

                bool suc2 = NativeMethods.EnumDisplaySettings(displayDevice.DeviceName, NativeMethods.ENUM_CURRENT_SETTINGS,
                    ref ds);
                if (!suc2)
                {
                    Logger.Error("failed to enumerate current settings for display {InternalName}", displayDevice.DeviceName);
                    continue;
                }

                Monitor heliosMonitor = new Monitor(ConvertPixels(ds.dmPositionX),
                    ConvertPixels(ds.dmPositionY),
                    ConvertPixels(ds.dmPelsWidth),
                    ConvertPixels(ds.dmPelsHeight),
                    ds.dmDisplayOrientation);

                // check this assumption that we make throughout the code: we identify the main display by its coordinates being
                // 0,0 at the top left
                Debug.Assert(displayDevice.StateFlags.HasFlag(NativeMethods.DisplayDeviceStateFlags.PrimaryDevice) ==
                             heliosMonitor.IsPrimaryDisplay);

                yield return heliosMonitor;
            }
        }
        #endregion

    }
}
