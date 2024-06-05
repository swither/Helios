//  Copyright 2014 Craig Courtney
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

namespace GadrocsWorkshop.Helios.Gauges.Falcon.MFD
{
	using GadrocsWorkshop.Helios.ComponentModel;
	using GadrocsWorkshop.Helios.Interfaces.Falcon;
	using System;
	using System.Windows;

	[HeliosControl("Helios.Falcon.MFD", "Falcon BMS MFD", "Falcon BMS F-16", typeof(GaugeRenderer))]
	public class MFD : BaseGauge
	{
		private FalconInterface _falconInterface;

		private GaugeImage _backplate;
		private GaugeImage _faceplate;

		private GaugeImage[] _buttonOSB = new GaugeImage[20];

		private GaugeImage _rockerGAIN;
		private GaugeImage _rockerSYM;
		private GaugeImage _rockerBRT;
		private GaugeImage _rockerCON;

		private HeliosTrigger[] _buttonOSBPushed = new HeliosTrigger[20];
		private HeliosTrigger[] _buttonOSBReleased = new HeliosTrigger[20];

		private HeliosTrigger _rockerGAINUpTrigger;
		private HeliosTrigger _rockerGAINDownTrigger;
		private HeliosTrigger _rockerSYMUpTrigger;
		private HeliosTrigger _rockerSYMDownTrigger;
		private HeliosTrigger _rockerBRTUpTrigger;
		private HeliosTrigger _rockerBRTDownTrigger;
		private HeliosTrigger _rockerCONUpTrigger;
		private HeliosTrigger _rockerCONDownTrigger;

		Rect[] _rectButtonOSB = new Rect[20];

		Rect _rectRockerGAINUp = new Rect(11, 23, 45, 35);
		Rect _rectRockerGAINCenter = new Rect(11, 23, 45, 75);
		Rect _rectRockerGAINDown = new Rect(11, 63, 45, 35);
		Rect _rectRockerSYMUp = new Rect(419, 23, 45, 35);
		Rect _rectRockerSYMCenter = new Rect(419, 23, 45, 75);
		Rect _rectRockerSYMDown = new Rect(419, 63, 45, 35);
		Rect _rectRockerBRTUp = new Rect(11, 375, 45, 35);
		Rect _rectRockerBRTCenter = new Rect(11, 375, 45, 75);
		Rect _rectRockerBRTDown = new Rect(11, 415, 45, 35);
		Rect _rectRockerCONUp = new Rect(419, 375, 45, 35);
		Rect _rectRockerCONCenter = new Rect(419, 375, 45, 75);
		Rect _rectRockerCONDown = new Rect(419, 415, 45, 35);

		private const string _backplateOffImage = "{HeliosFalcon}/Gauges/F-16/MFD/mfd_backplate_off.xaml";
		private const string _backplateDimImage = "{HeliosFalcon}/Gauges/F-16/MFD/mfd_backplate_dim.xaml";
		private const string _backplateBrtImage = "{HeliosFalcon}/Gauges/F-16/MFD/mfd_backplate_brt.xaml";
		private const string _faceplateImage = "{HeliosFalcon}/Gauges/F-16/MFD/mfd_faceplate.png";
		private const string _buttonOffImage = "{HeliosFalcon}/Gauges/F-16/MFD/mfd_button_off.png";
		private const string _buttonLitImage = "{HeliosFalcon}/Gauges/F-16/MFD/mfd_button_lit.png";
		private const string _buttonPressImage = "{HeliosFalcon}/Gauges/F-16/MFD/mfd_button_press.png";
		private const string _rockerGAINUpImage = "{HeliosFalcon}/Gauges/F-16/MFD/mfd_rocker_gain_up.png";
		private const string _rockerGAINCenterImage = "{HeliosFalcon}/Gauges/F-16/MFD/mfd_rocker_gain_center.png";
		private const string _rockerGAINDownImage = "{HeliosFalcon}/Gauges/F-16/MFD/mfd_rocker_gain_down.png";
		private const string _rockerSYMUpImage = "{HeliosFalcon}/Gauges/F-16/MFD/mfd_rocker_sym_up.png";
		private const string _rockerSYMCenterImage = "{HeliosFalcon}/Gauges/F-16/MFD/mfd_rocker_sym_center.png";
		private const string _rockerSYMDownImage = "{HeliosFalcon}/Gauges/F-16/MFD/mfd_rocker_sym_down.png";
		private const string _rockerBRTUpImage = "{HeliosFalcon}/Gauges/F-16/MFD/mfd_rocker_brt_up.png";
		private const string _rockerBRTCenterImage = "{HeliosFalcon}/Gauges/F-16/MFD/mfd_rocker_brt_center.png";
		private const string _rockerBRTDownImage = "{HeliosFalcon}/Gauges/F-16/MFD/mfd_rocker_brt_down.png";
		private const string _rockerCONUpImage = "{HeliosFalcon}/Gauges/F-16/MFD/mfd_rocker_con_up.png";
		private const string _rockerCONCenterImage = "{HeliosFalcon}/Gauges/F-16/MFD/mfd_rocker_con_center.png";
		private const string _rockerCONDownImage = "{HeliosFalcon}/Gauges/F-16/MFD/mfd_rocker_con_down.png";

