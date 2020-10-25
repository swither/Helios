//  Copyright 2014 Craig Courtney
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
    using GadrocsWorkshop.Helios.ComponentModel;
    using GadrocsWorkshop.Helios.Controls;
    using System.Windows;

    /// <summary>
    /// This is the revised version of the A-10C HARS panel which is designed for the A-10C II
    /// however it is exactly the same unit that exists in the older A-10C but moved rear-ward a little.
    /// </summary>
    /// 
    [HeliosControl("Helios.A10C.HARS", "HARS Panel", "A-10C Gauges", typeof(BackgroundImageRenderer), HeliosControlFlags.NotShownInUI)]
    class HARS_Panel : A10CDevice
    {
        // these two sections are the dead space in the HARS image.
        //private Rect _scaledScreenRectTL = new Rect(0, 0, 398, 116);
        //private Rect _scaledScreenRectB = new Rect(76, 384, 648, 87);
        private string _interfaceDeviceName = "HARS";
        private string _imageLocation = "{A-10C}/Images/A-10C/";
        private readonly HeliosPanel _bezel;
        const string HARS_PANEL_PNG = "A-10C_HARS_Panel.png";

        public HARS_Panel()
            : base("HARS", new Size(798, 306))
        {
            AddGauge("HARS_Sync Offset", new A10C.HARS.HARSSync(),new Point(230,24),new Size(137,79),_interfaceDeviceName, "SYN-IND Sync Needle");
            AddPanel("Scale Reflection", new Point(230, 24), new Size(137, 91), _imageLocation + "crystal_small.png", _interfaceDeviceName, "HARS Scale Reflection");
            _bezel = AddPanel("HARS Bezel", new Point(0,0), new Size(798, 306), _imageLocation + HARS_PANEL_PNG, _interfaceDeviceName, "HARS Bezel");
            AddToggleSwitch(
                    name: "Hemisphere Selector",
                    posn: new Point(500, 213),
                    size: new Size(32, 66),
                    defaultPosition: ToggleSwitchPosition.One,
                    defaultType: ToggleSwitchType.OnOn,
                    positionOneImage: _imageLocation + "A-10C_HARS_Small_Toggle_Up.png",
                    positionTwoImage: _imageLocation + "A-10C_HARS_Small_Toggle_Down.png",
                    interfaceDeviceName: _interfaceDeviceName,
                    interfaceElementName: "Hemisphere Selector",
                    clickType: LinearClickType.Swipe,
                    fromCenter: false
                    );
            
            AddToggleSwitch(
                    name: "Mode Switch",
                    posn: new Point(487, 37),
                    size: new Size(66, 122),
                    defaultPosition: ToggleSwitchPosition.One,
                    defaultType: ToggleSwitchType.OnOn,
                    positionOneImage: _imageLocation + "A-10C_HARS_Slave_Toggle_Up.png",
                    positionTwoImage: _imageLocation + "A-10C_HARS_Slave_Toggle_Down.png",
                    interfaceDeviceName: _interfaceDeviceName,
                    interfaceElementName: "Mode",
                    clickType: LinearClickType.Swipe,
                    fromCenter: false
                    );


            AddThreeWayToggle(
                    name: "MagVar Switch",
                    posn: new Point(353, 213),
                    size: new Size(32, 66),
                    defaultPosition: ThreeWayToggleSwitchPosition.Two,
                    defaultType: ThreeWayToggleSwitchType.OnOnOn,
                    positionOneImage: _imageLocation + "A-10C_HARS_Small_Toggle_Up.png",
                    positionTwoImage: _imageLocation + "A-10C_HARS_Small_Toggle_Middle.png",
                    positionThreeImage: _imageLocation + "A-10C_HARS_Small_Toggle_Down.png",
                    interfaceDeviceName: _interfaceDeviceName,
                    interfaceElementName: "Magnetic Variation",
                    clickType: LinearClickType.Swipe,
                    fromCenter: false
                    );
            
            AddPot(name: "Latitude Correction Dial",
                posn: new Point(562, 63),
                size: new Size(178, 178),
                knobImage: _imageLocation + "A-10C_HARS_Knob.png",
                initialRotation: 225,
                rotationTravel: 270,
                minValue: 0,
                maxValue: 1,
                initialValue: 0,
                stepValue: 0.1,
                interfaceDeviceName: _interfaceDeviceName,
                interfaceElementName: "Latitude Correction",
                isContinuous: true,
                fromCenter: false);

            AddPot(name: "Sync Knob",
                posn: new Point(121, 171),
                size: new Size(100, 100),
                knobImage: _imageLocation + "A-10C_HARS_Heading_Knob.png",
                initialRotation: 225,
                rotationTravel: 270,
                minValue: 0,
                maxValue: 1,
                initialValue: 0,
                stepValue: 0.1,
                interfaceDeviceName: _interfaceDeviceName,
                interfaceElementName: "Sync Button Rotate",
                isContinuous: false,
                fromCenter: false);

            AddButton(
                name: "Sync Button",
                posn: new Point(142, 191),
                size: new Size(48, 48),
                image: _imageLocation + "_Transparent.png",
                pushedImage: _imageLocation + "_Transparent.png",
                buttonText: "",
                interfaceDeviceName: _interfaceDeviceName,
                interfaceElementName: "Sync Button Push",
                fromCenter: false
                );
        }

        protected override void OnBackgroundImageChange()
        {
            _bezel.BackgroundImage = BackgroundImageIsCustomized ? null : System.IO.Path.Combine(_imageLocation, HARS_PANEL_PNG);
        }

        public override string DefaultBackgroundImage => _imageLocation + "_Transparent.png";
    }
}