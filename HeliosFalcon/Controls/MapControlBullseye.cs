﻿//  Copyright 2014 Craig Courtney
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

namespace GadrocsWorkshop.Helios.Controls
{
	using static GadrocsWorkshop.Helios.Controls.MapControls;
	using GadrocsWorkshop.Helios.ComponentModel;
	using GadrocsWorkshop.Helios.Interfaces.Falcon;
	using System;
	using System.Collections.Generic;
	using System.Windows;
	using System.Windows.Media;


	[HeliosControl("Helios.Falcon.MapControlBullseye", "Falcon BMS Bullseye", "Falcon BMS Common", typeof(Gauges.GaugeRenderer))]

	public class MapControlBullseye : Gauges.BaseGauge
	{
		private FalconInterface _falconInterface;

		private List<ITargetData> TargetDataList = new List<ITargetData>();

		private HeliosValue _selectionTargetsClear;
		private HeliosValue _selectionRangeRingsVisible;

		private Gauges.GaugeImage _SelectionBackground;
		private Gauges.GaugeImage _SelectionBullseye;
		private Gauges.CustomGaugeNeedle _SelectionRangeRings;
		private Gauges.CustomGaugeNeedle _SelectionAircraft;
		private Gauges.CustomGaugeNeedle _SelectionAircraftRemote;
		private MapControlLineRenderer _SelectionTargetLines;
		private MapControlTextRenderer _SelectionTextData;

		private Rect _imageSize = new Rect(0d, 0d, 400d, 400d);
		private Size _needleSize = new Size(400d, 400d);
		private Rect _needleClip = new Rect(0d, 0d, 400d, 400d);
		private Point _needleLocation = new Point(0d, 0d);
		private Point _needleCenter = new Point(200d, 200d);

		private const string _selectionBullseyeImage = "{HeliosFalcon}/Images/MapControl/Selection Bullseye.png";
		private const string _selectionBackgroundImage = "{HeliosFalcon}/Images/MapControl/Selection Background.png";
		private const string _selectionTargetImage = "{HeliosFalcon}/Images/MapControl/Selection Target.png";
		private const string _selectionRangeRingsImage = "{HeliosFalcon}/Images/MapControl/Selection Range Rings.png";
		private const string _selectionAircraftImage = "{HeliosFalcon}/Images/MapControl/Selection Aircraft.png";
		private const string _selectionAircraftRemoteImage = "{HeliosFalcon}/Images/MapControl/Selection Aircraft Remote.png";

		private const double _controlBaseSize = 200;
		private const double _mapFeetPerNauticalMile = 6076;
		private const double _targetBullseyeScale = 0.600d * 1.075d;

		private double _controlScaleFactor;
		private double _squareWidth = 0d;
		private double _squareHeight = 0d;
		private double _squarePosX = 0d;
		private double _squarePosY = 0d;
		private double _widthCenterPosition;
		private double _heightCenterPosition;
		private double _ratioHeightToWidth;
		private double _ratioWidthToHeight;

		private bool _selectionRangeRingsEnabled = true;
		private bool _targetSelected = false;
		private bool _inFlightLastValue = true;
		private bool _inhibitMouseAction = false;


		public MapControlBullseye()
			: base("BullseyeControl", new Size(400d, 400d))
		{
			AddComponents();
			AddActions();
			MapControlStaticResize();
			ProcessAircraftValues();
			Resized += new EventHandler(OnMapControl_Resized);
		}


		#region Components

