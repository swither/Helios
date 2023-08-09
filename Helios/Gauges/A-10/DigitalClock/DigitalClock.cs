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

namespace GadrocsWorkshop.Helios.Gauges.A10C
{
    using GadrocsWorkshop.Helios.ComponentModel;
    using GadrocsWorkshop.Helios.Controls;
    using System;
    using System.Windows;
    using System.Windows.Media;

    /// <summary>
    /// This is the A-10C Digital Clock panel which uses text displays instead of cutouts for the exported viewport.
    /// </summary>
    /// 
    [HeliosControl("Helios.A10.DigitalClock", "Digital Clock", "A-10C Gauges", typeof(BackgroundImageRenderer), HeliosControlFlags.NotShownInUI)]
    class DigitalClock : A10CDevice
    {

        public const double GLASS_REFLECTION_OPACITY_DEFAULT = 0.50;
        private string _imageLocation = "{A-10C}/Images/A-10C/";
        private string _interfaceDeviceName = "Digital Clock";
        private DigitalClockGauge _digitalClockGauge;
        private HeliosPanel _glass;
        private String _font = "Helios Virtual Cockpit A-10C_Digital_Clock";
        private Color _textColor = Color.FromArgb(0xff, 0xff, 0xff, 0xff);
        private Color _backGroundColor = Color.FromArgb(0, 100, 20, 50);
        private readonly HeliosPanel _bezel;
        private const string PANEL_IMAGE = "A-10C_Digital_Clock_Bezel.png";
        private const string REFLECTION_IMAGE = "Pilot_Reflection_25.png";

        public DigitalClock()
            : base("DigitalClock", new Size(414d, 405d))
        {
            AddClock("DigitalClockGauge", 0, 0, new Size(414, 405), _interfaceDeviceName, "Time Seconds");

            AddTextDisplay("HHMM", 100, 159, new Size(180, 80), 54d,
                "00:00", _interfaceDeviceName, "Time Hours:Mins");
            AddTextDisplay("SS", 160, 210, new Size(77, 80), 54d,
                "88", _interfaceDeviceName, "Time Seconds");
            AddTextDisplay("Clock Function", 180, 258, new Size(53, 60), 36d,
                "CET", _interfaceDeviceName, "Clock Function Type");
            _glass = AddPanel("Digital Clock Reflection", new Point(0,0), new Size(414d, 405d), _imageLocation + REFLECTION_IMAGE,"relection");
            _glass.Opacity = GLASS_REFLECTION_OPACITY_DEFAULT;
            _bezel = AddPanel("Digital Clock Bezel", new Point(0, 0), new Size(414d, 405d), _imageLocation + PANEL_IMAGE, "bezel");
            AddButton("Select", 65, 340, new Size(60, 60), "Toggle Clock and Elapsed Time Modes");
            AddButton("Control", 291, 340, new Size(60, 60), "Start, Stop and Reset Elapsed Timer");
        }

        public override string DefaultBackgroundImage => _imageLocation + "_Transparent.png";

        protected override void OnBackgroundImageChange()
        {
            _bezel.BackgroundImage = BackgroundImageIsCustomized ? null : System.IO.Path.Combine(_imageLocation, PANEL_IMAGE);
            _glass.BackgroundImage = BackgroundImageIsCustomized ? null : System.IO.Path.Combine(_imageLocation, REFLECTION_IMAGE);
        }

        private void AddClock(string name, double x, double y, Size size, string interfaceDevice, string interfaceElement)
        {
            string componentName = interfaceDevice + "_" + name;
            _digitalClockGauge = new DigitalClockGauge
            {
                Top = y,
                Left = x,
                Height = size.Height,
                Width = size.Width,
                Name = componentName
            };

            Children.Add(_digitalClockGauge);
            foreach (IBindingTrigger trigger in _digitalClockGauge.Triggers)
            {
                AddTrigger(trigger, trigger.Name);
            }
            foreach (IBindingAction action in _digitalClockGauge.Actions)
            {
                if (action.Name != "hidden")
                {
                    AddAction(action, action.Name);
                    // Create the automatic input bindings for the Digital Clock_Gauge sub component
                    AddDefaultInputBinding(
                        childName: "Digital Clock_DigitalClockGauge",
                        //childName: "Gauges",
                        deviceActionName: "set.DigitalClock_Second Hand",
                        interfaceTriggerName: "Digital Clock.Time Seconds.changed"
                        );
                }
            }
        }

        private void AddTextDisplay(string name, double x, double y, Size size, double baseFontsize, string testDisp,
            string interfaceDevice, string interfaceElement)
        {
            TextDisplay display = AddTextDisplay(
                name: name,
                posn: new Point(x, y),
                size: size,
                font: _font,
                baseFontsize: baseFontsize,
                horizontalAlignment: TextHorizontalAlignment.Right,
                verticalAligment: TextVerticalAlignment.Top,
                testTextDisplay: testDisp,
                textColor: _textColor,
                backgroundColor: _backGroundColor,
                useBackground: false,
                interfaceDeviceName: interfaceDevice,
                interfaceElementName: interfaceElement,
                textDisplayDictionary: ".=:"
                );
            //display.TextFormat.FontWeight = FontWeights.Heavy;
        }

        private void AddButton(string name, double x, double y, Size size, string interfaceElement)
        {
            Point pos = new Point(x, y);
            AddButton(
                name: name,
                posn: pos,
                size: size,
                image: _imageLocation + "A-10C_Digital_Clock_Knob_Unpressed.png",
                pushedImage: _imageLocation + "A-10C_Digital_Clock_Knob_Pressed.png",
                buttonText: "",
                interfaceDeviceName: _interfaceDeviceName,
                interfaceElementName: interfaceElement,
                fromCenter: false
                );
        }

        // unclickable
        public override bool HitTest(Point location) => false;
    }
}
