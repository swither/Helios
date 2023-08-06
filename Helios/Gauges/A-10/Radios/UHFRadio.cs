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

// ReSharper disable once CheckNamespace
namespace GadrocsWorkshop.Helios.Gauges.A10C
{
    using GadrocsWorkshop.Helios.Controls.Capabilities;
    using GadrocsWorkshop.Helios.ComponentModel;
    using GadrocsWorkshop.Helios.Controls;
    using System;
    using System.Windows;

    /// <summary>
    /// This is an A-10C UHF Radio that uses text displays instead of an exported viewport.
    /// </summary>
    [HeliosControl("Helios.A10C.UHFRadio", "UFH Radio", "A-10C Gauges", typeof(BackgroundImageRenderer), HeliosControlFlags.NotShownInUI)]
    class UHFRadio : A10CDevice
    {
        private const double SCREENRES = 1.0;
        private readonly string _interfaceDeviceName = "UHF Radio";
        private readonly string _imageLocation = "{A-10C}/Images/A-10CII/";
        private readonly IConfigurableBackgroundImage _channelDisplay;
        private readonly IConfigurableBackgroundImage _frequencyDisplay;

        public UHFRadio()
            : base("UHF Radio", new Size(625, 580))
        {
            RotarySwitchPositionCollection positions = new RotarySwitchPositionCollection();
            positions.Clear();
            positions.Add(new RotarySwitchPosition(this, 1, "Off", 300d));
            positions.Add(new RotarySwitchPosition(this, 2, "Main", 340d));
            positions.Add(new RotarySwitchPosition(this, 3, "Both", 20d));
            positions.Add(new RotarySwitchPosition(this, 4, "ADF", 60d));
            AddRotarySwitch("Frequency Dial", new Point(81, 455), new Size(115, 115), "1", 1, positions, "Frequency Dial");
            positions.Clear();
            positions.Add(new RotarySwitchPosition(this, 1, "Manual", 315d));
            positions.Add(new RotarySwitchPosition(this, 2, "Preset", 0d));
            positions.Add(new RotarySwitchPosition(this, 3, "Guard", 45d));
            AddRotarySwitch("Frequency Mode Dial", new Point(433, 455), new Size(115, 115), "1", 1, positions, "Frequency Mode Dial");
            positions.Clear();
            positions.Add(new RotarySwitchPosition(this, 1, "2", 225d));
            positions.Add(new RotarySwitchPosition(this, 2, "3", 270d));
            positions.Add(new RotarySwitchPosition(this, 3, "A", 315d));
            AddRotarySwitch("100Mhz Selector", new Point(54, 328), new Size(75,75), "3", 1, positions, "100Mhz Selector");
            RotaryEncoder enc;
            enc = AddEncoder("10Mhz Selector", new Point(161, 329), new Size(75, 75), _imageLocation + "A-10C_UHF_Radio_Selector_Knob_3.png", 0.1d, 30d, _interfaceDeviceName, "10Mhz Selector", false);
            enc.InitialRotation = 15;
            enc = AddEncoder("1Mhz Selector", new Point(263, 329), new Size(75, 75), _imageLocation + "A-10C_UHF_Radio_Selector_Knob_3.png", 0.1d, 30d, _interfaceDeviceName, "1Mhz Selector", false);
            enc.InitialRotation = 90;
            enc = AddEncoder("0:1Mhz Selector", new Point(382, 329), new Size(75, 75), _imageLocation + "A-10C_UHF_Radio_Selector_Knob_3.png", 0.1d, 30d, _interfaceDeviceName, "0.1Mhz Selector", false);
            enc.InitialRotation = 60;
            enc = AddEncoder("0:025Mhz Selector", new Point(491, 329), new Size(75, 75), _imageLocation + "A-10C_UHF_Radio_Selector_Knob_3.png", 0.1d, 30d, _interfaceDeviceName, "0.025Mhz Selector", false);
            enc.InitialRotation = 45;
            enc = AddEncoder("Channel Selector", new Point(474, 110), new Size(100, 100), _imageLocation + "A-10C_UHF_Radio_Selector_Knob_2.png", 0.1d, 30d, _interfaceDeviceName, "Preset Channel Selector", false);
            enc.InitialRotation = 10;
            AddPot("Volume", new Point(274, 420 ), new Size(60, 60), _imageLocation + "A-10C_UHF_Radio_Volume_Knob.png", 0d, 300d, 0d, 1d, 0.5, 0.1, _interfaceDeviceName, "Volume", false, RotaryClickType.Radial, false);
            _channelDisplay = AddPart("Channel Display", new UHFChanDisplay(), new Point(298, 111), new Size(173, 100), _interfaceDeviceName, "Channel Display") as IConfigurableBackgroundImage;
            _frequencyDisplay = AddPart("Frequency Display", new UHFFreqDisplay(), new Point(161, 219), new Size(289, 98), _interfaceDeviceName, "Frequency Display") as IConfigurableBackgroundImage;
            AddThreeWayToggle("T/Tone Switch", new Point(222, 511), new Size(59, 59), "T/Tone Switch");
            AddTwoWaySquelchToggle("Squelch Switch", new Point(354, 511), new Size(52, 61), "Squelch");

            AddButton("Test Display Button", new Point(109, 242), new Size(45, 47), _imageLocation + "A-10C_UHF_Radio_Test_Button_Unpressed.png", _imageLocation + "A-10C_UHF_Radio_Test_Button_Pressed.png", "", _interfaceDeviceName, "Test Display Button", false);
            AddButton("Status Button", new Point(466, 245), new Size(45, 45), _imageLocation + "A-10C_UHF_Radio_Status_Button_Unpressed.png", _imageLocation + "A-10C_UHF_Radio_Status_Button_Pressed.png", "", _interfaceDeviceName, "Status Button", false);

            AddCover("Cover", new Point(23, 0), new Size(268, 233), "Cover");
            // red load button is under the cover so needs to have special binding
            AddLoadButton("Load Button",74, 175, new Size(36, 37), "Load Button");
        }

