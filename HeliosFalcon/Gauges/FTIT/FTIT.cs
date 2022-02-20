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

namespace GadrocsWorkshop.Helios.Gauges.Falcon.FTIT
{
    using GadrocsWorkshop.Helios.ComponentModel;
    using GadrocsWorkshop.Helios.Interfaces.Falcon;
    using System;
    using System.Windows;

    [HeliosControl("Helios.Falcon.FTIT", "FTIT", "Falcon Simulator", typeof(GaugeRenderer))]
    public class FTIT : BaseGauge
    {
        private FalconInterface _falconInterface;
        private CalibrationPointCollectionDouble _needleCalibration;
        private GaugeImage _faceplateOff;
        private GaugeImage _faceplateDim;
        private GaugeImage _faceplateBrt;
        private GaugeNeedle _needleOff;
        private GaugeNeedle _needleDim;
        private GaugeNeedle _needleBrt;

        private const string _faceplateOffImage = "{HeliosFalcon}/Gauges/FTIT/ftit_faceplate_off.xaml";
        private const string _faceplateDimImage = "{HeliosFalcon}/Gauges/FTIT/ftit_faceplate_dim.xaml";
        private const string _faceplateBrtImage = "{HeliosFalcon}/Gauges/FTIT/ftit_faceplate_brt.xaml";
        private const string _needleOffImage = "{HeliosFalcon}/Gauges/FTIT/ftit_needle_off.xaml";
        private const string _needleDimImage = "{HeliosFalcon}/Gauges/FTIT/ftit_needle_dim.xaml";
        private const string _needleBrtImage = "{HeliosFalcon}/Gauges/FTIT/ftit_needle_brt.xaml";

        private bool _inFlightLastValue = true;

        public FTIT()
            : base("FTIT", new Size(360, 360))
        {
            AddComponents();
        }

        #region Components
        private void AddComponents()
        {
            _faceplateOff =new GaugeImage(_faceplateOffImage, new Rect(30d, 30d, 300d, 300d));
            _faceplateOff.IsHidden = false;
            Components.Add(_faceplateOff);

            _faceplateDim = new GaugeImage(_faceplateDimImage, new Rect(30d, 30d, 300d, 300d));
            _faceplateDim.IsHidden = true;
            Components.Add(_faceplateDim);

            _faceplateBrt = new GaugeImage(_faceplateBrtImage, new Rect(30d, 30d, 300d, 300d));
            _faceplateBrt.IsHidden = true;
            Components.Add(_faceplateBrt);

            _needleCalibration = new CalibrationPointCollectionDouble(200d, 18d, 1200d, 342d);
            _needleCalibration.Add(new CalibrationPointDouble(700d, 108d));
            _needleCalibration.Add(new CalibrationPointDouble(1000d, 306d));

            _needleOff = new GaugeNeedle(_needleOffImage, new Point(180d, 180d), new Size(60d, 144d), new Point(30d, 114d), 90d);
            _needleOff.Rotation = _needleCalibration.Interpolate(0);
            _needleOff.IsHidden = false;
            Components.Add(_needleOff);

            _needleDim = new GaugeNeedle(_needleDimImage, new Point(180d, 180d), new Size(60d, 144d), new Point(30d, 114d), 90d);
            _needleDim.Rotation = _needleCalibration.Interpolate(0);
            _needleDim.IsHidden = true;
            Components.Add(_needleDim);

            _needleBrt = new GaugeNeedle(_needleBrtImage, new Point(180d, 180d), new Size(60d, 144d), new Point(30d, 114d), 90d);
            _needleBrt.Rotation = _needleCalibration.Interpolate(0);
            _needleBrt.IsHidden = true;
            Components.Add(_needleBrt);

            #endregion Components
        }

        #region Methods

        protected override void OnProfileChanged(HeliosProfile oldProfile)
        {
            base.OnProfileChanged(oldProfile);

            if (oldProfile != null)
            {
                oldProfile.ProfileStarted -= new EventHandler(Profile_ProfileStarted);
                oldProfile.ProfileTick -= new EventHandler(Profile_ProfileTick);
                oldProfile.ProfileStopped -= new EventHandler(Profile_ProfileStopped);
            }

            if (Profile != null)
            {
                Profile.ProfileStarted += new EventHandler(Profile_ProfileStarted);
                Profile.ProfileTick += new EventHandler(Profile_ProfileTick);
                Profile.ProfileStopped += new EventHandler(Profile_ProfileStopped);
            }
        }

        private void Profile_ProfileStarted(object sender, EventArgs e)
        {
            if (Parent.Profile.Interfaces.ContainsKey("Falcon"))
            {
                _falconInterface = Parent.Profile.Interfaces["Falcon"] as FalconInterface;
            }
        }

        private void Profile_ProfileTick(object sender, EventArgs e)
        {
            if (_falconInterface != null)
            {
                BindingValue runtimeFlying = GetValue("Runtime", "Flying");
                bool inFlight = runtimeFlying.BoolValue;

                if (inFlight)
                {
                    ProcessBindingValues();
                    ProcessFTITValues();
                    ProcessBacklightValues();
                    _inFlightLastValue = true;
                }
                else
                {
                    if (_inFlightLastValue)
                    {
                        ResetFTIT();
                        _inFlightLastValue = false;
                    }
                }
            }
        }

        private void ResetFTIT()
        {
            Backlight = 0d;
            Temp = 0d;

            ProcessFTITValues();
            ProcessBacklightValues();
        }

        private void ProcessBacklightValues()
        {
            bool is_hidden_off = Backlight != 0;
            bool is_hidden_dim = Backlight != 1;
            bool is_hidden_brt = Backlight != 2;

            _needleOff.IsHidden = is_hidden_off;
            _needleBrt.IsHidden = is_hidden_brt;
            _needleDim.IsHidden = is_hidden_dim;
            _faceplateBrt.IsHidden = is_hidden_brt;
            _faceplateDim.IsHidden = is_hidden_dim;
            _faceplateOff.IsHidden = is_hidden_off;

        }

        private void ProcessFTITValues()
        {

            double rotation = _needleCalibration.Interpolate(Temp);

            _needleOff.Rotation = rotation;
            _needleDim.Rotation = rotation;
            _needleBrt.Rotation = rotation;
        }

        private void ProcessBindingValues()
        {
            BindingValue backlight = GetValue("Lighting", "instrument backlight");
            Backlight = backlight.DoubleValue;
            
            BindingValue temp = GetValue("Engine", "ftit");
            Temp = temp.DoubleValue;
        }

        private void Profile_ProfileStopped(object sender, EventArgs e)
        {
            _falconInterface = null;
        }

        private BindingValue GetValue(string device, string name)
        {
            return _falconInterface?.GetValue(device, name) ?? BindingValue.Empty;
        }

        #endregion Methods

        #region Properties

        private double Backlight { get; set; }
        private double Temp { get; set; }

        #endregion Properties
    }
}
