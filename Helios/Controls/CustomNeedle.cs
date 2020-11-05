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

namespace GadrocsWorkshop.Helios.Controls
{
    using System.Windows;

    public abstract class CustomNeedle : HeliosVisual
    {
        private string _knobImage;
        protected double _knobRotation;

        protected CustomNeedle(string name, Size defaultSize)
            : base(name, defaultSize)
        {
            // no code
        }

        #region Properties

        public string KnobImage
        {
            get => _knobImage;
            set
            {
                if ((_knobImage == null && value != null)
                    || (_knobImage != null && !_knobImage.Equals(value)))
                {
                    string oldValue = _knobImage;
                    _knobImage = value;
                    OnPropertyChanged("KnobImage", oldValue, value, true);
                    Refresh();
                }
            }
        }

        public double KnobRotation => _knobRotation;

        #endregion

        public override void MouseDown(Point location)
        {
           
        }

        public override void MouseDrag(Point location)
        {

        }

        public override void MouseUp(Point location)
        {
         
        }   
    }
}
