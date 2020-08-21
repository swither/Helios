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


using System.Windows;
using System.Windows.Media;

namespace GadrocsWorkshop.Helios.Controls
{
    public class CustomGaugeRenderer : HeliosVisualRenderer
    {
        private ImageSource _image, _backgroundImage;
        private Rect _imageRect, _backgroundRect;
        private Point _center, _nextToCenter;
        private double _rotation;
        private readonly Brush _scopeBrush = new SolidColorBrush(Color.FromRgb(255, 0, 0));
        private readonly Pen _scopePen;
        private CustomGauge _gauge;

        public CustomGaugeRenderer()
        {
            _scopePen = new Pen(_scopeBrush, 1d);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            if (_gauge != null)
            {
                // update for this frame
                _rotation = _gauge.KnobRotation;
            }

            if (_backgroundImage != null)
            {
                drawingContext.DrawImage(_backgroundImage, _backgroundRect);
            }

            // ReSharper disable once CompareOfFloatsByEqualityOperator checking for initialized
            if (_rotation != 0d)
            {
                drawingContext.PushTransform(new RotateTransform(_rotation, _center.X, _center.Y));
            }

            if (_image != null)
            {
                drawingContext.DrawImage(_image, _imageRect);
                drawingContext.DrawLine(_scopePen, _center, _nextToCenter); //draw rotation point for reference
            }

            // ReSharper disable once CompareOfFloatsByEqualityOperator checking for initialized
            if (_rotation != 0d)
            {
                drawingContext.Pop();
            }
        }

        protected override void OnRefresh()
        {
            _gauge = Visual as CustomGauge;
            if (_gauge == null)
            {
                _rotation = 0d;
                _image = null;
                _backgroundImage = null;
                return;
            }

            _imageRect.X = _gauge.Width * _gauge.Needle_PosX;
            _imageRect.Y = _gauge.Height * _gauge.Needle_PosY;
            _image = ConfigManager.ImageManager.LoadImage(_gauge.KnobImage) ?? ConfigManager.ImageManager.LoadImage("{Helios}/Images/General/missing_image.png");

            // WARNING: needle scale applied to image but not rotation point
            _imageRect.Height = _gauge.Height * _gauge.Needle_Scale;
            _imageRect.Width = _image.Width * (_imageRect.Height / _image.Height); // uniform image based on Height

            // calculate rotation point
            _center = new Point(_gauge.Width * _gauge.Needle_PivotX,
                _gauge.Height * _gauge.Needle_PivotY); 
            _nextToCenter = new Point((_gauge.Width * _gauge.Needle_PivotX) + 1,
                _gauge.Height * _gauge.Needle_PivotY);

            // optional background plate image
            if (!string.IsNullOrWhiteSpace(_gauge.BGPlateImage))
            {
                _backgroundRect.Width = _gauge.Width;
                _backgroundRect.Height = _gauge.Height;
                _backgroundImage = ConfigManager.ImageManager.LoadImage(_gauge.BGPlateImage);
            }
            else
            {
                _backgroundImage = null;
            }
        }
    }
}