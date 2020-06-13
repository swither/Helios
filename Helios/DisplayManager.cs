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

using System.Diagnostics;

namespace GadrocsWorkshop.Helios
{
    using System;
    using System.Runtime.InteropServices;

    public class DisplayManager
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private int _dpi;

        #region Properties

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
                Logger.Debug($"Helios has determined screen DPI is {_dpi}");
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

                NativeMethods.DISPLAY_DEVICE d=new NativeMethods.DISPLAY_DEVICE();
                d.cb=Marshal.SizeOf(d);
                try
                {
                    for (uint id = 0; NativeMethods.EnumDisplayDevices(null, id, ref d, 0); id++)
                    {
                        if (!d.StateFlags.HasFlag(NativeMethods.DisplayDeviceStateFlags.AttachedToDesktop))
                        {
                            continue;
                        }

                        NativeMethods.DEVMODE ds = new NativeMethods.DEVMODE();

                        bool suc2 = NativeMethods.EnumDisplaySettings(d.DeviceName, NativeMethods.ENUM_CURRENT_SETTINGS, ref ds);
                        if (!suc2)
                        {
                            Logger.Error("failed to enumerate current settings for display {InternalName}", d.DeviceName);
                            continue;
                        }
                        Monitor di = new Monitor(ConvertPixels(ds.dmPositionX),
                            ConvertPixels(ds.dmPositionY),
                            ConvertPixels(ds.dmPelsWidth),
                            ConvertPixels(ds.dmPelsHeight),
                            ds.dmDisplayOrientation);
                        displayCollection.Add(di);

                        // check this assumption that we make throughout the code: we identify the main display by its coordinates being
                        // 0,0 at the top left
                        Debug.Assert(d.StateFlags.HasFlag(NativeMethods.DisplayDeviceStateFlags.PrimaryDevice) == di.IsPrimaryDisplay);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Exception thrown enumerating display devices.");
                }

                return displayCollection;
            }
        }

        #endregion

    }
}
