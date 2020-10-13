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
    [HeliosControl("Helios.A10C.CMSP", "Counter Measures System Panel", "A-10C Gauges", typeof(A10CDeviceRenderer))]
    class CMSP : A10CDevice
    {
        // these two sections are the dead space in the CMSP image.
        //private Rect _scaledScreenRectTL = new Rect(0, 0, 398, 116);
        //private Rect _scaledScreenRectB = new Rect(76, 384, 648, 87);
        private string _interfaceDeviceName = "CMSP";
        private string _CMSPConversion = "";  // One character (diamond which might need this, but so far not seen this character being sent)
        private string _imageLocation = "{A-10C}/Images/A-10C/";
        private RotarySwitchPositionCollection _positions = new RotarySwitchPositionCollection();

        public CMSP()
            : base("CMSP", new Size(666, 404))
        {
            AddPanel("Filters", new Point(0,0), new Size(666, 404), _imageLocation + "A-10C_CMSP_Filter_Panel.png", _interfaceDeviceName, "Display Filters");
            AddOSBButton("OSB 1", 86, 168, new Size(64, 64), "OSB 1");
            AddOSBButton("OSB 2", 175, 168, new Size(64, 64), "OSB 2");
            AddOSBButton("OSB 3", 275, 168, new Size(64, 64), "OSB 3");
            AddOSBButton("OSB 4", 367, 168, new Size(64, 64), "OSB 4");
            AddRTNButton("RTN", 546, 50, new Size(68,67), "Return to Previous Rotary Menu");

            AddThreeWayToggle("Page Cycle", "Pushbutton", ThreeWayToggleSwitchType.MomOnMom, 492, 38, new Size(51, 97), "Page Cycle","NXT");
            AddThreeWayToggle("MWS Switch", 93, 262, new Size(41, 139), "MWS");
            AddThreeWayToggle("JMR Switch", 186, 262, new Size(41, 139), "JMR");
            AddThreeWayToggle("RWR Switch", 278, 262, new Size(41, 139), "RWR");
            AddThreeWayToggle("Disp Switch", 374, 262, new Size(41, 139), "DISP");
            AddTwoWayToggle("ECM Pod Jettison", 471, 139, new Size(53,128), "ECM Pod Jettison");
            AddPot("Brightness", new Point(550,176), new Size(60, 60), "Brightness");
            AddTextDisplay("Display Line 1 Text", 73, 35, new Size(404, 50), "Line 1 Display", "a1B2C3D4m5s6x7O8", _CMSPConversion);

            AddTextDisplay("Display MWS Text", 73, 87, new Size(101, 50), "Line 2 MWS Display", "amsx", _CMSPConversion);
            AddTextDisplay("Display JMR Text", 174, 87, new Size(101, 50), "Line 2 JMR Display", "amsx", _CMSPConversion);
            AddTextDisplay("Display RWR Text", 275, 87, new Size(101, 50), "Line 2 RWR Display", "amsx", _CMSPConversion);
            AddTextDisplay("Display Disp Text", 376, 87, new Size(101, 50), "Line 2 DISP Display", "amsx", _CMSPConversion);
            _positions.Add(new RotarySwitchPosition(this, 1, "OFF", 225d));
            _positions.Add(new RotarySwitchPosition(this, 2, "STBY", 270d));
            _positions.Add(new RotarySwitchPosition(this, 3, "MAN", 315d));
            _positions.Add(new RotarySwitchPosition(this, 4, "SEMI", 0d));
            _positions.Add(new RotarySwitchPosition(this, 5, "AUTO", 45d));
            AddRotarySwitch("Mode Dial", new Point(478, 269), new Size(125, 125), _imageLocation + "A-10C_CMSP_Knob.png", 1, _positions, _interfaceDeviceName, "Mode Select Dial", false);
        }


        public override string BezelImage
        {
            get { return _imageLocation + "A-10C_CMSP_Panel.png"; }
        }

        private void AddOSBButton(string name, double x, double y, Size size, string interfaceElement)
        {
            AddButton(
                name: name,
                posn: new Point(x, y),
                size: size,
                image: _imageLocation + "A-10C_CMSP_OSB_Pushbutton_Unpressed.png",
                pushedImage: _imageLocation + "A-10C_CMSP_OSB_Pushbutton_Pressed.png",
                buttonText: "",
                interfaceDeviceName: _interfaceDeviceName,
                interfaceElementName: interfaceElement,
                fromCenter: false
               );
        }
        private void AddRTNButton(string name, double x, double y, Size size, string interfaceElement)
        {
            AddButton(
                name: name,
                posn: new Point(x, y),
                size: size,
                image: _imageLocation + "A-10C_CMSP_RTN_Pushbutton_Unpressed.png",
                pushedImage: _imageLocation + "A-10C_CMSP_RTN_Pushbutton_Pressed.png",
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
                font: "A-10C_ALQ_213",
                baseFontsize: 29,
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
        private void AddTwoWayToggle(string name, double x, double y, Size size, string interfaceElementName)
        {
            ToggleSwitch toggle = AddToggleSwitch(
                name: name,
                posn: new Point(x, y),
                size: size,
                defaultPosition: ToggleSwitchPosition.One,
                defaultType: ToggleSwitchType.OnOn,
                positionOneImage: _imageLocation + "A-10C_CMSP_Jettison_Toggle_Up.png",
                positionTwoImage: _imageLocation + "A-10C_CMSP_Jettison_Toggle_Down.png",
                interfaceDeviceName: _interfaceDeviceName,
                interfaceElementName: interfaceElementName,
                clickType: LinearClickType.Swipe,
                fromCenter: false
                );
            toggle.Name = "CMSP_" + name;
        }
        private void AddThreeWayToggle(string name, double x, double y, Size size, string interfaceElementName) { AddThreeWayToggle(name, "Toggle", ThreeWayToggleSwitchType.MomOnOn, x, y, size, interfaceElementName,name); }
        private void AddThreeWayToggle(string name, string switchTypeName, ThreeWayToggleSwitchType toggleType, double x, double y, Size size, string interfaceElementName, string imageName)
        {
            string swName = "";
            if (switchTypeName == "Toggle") {
                swName = switchTypeName;
            } else
            {
                if(imageName != name)
                {
                    swName = imageName + "_" + switchTypeName;
                }
                else
                {
                    swName = name + "_" + switchTypeName;
                }
            }
            ThreeWayToggleSwitch toggle = AddThreeWayToggle(
                name: name,
                posn: new Point(x, y),
                size: size,
                defaultPosition: ThreeWayToggleSwitchPosition.Two,
                defaultType: toggleType,
                positionOneImage: _imageLocation + "A-10C_CMSP_"+ swName + "_Up.png",
                positionTwoImage: _imageLocation + "A-10C_CMSP_" + swName + "_Middle.png",
                positionThreeImage: _imageLocation + "A-10C_CMSP_" + swName + "_Down.png",
                interfaceDeviceName: _interfaceDeviceName,
                interfaceElementName: interfaceElementName,
                fromCenter: false
                );
            toggle.Name = "CMSP_" + name;
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