		private static Rect _rectBase = new Rect(0d, 0d, 475d, 475d);
		private double _backlight;
		private bool _inFlightLastValue = true;

		public MFD()
			: base("MFD", new Size(_rectBase.Width, _rectBase.Height))
		{
			AddRectangles();
			AddComponents();
			AddActions();
		}

		#region Components

		private void AddRectangles()
        {
			_rectButtonOSB[0] = new Rect(100, 8, 51, 51);
			_rectButtonOSB[1] = new Rect(156, 8, 51, 51);
			_rectButtonOSB[2] = new Rect(212, 8, 51, 51);
			_rectButtonOSB[3] = new Rect(268, 8, 51, 51);
			_rectButtonOSB[4] = new Rect(324, 8, 51, 51);
			_rectButtonOSB[5] = new Rect(416, 100, 51, 51);
			_rectButtonOSB[6] = new Rect(416, 156, 51, 51);
			_rectButtonOSB[7] = new Rect(416, 212, 51, 51);
			_rectButtonOSB[8] = new Rect(416, 268, 51, 51);
			_rectButtonOSB[9] = new Rect(416, 324, 51, 51);
			_rectButtonOSB[10] = new Rect(324, 416, 51, 51);
			_rectButtonOSB[11] = new Rect(268, 416, 51, 51);
			_rectButtonOSB[12] = new Rect(212, 416, 51, 51);
			_rectButtonOSB[13] = new Rect(156, 416, 51, 51);
			_rectButtonOSB[14] = new Rect(100, 416, 51, 51);
			_rectButtonOSB[15] = new Rect(8, 324, 51, 51);
			_rectButtonOSB[16] = new Rect(8, 268, 51, 51);
			_rectButtonOSB[17] = new Rect(8, 212, 51, 51);
			_rectButtonOSB[18] = new Rect(8, 156, 51, 51);
			_rectButtonOSB[19] = new Rect(8, 100, 51, 51);
		}

		private void AddComponents()
		{
			_backplate = new GaugeImage(_backplateOffImage, _rectBase);
			Components.Add(_backplate);

			_faceplate = new GaugeImage(_faceplateImage, _rectBase);
			Components.Add(_faceplate);

			for (int i = 0; i < 20; i++)
			{
				_buttonOSB[i] = new GaugeImage(_buttonOffImage, _rectButtonOSB[i]);
				Components.Add(_buttonOSB[i]);
			}

			_rockerGAIN = new GaugeImage(_rockerGAINCenterImage, _rectRockerGAINCenter);
			Components.Add(_rockerGAIN);

			_rockerSYM = new GaugeImage(_rockerSYMCenterImage, _rectRockerSYMCenter);
			Components.Add(_rockerSYM);

			_rockerBRT = new GaugeImage(_rockerBRTCenterImage, _rectRockerBRTCenter);
			Components.Add(_rockerBRT);

			_rockerCON = new GaugeImage(_rockerCONCenterImage, _rectRockerCONCenter);
			Components.Add(_rockerCON);
		}

		#endregion Components

		#region Actions

		private void AddActions()
		{
			for (int i = 0; i < 20; i++)
            {
				_buttonOSBPushed[i] = new HeliosTrigger(this, "", "", "Button OSB-" + (i + 1) + " Pushed", "Fired when button OSB-" + (i + 1) + " is pushed.", "Always returns true.", BindingValueUnits.Boolean);
				Triggers.Add(_buttonOSBPushed[i]);

				_buttonOSBReleased[i] = new HeliosTrigger(this, "", "", "Button OSB-" + (i + 1) + " Released", "Fired when button OSB-" + (i + 1) + " is released.", "Always returns false.", BindingValueUnits.Boolean);
				Triggers.Add(_buttonOSBReleased[i]);
			}
			
			_rockerGAINUpTrigger = new HeliosTrigger(this, "", "", "Rocker GAIN-UP Pushed", "Fired when rocker GAIN-UP is pushed.", "Always returns true.", BindingValueUnits.Boolean);
			Triggers.Add(_rockerGAINUpTrigger);

			_rockerGAINDownTrigger = new HeliosTrigger(this, "", "", "Rocker GAIN-DOWN Pushed", "Fired when rocker GAIN-DOWN is pushed.", "Always returns true.", BindingValueUnits.Boolean);
			Triggers.Add(_rockerGAINDownTrigger);

			_rockerBRTUpTrigger = new HeliosTrigger(this, "", "", "Rocker BRT-UP Pushed", "Fired when rocker BRT-UP is pushed.", "Always returns true.", BindingValueUnits.Boolean);
			Triggers.Add(_rockerBRTUpTrigger);

			_rockerBRTDownTrigger = new HeliosTrigger(this, "", "", "Rocker BRT-DOWN Pushed", "Fired when rocker BRT-DOWN is pushed.", "Always returns true.", BindingValueUnits.Boolean);
			Triggers.Add(_rockerBRTDownTrigger);

			_rockerSYMUpTrigger = new HeliosTrigger(this, "", "", "Rocker SYM-UP Pushed", "Fired when rocker SYM-UP is pushed.", "Always returns true.", BindingValueUnits.Boolean);
			Triggers.Add(_rockerSYMUpTrigger);

			_rockerSYMDownTrigger = new HeliosTrigger(this, "", "", "Rocker SYM=DOWN Pushed", "Fired when rocker SYM=DOWN is pushed.", "Always returns true.", BindingValueUnits.Boolean);
			Triggers.Add(_rockerSYMDownTrigger);

			_rockerCONUpTrigger = new HeliosTrigger(this, "", "", "Rocker CON-UP Pushed", "Fired when rocker CON-UP is pushed.", "Always returns true.", BindingValueUnits.Boolean);
			Triggers.Add(_rockerCONUpTrigger);

			_rockerCONDownTrigger = new HeliosTrigger(this, "", "", "Rocker CON=DOWN Pushed", "Fired when rocker CON=DOWN is pushed.", "Always returns true.", BindingValueUnits.Boolean);
			Triggers.Add(_rockerCONDownTrigger);
		}

