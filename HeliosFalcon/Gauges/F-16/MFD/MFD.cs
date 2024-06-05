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

	[HeliosControl("Helios.Falcon.MFD", "Falcon BMS MFD", "Falcon Simulator F-16", typeof(GaugeRenderer))]
	public class MFD : BaseGauge
	{
		private FalconInterface _falconInterface;
		private GaugeImage _backplate;
		private GaugeImage _faceplate;
		private GaugeImage _buttonOSB01;
		private GaugeImage _buttonOSB02;
		private GaugeImage _buttonOSB03;
		private GaugeImage _buttonOSB04;
		private GaugeImage _buttonOSB05;
		private GaugeImage _buttonOSB06;
		private GaugeImage _buttonOSB07;
		private GaugeImage _buttonOSB08;
		private GaugeImage _buttonOSB09;
		private GaugeImage _buttonOSB10;
		private GaugeImage _buttonOSB11;
		private GaugeImage _buttonOSB12;
		private GaugeImage _buttonOSB13;
		private GaugeImage _buttonOSB14;
		private GaugeImage _buttonOSB15;
		private GaugeImage _buttonOSB16;
		private GaugeImage _buttonOSB17;
		private GaugeImage _buttonOSB18;
		private GaugeImage _buttonOSB19;
		private GaugeImage _buttonOSB20;
		private GaugeImage _rockerGAIN;
		private GaugeImage _rockerSYM;
		private GaugeImage _rockerBRT;
		private GaugeImage _rockerCON;

		private HeliosTrigger _buttonOSB01Pushed;
		private HeliosTrigger _buttonOSB02Pushed;
		private HeliosTrigger _buttonOSB03Pushed;
		private HeliosTrigger _buttonOSB04Pushed;
		private HeliosTrigger _buttonOSB05Pushed;
		private HeliosTrigger _buttonOSB06Pushed;
		private HeliosTrigger _buttonOSB07Pushed;
		private HeliosTrigger _buttonOSB08Pushed;
		private HeliosTrigger _buttonOSB09Pushed;
		private HeliosTrigger _buttonOSB10Pushed;
		private HeliosTrigger _buttonOSB11Pushed;
		private HeliosTrigger _buttonOSB12Pushed;
		private HeliosTrigger _buttonOSB13Pushed;
		private HeliosTrigger _buttonOSB14Pushed;
		private HeliosTrigger _buttonOSB15Pushed;
		private HeliosTrigger _buttonOSB16Pushed;
		private HeliosTrigger _buttonOSB17Pushed;
		private HeliosTrigger _buttonOSB18Pushed;
		private HeliosTrigger _buttonOSB19Pushed;
		private HeliosTrigger _buttonOSB20Pushed;
		private HeliosTrigger _buttonOSB01Released;
		private HeliosTrigger _buttonOSB02Released;
		private HeliosTrigger _buttonOSB03Released;
		private HeliosTrigger _buttonOSB04Released;
		private HeliosTrigger _buttonOSB05Released;
		private HeliosTrigger _buttonOSB06Released;
		private HeliosTrigger _buttonOSB07Released;
		private HeliosTrigger _buttonOSB08Released;
		private HeliosTrigger _buttonOSB09Released;
		private HeliosTrigger _buttonOSB10Released;
		private HeliosTrigger _buttonOSB11Released;
		private HeliosTrigger _buttonOSB12Released;
		private HeliosTrigger _buttonOSB13Released;
		private HeliosTrigger _buttonOSB14Released;
		private HeliosTrigger _buttonOSB15Released;
		private HeliosTrigger _buttonOSB16Released;
		private HeliosTrigger _buttonOSB17Released;
		private HeliosTrigger _buttonOSB18Released;
		private HeliosTrigger _buttonOSB19Released;
		private HeliosTrigger _buttonOSB20Released;
		private HeliosTrigger _rockerGAINUpTrigger;
		private HeliosTrigger _rockerGAINDownTrigger;
		private HeliosTrigger _rockerSYMUpTrigger;
		private HeliosTrigger _rockerSYMDownTrigger;
		private HeliosTrigger _rockerBRTUpTrigger;
		private HeliosTrigger _rockerBRTDownTrigger;
		private HeliosTrigger _rockerCONUpTrigger;
		private HeliosTrigger _rockerCONDownTrigger;

		Rect _rectButtonOSB01 = new Rect(100, 8, 51, 51);
		Rect _rectButtonOSB02 = new Rect(156, 8, 51, 51);
		Rect _rectButtonOSB03 = new Rect(212, 8, 51, 51);
		Rect _rectButtonOSB04 = new Rect(268, 8, 51, 51);
		Rect _rectButtonOSB05 = new Rect(324, 8, 51, 51);
		Rect _rectButtonOSB06 = new Rect(416, 100, 51, 51);
		Rect _rectButtonOSB07 = new Rect(416, 156, 51, 51);
		Rect _rectButtonOSB08 = new Rect(416, 212, 51, 51);
		Rect _rectButtonOSB09 = new Rect(416, 268, 51, 51);
		Rect _rectButtonOSB10 = new Rect(416, 324, 51, 51);
		Rect _rectButtonOSB11 = new Rect(324, 416, 51, 51);
		Rect _rectButtonOSB12 = new Rect(268, 416, 51, 51);
		Rect _rectButtonOSB13 = new Rect(212, 416, 51, 51);
		Rect _rectButtonOSB14 = new Rect(156, 416, 51, 51);
		Rect _rectButtonOSB15 = new Rect(100, 416, 51, 51);
		Rect _rectButtonOSB16 = new Rect(8, 324, 51, 51);
		Rect _rectButtonOSB17 = new Rect(8, 268, 51, 51);
		Rect _rectButtonOSB18 = new Rect(8, 212, 51, 51);
		Rect _rectButtonOSB19 = new Rect(8, 156, 51, 51);
		Rect _rectButtonOSB20 = new Rect(8, 100, 51, 51);
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
			AddComponents();
			AddActions();
		}

		#region Components

		private void AddComponents()
		{
			_backplate = new GaugeImage(_backplateOffImage, _rectBase);
			Components.Add(_backplate);

			_faceplate = new GaugeImage(_faceplateImage, _rectBase);
			Components.Add(_faceplate);

			_buttonOSB01 = new GaugeImage(_buttonOffImage, _rectButtonOSB01);
			Components.Add(_buttonOSB01);

			_buttonOSB02 = new GaugeImage(_buttonOffImage, _rectButtonOSB02);
			Components.Add(_buttonOSB02);

			_buttonOSB03 = new GaugeImage(_buttonOffImage, _rectButtonOSB03);
			Components.Add(_buttonOSB03);

			_buttonOSB04 = new GaugeImage(_buttonOffImage, _rectButtonOSB04);
			Components.Add(_buttonOSB04);

			_buttonOSB05 = new GaugeImage(_buttonOffImage, _rectButtonOSB05);
			Components.Add(_buttonOSB05);

			_buttonOSB06 = new GaugeImage(_buttonOffImage, _rectButtonOSB06);
			Components.Add(_buttonOSB06);

			_buttonOSB07 = new GaugeImage(_buttonOffImage, _rectButtonOSB07);
			Components.Add(_buttonOSB07);

			_buttonOSB08 = new GaugeImage(_buttonOffImage, _rectButtonOSB08);
			Components.Add(_buttonOSB08);

			_buttonOSB09 = new GaugeImage(_buttonOffImage, _rectButtonOSB09);
			Components.Add(_buttonOSB09);

			_buttonOSB10 = new GaugeImage(_buttonOffImage, _rectButtonOSB10);
			Components.Add(_buttonOSB10);

			_buttonOSB11 = new GaugeImage(_buttonOffImage, _rectButtonOSB11);
			Components.Add(_buttonOSB11);

			_buttonOSB12 = new GaugeImage(_buttonOffImage, _rectButtonOSB12);
			Components.Add(_buttonOSB12);

			_buttonOSB13 = new GaugeImage(_buttonOffImage, _rectButtonOSB13);
			Components.Add(_buttonOSB13);

			_buttonOSB14 = new GaugeImage(_buttonOffImage, _rectButtonOSB14);
			Components.Add(_buttonOSB14);

			_buttonOSB15 = new GaugeImage(_buttonOffImage, _rectButtonOSB15);
			Components.Add(_buttonOSB15);

			_buttonOSB16 = new GaugeImage(_buttonOffImage, _rectButtonOSB16);
			Components.Add(_buttonOSB16);

			_buttonOSB17 = new GaugeImage(_buttonOffImage, _rectButtonOSB17);
			Components.Add(_buttonOSB17);

			_buttonOSB18 = new GaugeImage(_buttonOffImage, _rectButtonOSB18);
			Components.Add(_buttonOSB18);

			_buttonOSB19 = new GaugeImage(_buttonOffImage, _rectButtonOSB19);
			Components.Add(_buttonOSB19);

			_buttonOSB20 = new GaugeImage(_buttonOffImage, _rectButtonOSB20);
			Components.Add(_buttonOSB20);

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
			_buttonOSB01Pushed = new HeliosTrigger(this, "", "", "Button OSB-1 Pushed", "Fired when button OSB-1 is pushed.", "Always returns true.", BindingValueUnits.Boolean);
			Triggers.Add(_buttonOSB01Pushed);

			_buttonOSB01Released = new HeliosTrigger(this, "", "", "Button OSB-1 Released", "Fired when button OSB-1 is released.", "Always returns false.", BindingValueUnits.Boolean);
			Triggers.Add(_buttonOSB01Released);

			_buttonOSB02Pushed = new HeliosTrigger(this, "", "", "Button OSB-2 Pushed", "Fired when button OSB-2 is pushed.", "Always returns true.", BindingValueUnits.Boolean);
			Triggers.Add(_buttonOSB02Pushed);

			_buttonOSB02Released = new HeliosTrigger(this, "", "", "Button OSB-2 Released", "Fired when button OSB-2 is released.", "Always returns false.", BindingValueUnits.Boolean);
			Triggers.Add(_buttonOSB02Released);

			_buttonOSB03Pushed = new HeliosTrigger(this, "", "", "Button OSB-3 Pushed", "Fired when button OSB-3 is pushed.", "Always returns true.", BindingValueUnits.Boolean);
			Triggers.Add(_buttonOSB03Pushed);

			_buttonOSB03Released = new HeliosTrigger(this, "", "", "Button OSB-3 Released", "Fired when button OSB-3 is released.", "Always returns false.", BindingValueUnits.Boolean);
			Triggers.Add(_buttonOSB03Released);

			_buttonOSB04Pushed = new HeliosTrigger(this, "", "", "Button OSB-4 Pushed", "Fired when button OSB-4 is pushed.", "Always returns true.", BindingValueUnits.Boolean);
			Triggers.Add(_buttonOSB04Pushed);

			_buttonOSB04Released = new HeliosTrigger(this, "", "", "Button OSB-4 Released", "Fired when button OSB-4 is released.", "Always returns false.", BindingValueUnits.Boolean);
			Triggers.Add(_buttonOSB04Released);

			_buttonOSB05Pushed = new HeliosTrigger(this, "", "", "Button OSB-5 Pushed", "Fired when button OSB-5 is pushed.", "Always returns true.", BindingValueUnits.Boolean);
			Triggers.Add(_buttonOSB05Pushed);

			_buttonOSB05Released = new HeliosTrigger(this, "", "", "Button OSB-5 Released", "Fired when button OSB-5 is released.", "Always returns false.", BindingValueUnits.Boolean);
			Triggers.Add(_buttonOSB05Released);

			_buttonOSB06Pushed = new HeliosTrigger(this, "", "", "Button OSB-6 Pushed", "Fired when button OSB-6 is pushed.", "Always returns true.", BindingValueUnits.Boolean);
			Triggers.Add(_buttonOSB06Pushed);

			_buttonOSB06Released = new HeliosTrigger(this, "", "", "Button OSB-6 Released", "Fired when button OSB-6 is released.", "Always returns false.", BindingValueUnits.Boolean);
			Triggers.Add(_buttonOSB06Released);

			_buttonOSB07Pushed = new HeliosTrigger(this, "", "", "Button OSB-7 Pushed", "Fired when button OSB-7 is pushed.", "Always returns true.", BindingValueUnits.Boolean);
			Triggers.Add(_buttonOSB07Pushed);

			_buttonOSB07Released = new HeliosTrigger(this, "", "", "Button OSB-7 Released", "Fired when button OSB-7 is released.", "Always returns false.", BindingValueUnits.Boolean);
			Triggers.Add(_buttonOSB07Released);

			_buttonOSB08Pushed = new HeliosTrigger(this, "", "", "Button OSB-8 Pushed", "Fired when button OSB-8 is pushed.", "Always returns true.", BindingValueUnits.Boolean);
			Triggers.Add(_buttonOSB08Pushed);

			_buttonOSB08Released = new HeliosTrigger(this, "", "", "Button OSB-8 Released", "Fired when button OSB-8 is released.", "Always returns false.", BindingValueUnits.Boolean);
			Triggers.Add(_buttonOSB08Released);

			_buttonOSB09Pushed = new HeliosTrigger(this, "", "", "Button OSB-9 Pushed", "Fired when button OSB-9 is pushed.", "Always returns true.", BindingValueUnits.Boolean);
			Triggers.Add(_buttonOSB09Pushed);

			_buttonOSB09Released = new HeliosTrigger(this, "", "", "Button OSB-9 Released", "Fired when button OSB-9 is released.", "Always returns false.", BindingValueUnits.Boolean);
			Triggers.Add(_buttonOSB09Released);

			_buttonOSB10Pushed = new HeliosTrigger(this, "", "", "Button OSB-10 Pushed", "Fired when button OSB-10 is pushed.", "Always returns true.", BindingValueUnits.Boolean);
			Triggers.Add(_buttonOSB10Pushed);

			_buttonOSB10Released = new HeliosTrigger(this, "", "", "Button OSB-10 Released", "Fired when button OSB-10 is released.", "Always returns false.", BindingValueUnits.Boolean);
			Triggers.Add(_buttonOSB10Released);

			_buttonOSB11Pushed = new HeliosTrigger(this, "", "", "Button OSB-11 Pushed", "Fired when button OSB-11 is pushed.", "Always returns true.", BindingValueUnits.Boolean);
			Triggers.Add(_buttonOSB11Pushed);

			_buttonOSB11Released = new HeliosTrigger(this, "", "", "Button OSB-11 Released", "Fired when button OSB-11 is released.", "Always returns false.", BindingValueUnits.Boolean);
			Triggers.Add(_buttonOSB11Released);

			_buttonOSB12Pushed = new HeliosTrigger(this, "", "", "Button OSB-12 Pushed", "Fired when button OSB-12 is pushed.", "Always returns true.", BindingValueUnits.Boolean);
			Triggers.Add(_buttonOSB12Pushed);

			_buttonOSB12Released = new HeliosTrigger(this, "", "", "Button OSB-12 Released", "Fired when button OSB-12 is released.", "Always returns false.", BindingValueUnits.Boolean);
			Triggers.Add(_buttonOSB12Released);

			_buttonOSB13Pushed = new HeliosTrigger(this, "", "", "Button OSB-13 Pushed", "Fired when button OSB-13 is pushed.", "Always returns true.", BindingValueUnits.Boolean);
			Triggers.Add(_buttonOSB13Pushed);

			_buttonOSB13Released = new HeliosTrigger(this, "", "", "Button OSB-13 Released", "Fired when button OSB-13 is released.", "Always returns false.", BindingValueUnits.Boolean);
			Triggers.Add(_buttonOSB13Released);

			_buttonOSB14Pushed = new HeliosTrigger(this, "", "", "Button OSB-14 Pushed", "Fired when button OSB-14 is pushed.", "Always returns true.", BindingValueUnits.Boolean);
			Triggers.Add(_buttonOSB14Pushed);

			_buttonOSB14Released = new HeliosTrigger(this, "", "", "Button OSB-14 Released", "Fired when button OSB-14 is released.", "Always returns false.", BindingValueUnits.Boolean);
			Triggers.Add(_buttonOSB14Released);

			_buttonOSB15Pushed = new HeliosTrigger(this, "", "", "Button OSB-15 Pushed", "Fired when button OSB-15 is pushed.", "Always returns true.", BindingValueUnits.Boolean);
			Triggers.Add(_buttonOSB15Pushed);

			_buttonOSB15Released = new HeliosTrigger(this, "", "", "Button OSB-15 Released", "Fired when button OSB-15 is released.", "Always returns false.", BindingValueUnits.Boolean);
			Triggers.Add(_buttonOSB15Released);

			_buttonOSB16Pushed = new HeliosTrigger(this, "", "", "Button OSB-16 Pushed", "Fired when button OSB-16 is pushed.", "Always returns true.", BindingValueUnits.Boolean);
			Triggers.Add(_buttonOSB16Pushed);

			_buttonOSB16Released = new HeliosTrigger(this, "", "", "Button OSB-16 Released", "Fired when button OSB-16 is released.", "Always returns false.", BindingValueUnits.Boolean);
			Triggers.Add(_buttonOSB16Released);

			_buttonOSB17Pushed = new HeliosTrigger(this, "", "", "Button OSB-17 Pushed", "Fired when button OSB-17 is pushed.", "Always returns true.", BindingValueUnits.Boolean);
			Triggers.Add(_buttonOSB17Pushed);

			_buttonOSB17Released = new HeliosTrigger(this, "", "", "Button OSB-17 Released", "Fired when button OSB-17 is released.", "Always returns false.", BindingValueUnits.Boolean);
			Triggers.Add(_buttonOSB17Released);

			_buttonOSB18Pushed = new HeliosTrigger(this, "", "", "Button OSB-18 Pushed", "Fired when button OSB-18 is pushed.", "Always returns true.", BindingValueUnits.Boolean);
			Triggers.Add(_buttonOSB18Pushed);

			_buttonOSB18Released = new HeliosTrigger(this, "", "", "Button OSB-18 Released", "Fired when button OSB-18 is released.", "Always returns false.", BindingValueUnits.Boolean);
			Triggers.Add(_buttonOSB18Released);

			_buttonOSB19Pushed = new HeliosTrigger(this, "", "", "Button OSB-19 Pushed", "Fired when button OSB-19 is pushed.", "Always returns true.", BindingValueUnits.Boolean);
			Triggers.Add(_buttonOSB19Pushed);

			_buttonOSB19Released = new HeliosTrigger(this, "", "", "Button OSB-19 Released", "Fired when button OSB-19 is released.", "Always returns false.", BindingValueUnits.Boolean);
			Triggers.Add(_buttonOSB19Released);

			_buttonOSB20Pushed = new HeliosTrigger(this, "", "", "Button OSB-20 Pushed", "Fired when button OSB-20 is pushed.", "Always returns true.", BindingValueUnits.Boolean);
			Triggers.Add(_buttonOSB20Pushed);

			_buttonOSB20Released = new HeliosTrigger(this, "", "", "Button OSB-20 Released", "Fired when button OSB-20 is released.", "Always returns false.", BindingValueUnits.Boolean);
			Triggers.Add(_buttonOSB20Released);

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
			_buttonOSB01.Image = Backlight == 0 ? _buttonOffImage : _buttonLitImage;
			_buttonOSB02.Image = Backlight == 0 ? _buttonOffImage : _buttonLitImage;
			_buttonOSB03.Image = Backlight == 0 ? _buttonOffImage : _buttonLitImage;
			_buttonOSB04.Image = Backlight == 0 ? _buttonOffImage : _buttonLitImage;
			_buttonOSB05.Image = Backlight == 0 ? _buttonOffImage : _buttonLitImage;
			_buttonOSB06.Image = Backlight == 0 ? _buttonOffImage : _buttonLitImage;
			_buttonOSB07.Image = Backlight == 0 ? _buttonOffImage : _buttonLitImage;
			_buttonOSB08.Image = Backlight == 0 ? _buttonOffImage : _buttonLitImage;
			_buttonOSB09.Image = Backlight == 0 ? _buttonOffImage : _buttonLitImage;
			_buttonOSB10.Image = Backlight == 0 ? _buttonOffImage : _buttonLitImage;
			_buttonOSB11.Image = Backlight == 0 ? _buttonOffImage : _buttonLitImage;
			_buttonOSB12.Image = Backlight == 0 ? _buttonOffImage : _buttonLitImage;
			_buttonOSB13.Image = Backlight == 0 ? _buttonOffImage : _buttonLitImage;
			_buttonOSB14.Image = Backlight == 0 ? _buttonOffImage : _buttonLitImage;
			_buttonOSB15.Image = Backlight == 0 ? _buttonOffImage : _buttonLitImage;
			_buttonOSB16.Image = Backlight == 0 ? _buttonOffImage : _buttonLitImage;
			_buttonOSB17.Image = Backlight == 0 ? _buttonOffImage : _buttonLitImage;
			_buttonOSB18.Image = Backlight == 0 ? _buttonOffImage : _buttonLitImage;
			_buttonOSB19.Image = Backlight == 0 ? _buttonOffImage : _buttonLitImage;
			_buttonOSB20.Image = Backlight == 0 ? _buttonOffImage : _buttonLitImage;
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

			if (_rectButtonOSB01.Contains(point))
			{
				_buttonOSB01.Image = _buttonPressImage;
				_buttonOSB01Pushed.FireTrigger(new BindingValue(true));
			}
			if (_rectButtonOSB02.Contains(point))
			{
				_buttonOSB02.Image = _buttonPressImage;
				_buttonOSB02Pushed.FireTrigger(new BindingValue(true));
			}
			if (_rectButtonOSB03.Contains(point))
			{
				_buttonOSB03.Image = _buttonPressImage;
				_buttonOSB03Pushed.FireTrigger(new BindingValue(true));
			}
			if (_rectButtonOSB04.Contains(point))
			{
				_buttonOSB04.Image = _buttonPressImage;
				_buttonOSB04Pushed.FireTrigger(new BindingValue(true));
			}
			if (_rectButtonOSB05.Contains(point))
			{
				_buttonOSB05.Image = _buttonPressImage;
				_buttonOSB05Pushed.FireTrigger(new BindingValue(true));
			}
			if (_rectButtonOSB06.Contains(point))
			{
				_buttonOSB06.Image = _buttonPressImage;
				_buttonOSB06Pushed.FireTrigger(new BindingValue(true));
			}
			if (_rectButtonOSB07.Contains(point))
			{
				_buttonOSB07.Image = _buttonPressImage;
				_buttonOSB07Pushed.FireTrigger(new BindingValue(true));
			}
			if (_rectButtonOSB08.Contains(point))
			{
				_buttonOSB08.Image = _buttonPressImage;
				_buttonOSB08Pushed.FireTrigger(new BindingValue(true));
			}
			if (_rectButtonOSB09.Contains(point))
			{
				_buttonOSB09.Image = _buttonPressImage;
				_buttonOSB09Pushed.FireTrigger(new BindingValue(true));
			}
			if (_rectButtonOSB10.Contains(point))
			{
				_buttonOSB10.Image = _buttonPressImage;
				_buttonOSB10Pushed.FireTrigger(new BindingValue(true));
			}
			if (_rectButtonOSB11.Contains(point))
			{
				_buttonOSB11.Image = _buttonPressImage;
				_buttonOSB11Pushed.FireTrigger(new BindingValue(true));
			}
			if (_rectButtonOSB12.Contains(point))
			{
				_buttonOSB12.Image = _buttonPressImage;
				_buttonOSB12Pushed.FireTrigger(new BindingValue(true));
			}
			if (_rectButtonOSB13.Contains(point))
			{
				_buttonOSB13.Image = _buttonPressImage;
				_buttonOSB13Pushed.FireTrigger(new BindingValue(true));
			}
			if (_rectButtonOSB14.Contains(point))
			{
				_buttonOSB14.Image = _buttonPressImage;
				_buttonOSB14Pushed.FireTrigger(new BindingValue(true));
			}
			if (_rectButtonOSB15.Contains(point))
			{
				_buttonOSB15.Image = _buttonPressImage;
				_buttonOSB15Pushed.FireTrigger(new BindingValue(true));
			}
			if (_rectButtonOSB16.Contains(point))
			{
				_buttonOSB16.Image = _buttonPressImage;
				_buttonOSB16Pushed.FireTrigger(new BindingValue(true));
			}
			if (_rectButtonOSB17.Contains(point))
			{
				_buttonOSB17.Image = _buttonPressImage;
				_buttonOSB17Pushed.FireTrigger(new BindingValue(true));
			}
			if (_rectButtonOSB18.Contains(point))
			{
				_buttonOSB18.Image = _buttonPressImage;
				_buttonOSB18Pushed.FireTrigger(new BindingValue(true));
			}
			if (_rectButtonOSB19.Contains(point))
			{
				_buttonOSB19.Image = _buttonPressImage;
				_buttonOSB19Pushed.FireTrigger(new BindingValue(true));
			}
			if (_rectButtonOSB20.Contains(point))
			{
				_buttonOSB20.Image = _buttonPressImage;
				_buttonOSB20Pushed.FireTrigger(new BindingValue(true));
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

			if (_rectButtonOSB01.Contains(point))
			{
				_buttonOSB01Released.FireTrigger(new BindingValue(false));
			}
			if (_rectButtonOSB02.Contains(point))
			{
				_buttonOSB02Released.FireTrigger(new BindingValue(false));
			}
			if (_rectButtonOSB03.Contains(point))
			{
				_buttonOSB03Released.FireTrigger(new BindingValue(false));
			}
			if (_rectButtonOSB04.Contains(point))
			{
				_buttonOSB04Released.FireTrigger(new BindingValue(false));
			}
			if (_rectButtonOSB05.Contains(point))
			{
				_buttonOSB05Released.FireTrigger(new BindingValue(false));
			}
			if (_rectButtonOSB06.Contains(point))
			{
				_buttonOSB06Released.FireTrigger(new BindingValue(false));
			}
			if (_rectButtonOSB07.Contains(point))
			{
				_buttonOSB07Released.FireTrigger(new BindingValue(false));
			}
			if (_rectButtonOSB08.Contains(point))
			{
				_buttonOSB08Released.FireTrigger(new BindingValue(false));
			}
			if (_rectButtonOSB09.Contains(point))
			{
				_buttonOSB09Released.FireTrigger(new BindingValue(false));
			}
			if (_rectButtonOSB10.Contains(point))
			{
				_buttonOSB10Released.FireTrigger(new BindingValue(false));
			}
			if (_rectButtonOSB11.Contains(point))
			{
				_buttonOSB11Released.FireTrigger(new BindingValue(false));
			}
			if (_rectButtonOSB12.Contains(point))
			{
				_buttonOSB12Released.FireTrigger(new BindingValue(false));
			}
			if (_rectButtonOSB13.Contains(point))
			{
				_buttonOSB13Released.FireTrigger(new BindingValue(false));
			}
			if (_rectButtonOSB14.Contains(point))
			{
				_buttonOSB14Released.FireTrigger(new BindingValue(false));
			}
			if (_rectButtonOSB15.Contains(point))
			{
				_buttonOSB15Released.FireTrigger(new BindingValue(false));
			}
			if (_rectButtonOSB16.Contains(point))
			{
				_buttonOSB16Released.FireTrigger(new BindingValue(false));
			}
			if (_rectButtonOSB17.Contains(point))
			{
				_buttonOSB17Released.FireTrigger(new BindingValue(false));
			}
			if (_rectButtonOSB18.Contains(point))
			{
				_buttonOSB18Released.FireTrigger(new BindingValue(false));
			}
			if (_rectButtonOSB19.Contains(point))
			{
				_buttonOSB19Released.FireTrigger(new BindingValue(false));
			}
			if (_rectButtonOSB20.Contains(point))
			{
				_buttonOSB20Released.FireTrigger(new BindingValue(false));
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
