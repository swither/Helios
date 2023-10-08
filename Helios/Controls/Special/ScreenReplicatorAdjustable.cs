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
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Xml;
using GadrocsWorkshop.Helios.ComponentModel;
using NLog;

namespace GadrocsWorkshop.Helios.Controls.Special
{
    [HeliosControl("Helios.Base.ScreenReplicatorAdjustable", "Screen Replicator (Adjustable)", "Special Controls", typeof(ScreenReplicatorAdjustableRenderer))]
    public class ScreenReplicatorAdjustable : ScreenReplicator
    {
        private ImageAdjustment _imageAdjustments = new ImageAdjustment();
        private HeliosValue _hvBrightness;
        private HeliosValue _hvRedBrightness;
        private HeliosValue _hvGreenBrightness;
        private HeliosValue _hvBlueBrightness;
        private HeliosValue _hvContrast;
        private HeliosValue _hvGamma;
        private HeliosAction _haImageReset;

        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public ScreenReplicatorAdjustable()
            : base("Adjustable Screen Shot Extractor", new Size(300, 300))
        {
            _hvBrightness = new HeliosValue(this, new BindingValue(1.0d), "", "Brightness adjustment value", "Value 0.0 and 2.0", "", BindingValueUnits.Numeric);
            _hvBrightness.Execute += new HeliosActionHandler(Brightness_Execute);
            Actions.Add(_hvBrightness);
            Values.Add(_hvBrightness);
            _hvRedBrightness = new HeliosValue(this, new BindingValue(1.0d), "", "Red Brightness adjustment value", "Value 0.0 and 2.0", "", BindingValueUnits.Numeric);
            _hvRedBrightness.Execute += new HeliosActionHandler(RedBrightness_Execute);
            Actions.Add(_hvRedBrightness);
            Values.Add(_hvRedBrightness);
            _hvGreenBrightness = new HeliosValue(this, new BindingValue(1.0d), "", "Green Brightness adjustment value", "Value 0.0 and 2.0", "", BindingValueUnits.Numeric);
            _hvGreenBrightness.Execute += new HeliosActionHandler(GreenBrightness_Execute);
            Actions.Add(_hvGreenBrightness);
            Values.Add(_hvGreenBrightness);
            _hvBlueBrightness = new HeliosValue(this, new BindingValue(1.0d), "", "Blue Brightness adjustment value", "Value 0.0 and 2.0", "", BindingValueUnits.Numeric);
            _hvBlueBrightness.Execute += new HeliosActionHandler(BlueBrightness_Execute);
            Actions.Add(_hvBlueBrightness);
            Values.Add(_hvBlueBrightness);
            _hvContrast = new HeliosValue(this, new BindingValue(1.0d), "", "Contrast adjustment value", "Value 0.1 and 2.0", "", BindingValueUnits.Numeric);
            _hvContrast.Execute += new HeliosActionHandler(Contrast_Execute);
            Actions.Add(_hvContrast);
            Values.Add(_hvContrast); 
            _hvGamma = new HeliosValue(this, new BindingValue(1.0d), "", "Gamma adjustment value", "Value 0.1 and 2.0", "", BindingValueUnits.Numeric);
            _hvGamma.Execute += new HeliosActionHandler(Gamma_Execute);
            Actions.Add(_hvGamma);
            Values.Add(_hvGamma);
            _haImageReset = new HeliosAction(this,"","","Reset image","Sets the image adjustments to their neutral values.");
            _haImageReset.Execute += new HeliosActionHandler(ResetImageAdjustments_Execute);
            Actions.Add(_haImageReset);
        }
        void Brightness_Execute(object action, HeliosActionEventArgs e)
        {
            _hvBrightness.SetValue(e.Value, e.BypassCascadingTriggers);
            Brightness = (float)Convert.ToDouble(e.Value.DoubleValue);
        }
        void RedBrightness_Execute(object action, HeliosActionEventArgs e)
        {
            _hvRedBrightness.SetValue(e.Value, e.BypassCascadingTriggers);
            RedBrightness = (float)Convert.ToDouble(e.Value.DoubleValue);
        }
        void GreenBrightness_Execute(object action, HeliosActionEventArgs e)
        {
            _hvGreenBrightness.SetValue(e.Value, e.BypassCascadingTriggers);
            GreenBrightness = (float)Convert.ToDouble(e.Value.DoubleValue);
        }
        void BlueBrightness_Execute(object action, HeliosActionEventArgs e)
        {
            _hvBlueBrightness.SetValue(e.Value, e.BypassCascadingTriggers);
            BlueBrightness = (float)Convert.ToDouble(e.Value.DoubleValue);
        }
        void Contrast_Execute(object action, HeliosActionEventArgs e)
        {
            _hvContrast.SetValue(e.Value, e.BypassCascadingTriggers);
            Contrast = (float)Convert.ToDouble(e.Value.DoubleValue);
        }
        void Gamma_Execute(object action, HeliosActionEventArgs e)
        {
            _hvGamma.SetValue(e.Value, e.BypassCascadingTriggers);
            Gamma = (float)Convert.ToDouble(e.Value.DoubleValue);
        }
        void ResetImageAdjustments_Execute(object action, HeliosActionEventArgs e)
        {
            _hvBrightness.SetValue(new BindingValue(1.0d), e.BypassCascadingTriggers);
            _hvRedBrightness.SetValue(new BindingValue(1.0d), e.BypassCascadingTriggers);
            _hvGreenBrightness.SetValue(new BindingValue(1.0d), e.BypassCascadingTriggers);
            _hvBlueBrightness.SetValue(new BindingValue(1.0d), e.BypassCascadingTriggers);
            _hvContrast.SetValue(new BindingValue(1.0d), e.BypassCascadingTriggers);
            _hvGamma.SetValue(new BindingValue(1.0d), e.BypassCascadingTriggers);
            Brightness = 1.0f;
            RedBrightness = 1.0f;
            GreenBrightness = 1.0f;
            BlueBrightness = 1.0f;
            Gamma = 1.0f;
            Contrast = 1.0f;
        }

