//  Copyright 2014 Craig Courtney
//  Copyright 2020 Ammo Goettsch
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

namespace GadrocsWorkshop.Helios.Interfaces.DCS.A10C.Functions
{
    using GadrocsWorkshop.Helios.Interfaces.DCS.Common;
    using GadrocsWorkshop.Helios.UDPInterface;
    using GadrocsWorkshop.Helios.Util;
    using System;
    using System.Globalization;

    public class TACANChannel : DCSFunctionPair
    {
        private static readonly ExportDataElement[] DataElementsTemplate = { new DCSDataElement("2263", null, false), new DCSDataElement("266", "%0.1f", false) };

        private static readonly BindingValue XValue = new BindingValue(1);
        private static readonly BindingValue YValue = new BindingValue(2);

        private double _hundreds;
        private double _tens;
        private double _ones;

        private HeliosValue _channel;
        private HeliosValue _mode;

        public TACANChannel(BaseUDPInterface sourceInterface)
            : base(sourceInterface,
                   "TACAN", "Channel", "Currently tuned TACAN channel.",
                   "TACAN", "Channel Mode", "Current TACAN channel mode."
                  )
        {
            DoBuild();
        }

        // deserialization constructor
        public TACANChannel(BaseUDPInterface sourceInterface, System.Runtime.Serialization.StreamingContext context)
            : base(sourceInterface, context)
        {
            // no code
        }

        public override void BuildAfterDeserialization()
        {
            DoBuild();
        }

        private void DoBuild()
        {
            _channel = new HeliosValue(SourceInterface, BindingValue.Empty, SerializedDeviceName, SerializedFunctionName,
                SerializedDescription, "", BindingValueUnits.Numeric);
            Values.Add(_channel);
            Triggers.Add(_channel);

            _mode = new HeliosValue(SourceInterface, BindingValue.Empty, SerializedDeviceName2, SerializedFunctionName2,
                SerializedDescription2, "1=X, 2=Y", BindingValueUnits.Numeric);
            Values.Add(_mode);
            Triggers.Add(_mode);
        }

        protected override ExportDataElement[] DefaultDataElements => DataElementsTemplate;

        public override void ProcessNetworkData(string id, string value)
        {
            switch (id)
            {
                case "2263":
                    string[] parts = Tokenizer.TokenizeAtLeast(value, 3, ';');
                    _hundreds = ClampedParse(parts[0], 100d);
                    _tens = ClampedParse(parts[1], 10d);
                    _ones = ClampedParse(parts[2], 1d);

                    double channel = _hundreds + _tens + _ones;
                    _channel.SetValue(new BindingValue(channel), false);
                    break;
                case "266":
                    switch (value)
                    {
                        case "0.0":
                            _mode.SetValue(XValue, false);
                            break;
                        case "0.1":
                            _mode.SetValue(YValue, false);
                            break;
                    }
                    break;
            }
        }

        private double ClampedParse(string value, double scale)
        {
            if (!double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture.NumberFormat,
                out double scaledValue))
            {
                return scaledValue;
            }

            if (scaledValue < 1.0d)
            {
                scaledValue = Math.Truncate(scaledValue * 10d) * scale;
            }
            else
            {
                scaledValue = 0d;
            }
            return scaledValue;
        }

        public override void Reset()
        {
            _channel.SetValue(BindingValue.Empty, true);
            _mode.SetValue(BindingValue.Empty, true);
        }
    }
}
