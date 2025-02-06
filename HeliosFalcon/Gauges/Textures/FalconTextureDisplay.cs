﻿//  Copyright 2014 Craig Courtney
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using GadrocsWorkshop.Helios.Interfaces.Falcon.BMS;
using GadrocsWorkshop.Helios.Util;

namespace GadrocsWorkshop.Helios.Interfaces.Falcon.Gauges.Textures
{
    public abstract class FalconTextureDisplay : HeliosVisual
    {
        protected enum FalconTextures
        {
            HUD,
            PFL,
            DED,
            RWR,
            MFDLeft,
            MFDRight
        }

        private bool _isRunning = false;

        private Dictionary<FalconTextures, Rect> _textureRectangles = new Dictionary<FalconTextures, Rect>();
        private SharedMemory _textureMemory;
        private SharedMemory _sharedMemory2;
        private FlightData2 _lastFlightData2;
        private DispatcherTimer _dispatcherTimer;
        private bool _textureRefreshRate_30;
        private bool _textureRefreshRate_60;
        private bool _textureRefreshRate_90;

        /// <summary>
        /// backing field for background transparency
        /// </summary>
        private bool _transparency;
        private bool _horizontalFlip;
        private bool _verticalFlip;
        private string _aircraftName;
        private bool _flying;

        protected FalconTextureDisplay(string name, Size defaultSize)
            : base(name, defaultSize)
        {
        }

        #region Properties

        internal SharedMemory TextureMemory
        {
            get { return _textureMemory; }
        }

        public bool IsRunning
        {
            get
            {
                return _isRunning;
            }
            set
            {
                if (!_isRunning.Equals(value))
                {
                    bool oldValue = _isRunning;

                    _isRunning = value;
                    OnPropertyChanged("IsRunning", oldValue, value, false);

                    OnDisplayUpdate();
                }
            }
        }

        protected abstract Rect DefaultRect { get; }

        protected abstract FalconTextures Texture { get; }

        internal abstract String DefaultImage { get; }

        internal Rect TextureRect
        {
            get
            {
                if (_textureRectangles.ContainsKey(Texture))
                {
                    return _textureRectangles[Texture];
                }
                else
                {
                    return DefaultRect;
                }
            }
        }

        public bool TransparencyEnabled
        {
            get => _transparency;
            set
            {
                if (_transparency == value)
                {
                    return;
                }
                bool oldValue = _transparency;
                _transparency = value;
                OnPropertyChanged(nameof(TransparencyEnabled), oldValue, value, true);
            }
        }

        public bool HorizontalFlipEnabled
        {
            get => _horizontalFlip;
            set
            {
                if (_horizontalFlip == value)
                {
                    return;
                }
                bool oldValue = _horizontalFlip;
                _horizontalFlip = value;
                OnPropertyChanged(nameof(HorizontalFlipEnabled), oldValue, value, true);
            }
        }

        public bool VerticalFlipEnabled
        {
            get => _verticalFlip;
            set
            {
                if (_verticalFlip == value)
                {
                    return;
                }
                bool oldValue = _verticalFlip;
                _verticalFlip = value;
                OnPropertyChanged(nameof(VerticalFlipEnabled), oldValue, value, true);
            }
        }

        public bool Flying
        {
            get => _flying;
            set
            {
                if (_flying == value)
                {
                    return;
                }
                bool oldValue = _flying;
                _flying = value;
                OnPropertyChanged(nameof(Flying), oldValue, value, true);
            }
        }

        public string AircraftName
        {
            get => _aircraftName;
            set
            {
                if (_aircraftName == value)
                {
                    return;
                }
                string oldValue = _aircraftName;
                _aircraftName = value;
                OnPropertyChanged(nameof(AircraftName), oldValue, value, true);
            }
        }

        #endregion

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

        void Profile_ProfileStopped(object sender, EventArgs e)
        {
            PropertyChanged -= FalconTextureDisplay_PropertyChanged;

            if (_dispatcherTimer != null)
            {
                _dispatcherTimer.Stop();
                _dispatcherTimer = null;
            }

            _textureMemory?.Close();
            _textureMemory?.Dispose();
            _textureMemory = null;

            _sharedMemory2?.Close();
            _sharedMemory2?.Dispose();
            _sharedMemory2 = null;

            IsRunning = false;
        }

        void Profile_ProfileTick(object sender, EventArgs e)
        {
            if (_textureRefreshRate_30 || Texture == FalconTextures.DED || Texture == FalconTextures.PFL || Texture == FalconTextures.RWR)
            {
                RefreshTextures();
            }
        }

        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            if ((_textureRefreshRate_60 || _textureRefreshRate_90) && (Texture == FalconTextures.HUD || Texture == FalconTextures.MFDLeft || Texture == FalconTextures.MFDRight))
            {
                RefreshTextures();
            }
        }

        private void RefreshTextures()
        {
            if (_textureMemory != null && _textureMemory.IsDataAvailable)
            {
                FalconInterface falconInterface = Parent.Profile.Interfaces["Falcon"] as FalconInterface;

                Flying = falconInterface.GetValue("Runtime", "Flying").BoolValue;
                AircraftName = falconInterface.GetValue("Runtime", "Aircraft Name").StringValue;

                //If the profile was started prior to BMS running then get the texture area from shared memory
                if (_textureRectangles.Count == 0 && falconInterface.FalconType == FalconTypes.BMS)
                {
                    GetTextureArea(Texture);
                }
                
                OnDisplayUpdate();
            }
        }

