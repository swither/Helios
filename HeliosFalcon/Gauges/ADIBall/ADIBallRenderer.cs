//  Copyright 2014 Craig Courtney
//  Copyright 2022 Helios Contributors
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

namespace GadrocsWorkshop.Helios.Gauges.Falcon.ADIBallRenderer
{
	using GadrocsWorkshop.Helios.Gauges;
	using System;
	using System.IO;
	using System.Windows;
	using System.Drawing;
	using System.Drawing.Imaging;
	using System.Windows.Media;
	using System.Windows.Media.Imaging;

	public class ADIBallRenderer : GaugeComponent
	{
		private BitmapSource _ballBitmap;
		private Rect _displayRect = new Rect(0, 0, 0, 0);
		private double _displayCenterX;
		private double _displayCenterY;
		private double _ballRotationAngle;
		private double _ballVerticalValue;

		public ADIBallRenderer()
		{
		}

		#region Drawing

		protected override void OnRender(DrawingContext drawingContext)
		{
			double tapeHeightToWidth = 1.25;
			int tapeWidth = _ballBitmap.PixelWidth;
			double tapeHeight = _ballBitmap.PixelHeight;
			int tapeDisplayHeight = (int)(tapeWidth * tapeHeightToWidth);
			int tapeMaxVerticalOffset = (int)(tapeHeight - tapeDisplayHeight) / 2;

			double verticalOffset = BallVerticalValue * tapeHeight / BallTapeScaleLength * 2d;
			double tapeVerticalAdjustment = Math.Truncate(verticalOffset) - verticalOffset;
			int tapeVerticalOffset = ClampVerticalOffset((int)verticalOffset, tapeMaxVerticalOffset) + tapeMaxVerticalOffset;

			Int32Rect _rect = new Int32Rect(0, tapeVerticalOffset, tapeWidth, tapeDisplayHeight);
			CroppedBitmap image = new CroppedBitmap(_ballBitmap, _rect);

			ADIBallImageWarp imageWarpSource = new ADIBallImageWarp(BitmapFromImage(image));
			ADIBallImageWarp imageWarpResult = imageWarpSource.WarpBallImage();

			TransformGroup transform = new TransformGroup();
			transform.Children.Add(new TranslateTransform(0, tapeVerticalAdjustment));
			transform.Children.Add(new RotateTransform(BallRotationAngle, _displayCenterX, _displayCenterY));

			drawingContext.PushTransform(transform);

			drawingContext.DrawImage(ImageFromBitmap(imageWarpResult.Bitmap), _displayRect);

			drawingContext.Pop();
		}

		#endregion Drawing

		#region OnRefresh

		protected override void OnRefresh(double xScale, double yScale)
		{
			ImageBrush ballImage;

			ImageSource ballImageSource = ConfigManager.ImageManager.LoadImage(BallImage);
			{
				ballImage = new ImageBrush(ballImageSource)
				{
					Stretch = Stretch.Fill,
					TileMode = TileMode.None,
					Viewport = new Rect(0d, 0d, 1d, 1d),
					ViewportUnits = BrushMappingMode.RelativeToBoundingBox
				};
			}

			_ballBitmap = (BitmapSource)ballImage.ImageSource;

			_displayRect.Width = BallDiameter * xScale;
			_displayRect.Height = BallDiameter * yScale;
			_displayRect.X = BallLeft * xScale;
			_displayRect.Y = BallTop * yScale;

			_displayCenterX = _displayRect.X + _displayRect.Width / 2;
			_displayCenterY = _displayRect.Y + _displayRect.Height / 2;
		}

		#endregion OnRefresh

		#region Functions

		private Bitmap BitmapFromImage(BitmapSource image)
		{
			Bitmap bitmap;

			using (MemoryStream outStream = new MemoryStream())
			{
				BitmapEncoder enc = new BmpBitmapEncoder();
				enc.Frames.Add(BitmapFrame.Create(image));
				enc.Save(outStream);
				bitmap = new Bitmap(outStream);
			}

			return bitmap;
		}

		private BitmapImage ImageFromBitmap(Bitmap bmp)
		{
			Bitmap bitmap = new Bitmap(bmp);
			BitmapImage image = new BitmapImage();
			MemoryStream outStream = new MemoryStream();

			bitmap.Save(outStream, ImageFormat.Bmp);

			image.BeginInit();
			image.StreamSource = new MemoryStream(outStream.ToArray());
			image.EndInit();

			return image;
		}

		private int ClampVerticalOffset(int value, int maxvalue)
		{
			maxvalue = maxvalue - 40;

			if (value > maxvalue)
			{
				value = maxvalue;
			}

			if (value < -maxvalue)
			{
				value = -maxvalue;
			}

			return value;
		}

		#endregion Functions

		#region Properties

		internal string BallImage { get; set; }
		internal double BallTapeScaleLength { get; set; }
		internal double BallDiameter { get; set; }
		internal double BallTop { get; set; }
		internal double BallLeft { get; set; }

		public double BallRotationAngle
		{
			get
			{
				return _ballRotationAngle;
			}
			set
			{
				double oldValue = _ballRotationAngle;
				_ballRotationAngle = value;
				if (!_ballRotationAngle.Equals(oldValue))
				{
					OnDisplayUpdate();
				}
			}
		}

		public double BallVerticalValue
		{
			get
			{
				return _ballVerticalValue;
			}
			set
			{
				double oldValue = _ballVerticalValue;
				_ballVerticalValue = value;
				if (!_ballVerticalValue.Equals(oldValue))
				{
					OnDisplayUpdate();
				}
			}
		}

		#endregion Properties
	}
}