		private void AddComponents()
		{
			_SelectionBackground = new Gauges.GaugeImage(_selectionBackgroundImage, _imageSize);
			_SelectionBackground.Clip = new RectangleGeometry(_needleClip);
			_SelectionBackground.IsHidden = false;
			Components.Add(_SelectionBackground);

			_SelectionBullseye = new Gauges.GaugeImage(_selectionBullseyeImage, _imageSize);
			_SelectionBullseye.Clip = new RectangleGeometry(_needleClip);
			_SelectionBullseye.IsHidden = false;
			Components.Add(_SelectionBullseye);

			_SelectionRangeRings = new Gauges.CustomGaugeNeedle(_selectionRangeRingsImage, _needleLocation, _needleSize, _needleCenter);
			_SelectionRangeRings.Clip = new EllipseGeometry(_needleCenter, _needleCenter.X, _needleCenter.Y);
			_SelectionRangeRings.IsHidden = true;
			Components.Add(_SelectionRangeRings);

			_SelectionTargetLines = new MapControlLineRenderer();
			_SelectionTargetLines.Clip = new EllipseGeometry(_needleCenter, _needleCenter.X, _needleCenter.Y);
			_SelectionTargetLines.IsHidden = true;
			Components.Add(_SelectionTargetLines);

			_SelectionAircraft = new Gauges.CustomGaugeNeedle(_selectionAircraftImage, _needleLocation, _needleSize, _needleCenter);
			_SelectionAircraft.Clip = new EllipseGeometry(_needleCenter, _needleCenter.X, _needleCenter.Y);
			_SelectionAircraft.IsHidden = true;
			Components.Add(_SelectionAircraft);

			_SelectionAircraftRemote = new Gauges.CustomGaugeNeedle(_selectionAircraftRemoteImage, _needleLocation, _needleSize, _needleCenter);
			_SelectionAircraftRemote.Clip = new EllipseGeometry(_needleCenter, _needleCenter.X, _needleCenter.Y);
			_SelectionAircraftRemote.IsHidden = true;
			Components.Add(_SelectionAircraftRemote);

			_SelectionTextData = new MapControlTextRenderer();
			_SelectionTextData.Clip = new RectangleGeometry(_needleClip);
			_SelectionTextData.IsHidden = false;
			Components.Add(_SelectionTextData);
		}

		#endregion Components


		#region Actions

		private void AddActions()
		{
			_selectionTargetsClear = new HeliosValue(this, new BindingValue(false), "", "Target Selection Clear", "Clears the selected targets.", "Set true to clear the selected targets.", BindingValueUnits.Boolean);
			_selectionTargetsClear.Execute += new HeliosActionHandler(SelectionTargetsClear_Execute);
			Actions.Add(_selectionTargetsClear);
			Values.Add(_selectionTargetsClear);

			_selectionRangeRingsVisible = new HeliosValue(this, new BindingValue(true), "", "Target Selection Range Rings Visible", "Sets visibility of the target selection range rings.", "Set true to show target selection range rings.", BindingValueUnits.Boolean);
			_selectionRangeRingsVisible.Execute += new HeliosActionHandler(SelectionRangeRingsVisible_Execute);
			Actions.Add(_selectionRangeRingsVisible);
			Values.Add(_selectionRangeRingsVisible);
		}

		private void SelectionTargetsClear_Execute(object action, HeliosActionEventArgs e)
		{
			_selectionTargetsClear.SetValue(e.Value, e.BypassCascadingTriggers);
			bool selectionTargetsClear = _selectionTargetsClear.Value.BoolValue;

			if (selectionTargetsClear)
			{
				SelectionTargetsClear();
			}
		}

		private void SelectionRangeRingsVisible_Execute(object action, HeliosActionEventArgs e)
		{
			_selectionRangeRingsVisible.SetValue(e.Value, e.BypassCascadingTriggers);
			_selectionRangeRingsEnabled = _selectionRangeRingsVisible.Value.BoolValue;

			if (_selectionRangeRingsEnabled)
			{
				_SelectionRangeRings.IsHidden = false;
			}
			else
			{
				_SelectionRangeRings.IsHidden = true;
			}
		}

		#endregion Actions


		#region Methods

		protected override void OnProfileChanged(HeliosProfile oldProfile)
		{
			base.OnProfileChanged(oldProfile);

			if (oldProfile != null)
			{
				oldProfile.ProfileStarted -= new EventHandler(Profile_ProfileStarted);
				oldProfile.ProfileTick -= new EventHandler(Profile_ProfileTick);
				oldProfile.ProfileStopped -= new EventHandler(Profile_ProfileStopped);
			}

			if (Profile != null)
			{
				Profile.ProfileStarted += new EventHandler(Profile_ProfileStarted);
				Profile.ProfileTick += new EventHandler(Profile_ProfileTick);
				Profile.ProfileStopped += new EventHandler(Profile_ProfileStopped);
			}
		}

		private void Profile_ProfileStarted(object sender, EventArgs e)
		{
			if (Parent.Profile.Interfaces.ContainsKey("Falcon"))
			{
				_falconInterface = Parent.Profile.Interfaces["Falcon"] as FalconInterface;
			}
		}

