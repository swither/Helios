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

namespace GadrocsWorkshop.Helios.Gauges.A10C
{
    using GadrocsWorkshop.Helios.ComponentModel;
    using GadrocsWorkshop.Helios.Controls;
    using System;
    using System.Windows.Media;
    using System.Windows;
    using System.Windows.Threading;

    /// <summary>
    /// This is the revised version of the Option Display Unit which is larger and uses text displays instead of cutouts for the exported viewport.
    /// It has a slightly different name because the old version is retained to help with backward compatability
    /// </summary>
    /// 
    [HeliosControl("Helios.A10C.CMSC", "Counter Measures System Control", "A-10C Gauges", typeof(A10CDeviceRenderer))]
    class CMSC : A10CDevice
    {
        // these two sections are the dead space in the CMSC image.
        //private Rect _scaledScreenRectTL = new Rect(0, 0, 398, 116);
        //private Rect _scaledScreenRectB = new Rect(76, 384, 648, 87);
        private string _interfaceDeviceName = "CMSC";
        private string _cmscConversion = "";
        private string _imageLocation = "{A-10C}/Images/A-10C/";


        public CMSC()
            : base("Counter Measures System Control", new Size(1476, 520))
        {
            AddPanel("Filters", new Point(0,0), new Size(1476, 520), _imageLocation + "A-10C_CMSC_Filter_Panel.png", _interfaceDeviceName, "Display Filters");
            AddButton("Pri", 860, 321, new Size(96,96), "CMSC Option Select Pushbutton 1");
            AddButton("Sep", 1046, 321, new Size(96, 96), "CMSC Option Select Pushbutton 2");
            AddButton("Unk", 1235, 321, new Size(96, 96), "CMSC Option Select Pushbutton 3");
            AddButton("OSB", "1", 135, 86, new Size(96, 96), "CMSC Option Select Pushbutton 4");
            AddButton("OSB", "2", 135, 245, new Size(96, 96), "CMSC Option Select Pushbutton 5");

            AddPot("CMSC Brightness Control", new Point(261, 375), new Size(110, 110), "CMSC Brightness");
            AddPot("CMSC Audio Control", new Point(503, 375), new Size(110, 110), "CMSC Audio");

            AddTextDisplay("CMSC MWS Text", 279, 244, new Size(471, 113), "MWS Text", "amsx", _cmscConversion);
            AddTextDisplay("CMSC JMR Text", 279, 86, new Size(471, 113), "JMR Text", "1234", _cmscConversion);
            AddTextDisplay("CMSC Chaff Flare Text", 844, 86, new Size(471, 113), "Chaff Flare Text", "1234", _cmscConversion);

            AddIndicator("Missile Launch", "Red", 761, 408, new Size(48,48), "Missile Launch Indicator");
            AddIndicator("RWR 1", "Green", 869, 222, new Size(48, 48), "RWR 1 Indicator");
            AddIndicator("RWR 2", "Green", 1250, 222, new Size(48, 48), "RWR 2 Indicator");
        }


        public override string BezelImage
        {
            get { return _imageLocation + "A-10C_CMSC_Panel.png"; }
        }

        private void AddButton(string name, double x, double y, Size size, string description) { AddButton(name, "", x, y, size, description, false, false); }
        private void AddButton(string name, string buttonVariant, double x, double y, Size size, string description) { AddButton(name, buttonVariant, x, y, size, description, false, false); }
        private void AddButton(string name, string buttonVariant, double x, double y, Size size, string description, bool horizontal, bool altImage)
        {
            Helios.Controls.PushButton button = new Helios.Controls.PushButton();
            button.Top = y;
            button.Left = x;
            button.Width = size.Width;
            button.Height = size.Height;
            button.Image = _imageLocation + "A-10C_CMSC_" + name + "_Pushbutton_Unpressed.png";
            button.PushedImage = _imageLocation + "A -10C_CMSC_" + name + "_Pushbutton_Pressed.png";
            button.Text = "";
            button.Name = "CMSC_" + name + buttonVariant + "_Button";

            Children.Add(button);

            AddTrigger(button.Triggers["pushed"], "CMSC Key " + name + buttonVariant);
            AddTrigger(button.Triggers["released"], "CMSC Key " + name + buttonVariant);

            AddAction(button.Actions["push"], "CMSC Key " + name + buttonVariant);
            AddAction(button.Actions["release"], "CMSC Key " + name + buttonVariant);
            AddAction(button.Actions["set.physical state"], "CMSC Key " + name + buttonVariant);
        }
        private void AddPot(string name, Point posn, Size size, string interfaceElementName)
        {
            AddPot(name: name,
                posn: posn,
                size: size,
                knobImage: _imageLocation + "A-10C_HARS_Heading_Knob.png",
                initialRotation: 0,
                rotationTravel: 360,
                minValue: 0,
                maxValue: 1,
                initialValue: 0,
                stepValue: 0.1,
                interfaceDeviceName: _interfaceDeviceName,
                interfaceElementName: interfaceElementName,
                isContinuous: true,
                fromCenter: false);
        }
        private void AddTextDisplay(string name, double x, double y, Size size,
            string interfaceElementName, string testDisp, string conversionDictionary)
        {
            TextDisplay display = AddTextDisplay(
                name: name,
                posn: new Point(x, y),
                size: size,
                font: "Helios 10C_ALQ_213",
                baseFontsize: 88,
                horizontalAlignment: TextHorizontalAlignment.Left,
                verticalAligment: TextVerticalAlignment.Center,
                testTextDisplay: testDisp,
                textColor: Color.FromArgb(0xd5, 0x9c, 0xcf, 0x08),
                backgroundColor: Color.FromArgb(0xff, 0x00, 0x00, 0x00),
                useBackground: false,
                interfaceDeviceName: _interfaceDeviceName,
                interfaceElementName: interfaceElementName,
                textDisplayDictionary: conversionDictionary
                );
        }
        private void AddIndicator(string name, string colour, double x, double y, Size size, string interfaceElementName) { AddIndicator(name, colour, x, y, size, false, interfaceElementName); }
        private void AddIndicator(string name, string colour, double x, double y, Size size, bool _vertical, string interfaceElementName)
        {
            Indicator indicator = AddIndicator(
                name: name,
                posn: new Point(x, y),
                size: size,
                onImage: _imageLocation + "A-10C_CMSC_Indicator_" + colour + ".png",
                offImage: _imageLocation + "_Transparent.png",
                onTextColor: Color.FromArgb(0x00, 0xff, 0xff, 0xff),
                offTextColor: Color.FromArgb(0x00, 0x00, 0x00, 0x00),               
                font: "",
                vertical: _vertical,
                interfaceDeviceName: _interfaceDeviceName,
                interfaceElementName: interfaceElementName,
                fromCenter: false
                );
            indicator.Text = "";
            indicator.Name = "CMSC_" + name;
        }
        private void AddPanel(string name, Point posn, Size size, string background, string interfaceDevice, string interfaceElement)
        {
            HeliosPanel _panel = AddPanel(
                name: name,
                posn: posn,
                size: size,
                background: background
                );
            _panel.FillBackground = false;
            _panel.DrawBorder = false;
        }

        public override bool HitTest(Point location)
        {
            //if (_scaledScreenRectTL.Contains(location) || _scaledScreenRectB.Contains(location))
            //{
            //    return false;
            //}

            return true;
        }
        public override void MouseDown(Point location)
        {
            // No-Op
        }

        public override void MouseDrag(Point location)
        {
            // No-Op
        }

        public override void MouseUp(Point location)
        {
            // No-Op
        }

    }
}