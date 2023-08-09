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
    using System.Windows.Media;

    /// <summary>
    /// This is a version of the UHF Frequency Display which uses a bespoke font to provide data in a text display instead of cutouts for the exported viewport.
    /// </summary>
    /// 
    [HeliosControl("Helios.A10.UHFFrequencyDisplay", "UHF Frequency Display", "A-10C Gauges", typeof(BackgroundImageRenderer), HeliosControlFlags.NotShownInUI)]
    class UHFFreqDisplay : A10CDevice
    {
        public const double GLASS_REFLECTION_OPACITY_DEFAULT = 0.50;
        private string _imageLocation = "{A-10C}/Images/A-10CII/";
        private string _interfaceDeviceName = "UHF_RADIO";
        private string _font = "Helios Virtual Cockpit A-10C_ARC_164";
        private Color _textColor = Color.FromArgb(0xff, 0xff, 0xff, 0xff);
        private Color _backGroundColor = Color.FromArgb(0, 100, 20, 50);
        private readonly HeliosPanel _bezel;
        private const string RADIO_FREQUENCY_BEZEL_PNG = "A-10C_UHF_Radio_Frequency_Bezel.png";

        public UHFFreqDisplay()
            : base("UHFFreqDisplay", new Size(289d, 98d))
        {
            AddTextDisplay("Frequency Display", 72, 16, new Size(200, 68), 36d,
                "A76.543", _interfaceDeviceName, "Frequency Display");
            _bezel = AddPanel("UHF Radio Frequency Bezel", new Point(0, 0), new Size(289d, 98d), _imageLocation + RADIO_FREQUENCY_BEZEL_PNG, "bezel");
        }

        public override string DefaultBackgroundImage => _imageLocation + "A-10C_UHF_Radio_Frequency_Filter.png";
        
        protected override void OnBackgroundImageChange()
        {
            _bezel.BackgroundImage = BackgroundImageIsCustomized ? null : System.IO.Path.Combine(_imageLocation, RADIO_FREQUENCY_BEZEL_PNG);
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
