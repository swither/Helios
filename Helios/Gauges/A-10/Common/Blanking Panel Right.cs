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
    [HeliosControl("Helios.A10C.MISC.Panel.Right", "Blanking Panel Right", "A-10C Gauges", typeof(BackgroundImageRenderer), HeliosControlFlags.NotShownInUI)]
    class RightBlank : A10CDevice
    {
        // these three sections are the dead space in the HARS image.

        private string _imageLocation = "{A-10C}/Images/A-10CII/";
 
        public RightBlank()
            : base("Blanking Panel Right", new Size(798, 390))
        {
        }

        public override string DefaultBackgroundImage
        {
            get { return _imageLocation + "A-10CII_Right_Blank_Panel.png"; }
        }

        public override bool HitTest(Point location)
        {
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