        #region Properties

        public float Brightness
        {
            get => _imageAdjustments.Brightness;
            set
            {
                if (!_imageAdjustments.Brightness.Equals(value))
                {
                    float oldValue = _imageAdjustments.Brightness;
                    _imageAdjustments.Brightness = value;
                    _imageAdjustments.RedBrightness = value;
                    _imageAdjustments.GreenBrightness = value;
                    _imageAdjustments.BlueBrightness = value;
                    OnPropertyChanged("Brightness", oldValue, value, true);
                    OnDisplayUpdate();
                }
            }
        }

        public float RedBrightness
        {
            get => _imageAdjustments.RedBrightness;
            set
            {
                if (!_imageAdjustments.RedBrightness.Equals(value))
                {
                    float oldValue = _imageAdjustments.RedBrightness;
                    _imageAdjustments.RedBrightness = value;
                    OnPropertyChanged("RedBrightness", oldValue, value, true);
                    OnDisplayUpdate();
                }
            }
        }

        public float GreenBrightness
        {
            get => _imageAdjustments.GreenBrightness;
            set
            {
                if (!_imageAdjustments.GreenBrightness.Equals(value))
                {
                    float oldValue = _imageAdjustments.GreenBrightness;
                    _imageAdjustments.GreenBrightness = value;
                    OnPropertyChanged("GreenBrightness", oldValue, value, true);
                    OnDisplayUpdate();
                }
            }
        }

        public float BlueBrightness
        {
            get => _imageAdjustments.BlueBrightness;
            set
            {
                if (!_imageAdjustments.BlueBrightness.Equals(value))
                {
                    float oldValue = _imageAdjustments.BlueBrightness;
                    _imageAdjustments.BlueBrightness = value;
                    OnPropertyChanged("BlueBrightness", oldValue, value, true);
                    OnDisplayUpdate();
                }
            }
        }
        public float AlphaBrightness
        {
            get => _imageAdjustments.AlphaBrightness;
            set
            {
                if (!_imageAdjustments.AlphaBrightness.Equals(value))
                {
                    float oldValue = _imageAdjustments.AlphaBrightness;
                    _imageAdjustments.AlphaBrightness = value;
                    OnPropertyChanged("AlphaBrightness", oldValue, value, true);
                    OnDisplayUpdate();
                }
            }
        }

