//  Copyright 2014 Craig Courtney
//  Copyright 2022 Helios Contributors
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

namespace GadrocsWorkshop.Helios.Gauges.Falcon.Altimeter
{
	using GadrocsWorkshop.Helios.ComponentModel;
	using GadrocsWorkshop.Helios.Interfaces.Falcon;
	using System;
	using System.Windows;
	using System.Windows.Media;

	[HeliosControl("Helios.Falcon.Altimeter", "Falcon BMS Altimeter", "Falcon Simulator", typeof(GaugeRenderer))]
	public class Altimeter : BaseGauge
	{
		private FalconInterface _falconInterface;

		private CalibrationPointCollectionDouble _needleCalibration;
		private GaugeImage _backplate;
		private GaugeDrumCounter _tensDrumOff;
		private GaugeDrumCounter _tensDrumDim;
		private GaugeDrumCounter _tensDrumBrt;
		private GaugeDrumCounter _mainDrumOff;
		private GaugeDrumCounter _mainDrumDim;
		private GaugeDrumCounter _mainDrumBrt;
		private GaugeDrumCounter _pressureDrumOff;
		private GaugeDrumCounter _pressureDrumDim;
		private GaugeDrumCounter _pressureDrumBrt;
		private GaugeImage _faceplateOff;
		private GaugeImage _faceplateDim;
		private GaugeImage _faceplateBrt;
		private GaugeNeedle _needleOff;
		private GaugeNeedle _needleDim;
		private GaugeNeedle _needleBrt;

		private const string _backplateImage = "{HeliosFalcon}/Gauges/Altimeter/altimeter_backplate.xaml";
		private const string _tensDrumOffImage = "{HeliosFalcon}/Gauges/Altimeter/alt_drum_tape_off.xaml";
		private const string _tensDrumDimImage = "{HeliosFalcon}/Gauges/Altimeter/alt_drum_tape_dim.xaml";
		private const string _tensDrumBrtImage = "{HeliosFalcon}/Gauges/Altimeter/alt_drum_tape_brt.xaml";
		private const string _mainDrumOffImage = "{HeliosFalcon}/Gauges/Common/drum_tape_off.xaml";
		private const string _mainDrumDimImage = "{HeliosFalcon}/Gauges/Common/drum_tape_dim.xaml";
		private const string _mainDrumBrtImage = "{HeliosFalcon}/Gauges/Common/drum_tape_brt.xaml";
		private const string _pressureDrumOffImage = "{HeliosFalcon}/Gauges/Common/drum_tape_off.xaml";
		private const string _pressureDrumDimImage = "{HeliosFalcon}/Gauges/Common/drum_tape_dim.xaml";
		private const string _pressureDrumBrtImage = "{HeliosFalcon}/Gauges/Common/drum_tape_brt.xaml";
		private const string _faceplateOffImage = "{HeliosFalcon}/Gauges/Altimeter/altimeter_faceplate_off.xaml";
		private const string _faceplateDimImage = "{HeliosFalcon}/Gauges/Altimeter/altimeter_faceplate_dim.xaml";
		private const string _faceplateBrtImage = "{HeliosFalcon}/Gauges/Altimeter/altimeter_faceplate_brt.xaml";
		private const string _needleOffImage = "{HeliosFalcon}/Gauges/Altimeter/altimeter_needle_off.xaml";
		private const string _needleDimImage = "{HeliosFalcon}/Gauges/Altimeter/altimeter_needle_dim.xaml";
		private const string _needleBrtImage = "{HeliosFalcon}/Gauges/Altimeter/altimeter_needle_brt.xaml";

		private bool _inFlightLastValue = true;


		public Altimeter()
			: base("Altimeter", new Size(300, 300))
		{
			AddComponents();
		}


		#region Components

