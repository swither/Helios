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

namespace GadrocsWorkshop.Helios.Gauges.Falcon.ADIStandby
{
	using GadrocsWorkshop.Helios.ComponentModel;
	using GadrocsWorkshop.Helios.Interfaces.Falcon;
	using GadrocsWorkshop.Helios.Gauges.Falcon.ADIBallRenderer;
	using System;
	using System.Windows;
	using System.Windows.Media;

	[HeliosControl("Helios.Falcon.ADIStandby", "Falcon BMS Standby ADI", "Falcon Simulator", typeof(GaugeRenderer))]
	public class ADIStandby : BaseGauge
	{
		private FalconInterface _falconInterface;

		private GaugeImage _faceplate;
		private GaugeImage _backlightRing;
		private GaugeImage _offFlag;

		private ADIBallRenderer _ball;

		private const string _faceplateImage = "{HeliosFalcon}/Gauges/ADIStandby/adi_standby_faceplate.png";
		private const string _offFlagImage = "{HeliosFalcon}/Gauges/ADIStandby/adi_standby_off_flag.png";

		private const string _ringOffImage = "{HeliosFalcon}/Gauges/ADIStandby/adi_standby_ring_off.xaml";
		private const string _ringDimImage = "{HeliosFalcon}/Gauges/ADIStandby/adi_standby_ring_dim.xaml";
		private const string _ringBrtImage = "{HeliosFalcon}/Gauges/ADIStandby/adi_standby_ring_brt.xaml";

		private const string _ballOffImage = "{HeliosFalcon}/Gauges/ADIBall/adi_ball_standby_off.xaml";
		private const string _ballDimImage = "{HeliosFalcon}/Gauges/ADIBall/adi_ball_standby_dim.xaml";
		private const string _ballBrtImage = "{HeliosFalcon}/Gauges/ADIBall/adi_ball_standby_brt.xaml";

		private double _backlight;
		private bool _inFlightLastValue = true;
		private const double _ballDiameter = 192;
		private const double _ballVerticalCenter = 145;
		private const double _ballHorizontalCenter = 155;
		private const double _ballTapeScaleLength = 960;

		public ADIStandby()
			: base("ADIStandby", new Size(310, 300))
		{
			AddComponents();
			InitializeBallValues();
		}

		#region Components

		private void AddComponents()
		{
			_ball = new ADIBallRenderer();
			_ball.Clip = new EllipseGeometry(new Point(_ballHorizontalCenter, _ballVerticalCenter), _ballDiameter / 2, _ballDiameter / 2);
			Components.Add(_ball);

			_offFlag = new GaugeImage(_offFlagImage, new Rect(40d, 70d, 115d, 180d));
			_offFlag.IsHidden = true;
			Components.Add(_offFlag);

			_backlightRing = new GaugeImage(_ringOffImage, new Rect(0d, 0d, 310d, 300d));
			Components.Add(_backlightRing);

			_faceplate = new GaugeImage(_faceplateImage, new Rect(0d, 0d, 310d, 300d));
			Components.Add(_faceplate);
		}

		#endregion Components

		#region Methods

		private void InitializeBallValues()
		{
			_ball.BallImage = _ballOffImage;
			_ball.BallTapeScaleLength = _ballTapeScaleLength;

			_ball.BallDiameter = _ballDiameter;
			_ball.BallLeft = _ballHorizontalCenter - _ballDiameter / 2;
			_ball.BallTop = _ballVerticalCenter - _ballDiameter / 2; ;
		}

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
					ProcessStandbyADIValues();
					_inFlightLastValue = true;
				}
				else
				{
					if (_inFlightLastValue)
					{
						ResetStandbyADI();
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

			BindingValue pitch = GetValue("ADI", "pitch");
			PitchAngle = pitch.DoubleValue * 180 / Math.PI;

			BindingValue roll = GetValue("ADI", "roll");
			RollAngle = roll.DoubleValue * 180 / Math.PI;

			BindingValue flagOFF = GetValue("Backup ADI", "off flag");
			FlagOff = flagOFF.BoolValue;
		}

		private void ProcessStandbyADIValues()
		{
			_ball.BallVerticalValue = -PitchAngle;
			_ball.BallRotationAngle = -RollAngle;

			_offFlag.IsHidden = !FlagOff;
		}

		private void ProcessBacklightValues()
		{
			if (Backlight == 1)
			{
				_backlightRing.Image = _ringDimImage;
				_ball.BallImage = _ballDimImage;
			}
			else if (Backlight == 2)
			{
				_backlightRing.Image = _ringBrtImage;
				_ball.BallImage = _ballBrtImage;
			}
			else
			{
				_backlightRing.Image = _ringOffImage;
				_ball.BallImage = _ballOffImage;
			}

			Refresh();
		}

		public override void Reset()
		{
			ResetStandbyADI();
		}

		private void ResetStandbyADI()
		{
			Backlight = 0d;

			PitchAngle = 0d;
			RollAngle = 0d;

			FlagOff = false;

			ProcessStandbyADIValues();
			ProcessBacklightValues();
		}

		private BindingValue GetValue(string device, string name)
		{
			return _falconInterface?.GetValue(device, name) ?? BindingValue.Empty;
		}

		#endregion Methods

		#region Properties

		private double PitchAngle { get; set; }
		private double RollAngle { get; set; }

		private bool FlagOff { get; set; }

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