        public override string DefaultBackgroundImage => _imageLocation + "A-10C_UHF_Radio.png";

        protected override void OnBackgroundImageChange()
        {
            if (_channelDisplay != null)
            {
                _channelDisplay.BackgroundImage =
                    BackgroundImageIsCustomized ? null : _channelDisplay.DefaultBackgroundImage;
            }
            if (_frequencyDisplay != null)
            {
                _frequencyDisplay.BackgroundImage =
                    BackgroundImageIsCustomized ? null : _frequencyDisplay.DefaultBackgroundImage;
            }
        }

        private void AddCover(string name, Point posn, Size size, string interfaceElementName)
        {
            string componentName = _interfaceDeviceName + "_" + name;
            ToggleSwitch newSwitch = new ToggleSwitch
            {
                Name = componentName,
                SwitchType = ToggleSwitchType.OnOn,
                ClickType = LinearClickType.Swipe,
                DefaultPosition = ToggleSwitchPosition.Two,
                PositionOneImage = _imageLocation + "A-10C_UHF_Radio_Lid_Closed.png",
                PositionTwoImage = _imageLocation + "A-10C_UHF_Radio_Lid_Opened.png",
                Width = size.Width,
                Height = size.Height,
                Top = posn.Y,
                Left = posn.X,
                Orientation = ToggleSwitchOrientation.VerticalReversed
            };
            Children.Add(newSwitch);
            foreach (IBindingTrigger trigger in newSwitch.Triggers)
            {
                AddTrigger(trigger, componentName);
            }
            AddAction(newSwitch.Actions["set.position"], componentName);

            AddDefaultOutputBinding(
                childName: componentName,
                deviceTriggerName: "position.changed",
                interfaceActionName: _interfaceDeviceName + ".set." + interfaceElementName
            );

            AddDefaultInputBinding(
                childName: componentName,
                interfaceTriggerName: _interfaceDeviceName + "." + interfaceElementName + ".changed",
                deviceActionName: "set.position"
                );
        }

        /// <summary>
        /// A-10C UHF Radio Load Button.
        /// This includes binding the hidden action to the switch positions of the cover which sits beneath the button to make them reversed visually.
        /// This is also the first use case of DefaultInputBinding's deviceTriggerName & deviceTriggerBindingValue in composite visual
        /// </summary>
        /// <param name="name">name of element</param>
        /// <param name="x">Horizontal coordinate of the button</param>
        /// <param name="y">Vertical coordinate of the button</param>
        /// <param name="size">Size of the button</param>
        /// <param name="interfaceElementName">Name of the corresponding trigger</param>
        private void AddLoadButton(string name, double x, double y, Size size, string interfaceElementName)
        {
            string componentName = _interfaceDeviceName + "_" + name;
            PushButton newButton = new PushButton
            {
                Top = y,
                Left = x,
                Width = size.Width,
                Height = size.Height,
                Image = _imageLocation + "A-10C_UHF_Radio_Red_Button_Unpressed.png",
                PushedImage = _imageLocation + "A-10C_UHF_Radio_Red_Button_Pressed.png",
                Text = "",
                Name = componentName,
                IsHidden = true
            };
            Children.Add(newButton);

            AddTrigger(newButton.Triggers["pushed"], componentName);
            AddTrigger(newButton.Triggers["released"], componentName);
            AddAction(newButton.Actions["set.hidden"], componentName);
            AddAction(newButton.Actions["set.physical state"], componentName);

            AddDefaultOutputBinding(
                childName: componentName,
                deviceTriggerName: "position.changed",
                interfaceActionName: _interfaceDeviceName + ".set." + interfaceElementName
                );

            AddDefaultInputBinding(
                childName: componentName,
                interfaceTriggerName: _interfaceDeviceName + "." + interfaceElementName + ".changed",
                deviceActionName: "set.physical state");

            DefaultInputBindings.Add(new DefaultInputBinding(
                childName: componentName,
                interfaceTriggerName: "",
                deviceTriggerName: "UHF Radio_Cover.position two.entered",
                deviceActionName: "set.hidden",
                deviceTriggerBindingValue: new BindingValue(false)));

            DefaultInputBindings.Add(new DefaultInputBinding(
                childName: componentName,
                interfaceTriggerName: "",
                deviceTriggerName: "UHF Radio_Cover.position one.entered",
                deviceActionName: "set.hidden",
                deviceTriggerBindingValue: new BindingValue(true)));
        }
        private void AddTwoWaySquelchToggle(string name, Point posn, Size size, string interfaceElementName)
        {
            ToggleSwitch toggle = AddToggleSwitch(
                name: name,
                posn: posn,
                size: size,
                defaultPosition: ToggleSwitchPosition.Two,
                defaultType: ToggleSwitchType.OnOn,
                positionOneImage: _imageLocation + "A-10C_UHF_Radio_Squelch_Left.png",
                positionTwoImage: _imageLocation + "A-10C_UHF_Radio_Squelch_Right.png",
                interfaceDeviceName: _interfaceDeviceName,
                interfaceElementName: interfaceElementName,
                clickType: LinearClickType.Swipe,
                fromCenter: false
                );
            toggle.Orientation = ToggleSwitchOrientation.Horizontal;
        }

