﻿//  Copyright 2014 Craig Courtney
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

namespace GadrocsWorkshop.Helios.Gauges.Falcon.RPM
{
	using GadrocsWorkshop.Helios.ComponentModel;
	using GadrocsWorkshop.Helios.Interfaces.Falcon;
	using System;
	using System.Windows;

	[HeliosControl("Helios.Falcon.RPM", "Falcon BMS RPM", "Falcon BMS F-16", typeof(GaugeRenderer))]
	public class RPM : BaseGauge
	{
		private FalconInterface _falconInterface;
		private CalibrationPointCollectionDouble _needleCalibrationGE;
		private CalibrationPointCollectionDouble _needleCalibrationPW;
		private GaugeImage _backplate;
		private GaugeImage _faceplate;
		private GaugeNeedle _needle;
		private HeliosValue _setGaugeType;

		private const string _backplateImage = "{HeliosFalcon}/Gauges/F-16/Common/gauge_backplate.xaml";
		private const string _faceplateGE_OffImage = "{HeliosFalcon}/Gauges/F-16/RPM/rpm_faceplate_ge_off.xaml";
		private const string _faceplateGE_DimImage = "{HeliosFalcon}/Gauges/F-16/RPM/rpm_faceplate_ge_dim.xaml";
		private const string _faceplateGE_BrtImage = "{HeliosFalcon}/Gauges/F-16/RPM/rpm_faceplate_ge_brt.xaml";
		private const string _faceplatePW_OffImage = "{HeliosFalcon}/Gauges/F-16/RPM/rpm_faceplate_pw_off.xaml";
		private const string _faceplatePW_DimImage = "{HeliosFalcon}/Gauges/F-16/RPM/rpm_faceplate_pw_dim.xaml";
		private const string _faceplatePW_BrtImage = "{HeliosFalcon}/Gauges/F-16/RPM/rpm_faceplate_pw_brt.xaml";
		private const string _needleOffImage = "{HeliosFalcon}/Gauges/F-16/Common/needle_long_off.xaml";
		private const string _needleDimImage = "{HeliosFalcon}/Gauges/F-16/Common/needle_long_dim.xaml";
		private const string _needleBrtImage = "{HeliosFalcon}/Gauges/F-16/Common/needle_long_brt.xaml";

		private int _gaugeType = 0;
		private double _backlight;
		private double _gaugeTypeSetting = 2d;
		private string _aircraftName = "";
		private bool _inFlightLastValue = true;

		public RPM()
			: base("RPM", new Size(300, 300))
		{
			AddComponents();
		}

		#region Components