		private void Profile_ProfileTick(object sender, EventArgs e)
		{
			if (_falconInterface != null)
			{
				BindingValue runtimeFlying = GetValue("Runtime", "Flying");
				bool inFlight = runtimeFlying.BoolValue;

				if (inFlight)
				{
					ProcessOwnshipValues();
					ProcessTargetValues();
					ProcessAircraftValues();
					_inFlightLastValue = true;
				}
				else
				{
					if (_inFlightLastValue)
					{
						ResetTargetSelection();
						_inFlightLastValue = false;
					}
				}
			}
		}

		private void Profile_ProfileStopped(object sender, EventArgs e)
		{
			_falconInterface = null;
		}

		private void ProcessOwnshipValues()
		{
			BindingValue ownshipRotationAngle = GetValue("HSI", "current heading");
			OwnshipRotationAngle = ownshipRotationAngle.DoubleValue;

			BindingValue ownshipHorizontalValue = GetValue("Ownship", "y");
			OwnshipHorizontalValue = ownshipHorizontalValue.DoubleValue;

			BindingValue ownshipVerticalValue = GetValue("Ownship", "x");
			OwnshipVerticalValue = ownshipVerticalValue.DoubleValue;

			BindingValue bullseyeHorizontalValue = GetValue("Ownship", "deltaY from bulls");
			BullseyeHorizontalValue = bullseyeHorizontalValue.DoubleValue;

			BindingValue bullseyeVerticalValue = GetValue("Ownship", "deltaX from bulls");
			BullseyeVerticalValue = bullseyeVerticalValue.DoubleValue;
		}

		private void ProcessAircraftValues()
		{
			double xValue;
			double yValue;
			double xScaleValue;
			double yScaleValue;

			xValue = BullseyeHorizontalValue / _mapFeetPerNauticalMile;
			yValue = BullseyeVerticalValue / _mapFeetPerNauticalMile;

			xScaleValue = xValue / _targetBullseyeScale;
			yScaleValue = -yValue / _targetBullseyeScale;

			if (Height >= Width)
			{
				_SelectionRangeRings.TapePosX = xScaleValue;
				_SelectionRangeRings.TapePosY = yScaleValue * _ratioWidthToHeight + _SelectionBullseye.PosY;

				_SelectionRangeRings.Tape_CenterX = (xScaleValue + _needleCenter.X);
				_SelectionRangeRings.Tape_CenterY = (yScaleValue + _needleCenter.Y) * _ratioWidthToHeight + _SelectionBullseye.PosY;

				_SelectionTargetLines.AircraftPosition_X = xScaleValue * Width / _needleSize.Width + _widthCenterPosition;
				_SelectionTargetLines.AircraftPosition_Y = yScaleValue * Width / _needleSize.Width + _heightCenterPosition;
			}
			else
			{
				_SelectionRangeRings.TapePosX = xScaleValue * _ratioHeightToWidth + _SelectionBullseye.PosX;
				_SelectionRangeRings.TapePosY = yScaleValue;

				_SelectionRangeRings.Tape_CenterX = (xScaleValue + _needleCenter.X) * _ratioHeightToWidth + _SelectionBullseye.PosX;
				_SelectionRangeRings.Tape_CenterY = (yScaleValue + _needleCenter.Y);

				_SelectionTargetLines.AircraftPosition_X = xScaleValue * Height / _needleSize.Height + _widthCenterPosition;
				_SelectionTargetLines.AircraftPosition_Y = yScaleValue * Height / _needleSize.Height + _heightCenterPosition;
			}

			_SelectionAircraft.TapePosX = _SelectionRangeRings.TapePosX;
			_SelectionAircraft.TapePosY = _SelectionRangeRings.TapePosY;

			_SelectionAircraft.Tape_CenterX = _SelectionRangeRings.Tape_CenterX;
			_SelectionAircraft.Tape_CenterY = _SelectionRangeRings.Tape_CenterY;

			AircraftDistance = GetHypotenuse(xValue, yValue);
			AircraftBearing = GetBearing(xValue, yValue);

			_SelectionTextData.AircraftDistance = AircraftDistance;
			_SelectionTextData.AircraftBearing = AircraftBearing;

			_SelectionRangeRings.Rotation = OwnshipRotationAngle;
			_SelectionAircraft.Rotation = OwnshipRotationAngle;
			_SelectionAircraftRemote.Rotation = AircraftBearing;

			if (AircraftDistance <= 120)
			{
				_SelectionAircraft.IsHidden = false;
				_SelectionAircraftRemote.IsHidden = true;

				if (_selectionRangeRingsEnabled)
				{
					_SelectionRangeRings.IsHidden = false;
				}
				else
				{
					_SelectionRangeRings.IsHidden = true;
				}
			}
			else
			{
				_SelectionAircraft.IsHidden = true;
				_SelectionAircraftRemote.IsHidden = false;
				_SelectionRangeRings.IsHidden = true;
			}
		}

