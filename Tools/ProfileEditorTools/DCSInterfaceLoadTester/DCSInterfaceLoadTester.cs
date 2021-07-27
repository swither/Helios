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

using GadrocsWorkshop.Helios.ComponentModel;
using GadrocsWorkshop.Helios.Tools.Capabilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Threading;
using GadrocsWorkshop.Helios.Interfaces.DCS.Common;
using GadrocsWorkshop.Helios.Tools;
using GadrocsWorkshop.Helios.UDPInterface;

namespace GadrocsWorkshop.Helios.ProfileEditorTools.DCSInterfaceLoadTester
{
    [HeliosTool]
    public class DCSInterfaceLoadTester : ProfileTool, IMenuSectionFactory
    {
        // NOTE: access to C sprintf to emulate Lua format
        [DllImport("msvcrt.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern int _snwprintf_s([MarshalAs(UnmanagedType.LPWStr)] StringBuilder str, IntPtr bufferSize,
            IntPtr length, string format, int p);

        // NOTE: access to C sprintf to emulate Lua format
        [DllImport("msvcrt.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern int _snwprintf_s([MarshalAs(UnmanagedType.LPWStr)] StringBuilder str, IntPtr bufferSize,
            IntPtr length, string format, double p);

        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private List<TesterBase> _updateEveryFrame;
        private List<TesterBase> _updateSlowly;
        private DateTime _lastTime;
        private DispatcherTimer _timer;
        private HeliosBinding.IHeliosBindingTracer _previousTracer;

        public DCSInterface Target { get; private set; }

        private const double UPDATES_PER_SECOND = 30d;
        private const double SLOW_UPDATES_PER_SECOND = 5d;

        internal static string Format(string formatString, double value)
        {
            // handle Lua formatting cases that don't work with sprintf
            switch (formatString)
            {
                case "%1d":
                    return ((int) value).ToString(CultureInfo.InvariantCulture);
            }

            // let sprintf do everything else
            StringBuilder str = new StringBuilder(100);
            _snwprintf_s(str, (IntPtr) 100, (IntPtr) 32, formatString, value);
            return str.ToString();
        }

        internal static string Format(string formatString, int value)
        {
            StringBuilder str = new StringBuilder(100);
            _snwprintf_s(str, (IntPtr) 100, (IntPtr) 32, formatString, value);
            return str.ToString();
        }

        private void Tick(object sender, EventArgs e)
        {
            DateTime now = DateTime.Now;
            TimeSpan elapsed = now.Subtract(_lastTime);
            _updateEveryFrame.ForEach(t => Update(t, now, elapsed));
            if (elapsed > TimeSpan.FromMilliseconds(1d / SLOW_UPDATES_PER_SECOND))
            {
                _updateSlowly.ForEach(t => Update(t, now, elapsed));
            }

            _lastTime = now;
        }

        private void Update(TesterBase tester, DateTime now, TimeSpan elapsed)
        {
            string testValue = tester.Update(now, elapsed);
            if (testValue == null)
            {
                // nothing to do right now
                return;
            }

            if (!Target.DispatchReceived(tester.Data.ID, testValue))
            {
                Logger.Warn("failed to dispatch value {Value} synthesized for id {Id} in DCS interface being tested",
                    testValue, tester.Data.ID);
            }
        }

        public override void Close(HeliosProfile oldProfile)
        {
            Stop();
            base.Close(oldProfile);
        }

        private void Start()
        {
            _updateEveryFrame = new List<TesterBase>();
            _updateSlowly = new List<TesterBase>();

            if (Profile == null)
            {
                return;
            }

            Target = Profile.Interfaces.OfType<DCSInterface>().FirstOrDefault();
            if (Target == null)
            {
                return;
            }

            _timer = new DispatcherTimer(TimeSpan.FromSeconds(1d / UPDATES_PER_SECOND), DispatcherPriority.DataBind,
                Tick, System.Windows.Application.Current.Dispatcher);

            _previousTracer = HeliosBinding.BindingTracer;
            HeliosBinding.BindingTracer = new SoftLoopTracer();
            foreach (NetworkFunction networkFunction in Target.Functions)
            {
                foreach (DCSDataElement dataElement in networkFunction.DataElements.OfType<DCSDataElement>())
                {
                    TesterBase tester = CreateTester(networkFunction, dataElement);
                    if (dataElement.IsExportedEveryFrame)
                    {
                        _updateEveryFrame.Add(tester);
                    }
                    else
                    {
                        _updateSlowly.Add(tester);
                    }
                }
            }

            _lastTime = DateTime.Now;
            _timer.Start();
        }

        public override bool CanStart =>
            base.CanStart &&
            !IsStarted &&
            Profile.Interfaces.OfType<DCSInterface>().Any();

        private bool IsStarted => _timer?.IsEnabled ?? false;

        private void Stop()
        {
            if (!IsStarted)
            {
                return;
            }

            _timer.Stop();
            _updateSlowly.ForEach(t => t.Dispose());
            _updateSlowly.Clear();
            _updateEveryFrame.ForEach(t => t.Dispose());
            _updateEveryFrame.Clear();
            Target = null;

            Profile?.Reset();
            HeliosBinding.BindingTracer = _previousTracer;
            _previousTracer = null;
            _timer = null;
        }

        private static readonly Regex FloatFormat =
            new Regex(@"%0?\.([1-9][0-9]*)f", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private TesterBase CreateTester(NetworkFunction networkFunction, DCSDataElement dataElement)
        {
            // specific types we want to test a certain way
            switch (networkFunction)
            {
                case PushButton button:
                    _ = button;
                    return new BooleanTester(dataElement);
                case Switch switchFunction:
                    return new SwitchTester(dataElement, switchFunction);
            }

            // infer based on format
            switch (dataElement.Format)
            {
                case "%1d":
                    // only testing 0 and 1 even if this is a switch with multiple values
                    return new BooleanTester(dataElement);
                case null:
                    if (networkFunction is NetworkValue)
                    {
                        // at least simulate 0..1 for generic values
                        return new NumericTester(dataElement, 3);
                    }

                    // no tester for values created manually in script (yet)
                    return new UnsupportedTester(dataElement);
                default:
                    Match match = FloatFormat.Match(dataElement.Format);
                    if (match.Success && match.Groups[1].Success &&
                        int.TryParse(match.Groups[1].Value, out int precision))
                    {
                        return new NumericTester(dataElement, precision);
                    }

                    break;
            }

            return new UnsupportedTester(dataElement);
        }

        public MenuSectionModel CreateMenuSection()
        {
            return new MenuSectionModel("DCS Interface Test", new List<MenuItemModel>
            {
                new MenuItemModel("Start DCS Interface Test", new Windows.RelayCommand(parameter => { Start(); },
                    parameter => CanStart)),
                new MenuItemModel("Stop DCS Interface Test", new Windows.RelayCommand(parameter => { Stop(); },
                    parameter => IsStarted))
            });
        }
    }
}