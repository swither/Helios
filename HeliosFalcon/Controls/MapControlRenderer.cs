//  Copyright 2014 Craig Courtney
//  Copyright 2021 Helios Contributors
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
	using GadrocsWorkshop.Helios.Gauges;
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Windows;
	using System.Windows.Media;


	public class MapControlRenderer : GaugeComponent
	{
		private Point _location;
		private Size _size;
		private Point _center;
		private double _baseRotation;
		private double _rotation;
		private double _horizontalOffset;
		private double _verticalOffset;
		private double _xScale = 1.0d;
		private double _yScale = 1.0d;
		private double _mapWidth;
		private double _mapHeight;
		private double _mapSizeFeet;
		private double _pixelsPerDip = 1.0d;

		private double[,] _navPoints_WP = new double[24, 2];
		private double[,] _navPoints_MK = new double[19, 2];
		private double[,] _navPoints_PT = new double[15, 3];
		private string[] _navNames_PT = new string[15];

		private double[,] _mapPoints_WP = new double[24, 2];
		private double[,] _mapPoints_MK = new double[19, 2];
		private double[,] _mapPoints_PT = new double[15, 3];

		private ImageSource[] _waypointImages = new ImageSource[24];
		private Rect[] _waypointRect = new Rect[24];

		private const double _minThreatCircleRadius = 30000d;
		private const double _mapBaseUnit = 1000d;
		private double _mapScaleUnit;
		private const double _waypointBaseScale = 30d;
		private double _waypointSize;
		private double _waypointLineWidth;

		private Pen _linePen;
		private Brush _lineBrush;
		private Brush _pointFillBrush;
		private Brush _backgroundFillBrush;
		private Brush _transparentFillBrush;

		private FormattedText _formattedText;
		private const double _fontBaseSize = 30d;
		private double _fontScaleSize;


		public MapControlRenderer(Point location, Size size, Point center)
			: this(location, size, center, 0d)
		{
			GetPixelsPerDip();
			InitializeWaypointImagesArray();
		}

		public MapControlRenderer(Point location, Size size, Point center, double baseRotation)
		{
			_location = location;
			_size = size;
			_center = center;
			_baseRotation = baseRotation;
		}


		#region Actions

		void InitializeWaypointImagesArray()
		{
			string imagePath;

			for (int i = 0; i < _waypointImages.GetLength(0); i++)
			{
				imagePath = "{HeliosFalcon}/Images/Waypoints/Waypoint_" + (i + 1).ToString("D2") + ".png";

				_waypointImages[i] = ConfigManager.ImageManager.LoadImage(imagePath, 120, 120);
			}
		}

		internal void ProcessNavPointValues(List<string> navPoints )
		{
			int lenWP = _navPoints_WP.GetLength(0);
			int lenMK = _navPoints_MK.GetLength(0);
			int lenPT = _navPoints_PT.GetLength(0);

			int posWP = 0;
			int posMK = 0;
			int posPT = 0;

			Array.Clear(_navPoints_WP, 0, _navPoints_WP.Length);
			Array.Clear(_navPoints_MK, 0, _navPoints_MK.Length);
			Array.Clear(_navPoints_PT, 0, _navPoints_PT.Length);
			Array.Clear(_navNames_PT, 0, _navNames_PT.Length);

			foreach (string navLine in navPoints)
			{
				string[] navValues = navLine.Split(',');

				if (navValues.Length >= 4)
				{
					switch (navValues[1])
					{
						case "WP":
							{
								if (posWP < lenWP)
								{
									_navPoints_WP[posWP, 0] = NavPointToDouble(navValues[3]);
									_navPoints_WP[posWP, 1] = NavPointToDouble(navValues[2]);
									posWP++;
								}
								break;
							}
						case "MK":
							{
								if (posMK < lenMK)
								{
									_navPoints_MK[posMK, 0] = NavPointToDouble(navValues[3]);
									_navPoints_MK[posMK, 1] = NavPointToDouble(navValues[2]);
									posMK++;
								}
								break;
							}
						case "PT":
							{
								if (posPT < lenPT)
								{
									_navPoints_PT[posPT, 0] = NavPointToDouble(navValues[3]);
									_navPoints_PT[posPT, 1] = NavPointToDouble(navValues[2]);

									if (navValues.Length >= 7)
									{
										_navPoints_PT[posPT, 2] = NavPointToDouble(navValues[6]);
										_navNames_PT[posPT] = NavPointToName(navValues[5]);
									}
									posPT++;
								}
								break;
							}
					}
				}
			}
		}

		double NavPointToDouble(string navValue)
		{
			return decimal.ToDouble(decimal.Parse(navValue, NumberStyles.Float));
		}

		string NavPointToName(string navValue)
		{
			string[] navValues = navValue.Split(':');
			if (navValues.Length >= 2)
			{
				return navValues[1].Trim('"');
			}
			else
			{
				return "";
			}
		}

		void GetPixelsPerDip()
		{
			DisplayManager displayManager = new DisplayManager();

			if (displayManager.PixelsPerDip != 0d)
			{
				_pixelsPerDip = displayManager.PixelsPerDip;
			}
		}

		#endregion Actions


		#region Drawing

		protected override void OnRender(System.Windows.Media.DrawingContext drawingContext)
		{
			TransformGroup transform = new TransformGroup();
			transform.Children.Add(new TranslateTransform((_location.X + HorizontalOffset) * _xScale, (_location.Y + VerticalOffset) * _yScale));
			transform.Children.Add(new RotateTransform(_rotation + _baseRotation, _center.X * _xScale, _center.Y * _yScale));

			drawingContext.PushTransform(transform);

			drawingContext.PushOpacity(ThreatVisibility);
			DrawThreatCircles(drawingContext);
			DrawDesignatedTargets(drawingContext);
			DrawThreatNames(drawingContext);
			drawingContext.Pop();

			drawingContext.PushOpacity(WaypointVisibility);
			DrawWaypointLines(drawingContext);
			DrawWaypointImages(drawingContext);
			drawingContext.Pop();

			drawingContext.Pop();
		}

		void DrawThreatCircles(DrawingContext drawingContext)
		{
			_lineBrush = new SolidColorBrush(Color.FromRgb(255, 0, 0));
			_linePen = new Pen(_lineBrush, _mapScaleUnit * 4d);
			_pointFillBrush = new SolidColorBrush(Color.FromRgb(255, 0, 0));
			_transparentFillBrush = new SolidColorBrush(Color.FromRgb(255, 0, 0)) {Opacity = 0.15d};

			for (int i = 0; i < _mapPoints_PT.GetLength(0); i++)
			{
				if (_mapPoints_PT[i, 0] > 0d)
				{
					if (_mapPoints_PT[i, 2] > 0d)
					{
						drawingContext.DrawEllipse(_transparentFillBrush, _linePen, new Point(_mapPoints_PT[i, 0], _mapPoints_PT[i, 1]), _mapPoints_PT[i, 2], _mapPoints_PT[i, 2]);
					}

					drawingContext.DrawEllipse(_pointFillBrush, null, new Point(_mapPoints_PT[i, 0], _mapPoints_PT[i, 1]), _mapScaleUnit * 12d, _mapScaleUnit * 12d);
				}
			}
		}

		void DrawDesignatedTargets(DrawingContext drawingContext)
		{
			_pointFillBrush = new SolidColorBrush(Color.FromRgb(0, 0, 255));
			_backgroundFillBrush = new SolidColorBrush(Color.FromRgb(255, 255, 255));

			for (int i = 0; i < _mapPoints_MK.GetLength(0); i++)
			{
				if (_mapPoints_MK[i, 0] > 0d)
				{
					drawingContext.DrawEllipse(_backgroundFillBrush, null, new Point(_mapPoints_MK[i, 0], _mapPoints_MK[i, 1]), _mapScaleUnit * 18d, _mapScaleUnit * 18d);

					drawingContext.DrawEllipse(_pointFillBrush, null, new Point(_mapPoints_MK[i, 0], _mapPoints_MK[i, 1]), _mapScaleUnit * 12d, _mapScaleUnit * 12d);
				}
			}
		}

		void DrawThreatNames(DrawingContext drawingContext)
		{
			// This prevents jittery text when there is no rotation.
			drawingContext.PushTransform(new RotateTransform(0.000001d));

			for (int i = 0; i < _mapPoints_PT.GetLength(0); i++)
			{
				if (_mapPoints_PT[i, 0] > 0d && !string.IsNullOrEmpty(_navNames_PT[i]))
				{
					_formattedText = new FormattedText(_navNames_PT[i], CultureInfo.GetCultureInfo("en-us"),
						FlowDirection.LeftToRight, new Typeface("Lucida Console Regular"), _fontScaleSize, Brushes.White, _pixelsPerDip);

					drawingContext.DrawText(_formattedText, new Point(_mapPoints_PT[i, 0] + _fontScaleSize * 0.5d, _mapPoints_PT[i, 1] - _fontScaleSize * 1.2d));
				}
			}

			drawingContext.Pop();
		}

		void DrawWaypointLines(DrawingContext drawingContext)
		{
			_lineBrush = new SolidColorBrush(Color.FromRgb(255, 255, 255));
			_linePen = new Pen(_lineBrush, _waypointLineWidth) {DashStyle = DashStyles.Dash};

			for (int i = 0; i < _mapPoints_WP.GetLength(0) - 1; i++)
			{
				if (_mapPoints_WP[i, 0] == 0d || _mapPoints_WP[i + 1, 0] == 0d)
					break;

				drawingContext.DrawLine(_linePen, new Point(_mapPoints_WP[i, 0], _mapPoints_WP[i, 1]), new Point(_mapPoints_WP[i + 1, 0], _mapPoints_WP[i + 1, 1]));
			}
		}

		void DrawWaypointImages(DrawingContext drawingContext)
		{
			for (int i = 0; i < _navPoints_WP.GetLength(0); i++)
			{
				if (_navPoints_WP[i, 0] > 0d)
				{
					drawingContext.DrawImage(_waypointImages[i], _waypointRect[i]);
				}
				else
				{
					break;
				}
			}
		}

		protected override void OnRefresh(double xScale, double yScale)
		{
			_xScale = xScale;
			_yScale = yScale;

			_mapScaleUnit = FeetToMapUnits_ScaleUnit(_mapBaseUnit, _xScale, _yScale);
			_fontScaleSize = _mapScaleUnit * _fontBaseSize;

			_waypointSize = MapShortestSize / _waypointBaseScale * MapScaleMultiplier;

			if (MapScaleMultiplier == 1d)
			{
				_waypointSize = _waypointSize * 1.33d;
			}
			else if (MapScaleMultiplier == 4d)
			{
				_waypointSize = _waypointSize / 1.33d;
			}

			_waypointLineWidth = _waypointSize / 10d;

			double waypointOffset = _waypointSize / 2;

			for (int i = 0; i < _navPoints_WP.GetLength(0); i++)
			{
				_mapPoints_WP[i, 0] = FeetToMapUnits_X(_navPoints_WP[i, 0], _xScale);
				_mapPoints_WP[i, 1] = FeetToMapUnits_Y(_navPoints_WP[i, 1], _yScale);
			}

			for (int i = 0; i < _navPoints_MK.GetLength(0); i++)
			{
				_mapPoints_MK[i, 0] = FeetToMapUnits_X(_navPoints_MK[i, 0], _xScale);
				_mapPoints_MK[i, 1] = FeetToMapUnits_Y(_navPoints_MK[i, 1], _yScale);
			}

			for (int i = 0; i < _navPoints_PT.GetLength(0); i++)
			{
				_mapPoints_PT[i, 0] = FeetToMapUnits_X(_navPoints_PT[i, 0], _xScale);
				_mapPoints_PT[i, 1] = FeetToMapUnits_Y(_navPoints_PT[i, 1], _yScale);
				_mapPoints_PT[i, 2] = FeetToMapUnits_CircleRadius(_navPoints_PT[i, 2], _xScale, _yScale);
			}

			for (int i = 0; i < _waypointRect.GetLength(0); i++)
			{
				_waypointRect[i] = new Rect(_mapPoints_WP[i, 0] - waypointOffset, _mapPoints_WP[i, 1] - waypointOffset, _waypointSize, _waypointSize);
			}
		}

		double FeetToMapUnits_X(double xPosFeet, double xScale)
		{
			if (xPosFeet > 0d)
			{
				return xPosFeet / MapSizeFeet * MapWidth * xScale;
			}
			else
			{
				return 0d;
			}
		}

		double FeetToMapUnits_Y(double yPosFeet, double yScale)
		{
			if (yPosFeet > 0d)
			{
				return (MapSizeFeet - yPosFeet) / MapSizeFeet * MapHeight * yScale;
			}
			else
			{
				return 0d;
			}
		}

		double FeetToMapUnits_CircleRadius(double radiusFeet, double xScale, double yScale)
		{
			if (radiusFeet < _minThreatCircleRadius)
			{
				return 0d;
			}
			else if (MapHeight >= MapWidth)
			{
				return radiusFeet / MapSizeFeet * MapWidth * xScale;
			}
			else
			{
				return radiusFeet / MapSizeFeet * MapHeight * yScale;
			}
		}

		double FeetToMapUnits_ScaleUnit(double unit, double xScale, double yScale)
		{
			if (MapHeight >= MapWidth)
			{
				return unit / MapSizeFeet * MapWidth * xScale;
			}
			else
			{
				return unit / MapSizeFeet * MapHeight * yScale;
			}
		}

		#endregion Drawing


		#region Properties

		public double ThreatVisibility { get; set; }

		public double WaypointVisibility { get; set; }

		public double MapShortestSize { get; set; }

		public double MapScaleMultiplier { get; set; }

		public double MapWidth
		{
			get
			{
				return _mapWidth;
			}
			set
			{
				double newValue = Math.Round(value, 1);
				if (!_mapWidth.Equals(newValue))
				{
					_mapWidth = value;
				}
			}
		}

		public double MapHeight
		{
			get
			{
				return _mapHeight;
			}
			set
			{
				double newValue = Math.Round(value, 1);
				if (!_mapHeight.Equals(newValue))
				{
					_mapHeight = value;
				}
			}
		}

		public double MapSizeFeet
		{
			get
			{
				return _mapSizeFeet;
			}
			set
			{
				double newValue = Math.Round(value, 1);
				if (!_mapSizeFeet.Equals(newValue))
				{
					_mapSizeFeet = value;
				}
			}
		}

		public double Tape_Width
		{
			get
			{
				return _size.Width;
			}
			set
			{
				_size.Width = value;
			}
		}

		public double Tape_Height
		{
			get
			{
				return _size.Height;
			}
			set
			{
				_size.Height = value;
			}
		}

		public double TapePosX
		{
			get
			{
				return _location.X;
			}
			set
			{
				_location.X = value;
			}
		}

		public double TapePosY
		{
			get
			{
				return _location.Y;
			}
			set
			{
				_location.Y = value;
			}
		}

		public double Tape_CenterX
		{
			get
			{
				return _center.X;
			}
			set
			{
				_center.X = value;
			}
		}

		public double Tape_CenterY
		{
			get
			{
				return _center.Y;
			}
			set
			{
				_center.Y = value;
			}
		}

		public double BaseRotation
		{
			get
			{
				return _baseRotation;
			}
			set
			{
				_baseRotation = value;
				OnDisplayUpdate();
			}
		}

		public double Rotation
		{
			get
			{
				return _rotation;
			}
			set
			{
				double newValue = Math.Round(value, 1);
				if (!_rotation.Equals(newValue))
				{
					_rotation = value;
					OnDisplayUpdate();
				}
			}
		}

		public double HorizontalOffset
		{
			get
			{
				return _horizontalOffset;
			}
			set
			{
				double newValue = Math.Round(value, 1);
				if (!_horizontalOffset.Equals(newValue))
				{
					_horizontalOffset = value;
					OnDisplayUpdate();
				}
			}
		}

		public double VerticalOffset
		{
			get
			{
				return _verticalOffset;
			}
			set
			{
				double newValue = Math.Round(value, 1);
				if (!_verticalOffset.Equals(newValue))
				{
					_verticalOffset = value;
					OnDisplayUpdate();
				}
			}
		}

		#endregion

	}
}
