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
    /// This is a version of the UHF Channel Display which uses a bespoke font to provide data in a text display instead of cutouts for the exported viewport.
    /// </summary>
    /// 

    [HeliosControl("Helios.A10.UHFChannelDisplay", "UHF Channel Display", "A-10C Gauges", typeof(BackgroundImageRenderer), HeliosControlFlags.NotShownInUI)]
    class UHFChanDisplay : A10CDevice
    {

        public const double GLASS_REFLECTION_OPACITY_DEFAULT = 0.50;
        private string _imageLocation = "{A-10C}/Images/A-10CII/";
        private string _interfaceDeviceName = "UHF_RADIO";
        private String _font = "Helios Virtual Cockpit A-10C_ARC_164";
        private Color _textColor = Color.FromArgb(0xff, 0xff, 0xff, 0xff);
        private Color _backGroundColor = Color.FromArgb(0, 100, 20, 50);
        private HeliosPanel _bezel;
        private const string RADIO_CHANNEL_BEZEL_PNG = "A-10C_UHF_Radio_Channel_Bezel.png";

        public UHFChanDisplay()
            : base("UHFChanDisplay", new Size(173d, 100d))
        {
            AddTextDisplay("Channel Display", 74, 18, new Size(70, 69), 36d,
                "23", _interfaceDeviceName, "Channel Display");
            _bezel = AddPanel("UHF Radio Channel Bezel", new Point(0, 0), new Size(173d, 100d), _imageLocation + RADIO_CHANNEL_BEZEL_PNG, "bezel");
        }

        protected override void OnBackgroundImageChange()
        {
            _bezel.BackgroundImage = BackgroundImageIsCustomized ? null : System.IO.Path.Combine(_imageLocation, RADIO_CHANNEL_BEZEL_PNG);
        }

        public override string DefaultBackgroundImage => _imageLocation + "A-10C_UHF_Radio_Channel_Filter.png";

        private void AddTextDisplay(string name, double x, double y, Size size, double baseFontsize, string testDisp,
            string interfaceDevice, string interfaceElement)
        {
            TextDisplay display = AddTextDisplay(
                name: name,
                posn: new Point(x, y),
                size: size,
                font: _font,
                baseFontsize: baseFontsize,
                horizontalAlignment: TextHorizontalAlignment.Left,
                verticalAligment: TextVerticalAlignment.Center,
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
