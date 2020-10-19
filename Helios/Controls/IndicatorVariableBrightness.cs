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
    using GadrocsWorkshop.Helios.ComponentModel;
    using System.Xml;

    [HeliosControl("Helios.Base.Indicator.VariableBrightness", "Caution Indicator with Brightness", "Indicators", typeof(IndicatorRenderer), HeliosControlFlags.NotShownInUI)]
    public class IndicatorVariableBrightness : Indicator
    {
        private int _defaultBrightnessLevel;

        public IndicatorVariableBrightness()
            : base()
        {
            HeliosValue brightness = new HeliosValue(this, new BindingValue(false), "", "indicator brightness", "Current Brightness % for this indicator.", "Brightness value of this indicator as a percentage.", BindingValueUnits.Numeric);
            brightness.Execute += new HeliosActionHandler(On_Execute);
            Values.Add(brightness);
            Actions.Add(brightness);
        }

        #region Properties
        public int DefaultBrightness
        {
            get
            {
                return _defaultBrightnessLevel;
            }
            set
            {
                if (!_defaultBrightnessLevel.Equals(value))
                {
                    int oldValue = _defaultBrightnessLevel;
                    _defaultBrightnessLevel = value;
                    OnPropertyChanged("DefaultBrightness", oldValue, value, true);
                    Refresh();
                }
            }
        }

        #endregion

        void On_Execute(object action, HeliosActionEventArgs e)
        {
            BeginTriggerBypass(e.BypassCascadingTriggers);
            On = e.Value.BoolValue;
            EndTriggerBypass(e.BypassCascadingTriggers);
        }

        public override void WriteXml(XmlWriter writer)
        {
            base.WriteXml(writer);
            writer.WriteElementString("DefaultBrightness", _defaultBrightnessLevel.ToString());
        }

        public override void ReadXml(XmlReader reader)
        {
            if (!int.TryParse(reader.ReadElementString("DefaultBrightness"), out _defaultBrightnessLevel)) {
                _defaultBrightnessLevel = 100;
            }
            base.ReadXml(reader);
        }
    }
}
