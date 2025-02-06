﻿//  Copyright 2014 Craig Courtney
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

using System.Windows;
using GadrocsWorkshop.Helios.ComponentModel;

namespace GadrocsWorkshop.Helios.Interfaces.Falcon.Gauges.Textures
{
    [HeliosControl("Helios.Falcon.PFL", "PFL", "Falcon BMS Textures", typeof(FalconTextureDisplayRenderer))]
    public class PFL : FalconTextureDisplay
    {
        private static readonly Rect _defaultRect = new Rect(5, 345, 150, 70);

        public PFL()
            : base("PFL", new Size(160, 63))
        {
        }

        protected override FalconTextureDisplay.FalconTextures Texture
        {
            get { return FalconTextureDisplay.FalconTextures.PFL; }
        }

        internal override string DefaultImage
        {
            get { return "{HeliosFalcon}/Images/Textures/pfl.png"; }
        }

        protected override Rect DefaultRect
        {
            get { return _defaultRect; }
        }
    }
}