        public float Contrast
        {
            get => _imageAdjustments.Contrast;
            set
            {
                if (!_imageAdjustments.Contrast.Equals(value))
                {
                    float oldValue = _imageAdjustments.Contrast;
                    _imageAdjustments.Contrast = value;
                    OnPropertyChanged("Contrast", oldValue, value, true);
                    OnDisplayUpdate();
                }
            }
        }
        public float Gamma
        {
            get => _imageAdjustments.Gamma;
            set
            {
                if (!_imageAdjustments.Gamma.Equals(value))
                {
                    float oldValue = _imageAdjustments.Gamma;
                    _imageAdjustments.Gamma = value;
                    OnPropertyChanged("Gamma", oldValue, value, true);
                    OnDisplayUpdate();
                }
            }
        }
        public ImageAdjustment ImageAdjustment
        {
            get => _imageAdjustments;
        }

        #endregion

        protected override void OnProfileChanged(HeliosProfile oldProfile)
        {
            base.OnProfileChanged(oldProfile);

        }


        public override void MouseDown(System.Windows.Point location)
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

        public override void WriteXml(XmlWriter writer)
        {

            base.WriteXml(writer);
            if (_imageAdjustments != null)
            {
                writer.WriteStartElement("ImageAdjustments");
                writer.WriteElementString("Brightness", _imageAdjustments.Brightness.ToString(CultureInfo.InvariantCulture));
                writer.WriteElementString("RedBrightness", _imageAdjustments.RedBrightness.ToString(CultureInfo.InvariantCulture));
                writer.WriteElementString("GreenBrightness", _imageAdjustments.GreenBrightness.ToString(CultureInfo.InvariantCulture));
                writer.WriteElementString("BlueBrightness", _imageAdjustments.BlueBrightness.ToString(CultureInfo.InvariantCulture));
                writer.WriteElementString("AlphaBrightness", _imageAdjustments.AlphaBrightness.ToString(CultureInfo.InvariantCulture));
                writer.WriteElementString("Contrast", _imageAdjustments.Contrast.ToString(CultureInfo.InvariantCulture));
                writer.WriteElementString("Gamma", _imageAdjustments.Gamma.ToString(CultureInfo.InvariantCulture));
                writer.WriteEndElement();
            }
        }

        public override void ReadXml(XmlReader reader)
        {
            TypeConverter boolConverter = TypeDescriptor.GetConverter(typeof(bool));
            bool childrenProcessing = false;
            base.ReadXml(reader);
            while (reader.NodeType == XmlNodeType.Element)
            {
                switch (reader.Name)
                {
                    case "ImageAdjustments":
                        reader.ReadStartElement("ImageAdjustments");
                        _imageAdjustments.Brightness = float.Parse(reader.ReadElementString("Brightness"), CultureInfo.InvariantCulture);
                        _imageAdjustments.RedBrightness = float.Parse(reader.ReadElementString("RedBrightness"), CultureInfo.InvariantCulture);
                        _imageAdjustments.GreenBrightness = float.Parse(reader.ReadElementString("GreenBrightness"), CultureInfo.InvariantCulture);
                        _imageAdjustments.BlueBrightness = float.Parse(reader.ReadElementString("BlueBrightness"), CultureInfo.InvariantCulture);
                        _imageAdjustments.AlphaBrightness = float.Parse(reader.ReadElementString("AlphaBrightness"), CultureInfo.InvariantCulture);
                        _imageAdjustments.Contrast = float.Parse(reader.ReadElementString("Contrast"), CultureInfo.InvariantCulture);
                        _imageAdjustments.Gamma = float.Parse(reader.ReadElementString("Gamma"), CultureInfo.InvariantCulture);
                        reader.ReadEndElement(); 
                        break;
                    case "Children":
                        if (!reader.IsEmptyElement)
                        {
                            reader.ReadStartElement("Children");
                            childrenProcessing = true;
                        }
                        else
                        {
                            return;
                        }
                        break;
                    default:
                        // ignore unsupported settings
                        string elementName = reader.Name;
                        string discard = reader.ReadInnerXml();
                        Logger.Warn(
                            $"Ignored unsupported {GetType().Name} setting '{elementName}' with value '{discard}'");
                        break;
                }
            }
            if (childrenProcessing)
            {
                reader.ReadEndElement();
            }
        }
    }
}
