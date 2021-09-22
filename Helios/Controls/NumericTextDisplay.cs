// Copyright 2021 Helios Contributors
// 
// Helios is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// Helios is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
// 

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;
using GadrocsWorkshop.Helios.ComponentModel;

namespace GadrocsWorkshop.Helios.Controls
{
    [HeliosControl(TYPE_ID, "Numeric Text Display", "Text Displays", typeof(TextDisplayRenderer))]
    public class NumericTextDisplay : TextDisplayRect
    {
        public const string TYPE_ID = "Helios.Base.NumericTextDisplay";
        protected const int DEFAULT_PRECISION = 0;

        private HeliosValue _value;

        /// <summary>
        /// backing field for property Unit, contains
        /// the binding value unit used for this numeric display, used to convert inputs
        /// </summary>
        private BindingValueUnit _unit = BindingValueUnits.Numeric;

        /// <summary>
        /// backing field for property Precision, contains
        /// number of digits after the decimal point to display, or 0
        /// </summary>
        private int _precision = DEFAULT_PRECISION;

        public NumericTextDisplay()
            : base("NumericTextDisplay", new System.Windows.Size(100, 25))
        {
            BuildValue();
        }

        /// <summary>
        /// rebuild the Helios value to use the configured unit type
        /// </summary>
        private void BuildValue()
        {
            HeliosValue oldValue = _value;
            if (oldValue != null)
            {
                Values.Remove(oldValue);
                Actions.Remove(oldValue);
                oldValue.Execute -= Value_Execute;
            }

            _value = new HeliosValue(this, new BindingValue(0.0), "", "Number", "The number to display",
                "The value to display.", Unit);
            _value.Execute += Value_Execute;
            Values.Add(_value);
            Actions.Add(_value);

            if (oldValue != null)
            {
                // update any bindings to it, since we cannot change the target of a binding
                List<HeliosBinding> redirect = InputBindings.Where(binding => ReferenceEquals(binding.Action, oldValue)).ToList();
                redirect.ForEach(binding =>
                {
                    binding.Action = _value;
                });
            }
        }

        #region Event Handlers

        private void Value_Execute(object action, HeliosActionEventArgs e)
        {
            BeginTriggerBypass(e.BypassCascadingTriggers);
            TextValue = e.Value.DoubleValue.ToString($"F{Math.Max(0, Precision)}", CultureInfo.InvariantCulture);
            EndTriggerBypass(e.BypassCascadingTriggers);
        }

        #endregion

        #region Overrides

        protected override void OnTextValueChange()
        {
            if (!double.TryParse(_textValue, NumberStyles.Any, CultureInfo.InvariantCulture, out double value))
            {
                value = 0d;
            }

            _value.SetValue(new BindingValue(value), BypassTriggers);
        }

        protected override void ReadAdditionalXml(XmlReader reader)
        {
            base.ReadAdditionalXml(reader);

            if (reader.Name == "Unit")
            {
                Unit = BindingValueUnits.FetchUnitByName(reader.ReadElementString("Unit")) ??
                       BindingValueUnits.Numeric;
            }

            if (reader.Name == "Precision")
            {
                int.TryParse(reader.ReadElementString("Precision"), NumberStyles.Integer,
                    CultureInfo.InvariantCulture, out _precision);
            }
        }

        protected override void WriteAdditionalXml(XmlWriter writer)
        {
            base.WriteAdditionalXml(writer);

            if (!ReferenceEquals(Unit, BindingValueUnits.Numeric))
            {
                string className = BindingValueUnits.FetchUnitName(Unit);
                if (className != null)
                {
                    writer.WriteElementString("Unit", className);
                }
            }

            if (Precision != DEFAULT_PRECISION)
            {
                writer.WriteElementString("Precision", Precision.ToString(CultureInfo.InvariantCulture));
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// the binding value unit used for this numeric display, used to convert inputs
        /// </summary>
        public BindingValueUnit Unit
        {
            get => _unit;
            set
            {
                if (ReferenceEquals(_unit, value))
                {
                    return;
                }

                BindingValueUnit oldValue = _unit;
                _unit = value;

                // now actually change unit
                BuildValue();
                OnPropertyChanged(nameof(Unit), oldValue, value, true);
            }
        }

        /// <summary>
        /// number of digits after the decimal point to display, or 0
        /// </summary>
        public int Precision
        {
            get => _precision;
            set
            {
                if (_precision == value)
                {
                    return;
                }

                int oldValue = _precision;
                _precision = value;
                OnPropertyChanged(nameof(Precision), oldValue, value, true);
            }
        }

        #endregion
    }
}