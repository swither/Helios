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

using GadrocsWorkshop.Helios.Interfaces.DCS.A10C;

namespace GadrocsWorkshop.Helios.Gauges.A10C
{
    using GadrocsWorkshop.Helios.ComponentModel;
    using GadrocsWorkshop.Helios.Controls;
    using System;
    using System.Windows.Media;
    using System.Windows;
    using System.Windows.Threading;

    /// <summary>
    /// This is the a panel for the A-10CII Scorpion HMCS system
    /// </summary>
    /// 
    [HeliosControl("Helios.A10C.HMCS", "Scorpion HMCS Panel", "A-10C Gauges", typeof(BackgroundImageRenderer), HeliosControlFlags.NotShownInUI)]
    class HMCS : A10CDevice
    {
        // these two sections are the dead space in the HMCS image.
        //private Rect _scaledScreenRectTL = new Rect(0, 0, 398, 116);
        //private Rect _scaledScreenRectB = new Rect(76, 384, 648, 87);
        private string _interfaceDeviceName = "Scorpion HMCS";
        private string _imageLocation = "{A-10C}/Images/A-10CII/";
        private RotarySwitchPositionCollection _positions = new RotarySwitchPositionCollection();

        public HMCS()
            : base("Scorpion HMCS", new Size(798, 355))
        {
            SupportedInterfaces = new[] { typeof(A10C2Interface) };
            AddThreeWayToggle("Power",new Point(640, 110), new Size(62, 131),
                ThreeWayToggleSwitchPosition.Two, ThreeWayToggleSwitchType.OnOnOn,
                _interfaceDeviceName,"Power",
                false,
                _imageLocation + "A-10CII_HMCS_Toggle_Up.png",
                _imageLocation + "A-10CII_HMCS_Toggle_Middle.png",
                _imageLocation + "A-10CII_HMCS_Toggle_Down.png",
                LinearClickType.Swipe);
        }

        public override string DefaultBackgroundImage
        {
            get { return _imageLocation + "A-10CII_HMCS_Panel.png"; }
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