		private void ProcessTargetValues()
		{
			double xValue;
			double yValue;
			double distance;
			double bearing;

			if (_targetSelected)
			{
				for (int i = 0; i < TargetDataList.Count; i++)
				{
					TargetDataList[i].MapTargetHorizontalValue = OwnshipHorizontalValue + TargetDataList[i].TargetHorizontalValue - BullseyeHorizontalValue;
					TargetDataList[i].MapTargetVerticalValue = OwnshipVerticalValue + TargetDataList[i].TargetVerticalValue - BullseyeVerticalValue;

					xValue = (TargetDataList[i].TargetHorizontalValue - BullseyeHorizontalValue) / _mapFeetPerNauticalMile;
					yValue = (TargetDataList[i].TargetVerticalValue - BullseyeVerticalValue) / _mapFeetPerNauticalMile;

					distance = GetHypotenuse(xValue, yValue);
					bearing = GetBearing(xValue, yValue);

					TargetDataList[i].CourseDistance = distance;
					TargetDataList[i].CourseBearing = bearing;
				}
			}
		}

		public override void MouseDown(Point location)
		{
			int target_Num;
			double target_posX;
			double target_posY;
			double distance_posX;
			double distance_posY;
			double bearing;
			double distance;

			if (_inhibitMouseAction)
			{
				return;
			}

			_inhibitMouseAction = true;
			
			target_Num = GetTargetAtLocation(location.X, location.Y);

			if (target_Num >= 0)
			{
				SelectionRemoveTarget(target_Num);
				_inhibitMouseAction = false;

				return;
			}

			target_posX = (location.X - _widthCenterPosition) * _needleSize.Width / Width;
			target_posY = (location.Y - _heightCenterPosition) * _needleSize.Height / Height;

			if (Height >= Width)
			{
				distance_posX = target_posX * _targetBullseyeScale;
				distance_posY = -target_posY * _targetBullseyeScale * _ratioHeightToWidth;
			}
			else
			{
				distance_posX = target_posX * _targetBullseyeScale * _ratioWidthToHeight;
				distance_posY = -target_posY * _targetBullseyeScale;
			}

			bearing = GetBearing(distance_posX, distance_posY);
			distance = GetHypotenuse(distance_posX, distance_posY);

			if (distance <= 125)
			{
				TargetDataList.Insert(0, new TargetData
				{
					TargetImage = new Gauges.GaugeImage(_selectionTargetImage, _imageSize),
					TargetBearing = bearing,
					TargetDistance = distance,
					TargetPosition_X = location.X,
					TargetPosition_Y = location.Y,
					TargetHorizontalValue = distance_posX * _mapFeetPerNauticalMile,
					TargetVerticalValue = distance_posY * _mapFeetPerNauticalMile
				});

				TargetDataList[0].TargetImage.IsHidden = true;
				TargetDataList[0].TargetImage.Width = _squareWidth;
				TargetDataList[0].TargetImage.Height = _squareHeight;

				if (Height >= Width)
				{
					TargetDataList[0].TargetImage.PosX = target_posX;
					TargetDataList[0].TargetImage.PosY = target_posY + _SelectionBullseye.PosY;

					TargetDataList[0].TargetImage.Clip = new EllipseGeometry(_needleCenter, _needleCenter.X, _needleCenter.Y * _ratioWidthToHeight);
				}
				else
				{
					TargetDataList[0].TargetImage.PosX = target_posX + _SelectionBullseye.PosX;
					TargetDataList[0].TargetImage.PosY = target_posY;

					TargetDataList[0].TargetImage.Clip = new EllipseGeometry(_needleCenter, _needleCenter.X * _ratioHeightToWidth, _needleCenter.Y);
				}

				Components.Insert(Components.IndexOf(_SelectionAircraftRemote) + 1, TargetDataList[0].TargetImage);

				TargetDataList[0].TargetImage.IsHidden = false;

				_SelectionTextData.TargetSelected = true;
				_SelectionTextData.SetTargetData(TargetDataList);

				_SelectionTargetLines.IsHidden = false;
				_SelectionTargetLines.SetTargetData(TargetDataList);

				_targetSelected = true;

				ProcessTargetValues();
				ProcessAircraftValues();
				Refresh();
			}

			_inhibitMouseAction = false;
		}

