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

using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Drawing.Imaging;


namespace GadrocsWorkshop.Helios.Controls.Special
{
    public class ScreenReplicatorAdjustableRenderer : ScreenReplicatorRenderer
    {
        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        private ImageAdjustment _imageAdjustment = null;

        public ScreenReplicatorAdjustableRenderer() : base()
        {

        }
        protected override void OnRender(DrawingContext drawingContext)
        { if(Visual is ScreenReplicatorAdjustable replicator)
            {
                _imageAdjustment = replicator.ImageAdjustment;
            }
        base.OnRender(drawingContext); 
        }
        protected override ImageBrush CreateImageBrush(ScreenReplicator replicator)
        {
            BitmapSource bmp;
            using (Bitmap image = new Bitmap(replicator.CaptureRectangle.Width, replicator.CaptureRectangle.Height))
            {
                using (Graphics g = Graphics.FromImage(image))
                {
                    g.CopyFromScreen(replicator.CaptureRectangle.X, replicator.CaptureRectangle.Y, 0, 0, new System.Drawing.Size(image.Width, image.Height));
                    if (_imageAdjustment != null)
                    {
                        g.DrawImage(image, new Rectangle(0, 0, image.Width, image.Height)
                            , 0, 0, image.Width, image.Height,
                            GraphicsUnit.Pixel, _imageAdjustment.ImageAttributes);
                    }
                }
                IntPtr hBitmap = image.GetHbitmap();
                try
                {
                    bmp = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromWidthAndHeight(image.Width, image.Height));
                }
                finally
                {
                    DeleteObject(hBitmap);
                }
                return new ImageBrush(bmp);
            }
        }
        public ImageAdjustment ImageAdjustment
        {
            get => _imageAdjustment;
            set
            {
                _imageAdjustment = _imageAdjustment ?? new ImageAdjustment();
                _imageAdjustment = value;
            }
        }
    }
        public class ImageAdjustment
        {
            private ImageAttributes _imageAttributes = new ImageAttributes();
            private float _redBrightness = 1.0f;
            private float _greenBrightness = 1.0f;
            private float _blueBrightness = 1.0f;
            private float _alphaBrightness = 1.0f;
            private float _brightness = 1.0f;
            private float _contrast = 1.0f;
            private float _gamma = 1.0f;

            public ImageAdjustment()
            {
            MakeImageAdjustments();
            }
            public float Brightness
            {
                get => _brightness;
                set
                {
                    float oldValue = _brightness;
                    if (value != oldValue)
                    {
                        _brightness = _redBrightness = _greenBrightness = _blueBrightness = value;
                        MakeImageAdjustments();
                    }
                }
            }
            public float RedBrightness
            {
                get => _redBrightness;
                set
                {
                    float oldValue = _redBrightness;
                    if (value != oldValue)
                    {
                        _redBrightness = value;
                        MakeImageAdjustments();
                    }
                }
            }
            public float GreenBrightness
            {
                get => _greenBrightness;
                set
                {
                    float oldValue = _greenBrightness;
                    if (value != oldValue)
                    {
                        _redBrightness = value;
                        MakeImageAdjustments();
                    }
                }
            }
            public float BlueBrightness
            {
                get => _blueBrightness;
                set
                {
                    float oldValue = _blueBrightness;
                    if (value != oldValue)
                    {
                        _blueBrightness = value;
                        MakeImageAdjustments();
                    }
                }
            }
            public float AlphaBrightness
            {
                get => _alphaBrightness;
                set
                {
                    float oldValue = _alphaBrightness;
                    if (value != oldValue)
                    {
                        _alphaBrightness = value;
                        MakeImageAdjustments();
                    }
                }
            }
            public float Gamma
            {
                get => _gamma;
                set
                {
                    float oldValue = _gamma;
                    if (value != oldValue)
                    {
                        _gamma = value < 0.1 ? 0.1f : value;
                        MakeImageAdjustments();
                    }
                }
            }
            public float Contrast
            {
                get => _contrast;
                set
                {
                    float oldValue = _contrast;
                    if (value != oldValue)
                    {
                        _contrast = value < 0.1 ? 0.1f : value;
                        MakeImageAdjustments();
                    }
                }
            }
            public ImageAttributes ImageAttributes
            {
                get => _imageAttributes;
            }

            private void MakeImageAdjustments()
            {
                if (_imageAttributes == null) { _imageAttributes = new ImageAttributes(); }
                // create matrix to adjust the image
                float[][] ptsArray ={
                new float[] {_contrast, 0, 0, 0, 0}, // scale red
                new float[] {0, _contrast, 0, 0, 0}, // scale green
                new float[] {0, 0, _contrast, 0, 0}, // scale blue
                new float[] {0, 0, 0, 1.0f, 0}, // don't scale alpha
                new float[] { _redBrightness - 1.0f, _greenBrightness - 1.0f, _redBrightness - 1.0f, 0, 1}};
                _imageAttributes.ClearColorMatrix();
                _imageAttributes.SetColorMatrix(new ColorMatrix(ptsArray), ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                _imageAttributes.SetGamma(_gamma, ColorAdjustType.Bitmap);
            }
        }
}
