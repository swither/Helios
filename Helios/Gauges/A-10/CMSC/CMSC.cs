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
        private string _cmscConversion = "";  // One character (diamond which might need this, but so far not seen this character being sent)
        private string _imageLocation = "{A-10C}/Images/A-10C/";


        public CMSC()
            : base("CMSC", new Size(1476, 520))
        {
            AddPanel("Filters", new Point(0,0), new Size(1476, 520), _imageLocation + "A-10C_CMSC_Filter_Panel.png", _interfaceDeviceName, "Display Filters");
            AddRWRButton("Pri Button", 860, 321, new Size(96,96), "Priority Button");
            AddRWRButton("Sep Button", 1046, 321, new Size(96, 96), "Separate Button");
            AddRWRButton("Unk Button", 1235, 321, new Size(96, 96), "Unknown Button");
            AddOSBButton("JMR Option Button", 135, 86, new Size(96, 96), "Cycle JMR Program Button");
            AddOSBButton("MWS Option Button", 135, 245, new Size(96, 96), "Cycle MWS Program Button");

            AddPot("Brightness Control", new Point(261, 375), new Size(110, 110), "Brightness");
            AddPot("RWR Volume Control", new Point(503, 375), new Size(110, 110), "RWR Volume");

            AddTextDisplay("MWS Text", 279, 258, new Size(471, 113), "MWS Display", "amsxIJKL", _cmscConversion);
            AddTextDisplay("JMR Text", 279, 100, new Size(471, 113), "JMR Display", "ABCDEFGH", _cmscConversion);
            AddTextDisplay("Chaff Text", 844, 100, new Size(471, 113), "Chaff Flare Display", "12345678", _cmscConversion);

            AddIndicator("Indicator: Missile Launch", "Red", 761, 408, new Size(48,48), "Missle Launch Indicator");
            AddIndicator("Indicator: Priority", "Green", 869, 222, new Size(48, 48), "Priority Status Indicator");
            AddIndicator("Indicator: Unknown", "Green", 1250, 222, new Size(48, 48), "Unknown Status Indicator");
        }

        public override string BezelImage
        {
            get { return _imageLocation + "A-10C_CMSC_Panel.png"; }
        }

        private void AddOSBButton(string name, double x, double y, Size size, string interfaceElement)
        {
            AddButton(
                name: name,
                posn: new Point(x, y),
                size: size,
                image: _imageLocation + "A-10C_CMSC_OSB_Pushbutton_Unpressed.png",
                pushedImage: _imageLocation + "A-10C_CMSC_OSB_Pushbutton_Unpressed.png",
                buttonText: "",
                interfaceDeviceName: _interfaceDeviceName,
                interfaceElementName: interfaceElement,
                fromCenter: false
               );
        }

        private void AddRWRButton(string name, double x, double y, Size size, string interfaceElement)
        {
            AddButton(
                name: name,
                posn: new Point(x, y),
                size: size,
                image: _imageLocation + "A-10C_CMSC_" + name.Substring(0, 3) + "_Pushbutton_Unpressed.png",
                pushedImage: _imageLocation + "A-10C_CMSC_" + name.Substring(0, 3) + "_Pushbutton_Pressed.png",
                buttonText: "",
                interfaceDeviceName: _interfaceDeviceName,
                interfaceElementName: interfaceElement,
                fromCenter: false
               );
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
                font: "Helios Virtual Cockpit A-10C_ALQ_213",
                baseFontsize: 76,
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