		#endregion Actions

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
					 _inFlightLastValue = true;
				}
				else
				{
					if (_inFlightLastValue)
					{
						ResetMFD();
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
		}

		private void ProcessBacklightValues()
		{
			if (Backlight == 1)
			{
				_backplate.Image = _backplateDimImage;
				SetButtonUpImage();
			}
			else if (Backlight == 2)
			{
				_backplate.Image = _backplateBrtImage;
				SetButtonUpImage();
			}
			else
			{
				_backplate.Image = _backplateOffImage;
				SetButtonUpImage();
			}

			Refresh();
		}

		private void SetButtonUpImage()
		{
			for (int i = 0; i < 20; i++)
            {
				_buttonOSB[i].Image = Backlight == 0 ? _buttonOffImage : _buttonLitImage;
			}
		}

		public override void Reset()
		{
			ResetMFD();
		}

		private void ResetMFD()
		{
			Backlight = 0d;

			ProcessBacklightValues();
		}

		private BindingValue GetValue(string device, string name)
		{
			return _falconInterface?.GetValue(device, name) ?? BindingValue.Empty;
		}

		#endregion Methods

		#region Mouse Actions

		public override void MouseDown(Point location)
		{
			Point point = new Point(location.X * _rectBase.Width / Width, location.Y * _rectBase.Height / Height);

			for (int i = 0; i < 20; i++)
            {
				if (_rectButtonOSB[i].Contains(point))
				{
					_buttonOSB[i].Image = _buttonPressImage;
					_buttonOSBPushed[i].FireTrigger(new BindingValue(true));
				}
			}

			if (_rectRockerGAINUp.Contains(point))
			{
				_rockerGAIN.Image = _rockerGAINUpImage;
				_rockerGAINUpTrigger.FireTrigger(new BindingValue(true));
			}
			if (_rectRockerGAINDown.Contains(point))
			{
				_rockerGAIN.Image = _rockerGAINDownImage;
				_rockerGAINDownTrigger.FireTrigger(new BindingValue(true));
			}
			if (_rectRockerSYMUp.Contains(point))
			{
				_rockerSYM.Image = _rockerSYMUpImage;
				_rockerSYMUpTrigger.FireTrigger(new BindingValue(true));
			}
			if (_rectRockerSYMDown.Contains(point))
			{
				_rockerSYM.Image = _rockerSYMDownImage;
				_rockerSYMDownTrigger.FireTrigger(new BindingValue(true));
			}
			if (_rectRockerBRTUp.Contains(point))
			{
				_rockerBRT.Image = _rockerBRTUpImage;
				_rockerBRTUpTrigger.FireTrigger(new BindingValue(true));
			}
			if (_rectRockerBRTDown.Contains(point))
			{
				_rockerBRT.Image = _rockerBRTDownImage;
				_rockerBRTDownTrigger.FireTrigger(new BindingValue(true));
			}
			if (_rectRockerCONUp.Contains(point))
			{
				_rockerCON.Image = _rockerCONUpImage;
				_rockerCONUpTrigger.FireTrigger(new BindingValue(true));
			}
			if (_rectRockerCONDown.Contains(point))
			{
				_rockerCON.Image = _rockerCONDownImage;
				_rockerCONDownTrigger.FireTrigger(new BindingValue(true));
			}

			Refresh();
		}

		public override void MouseUp(Point location)
		{
			Point point = new Point(location.X * _rectBase.Width / Width, location.Y * _rectBase.Height / Height);

			for (int i = 0; i < 20; i++)
            {
				if (_rectButtonOSB[i].Contains(point))
				{
					_buttonOSBReleased[i].FireTrigger(new BindingValue(false));
				}
			}

			SetButtonUpImage();

			_rockerGAIN.Image = _rockerGAINCenterImage;
			_rockerSYM.Image = _rockerSYMCenterImage;
			_rockerBRT.Image = _rockerBRTCenterImage;
			_rockerCON.Image = _rockerCONCenterImage;

			Refresh();
		}

		#endregion Mouse Actions

		#region Properties

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