        private void AddThreeWayToggle(string name, Point posn, Size size,
            string interfaceElementName)
        {
            string componentName = _interfaceDeviceName + "_" + name;
            ThreeWayToggleSwitch toggle = new ThreeWayToggleSwitch
            {
                Top = posn.Y,
                Left = posn.X,
                Width = size.Width,
                Height = size.Height,
                DefaultPosition = ThreeWayToggleSwitchPosition.Two,
                PositionOneImage = _imageLocation + "A-10C_UHF_Radio_Tone_Right.png",
                PositionTwoImage = _imageLocation + "A-10C_UHF_Radio_Tone_Middle.png",
                PositionThreeImage = _imageLocation + "A-10C_UHF_Radio_Tone_Left.png",
                SwitchType = ThreeWayToggleSwitchType.MomOnMom,
                Name = _interfaceDeviceName + "_" + name,
                ClickType = LinearClickType.Swipe,
                Orientation = ToggleSwitchOrientation.HorizontalReversed
            };

            Children.Add(toggle);
            foreach (IBindingTrigger trigger in toggle.Triggers)
            {
                AddTrigger(trigger, componentName);
            }
            AddAction(toggle.Actions["set.position"], componentName);
            AddDefaultOutputBinding(
                childName: componentName,
                deviceTriggerName: "position.changed",
                interfaceActionName: _interfaceDeviceName + ".set." + interfaceElementName);

            AddDefaultInputBinding(
            childName: componentName,
                interfaceTriggerName: _interfaceDeviceName + "." + interfaceElementName + ".changed",
                deviceActionName: "set.position");
        }

        private RotarySwitch AddRotarySwitch(string name, Point posn, Size size, string knobNumber, int defaultPosition, RotarySwitchPositionCollection positions, string interfaceElementName)
        {
            RotarySwitch newSwitch = new RotarySwitch
            {
                Name = _interfaceDeviceName + "_" + name,
                KnobImage = _imageLocation + "A-10C_UHF_Radio_Selector_Knob_" + knobNumber + ".png",
                DrawLabels = false,
                DrawLines = false,
                Top = posn.Y,
                Left = posn.X,
                Width = size.Width,
                Height = size.Height,
                DefaultPosition = defaultPosition,
                IsContinuous = false
            };
            newSwitch.Positions.Clear();
            foreach (RotarySwitchPosition swPosn in positions)
            {
                newSwitch.Positions.Add(swPosn);
            }

            AddRotarySwitchBindings(name, posn, size, newSwitch, _interfaceDeviceName, interfaceElementName);
            return newSwitch;
        }

        private CompositeVisual AddPart(string name, CompositeVisual part, Point posn, Size size, string interfaceDevice, string interfaceElement)
        {
            size.Width *= SCREENRES;
            size.Height *= SCREENRES;
            posn.X *= SCREENRES;
            posn.Y *= SCREENRES;

            CompositeVisual newPart = AddDevice(
                name: name,
                device: part,
                size: size,
                posn: posn,
                interfaceDeviceName: interfaceDevice,
                interfaceElementName: interfaceElement
            );

            // NOTE: this shortens the name of the generated part
            newPart.Name = name;
            return newPart;
        }

        public override bool HitTest(Point location) => false;
    }
}