		void AddComponents()
		{
			_backplate = new GaugeImage(_backplateImage, new Rect(0d, 0d, 300d, 300d));
			_backplate.IsHidden = false;
			Components.Add(_backplate);

			_tensDrumOff = new GaugeDrumCounter(_tensDrumOffImage, new Point(39d, 126d), "##", new Size(10d, 15d), new Size(30d, 45d));
			_tensDrumOff.Clip = new RectangleGeometry(new Rect(39d, 106d, 30d, 81d));
			_tensDrumOff.IsHidden = false;
			Components.Add(_tensDrumOff);

			_tensDrumDim = new GaugeDrumCounter(_tensDrumDimImage, new Point(39d, 126d), "##", new Size(10d, 15d), new Size(30d, 45d));
			_tensDrumDim.Clip = new RectangleGeometry(new Rect(39d, 106d, 30d, 81d));
			_tensDrumDim.IsHidden = true;
			Components.Add(_tensDrumDim);

			_tensDrumBrt = new GaugeDrumCounter(_tensDrumBrtImage, new Point(39d, 126d), "##", new Size(10d, 15d), new Size(30d, 45d));
			_tensDrumBrt.Clip = new RectangleGeometry(new Rect(39d, 106d, 30d, 81d));
			_tensDrumBrt.IsHidden = true;
			Components.Add(_tensDrumBrt);

			_mainDrumOff = new GaugeDrumCounter(_mainDrumOffImage, new Point(69d, 126d), "#%00", new Size(10d, 15d), new Size(30d, 45d));
			_mainDrumOff.Clip = new RectangleGeometry(new Rect(69d, 106d, 150d, 81d));
			_mainDrumOff.IsHidden = false;
			Components.Add(_mainDrumOff);

			_mainDrumDim = new GaugeDrumCounter(_mainDrumDimImage, new Point(69d, 126d), "#%00", new Size(10d, 15d), new Size(30d, 45d));
			_mainDrumDim.Clip = new RectangleGeometry(new Rect(69d, 106d, 150d, 81d));
			_mainDrumDim.IsHidden = true;
			Components.Add(_mainDrumDim);

			_mainDrumBrt = new GaugeDrumCounter(_mainDrumBrtImage, new Point(69d, 126d), "#%00", new Size(10d, 15d), new Size(30d, 45d));
			_mainDrumBrt.Clip = new RectangleGeometry(new Rect(69d, 106d, 150d, 81d));
			_mainDrumBrt.IsHidden = true;
			Components.Add(_mainDrumBrt);

			_pressureDrumOff = new GaugeDrumCounter(_pressureDrumOffImage, new Point(182d, 195d), "###%", new Size(10d, 15d), new Size(15d, 20d));
			_pressureDrumOff.Clip = new RectangleGeometry(new Rect(182d, 195d, 60d, 20d));
			_pressureDrumOff.IsHidden = false;
			Components.Add(_pressureDrumOff);

			_pressureDrumDim = new GaugeDrumCounter(_pressureDrumDimImage, new Point(182d, 195d), "###%", new Size(10d, 15d), new Size(15d, 20d));
			_pressureDrumDim.Clip = new RectangleGeometry(new Rect(182d, 195d, 60d, 20d));
			_pressureDrumDim.IsHidden = true;
			Components.Add(_pressureDrumDim);

			_pressureDrumBrt = new GaugeDrumCounter(_pressureDrumBrtImage, new Point(182d, 195d), "###%", new Size(10d, 15d), new Size(15d, 20d));
			_pressureDrumBrt.Clip = new RectangleGeometry(new Rect(182d, 195d, 60d, 20d));
			_pressureDrumBrt.IsHidden = true;
			Components.Add(_pressureDrumBrt);

			_faceplateOff = new GaugeImage(_faceplateOffImage, new Rect(0d, 0d, 300d, 300d));
			_faceplateOff.IsHidden = false;
			Components.Add(_faceplateOff);

			_faceplateDim = new GaugeImage(_faceplateDimImage, new Rect(0d, 0d, 300d, 300d));
			_faceplateDim.IsHidden = true;
			Components.Add(_faceplateDim);

			_faceplateBrt = new GaugeImage(_faceplateBrtImage, new Rect(0d, 0d, 300d, 300d));
			_faceplateBrt.IsHidden = true;
			Components.Add(_faceplateBrt);

			_needleCalibration = new CalibrationPointCollectionDouble(-1000d, -360d, 1000d, 360d);

			_needleOff = new GaugeNeedle(_needleOffImage, new Point(150d, 150d), new Size(16d, 257d), new Point(8d, 138.5d));
			_needleOff.IsHidden = false;
			Components.Add(_needleOff);

			_needleDim = new GaugeNeedle(_needleDimImage, new Point(150d, 150d), new Size(16d, 257d), new Point(8d, 138.5d));
			_needleDim.IsHidden = true;
			Components.Add(_needleDim);

			_needleBrt = new GaugeNeedle(_needleBrtImage, new Point(150d, 150d), new Size(16d, 257d), new Point(8d, 138.5d));
			_needleBrt.IsHidden = true;
			Components.Add(_needleBrt);
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

		void Profile_ProfileStarted(object sender, EventArgs e)
		{
			if (Parent.Profile.Interfaces.ContainsKey("Falcon"))
			{
				_falconInterface = Parent.Profile.Interfaces["Falcon"] as FalconInterface;
			}
		}

		void Profile_ProfileTick(object sender, EventArgs e)
		{
			if (_falconInterface != null)
			{
				BindingValue runtimeFlying = GetValue("Runtime", "Flying");
				bool inFlight = runtimeFlying.BoolValue;

				if (inFlight)
				{
					ProcessBindingValues();
					ProcessAltimeterValues();
					ProcessBacklightValues();
					_inFlightLastValue = true;
				}
				else
				{
					if (_inFlightLastValue)
					{
						ResetAltimeter();
						_inFlightLastValue = false;
					}
				}
			}
		}

		void Profile_ProfileStopped(object sender, EventArgs e)
		{
			_falconInterface = null;
		}

		void ProcessBindingValues()
		{
			BindingValue backlight = GetValue("Lighting", "instrument backlight");
			Backlight = backlight.DoubleValue;

			BindingValue altitude = GetValue("Altimeter", "indicated altitude");
			Altitude = altitude.DoubleValue;

			BindingValue pressure = GetValue("Altimeter", "barimetric pressure");
			Pressure = pressure.DoubleValue > 0 ? pressure.DoubleValue : 2992d;
		}

		void ProcessAltimeterValues()
		{
			double rotation = _needleCalibration.Interpolate(Altitude % 1000d);
			double tensValue = (Altitude < 0 ? 99999 - Math.Abs(Altitude) : Altitude) / 1000d;
			double drumValue = Altitude < 0 ? 99999 - Math.Abs(Altitude) : Altitude;

			_needleOff.Rotation = rotation;
			_needleDim.Rotation = rotation;
			_needleBrt.Rotation = rotation;

			_tensDrumOff.Value = tensValue;
			_tensDrumDim.Value = tensValue;
			_tensDrumBrt.Value = tensValue;

			_mainDrumOff.Value = drumValue;
			_mainDrumDim.Value = drumValue;
			_mainDrumBrt.Value = drumValue;

			_pressureDrumOff.Value = Pressure;
			_pressureDrumDim.Value = Pressure;
			_pressureDrumBrt.Value = Pressure;
		}

		void ProcessBacklightValues()
		{
			bool is_hidden_off = Backlight != 0;
			bool is_hidden_dim = Backlight != 1;
			bool is_hidden_brt = Backlight != 2;

			_tensDrumOff.IsHidden = is_hidden_off;
			_mainDrumOff.IsHidden = is_hidden_off;
			_pressureDrumOff.IsHidden = is_hidden_off;
			_faceplateOff.IsHidden = is_hidden_off;
			_needleOff.IsHidden = is_hidden_off;

			_tensDrumDim.IsHidden = is_hidden_dim;
			_mainDrumDim.IsHidden = is_hidden_dim;
			_pressureDrumDim.IsHidden = is_hidden_dim;
			_faceplateDim.IsHidden = is_hidden_dim;
			_needleDim.IsHidden = is_hidden_dim;

			_tensDrumBrt.IsHidden = is_hidden_brt;
			_mainDrumBrt.IsHidden = is_hidden_brt;
			_pressureDrumBrt.IsHidden = is_hidden_brt;
			_faceplateBrt.IsHidden = is_hidden_brt;
			_needleBrt.IsHidden = is_hidden_brt;
		}

		public override void Reset()
		{
			ResetAltimeter();
		}

		void ResetAltimeter()
		{
			Backlight = 0d;
			Altitude = 0d;
			Pressure = 2992d;

			ProcessAltimeterValues();
			ProcessBacklightValues();
		}

		private BindingValue GetValue(string device, string name)
		{
			return _falconInterface?.GetValue(device, name) ?? BindingValue.Empty;
		}

		#endregion Methods


		#region Properties

		private double Backlight { get; set; }
		private double Altitude { get; set; }
		private double Pressure { get; set; }

		#endregion Properties

	}
}