		private void AddComponents()
		{
			_backplate = new GaugeImage(_backplateImage, new Rect(0d, 0d, 300d, 300d));
			 Components.Add(_backplate);

			_faceplate =new GaugeImage(_faceplateGE_OffImage, new Rect(0d, 0d, 300d, 300d));
			Components.Add(_faceplate);

			_needleCalibrationGE = new CalibrationPointCollectionDouble(0d, 0d, 110d, 326d);
			_needleCalibrationGE.Add(new CalibrationPointDouble(30d, 54d));
			_needleCalibrationGE.Add(new CalibrationPointDouble(60d, 115.5d));
			_needleCalibrationGE.Add(new CalibrationPointDouble(70d, 143d));
			_needleCalibrationGE.Add(new CalibrationPointDouble(90d, 230d));

			_needleCalibrationPW = new CalibrationPointCollectionDouble(0d, 0d, 100d, 324d);
			_needleCalibrationPW.Add(new CalibrationPointDouble(60d, 124d));
			_needleCalibrationPW.Add(new CalibrationPointDouble(70d, 172d));

			_needle = new GaugeNeedle(_needleOffImage, new Point(150d, 150d), new Size(50d, 200d), new Point(25d, 150d), 355d);
			_needle.Rotation = _needleCalibrationGE.Interpolate(0);
			Components.Add(_needle);

			_setGaugeType = new HeliosValue(this, new BindingValue(0d), "", "rpm gauge type", "Sets the type of RPM gauge.", "0 = GE, 1 = PW, 2 = Auto (Default Value)", BindingValueUnits.Numeric);
			_setGaugeType.SetValue(new BindingValue(0), true);
			_setGaugeType.Execute += new HeliosActionHandler(SetGaugeType_Execute);
			Actions.Add(_setGaugeType);
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
					ProcessRPMValues();
					_inFlightLastValue = true;
				}
				else
				{
					if (_inFlightLastValue)
					{
						ResetRPM();
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
			BindingValue aircraftName = GetValue("Runtime", "Aircraft Name");
			AircraftName = aircraftName.StringValue;

			BindingValue backlight = GetValue("Lighting", "instrument backlight");
			Backlight = backlight.DoubleValue;

			BindingValue rpmPercent = GetValue("Engine", "rpm");
			RPMPercent = rpmPercent.DoubleValue;
		}

		private void SetGaugeType_Execute(object action, HeliosActionEventArgs e)
		{
			GaugeTypeSetting = e.Value.DoubleValue;
		}

		private void SetGaugeType()
		{
			if (GaugeTypeSetting == 0d)
			{
				GaugeType = 0;
			}
			else if (GaugeTypeSetting == 1d)
			{
				GaugeType = 1;
			}
			else
			{
				string AN6 = AircraftName.Substring(0, Math.Min(AircraftName.Length, 6));
				string AN8 = AircraftName.Substring(0, Math.Min(AircraftName.Length, 8));
				string AN9 = AircraftName.Substring(0, Math.Min(AircraftName.Length, 9));

				if (AN8 == "F-16C-30" || AN8 == "F-16C-40" || AN8 == "F-16C-50" || AN9 == "F-16CM-40" ||
					AN9 == "F-16CM-50" || AN9 == "F-16DM-40" || AN9 == "F-16DG-30" || AN9 == "F-16DG-40")
				{
					GaugeType = 0;
				}
				else if (AN6 == "F-16AM" || AN8 == "F-16A-15" || AN8 == "F-16B-15" || AN8 == "F-16C-25" || AN8 == "F-16C-32" ||
                         AN8 == "F-16C-52" || AN8 == "F-16D-52" || AN8 == "F-16I-52" || AN9 == "F-16CM-42" || AN9 == "F-16CM-52" || AN9 == "F-16DM-52")
				{
					GaugeType = 1;
				}
				else
				{
					GaugeType = 0;
				}
			}
		}

		private void ProcessRPMValues()
		{
			if (GaugeType == 0)
			{
				_needle.Rotation = _needleCalibrationGE.Interpolate(RPMPercent);
			}

			if (GaugeType == 1)
			{
				_needle.Rotation = _needleCalibrationPW.Interpolate(RPMPercent);
			}
		}

		private void ProcessGaugeValues()
		{
			if (Backlight == 1)
			{
				 _needle.Image = _needleDimImage;
			}
			else if (Backlight == 2)
			{
				 _needle.Image = _needleBrtImage;
			}
			else
			{
				 _needle.Image = _needleOffImage;
			}

			if (GaugeType == 0)
			{
				if (Backlight == 1)
				{
					_faceplate.Image = _faceplateGE_DimImage;
				}
				else if (Backlight == 2)
				{
					_faceplate.Image = _faceplateGE_BrtImage;
				 }
				else
				{
					_faceplate.Image = _faceplateGE_OffImage;
				}

				_needle.BaseRotation = 355d;
			}

			if (GaugeType == 1)
			{
				if (Backlight == 1)
				{
					_faceplate.Image = _faceplatePW_DimImage;
				}
				else if (Backlight == 2)
				{
					_faceplate.Image = _faceplatePW_BrtImage;
				}
				else
				{
					_faceplate.Image = _faceplatePW_OffImage;
				}

				_needle.BaseRotation = 0d;
			}

			Refresh();
		}

		public override void Reset()
		{
			ResetRPM();
		}

		private void ResetRPM()
		{
			_gaugeType = 0;
			_backlight = 0d;
			_gaugeTypeSetting = 2d;
			_aircraftName = "";
			RPMPercent = 0d;

			ProcessRPMValues();
			SetGaugeType();
			ProcessGaugeValues();
		}

		private BindingValue GetValue(string device, string name)
		{
			return _falconInterface?.GetValue(device, name) ?? BindingValue.Empty;
		}

		#endregion Methods

		#region Properties

		private double RPMPercent { get; set; }

		private string AircraftName
		{
			get => _aircraftName;
			set
			{
				string oldValue = _aircraftName;
				_aircraftName = value;
				if (!_aircraftName.Equals(oldValue))
				{
					SetGaugeType();
				}
			}
		}

		private double Backlight
		{
			get => _backlight;
			set
			{
				double oldValue = _backlight;
				_backlight = value;
				if (!_backlight.Equals(oldValue))
				{
					ProcessGaugeValues();
				}
			}
		}

		private double GaugeTypeSetting
		{
			get => _gaugeTypeSetting;
			set
			{
				double oldValue = _gaugeTypeSetting;
				_gaugeTypeSetting = value;
				if (!_gaugeTypeSetting.Equals(oldValue))
				{
					SetGaugeType();
				}
			}
		}

		private int GaugeType
		{
			get => _gaugeType;
			set
			{
				int oldValue = _gaugeType;
				_gaugeType = value;
				if (!_gaugeType.Equals(oldValue))
				{
					ProcessGaugeValues();
				}
			}
		}

		#endregion Properties
	}
}