		private int GetTargetAtLocation(double location_X, double location_Y)
		{
			double radius_Max = 8d * _controlScaleFactor;
			double radius_Min = radius_Max;
			double radius_Location;
			double diff_X;
			double diff_Y;

			int target_Num = -1;

			for (int i = 0; i < TargetDataList.Count; i++)
			{
				diff_X = TargetDataList[i].TargetPosition_X - location_X;
				diff_Y = TargetDataList[i].TargetPosition_Y - location_Y;

				radius_Location = GetHypotenuse(diff_X, diff_Y);

				if (radius_Location <= radius_Min)
				{
					radius_Min = radius_Location;
					target_Num = i;
				}
			}

			return target_Num;
		}

		private void SelectionRemoveTarget(int index)
		{
			if (TargetDataList.Count > index)
			{
				Components.Remove(TargetDataList[index].TargetImage);

				TargetDataList.RemoveAt(index);
			}

			if (TargetDataList.Count == 0)
			{
				_SelectionTextData.TargetSelected = false;
				_SelectionTextData.TargetBearing = 0;
				_SelectionTextData.TargetDistance = 0;

				_SelectionTargetLines.IsHidden = true;

				_targetSelected = false;
			}

			ProcessTargetValues();
			Refresh();
		}

		private void SelectionTargetsClear()
		{
			while (TargetDataList.Count > 0)
			{
				Components.Remove(TargetDataList[TargetDataList.Count - 1].TargetImage);

				TargetDataList.RemoveAt(TargetDataList.Count - 1);
			}

			_SelectionTextData.TargetSelected = false;
			_SelectionTextData.TargetBearing = 0;
			_SelectionTextData.TargetDistance = 0;

			_SelectionTargetLines.IsHidden = true;

			_targetSelected = false;

			ProcessTargetValues();
			Refresh();
		}

		public override void Reset()
		{
			ResetTargetSelection();
		}

		private void ResetTargetSelection()
		{
			SelectionTargetsClear();

			OwnshipRotationAngle = 0d;
			OwnshipHorizontalValue = 0d;
			OwnshipVerticalValue = 0d;

			BullseyeHorizontalValue = 0d;
			BullseyeVerticalValue = 0d;

			_SelectionTextData.TargetSelected = false;
			_SelectionTextData.TargetBearing = 0;
			_SelectionTextData.TargetDistance = 0;

			_SelectionTargetLines.IsHidden = true;

			_selectionRangeRingsEnabled = true;

			_targetSelected = false;

			ProcessAircraftValues();
			ProcessTargetValues();
		}

		#endregion Methods


		#region Functions

		private BindingValue GetValue(string device, string name)
		{
			return _falconInterface?.GetValue(device, name) ?? BindingValue.Empty;
		}

		private double GetHypotenuse(double xValue, double yValue)
		{
			return Math.Round(Math.Sqrt((xValue * xValue + yValue * yValue)));
		}

		private double GetBearing(double xValue, double yValue)
		{
			double bearing = Math.Round(Math.Atan2(xValue, yValue) * 180 / Math.PI);

			if (bearing < 0)
			{
				bearing = 360 + bearing;
			}

			return bearing;
		}

		#endregion Functions


		#region Scaling

		private void OnMapControl_Resized(object sender, EventArgs e)
		{
			MapControlStaticResize();
		}

