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
    /// This is the A-10C UHF Repeater Panel which uses text displays instead of cutouts for the exported viewport.
    /// </summary>
    /// 
    [HeliosControl("Helios.A10.UHFFreqRepeater", "UHF Freq Repeater", "A-10C Gauges", typeof(BackgroundImageRenderer), HeliosControlFlags.NotShownInUI)]
    class UHFFreqRepeater : A10CDevice
    {
        public const double GLASS_REFLECTION_OPACITY_DEFAULT = 0.50;
        private string _imageLocation = "{A-10C}/Images/A-10C/";
        private string _interfaceDeviceName = "UHF_RADIO";
        private HeliosPanel _glass;
        private String _font = "Helios Virtual Cockpit A-10C_ARC_164";
        private Color _textColor = Color.FromArgb(0xff, 0xff, 0xff, 0xff);
        private Color _backGroundColor = Color.FromArgb(0, 100, 20, 50);
        private readonly HeliosPanel _bezel;
        private const string PANEL_IMAGE = "A-10C_UHF_Repeater_Panel.png";

        public UHFFreqRepeater()
            : base("UHFFreqRepeater", new Size(387d, 374d))
        {
            AddTextDisplay("UHF Repeater Display", 88, 142, new Size(224, 62), 52d,
                "888.888", _interfaceDeviceName, "Repeater Display");
            _glass = AddPanel("UHF Reflection", new Point(0,0), new Size(387d, 374d), _imageLocation + "Pilot_Reflection_25.png", "reflection");
            _glass.Opacity = GLASS_REFLECTION_OPACITY_DEFAULT;
            _bezel = AddPanel("UHF Repeater Bezel", new Point(0, 0), new Size(387d, 374d), _imageLocation + PANEL_IMAGE, "bezel");
        }

        public override string DefaultBackgroundImage => _imageLocation + "A-10C_UHF_Repeater_Filter_Panel.png";

        protected override void OnBackgroundImageChange()
        {
            _bezel.BackgroundImage = BackgroundImageIsCustomized
                ? null
                : System.IO.Path.Combine(_imageLocation, PANEL_IMAGE);
        }

        private void AddTextDisplay(string name, double x, double y, Size size, double baseFontsize, string testDisp,
            string interfaceDevice, string interfaceElement)
        {
            AddTextDisplay(
                name: name,
                posn: new Point(x, y),
                size: size,
                font: _font,
                baseFontsize: baseFontsize,
                horizontalAlignment: TextHorizontalAlignment.Center,
                verticalAligment: TextVerticalAlignment.Top,
                testTextDisplay: testDisp,
                textColor: _textColor,
                backgroundColor: _backGroundColor,
                useBackground: false,
                interfaceDeviceName: interfaceDevice,
                interfaceElementName: interfaceElement,
                textDisplayDictionary: ""
                );
            //display.TextFormat.FontWeight = FontWeights.Heavy;
        }

        // unclickable
        public override bool HitTest(Point location) => false;
    }
}
