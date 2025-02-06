﻿//  Copyright 2014 Craig Courtney
//  Copyright 2022 Helios Contributors
//  Copyright 2024 Helios Contributors
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

namespace GadrocsWorkshop.Helios.Gauges.Falcon.HydraulicPressure
{
	using GadrocsWorkshop.Helios.ComponentModel;
	using GadrocsWorkshop.Helios.Interfaces.Falcon;
	using System;
	using System.Windows;

	public abstract class BaseHydraulicPressure : BaseGauge
	{
		private FalconInterface _falconInterface;
		private GaugeNeedle _needle;
		private GaugeImage _backplate;
		private GaugeImage _faceplate;
		private string _inputBinding;

		private CalibrationPointCollectionDouble _needleCalibration;

		private const string _backplateImage = "{HeliosFalcon}/Gauges/F-16/Common/gauge_backplate.xaml";
		private const string _faceplateOffImage = "{HeliosFalcon}/Gauges/F-16/HydraulicPressure/hydraulic_faceplate_off.xaml";
		private const string _faceplateDimImage = "{HeliosFalcon}/Gauges/F-16/HydraulicPressure/hydraulic_faceplate_dim.xaml";
		private const string _faceplateBrtImage = "{HeliosFalcon}/Gauges/F-16/HydraulicPressure/hydraulic_faceplate_brt.xaml";
		private const string _needleOffImage = "{HeliosFalcon}/Gauges/F-16/Common/needle_short_off.xaml";
		private const string _needleDimImage = "{HeliosFalcon}/Gauges/F-16/Common/needle_short_dim.xaml";
		private const string _needleBrtImage = "{HeliosFalcon}/Gauges/F-16/Common/needle_short_brt.xaml";

		private double _backlight;
		private bool _inFlightLastValue = true;

		public BaseHydraulicPressure(string name, Size size)
			: base(name, size)
		{
			AddComponents();
		}

		#region Components

		private void AddComponents()
		{
			_backplate = new GaugeImage(_backplateImage, new Rect(0d, 0d, 300d, 300d));
			Components.Add(_backplate);

			_faceplate = new GaugeImage(_faceplateOffImage, new Rect(0d, 0d, 300d, 300d));
			Components.Add(_faceplate);

			_needleCalibration = new CalibrationPointCollectionDouble(0d, 0d, 4000d, 320d);

			_needle = new GaugeNeedle(_needleOffImage, new Point(150d, 150d), new Size(60d, 144d), new Point(30d, 114d), 110d)
			{
				Rotation = _needleCalibration.Interpolate(0)
			};
			Components.Add(_needle);
		}

		#endregion Components

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
					ProcessHydraulicPressureValues();
					_inFlightLastValue = true;
				}
				else
				{
					if (_inFlightLastValue)
					{
						ResetHydraulicPressure();
						_inFlightLastValue = false;
					}
				}
			}
		}

		private void Profile_ProfileStopped(object sender, EventArgs e)
		{
			_falconInterface = null;
		}

		private void ProcessBindingValues()
		{
			BindingValue backlight = GetValue("Lighting", "instrument backlight");
			Backlight = backlight.DoubleValue;

			BindingValue hydraulicpressure = GetValue("Hydraulic", _inputBinding);
			Pressure = hydraulicpressure.DoubleValue;
		}

		private void ProcessHydraulicPressureValues()
		{
			_needle.Rotation = _needleCalibration.Interpolate(Pressure);
		}

		private void ProcessBacklightValues()
		{
			if (Backlight == 1)
			{
				_faceplate.Image = _faceplateDimImage;
				_needle.Image = _needleDimImage;
			}
			else if (Backlight == 2)
			{
				_faceplate.Image = _faceplateBrtImage;
				_needle.Image = _needleBrtImage;
			}
			else
			{
				_faceplate.Image = _faceplateOffImage;
				_needle.Image = _needleOffImage;
			}

			Refresh();
		}

		public override void Reset()
		{
			ResetHydraulicPressure();
		}

		private void ResetHydraulicPressure()
		{
			Backlight = 0d;
			Pressure = 0d;

			ProcessHydraulicPressureValues();
			ProcessBacklightValues();
		}

		private BindingValue GetValue(string device, string name)
		{
			return _falconInterface?.GetValue(device, name) ?? BindingValue.Empty;
		}

		#endregion Methods

		#region Properties

		protected string InputBinding
        {
			set
            {
				_inputBinding = value;
            }
        }

		private double Pressure { get; set; }

		private double Backlight
		{
			get
			{
				return _backlight;
			}
			set
			{
				double oldValue = _backlight;
				_backlight = value;
				if (!_backlight.Equals(oldValue))
				{
					ProcessBacklightValues();
				}
			}
		}

		#endregion Properties
	}
}