		private void MapControlStaticResize()
		{
			_widthCenterPosition = Width / 2;
			_heightCenterPosition = Height / 2;

			_ratioHeightToWidth = Height / Width;
			_ratioWidthToHeight = Width / Height;

			_controlScaleFactor = Math.Min(Width, Height) / _controlBaseSize;

			if (Height >= Width)
			{
				_squareWidth = _needleSize.Width;
				_squareHeight = _needleSize.Height * _ratioWidthToHeight;
				_squarePosX = 0d;
				_squarePosY = _needleSize.Height * (1 - _ratioWidthToHeight) / 2d;

				_SelectionRangeRings.Clip = new EllipseGeometry(_needleCenter, _needleCenter.X, _needleCenter.Y * _ratioWidthToHeight);
				_SelectionTargetLines.Clip = new EllipseGeometry(_needleCenter, _needleCenter.X, _needleCenter.Y * _ratioWidthToHeight);
				_SelectionAircraft.Clip = new EllipseGeometry(_needleCenter, _needleCenter.X, _needleCenter.Y * _ratioWidthToHeight);
				_SelectionAircraftRemote.Clip = new EllipseGeometry(_needleCenter, _needleCenter.X, _needleCenter.Y * _ratioWidthToHeight);
			}
			else
			{
				_squareWidth = _needleSize.Width * _ratioHeightToWidth;
				_squareHeight = _needleSize.Height;
				_squarePosX = _needleSize.Width * (1 - _ratioHeightToWidth) / 2d;
				_squarePosY = 0d;

				_SelectionRangeRings.Clip = new EllipseGeometry(_needleCenter, _needleCenter.X * _ratioHeightToWidth, _needleCenter.Y);
				_SelectionTargetLines.Clip = new EllipseGeometry(_needleCenter, _needleCenter.X * _ratioHeightToWidth, _needleCenter.Y);
				_SelectionAircraft.Clip = new EllipseGeometry(_needleCenter, _needleCenter.X * _ratioHeightToWidth, _needleCenter.Y);
				_SelectionAircraftRemote.Clip = new EllipseGeometry(_needleCenter, _needleCenter.X * _ratioHeightToWidth, _needleCenter.Y);
			}

			_SelectionBullseye.Width = _squareWidth;
			_SelectionBullseye.Height = _squareHeight;
			_SelectionBullseye.PosX = _squarePosX;
			_SelectionBullseye.PosY = _squarePosY;

			_SelectionRangeRings.Tape_Width = _squareWidth;
			_SelectionRangeRings.Tape_Height = _squareHeight;
			_SelectionRangeRings.TapePosX = _squarePosX;
			_SelectionRangeRings.TapePosY = _squarePosY;

			_SelectionAircraft.Tape_Width = _squareWidth;
			_SelectionAircraft.Tape_Height = _squareHeight;
			_SelectionAircraft.TapePosX = _squarePosX;
			_SelectionAircraft.TapePosY = _squarePosY;

			_SelectionAircraftRemote.Tape_Width = _squareWidth;
			_SelectionAircraftRemote.Tape_Height = _squareHeight;
			_SelectionAircraftRemote.TapePosX = _squarePosX;
			_SelectionAircraftRemote.TapePosY = _squarePosY;

			_SelectionTextData.MapControlWidth = Width;
			_SelectionTextData.MapControlHeight = Height;

			Refresh();
		}

		#endregion Scaling


		#region Properties

		public class TargetData : ITargetData
		{
			public Gauges.GaugeImage TargetImage { get; set; }
			public double TargetBearing { get; set; }
			public double TargetDistance { get; set; }
			public double TargetPosition_X { get; set; }
			public double TargetPosition_Y { get; set; }
			public double TargetHorizontalValue { get; set; }
			public double TargetVerticalValue { get; set; }
			public double MapTargetHorizontalValue { get; set; }
			public double MapTargetVerticalValue { get; set; }
			public double CourseBearing { get; set; }
			public double CourseDistance { get; set; }
		}

		public double AircraftBearing { get; set; }
		public double AircraftDistance { get; set; }
		private double OwnshipRotationAngle { get; set; }
		private double OwnshipHorizontalValue { get; set; }
		private double OwnshipVerticalValue { get; set; }
		private double BullseyeHorizontalValue { get; set; }
		private double BullseyeVerticalValue { get; set; }

		#endregion Properties

	}
}