        void Profile_ProfileStarted(object sender, EventArgs e)
        {
            if (Parent != null && Parent.Profile != null && Parent.Profile.Interfaces.ContainsKey("Falcon"))
            {
                FalconInterface falconInterface = Parent.Profile.Interfaces["Falcon"] as FalconInterface;
                
                Flying = falconInterface.GetValue("Runtime", "Flying").BoolValue;
                AircraftName = falconInterface.GetValue("Runtime", "Aircraft Name").StringValue;

                if(falconInterface.FalconType == FalconTypes.BMS)
                {
                    GetTextureArea(Texture);
                }
                else if (falconInterface.FalconType == FalconTypes.OpenFalcon)
                {
                    ParseDatFile(falconInterface.CockpitDatFile);
                }

                _textureRefreshRate_30 = falconInterface.TextureRefreshRate_30;
                _textureRefreshRate_60 = falconInterface.TextureRefreshRate_60;
                _textureRefreshRate_90 = falconInterface.TextureRefreshRate_90;

                PropertyChanged += FalconTextureDisplay_PropertyChanged;
            }
            
            _textureMemory = new SharedMemory("FalconTexturesSharedMemoryArea");
            _textureMemory.Open();

            _dispatcherTimer = new DispatcherTimer();
            _dispatcherTimer.Tick += new EventHandler(DispatcherTimer_Tick);

            if (_textureRefreshRate_60)
            {
                _dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 16);
                _dispatcherTimer.Start();
            }
            else if (_textureRefreshRate_90)
            {
                _dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 11);
                _dispatcherTimer.Start();
            }
            
            IsRunning = true;
        }

        private void FalconTextureDisplay_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName == nameof(AircraftName) && _textureRectangles.ContainsKey(Texture))
            {
                _textureRectangles.Remove(Texture);
            }
        }

        private void GetTextureArea(FalconTextures texture)
        {
            _textureRectangles.Remove(Texture);
            _sharedMemory2 = new SharedMemory("FalconSharedMemoryArea2");

            if (_sharedMemory2 != null & _sharedMemory2.IsDataAvailable)
            {
                _lastFlightData2 = (FlightData2)_sharedMemory2.MarshalTo(typeof(FlightData2));

                var left = _lastFlightData2.RTT_area[(int)texture * 4];
                var top = _lastFlightData2.RTT_area[(int)texture * 4 + 1];
                var right = _lastFlightData2.RTT_area[(int)texture * 4 + 2];
                var bottom = _lastFlightData2.RTT_area[(int)texture * 4 + 3];
                var width = (right - 1) - left;
                var height = (bottom - 1) - top;
                if (width > 0 && height > 0)
                {
                    _textureRectangles.Add(texture, new Rect(left, top, width, height));
                }
            }
        }

        private void ParseDatFile(string datFile)
        {
            _textureRectangles.Clear();
            if (File.Exists(datFile))
            {
                StreamReader reader = new StreamReader(datFile);
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    string[] tokens = line.Split(new string[] { " ", "\t" }, StringSplitOptions.RemoveEmptyEntries);

                    if (tokens.Length > 1)
                    {
                        switch (tokens[0].ToLower())
                        {
                            case "hud":
                                _textureRectangles.Add(FalconTextures.HUD, ParseRttRectangle(tokens));
                                break;

                            case "pfl":
                                _textureRectangles.Add(FalconTextures.PFL, ParseRttRectangle(tokens));
                                break;

                            case "ded":
                                _textureRectangles.Add(FalconTextures.DED, ParseRttRectangle(tokens));
                                break;

                            case "rwr":
                                _textureRectangles.Add(FalconTextures.RWR, ParseRttRectangle(tokens));
                                break;

                            case "mfdleft":
                                _textureRectangles.Add(FalconTextures.MFDLeft, ParseRttRectangle(tokens));
                                break;

                            case "mfdright":
                                _textureRectangles.Add(FalconTextures.MFDRight, ParseRttRectangle(tokens));
                                break;
                        }
                    }
                }
            }
        }

        private Rect ParseRttRectangle(string[] tokens)
        {
            int rttX1 = int.Parse(tokens[10]);
            int rttX2 = int.Parse(tokens[12]);
            int rttY1 = int.Parse(tokens[11]);
            int rttY2 = int.Parse(tokens[13]);

            return new Rect(rttX1, rttY1, rttX2 - rttX1, rttY2 - rttY1);
        }
        public override void MouseDown(Point location)
        {
            // No-Op
        }

        public override void MouseDrag(System.Windows.Point location)
        {
            // No-Op
        }

        public override void MouseUp(System.Windows.Point location)
        {
            // No-Op
        }

        public override void WriteXml(System.Xml.XmlWriter writer)
        {
            base.WriteXml(writer);
            writer.WriteElementString("TransparentBackground", TransparencyEnabled.ToString(System.Globalization.CultureInfo.InvariantCulture));
            writer.WriteElementString("HorizontalImageFlip", HorizontalFlipEnabled.ToString(System.Globalization.CultureInfo.InvariantCulture));
            writer.WriteElementString("VerticalImageFlip", VerticalFlipEnabled.ToString(System.Globalization.CultureInfo.InvariantCulture));
        }

        public override void ReadXml(System.Xml.XmlReader reader)
        {
            base.ReadXml(reader);

            try
            {
                TransparencyEnabled = bool.Parse(reader.ReadElementString("TransparentBackground"));
            }
            catch
            {
                TransparencyEnabled = false;
            }

            try
            {
                HorizontalFlipEnabled = bool.Parse(reader.ReadElementString("HorizontalImageFlip"));
            }
            catch
            {
                HorizontalFlipEnabled = false;
            }

            try
            {
                VerticalFlipEnabled = bool.Parse(reader.ReadElementString("VerticalImageFlip"));
            }
            catch
            {
                VerticalFlipEnabled = false;
            }
        }
    }
}
