﻿//  Copyright 2014 Craig Courtney
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
    using GadrocsWorkshop.Helios.ComponentModel;
    using GadrocsWorkshop.Helios.Controls.Capabilities;
    using NLog;
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Media;
    using System.Xml;

    [HeliosControl("Helios.Base.PushButton", "Tactile Square", "Push Buttons", typeof(PushButtonRenderer))]
    public class PushButton : HeliosVisual, IConfigurableImageLocation, IRefreshableImage
    {
        private PushButtonType _buttonType;
        private string _imageFile = "{Helios}/Images/Buttons/tactile-dark-square.png";
        private string _pushedImageFile = "{Helios}/Images/Buttons/tactile-dark-square-in.png";

        private string _label = "";
        private TextFormat _labelFormat = new TextFormat();
        private Color _labelColor = Colors.White;
        private Point _labelPushedOffset = new Point(1, 1);

        private PushButtonGlyph _labelGlyph;
        private double _glyphScale = 0.8d;
        private Color _glyphColor = Colors.White;
        private double _glyphThickness = 2d;

        private bool _pushed;
        private bool _closed;

        private HeliosAction _pushAction;
        private HeliosAction _releaseAction;

        private HeliosTrigger _openTrigger;
        private HeliosTrigger _closedTrigger;
        private HeliosTrigger _pushedTrigger;
        private HeliosTrigger _releasedTrigger;
        private HeliosValue _value;
        private HeliosValue _pushedValue;

        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public PushButton()
            : base("Push Button", new Size(50, 50))
        {
            Setup();
        }

        protected PushButton(string name, Size nativeSize)
            : base(name, nativeSize)
        {
            Setup();
        }

        private void Setup()
        {
            _labelFormat.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(Format_PropertyChanged);

            _labelFormat.FontSize = 20;
            _labelFormat.HorizontalAlignment = TextHorizontalAlignment.Center;
            _labelFormat.VerticalAlignment = TextVerticalAlignment.Center;
            _referenceHeight = Height;
            _scalingMode = TextScalingMode.Height;

            _pushedTrigger = new HeliosTrigger(this, "", "", "pushed", "Fired when this button is pushed.", "Always returns true.", BindingValueUnits.Boolean);
            _releasedTrigger = new HeliosTrigger(this, "", "", "released", "Fired when this button is released.", "Always returns false.", BindingValueUnits.Boolean);
            _closedTrigger = new HeliosTrigger(this, "", "", "closed", "Fired when this button is in the closed state.", "Always returns true.", BindingValueUnits.Boolean);
            _openTrigger = new HeliosTrigger(this, "", "", "open", "Fired when this button is in the open state.", "Always returns false.", BindingValueUnits.Boolean);
            Triggers.Add(_pushedTrigger);
            Triggers.Add(_releasedTrigger);
            Triggers.Add(_closedTrigger);
            Triggers.Add(_openTrigger);

            _pushAction = new HeliosAction(this, "", "", "push", "Simulate physically pushing this button.");
            _pushAction.Execute += new HeliosActionHandler(Push_ExecuteAction);
            _releaseAction = new HeliosAction(this, "", "", "release", "Simulate physically removing pressure from this button.");
            _releaseAction.Execute += new HeliosActionHandler(Release_ExecuteAction);
            Actions.Add(_pushAction);
            Actions.Add(_releaseAction);

            _pushedValue = new HeliosValue(this, new BindingValue(false), "", "physical state", "Current state of this button.", "True if the button is currently pushed (either via pressure or toggle), otherwise false.  Setting this value will not fire pushed/released triggers, but will fire on/off triggers.  Directly setting this state to on for a momentary buttons will not auto release, the state must be manually reset to false.", BindingValueUnits.Boolean);
            _pushedValue.Execute += new HeliosActionHandler(PushedValue_Execute);
            Values.Add(_pushedValue);
            Actions.Add(_pushedValue);

            _value = new HeliosValue(this, new BindingValue(false), "", "circuit state", "Current open/closed state of this buttons circuit.", "True if the button is currently closed (on), otherwise false.", BindingValueUnits.Boolean);
            Values.Add(_value);
        }

        #region Properties

        public PushButtonGlyph Glyph
        {
            get
            {
                return _labelGlyph;
            }
            set
            {
                if (!_labelGlyph.Equals(value))
                {
                    PushButtonGlyph oldValue = _labelGlyph;
                    _labelGlyph = value;
                    OnPropertyChanged("Glyph", oldValue, value, true);
                    Refresh();
                }
            }
        }

        public double GlyphScale
        {
            get
            {
                return _glyphScale;
            }
            set
            {
                if (!_glyphScale.Equals(value))
                {
                    double oldValue = _glyphScale;
                    _glyphScale = value;
                    OnPropertyChanged("GlyphScale", oldValue, value, true);
                    Refresh();
                }
            }
        }

        public Color GlyphColor
        {
            get
            {
                return _glyphColor;
            }
            set
            {
                if (!_glyphColor.Equals(value))
                {
                    Color oldValue = _glyphColor;
                    _glyphColor = value;
                    OnPropertyChanged("GlyphColor", oldValue, value, true);
                    Refresh();
                }
            }
        }

        public double GlyphThickness
        {
            get
            {
                return _glyphThickness;
            }
            set
            {
                if (!_glyphThickness.Equals(value))
                {
                    double oldValue = _glyphThickness;
                    _glyphThickness = value;
                    OnPropertyChanged("GlyphThickness", oldValue, value, true);
                    Refresh();
                }
            }
        }


        public string Image
        {
            get
            {
                return _imageFile;
            }
            set
            {
                if ((_imageFile == null && value != null)
                    || (_imageFile != null && !_imageFile.Equals(value)))
                {
                    string oldValue = _imageFile;
                    _imageFile = value;
                    OnPropertyChanged("Image", oldValue, value, true);
                    Refresh();
                }
            }
        }

        public string PushedImage
        {
            get
            {
                return _pushedImageFile;
            }
            set
            {
                if ((_pushedImageFile == null && value != null)
                    || (_pushedImageFile != null && !_pushedImageFile.Equals(value)))
                {
                    string oldValue = _pushedImageFile;
                    _pushedImageFile = value;
                    OnPropertyChanged("PushedImage", oldValue, value, true);
                    Refresh();
                }
            }
        }

        public bool Pushed
        {
            get
            {
                return _pushed;
            }
            set
            {
                if (!_pushed.Equals(value))
                {
                    _pushed = value;
                    _pushedValue.SetValue(new BindingValue(_pushed), BypassTriggers);
                    OnPropertyChanged("Pushed", !value, value, false);
                    OnDisplayUpdate();
                }
            }
        }

        public bool IsClosed
        {
            get
            {
                return _closed;
            }
            set
            {
                if (!_closed.Equals(value))
                {
                    bool oldValue = _closed;

                    _closed = value;
                    _value.SetValue(new BindingValue(_pushed), BypassTriggers);
                    if (!BypassTriggers)
                    {
                        if (_closed)
                        {
                            _closedTrigger.FireTrigger(_value.Value);
                        }
                        else
                        {
                            _openTrigger.FireTrigger(_value.Value);
                        }
                    }
                    OnPropertyChanged("IsClosed", oldValue, value, false);
                }
            }
        }


        public PushButtonType ButtonType
        {
            get
            {
                return _buttonType;
            }
            set
            {
                if (!_buttonType.Equals(value))
                {
                    PushButtonType oldValue = _buttonType;
                    _buttonType = value;
                    OnPropertyChanged("ButtonType", oldValue, value, true);
                }
            }
        }

        public string Text
        {
            get
            {
                return _label;
            }
            set
            {
                if ((_label == null && value != null)
                    || (_label != null && !_label.Equals(value)))
                {
                    string oldValue = _label;
                    _label = value;
                    OnPropertyChanged("Text", oldValue, value, true);
                    OnDisplayUpdate();
                }
            }
        }

        public Color TextColor
        {
            get
            {
                return _labelColor;
            }
            set
            {
                if (!_labelColor.Equals(value))
                {
                    Color oldValue = _labelColor;
                    _labelColor = value;
                    OnPropertyChanged("TextColor", oldValue, value, true);
                    Refresh();
                }
            }
        }

        public TextFormat TextFormat
        {
            get { return _labelFormat; }
        }


        public Point TextPushOffset
        {
            get
            {
                return _labelPushedOffset;
            }
            set
            {
                if (!_labelPushedOffset.Equals(value))
                {
                    Point oldValue = _labelPushedOffset;
                    _labelPushedOffset = value;
                    OnPropertyChanged("TextPushOffset", oldValue, value, true);
                    Refresh();
                }
            }
        }
        /// <summary>
        /// backing field for property ScalingMode, contains
        /// the selected automatic font size scaling mode
        /// </summary>
        private TextScalingMode _scalingMode;

        /// <summary>
        /// the height this display had when the font size was configured
        /// </summary>
        private double _referenceHeight;

        /// <summary>
        /// the selected automatic font size scaling mode
        /// </summary>
        public TextScalingMode ScalingMode
        {
            get => _scalingMode;
            set
            {
                if (_scalingMode == value) return;
                TextScalingMode oldValue = _scalingMode;
                _scalingMode = value;
                OnPropertyChanged("ScalingMode", oldValue, value, true);
            }
        }
        #endregion

        void Format_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            PropertyNotificationEventArgs origArgs = e as PropertyNotificationEventArgs;
            if (origArgs != null)
            {
                OnPropertyChanged("TextFormat", origArgs);
            }
            OnDisplayUpdate();
        }

        void PushedValue_Execute(object action, HeliosActionEventArgs e)
        {
            BeginTriggerBypass(e.BypassCascadingTriggers);

            Pushed = e.Value.BoolValue;
            IsClosed = Pushed;

            EndTriggerBypass(e.BypassCascadingTriggers);
        }

        void Push_ExecuteAction(object action, HeliosActionEventArgs e)
        {
            BeginTriggerBypass(e.BypassCascadingTriggers);

            if (!BypassTriggers)
            {
                _pushedTrigger.FireTrigger(new BindingValue(true));
            }

            if (ButtonType == PushButtonType.Momentary)
            {
                Pushed = true;
                IsClosed = true;
            }
            else
            {
                Pushed = !Pushed;
                IsClosed = Pushed;
            }

            EndTriggerBypass(e.BypassCascadingTriggers);
        }

        void Release_ExecuteAction(object action, HeliosActionEventArgs e)
        {

            BeginTriggerBypass(e.BypassCascadingTriggers);

            if (!BypassTriggers)
            {
                _releasedTrigger.FireTrigger(new BindingValue(false));
            }

            if (ButtonType == PushButtonType.Momentary)
            {
                Pushed = false;
                IsClosed = false;
            }

            EndTriggerBypass(e.BypassCascadingTriggers);
        }

        void Label_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnDisplayUpdate();
        }

        protected override void OnPropertyChanged(PropertyNotificationEventArgs args)
        {
            OnDisplayUpdate();
            base.OnPropertyChanged(args);
        }

        public override void Reset()
        {
            base.Reset();
            
            BeginTriggerBypass(true);
            Pushed = false;
            IsClosed = false;
            EndTriggerBypass(true);
        }
        /// <summary>
        /// Performs a replace of text in this controls image names
        /// </summary>
        /// <param name="oldName"></param>
        /// <param name="newName"></param>
        public void ReplaceImageNames(string oldName, string newName)
        {
            Image = string.IsNullOrEmpty(Image) ? Image : string.IsNullOrEmpty(oldName) ? newName + Image : Image.Replace(oldName, newName);
            PushedImage = string.IsNullOrEmpty(PushedImage) ? PushedImage : string.IsNullOrEmpty(oldName) ? newName + PushedImage : PushedImage.Replace(oldName, newName);
        }
        public override bool ConditionalImageRefresh(string imageName)
        {
            if ((PushedImage ?? "").ToLower().Replace("/", @"\") == imageName ||
                (Image ?? "").ToLower().Replace("/", @"\") == imageName)
            {
                ImageRefresh = true;
                Refresh();
            }
            return ImageRefresh;
        }
        public override void MouseDown(Point location)
        {
            if (!BypassTriggers)
            {
                _pushedTrigger.FireTrigger(new BindingValue(true));
            }

            switch (ButtonType)
            {
                case PushButtonType.Momentary:
                    Pushed = true;
                    IsClosed = true;
                    break;

                case PushButtonType.Toggle:
                    Pushed = !Pushed;
                    IsClosed = Pushed;
                    break;
            }
        }

        public override void MouseDrag(Point location)
        {
            // No-Op
        }

        public override void MouseUp(Point location)
        {
            if (!BypassTriggers)
            {
                _releasedTrigger.FireTrigger(new BindingValue(false));
            }

            if (ButtonType == PushButtonType.Momentary)
            {
                Pushed = false;
                IsClosed = false;
            }
        }

        public override void ReadXml(XmlReader reader)
        {
            TypeConverter colorConverter = TypeDescriptor.GetConverter(typeof(Color));
            TypeConverter pointConverter = TypeDescriptor.GetConverter(typeof(Point));

            base.ReadXml(reader);

            ButtonType = (PushButtonType)Enum.Parse(typeof(PushButtonType), reader.ReadElementString("Type"));
            Image = reader.ReadElementString("Image");
            PushedImage = reader.ReadElementString("PushedImage");

            if (reader.Name.Equals("Glyph"))
            {
                reader.ReadStartElement("Glyph");

                Glyph = (PushButtonGlyph)Enum.Parse(typeof(PushButtonGlyph), reader.ReadElementString("Type"));
                GlyphColor = (Color)colorConverter.ConvertFromInvariantString(reader.ReadElementString("Color"));
                GlyphThickness = double.Parse(reader.ReadElementString("Thickness"), CultureInfo.InvariantCulture);
                GlyphScale = double.Parse(reader.ReadElementString("Scale"), CultureInfo.InvariantCulture);

                reader.ReadEndElement();
            }

            if (reader.Name.Equals("Text"))
            {
                reader.ReadStartElement("Text");

                TextColor = (Color)colorConverter.ConvertFromInvariantString(reader.ReadElementString("Color"));
                reader.ReadStartElement("Font");
                TextFormat.ReadXml(reader);
                reader.ReadEndElement();

                Text = reader.ReadElementString("Text");
                if (reader.Name.Equals("TextPushOffset"))
                {
                    TextPushOffset = (Point)pointConverter.ConvertFromInvariantString(reader.ReadElementString("TextPushOffset"));
                }
                if (reader.Name.Equals("ScalingMode") && Enum.TryParse(reader.ReadElementString("ScalingMode"), out TextScalingMode configured))
                {
                    ScalingMode = configured;
                }
                else
                {
                    ScalingMode = TextScalingMode.Legacy;
                }
                
            reader.ReadEndElement();
            // now the auto scaling has messed up our font size, so we restore it
            TextFormat.FontSize = TextFormat.ConfiguredFontSize;
            _referenceHeight = Height;

            }
        }

        public override void WriteXml(XmlWriter writer)
        {
            TypeConverter colorConverter = TypeDescriptor.GetConverter(typeof(Color));
            TypeConverter pointConverter = TypeDescriptor.GetConverter(typeof(Point));

            base.WriteXml(writer);

            writer.WriteElementString("Type", ButtonType.ToString());
            writer.WriteElementString("Image", Image);
            writer.WriteElementString("PushedImage", PushedImage);

            if (Glyph != PushButtonGlyph.None)
            {
                writer.WriteStartElement("Glyph");
                writer.WriteElementString("Type", Glyph.ToString());
                writer.WriteElementString("Color", colorConverter.ConvertToInvariantString(GlyphColor));
                writer.WriteElementString("Thickness", GlyphThickness.ToString(CultureInfo.InvariantCulture));
                writer.WriteElementString("Scale", GlyphScale.ToString(CultureInfo.InvariantCulture));
                writer.WriteEndElement();
            }

            if (Text != null && Text.Length > 0)
            {
                writer.WriteStartElement("Text");

                writer.WriteElementString("Color", colorConverter.ConvertToInvariantString(TextColor));

                writer.WriteStartElement("Font");
                TextFormat.WriteXml(writer);
                writer.WriteEndElement();

                writer.WriteElementString("Text", Text);
                if (TextPushOffset != new Point(1, 1))  // 1,1 is the value hard coded
                {
                    writer.WriteElementString("TextPushOffset", pointConverter.ConvertToString(null, CultureInfo.InvariantCulture, TextPushOffset));
                }
                if (ScalingMode != TextScalingMode.Legacy)
                {
                    writer.WriteElementString("ScalingMode", ScalingMode.ToString());
                }
                writer.WriteEndElement();
            }
        }

        #region Overrides of HeliosVisual

        public override void ScaleChildren(double scaleX, double scaleY)
        {
            //if (GlobalOptions.HasScaleAllText)
            //{
            //    _labelFormat.FontSize *= Math.Max(scaleX, scaleY);
            //}
            base.ScaleChildren(scaleX, scaleY);
        }
        // WARNING: this virtual method is called from the base constructor (indirectly)
        protected override void PostUpdateRectangle(Rect previous, Rect current)
        {
            if (GlobalOptions.HasScaleAllText)
            {
                switch (ScalingMode)
                {
                    case TextScalingMode.Height:
                        if (_referenceHeight < 0.001)
                        {
                            _labelFormat.FontSize = _labelFormat.ConfiguredFontSize;
                            break;
                        }
                        // avoid accumulating error from repeated resizing by calculating from a reference point
                        Logger.Debug("scaling font based on new height {Height} versus reference {ReferenceSize} at height {ReferenceHeight}",
                            current.Height, _labelFormat.ConfiguredFontSize, _referenceHeight);
                        _labelFormat.FontSize = Clamp(_labelFormat.ConfiguredFontSize * current.Height / _referenceHeight, 1, 2000);
                        break;
                    case TextScalingMode.None:
                        return;
                    case TextScalingMode.Legacy:
                        if (previous.Height != 0)
                        {
                            double scale = current.Height / previous.Height;
                            _labelFormat.FontSize = Clamp(scale * _labelFormat.FontSize, 1, 100);
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                Logger.Debug("Font Size " + _labelFormat.FontSize);
            }
        }

        #endregion
        private double Clamp(double value, double min, double max)
        {
            if (value < min)
            {
                return min;
            }
            if (value > max)
            {
                return max;
            }
            return value;
        }

    }
}
