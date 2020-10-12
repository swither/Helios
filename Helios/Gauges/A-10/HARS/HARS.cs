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
    [HeliosControl("Helios.A10C.HARS", "HARS Panel", "A-10C Gauges", typeof(A10CDeviceRenderer))]
    class HARS : A10CDevice
    {
        // these two sections are the dead space in the HARS image.
        //private Rect _scaledScreenRectTL = new Rect(0, 0, 398, 116);
        //private Rect _scaledScreenRectB = new Rect(76, 384, 648, 87);
        private string _interfaceDeviceName = "HARS";
        private string _imageLocation = "{A-10C}/Images/A-10C/";
 
        public HARS()
            : base("HARS Panel", new Size(798, 306))
        {
            AddPanel("Scale", new Point(230, 24), new Size(137, 79), _imageLocation + "A-10C_HARS_Scale_Panel.png", _interfaceDeviceName, "HARS Sync Scale");
            AddPanel("Scale Reflection", new Point(230, 24), new Size(137, 91), _imageLocation + "crystal_small.png", _interfaceDeviceName, "HARS Scale Reflection");
            AddPanel("HARS Bezel", new Point(0,0), new Size(798, 306), _imageLocation + "A-10C_HARS_Panel.png", _interfaceDeviceName, "HARS Scale Reflection");

            //AddThreeWayToggle("MWS", 93, 262, new Size(41, 139), "MWS Switch");
            //AddTwoWayToggle("Jettison", 471, 139, new Size(53, 128), "Jettison Switch");
            //AddTwoWayToggle("Jettison", 471, 139, new Size(53, 128), "Jettison Switch");
            //AddPot("HARS Brightness", new Point(550, 176), new Size(60, 60), "HARS Brightness");
            //AddPot("Heading Knob", new Point(550, 176), new Size(60, 60), "Heading");
            //AddButton("Heading Button", "1", 86, 168, new Size(64, 64), "HARS Heading Pushbutton");
        }


        public override string BezelImage
        {
            get { return _imageLocation + "_Transparent.png"; }
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
            button.ButtonType = PushButtonType.Momentary;
            button.Image = _imageLocation + "A-10C_HARS_" + name + "_Pushbutton_Unpressed.png";
            button.PushedImage = _imageLocation + "A -10C_HARS_" + name + "_Pushbutton_Pressed.png";
            button.Text = "";
            button.Name = "HARS_" + name + buttonVariant + "_Button";

            Children.Add(button);

            AddTrigger(button.Triggers["pushed"], "HARS Key " + name + buttonVariant);
            AddTrigger(button.Triggers["released"], "HARS Key " + name + buttonVariant);

            AddAction(button.Actions["push"], "HARS Key " + name + buttonVariant);
            AddAction(button.Actions["release"], "HARS Key " + name + buttonVariant);
            AddAction(button.Actions["set.physical state"], "HARS Key " + name + buttonVariant);
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
        private void AddTwoWayToggle(string name, double x, double y, Size size, string interfaceElementName)
        {
            ToggleSwitch toggle = AddToggleSwitch(
                name: name,
                posn: new Point(x, y),
                size: size,
                defaultPosition: ToggleSwitchPosition.One,
                defaultType: ToggleSwitchType.OnOn,
                positionOneImage: _imageLocation + "A-10C_HARS_" + name + "_Toggle_Up.png",
                positionTwoImage: _imageLocation + "A-10C_HARS_" + name + "_Toggle_Down.png",
                interfaceDeviceName: _interfaceDeviceName,
                interfaceElementName: interfaceElementName,
                clickType: LinearClickType.Swipe,
                fromCenter: false
                );
            toggle.Name = "HARS_" + name;
        }
        private void AddThreeWayToggle(string name, double x, double y, Size size, string interfaceElementName) { AddThreeWayToggle(name, "Toggle", ThreeWayToggleSwitchType.MomOnOn, x, y, size, interfaceElementName); }
        private void AddThreeWayToggle(string name, string switchTypeName, ThreeWayToggleSwitchType toggleType, double x, double y, Size size, string interfaceElementName)
        {
            string swName = "";
            if (switchTypeName == "Toggle") {
                swName = switchTypeName;
            } else
            {
                swName = name + "_" + switchTypeName;
            }
            ThreeWayToggleSwitch toggle = AddThreeWayToggle(
                name: name,
                posn: new Point(x, y),
                size: size,
                defaultPosition: ThreeWayToggleSwitchPosition.Two,
                defaultType: toggleType,
                positionOneImage: _imageLocation + "A-10C_HARS_"+ swName + "_Up.png",
                positionTwoImage: _imageLocation + "A-10C_HARS_" + swName + "_Middle.png",
                positionThreeImage: _imageLocation + "A-10C_HARS_" + swName + "_Down.png",
                interfaceDeviceName: _interfaceDeviceName,
                interfaceElementName: interfaceElementName,
                fromCenter: false
                );
            toggle.Name = "HARS_" + name;
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