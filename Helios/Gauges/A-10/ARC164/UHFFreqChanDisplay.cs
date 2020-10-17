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
    using System.Windows;
    using System.Windows.Media;
    using System.Xml;
    using System.Globalization;

    [HeliosControl("Helios.A10.UHFFreqDisplay", "UHF Frequency & Channel Display", "A-10C Gauges", typeof(A10CDeviceRenderer))]
    class UHFFreqChanDisplay : A10CDevice
    {

        public const double GLASS_REFLECTION_OPACITY_DEFAULT = 0.50;
        private string _imageLocation = "{A-10C}/Images/A-10CII/";
        private string _interfaceDeviceName = "UHF_RADIO";
        private HeliosPanel _glass;
        private String _font = "Helios Virtual Cockpit A-10C_ARC_164";
        private Color _textColor = Color.FromArgb(0xff, 0xff, 0xff, 0xff);
        private Color _backGroundColor = Color.FromArgb(0, 100, 20, 50);


        public UHFFreqChanDisplay()
            : base("UHFFreqChanDisplay", new Size(311d, 202d))
        {
            AddTextDisplay("Frequency Display", 72, 120, new Size(166, 68), 36d,
                "A76.543", _interfaceDeviceName, "Frequency Display");
            AddTextDisplay("Channel Display", 210, 18, new Size(70, 69), 36d,
                "23", _interfaceDeviceName, "Channel Display");
            //_glass = AddPanel("UHF Reflection", new Point(0,0), new Size(311d, 202d), _imageLocation + "Pilot_Reflection_25.png","relection");
            //_glass.Opacity = GLASS_REFLECTION_OPACITY_DEFAULT;
            HeliosPanel bezel = AddPanel("UHF Radio Bezel", new Point(0, 0), new Size(311d, 202d), _imageLocation + "A-10C_UHF_Radio_Displays.png", "bezel");
        }

        #region Properties
        #endregion 



        protected override void OnProfileChanged(HeliosProfile oldProfile)
        {
            base.OnProfileChanged(oldProfile);
        }

        public override string BezelImage
        {
            get { return _imageLocation + "A-10C_UHF_Radio_Filters.png"; }
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

        private HeliosPanel AddPanel(string name, Point posn, Size size, string background, string interfaceElement)
        {
            HeliosPanel _panel = AddPanel(
                name: name,
                posn: posn,
                size: size,
                background: background
                );
            _panel.FillBackground = false;
            _panel.DrawBorder = false;
            return _panel;
        }

        public override bool HitTest(Point location)
        {
            //if (_scaledScreenRect.Contains(location))
            //{
                return false;
            //}

            //return true;
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
