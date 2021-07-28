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

namespace GadrocsWorkshop.Helios.Interfaces.DCS.AV8B.Functions
{
    using GadrocsWorkshop.Helios.Interfaces.DCS.Common;
    using GadrocsWorkshop.Helios.UDPInterface;
    using GadrocsWorkshop.Helios.Util;
    using Newtonsoft.Json;
    using System;
    using System.Globalization;

    public class Altimeter : DCSFunctionPair
    {
        private HeliosValue _altitude;
        private HeliosValue _pressure;

        [JsonProperty("altitudeComments")]
        private string _altitudeComments;

        [JsonProperty("pressureComments")]
        private string _pressureComments;

        public Altimeter(BaseUDPInterface sourceInterface, string instrumentClass, string altitudeId, string altitudeName, string altitudeDescription,string altitudeComments, string pressureId,string pressureName,string pressureDescription, string pressureComments)
            : base(sourceInterface, 
                  instrumentClass, altitudeName, altitudeDescription,
                  instrumentClass, pressureName, pressureDescription)
        {
            _altitudeComments = altitudeComments;
            _pressureComments = pressureComments;
            DefaultDataElements = new ExportDataElement[] { new DCSDataElement(altitudeId, null, true), new DCSDataElement(pressureId, null, true) };
            DoBuild();
        }

        // deserialization constructor
        public Altimeter(BaseUDPInterface sourceInterface, System.Runtime.Serialization.StreamingContext context)
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
            _altitude = new HeliosValue(SourceInterface, BindingValue.Empty, SerializedDeviceName, SerializedFunctionName,
                SerializedDescription, _altitudeComments, BindingValueUnits.Feet);
            Values.Add(_altitude);
            Triggers.Add(_altitude);

            _pressure = new HeliosValue(SourceInterface, BindingValue.Empty, SerializedDeviceName2, SerializedFunctionName2,
                SerializedDescription2, _pressureComments, BindingValueUnits.InchesOfMercury);
            Values.Add(_pressure);
            Triggers.Add(_pressure);
        }

        protected override ExportDataElement[] DefaultDataElements { get; }

        public override void ProcessNetworkData(string id, string value)
        {
            string[] parts;

            if (id == DataElements[0].ID)
            {
                parts = Tokenizer.TokenizeAtLeast(value, 3, ';');
                double tenThousands = ClampedParse(parts[0], 10000d);
                double thousands = ClampedParse(parts[1], 1000d);
                double hundreds = Parse(parts[2], 100d);

                double altitude = tenThousands + thousands + hundreds;
                _altitude.SetValue(new BindingValue(altitude), false);
            }
            else if (id == DataElements[1].ID)
            {
                parts = Tokenizer.TokenizeAtLeast(value, 4, ';');
                double tens = ClampedParse(parts[0], 10d);
                double ones = ClampedParse(parts[1], 1d);
                double tenths = ClampedParse(parts[2], .1d);
                double hundredths = Parse(parts[3], .01d);

                double pressure = tens + ones + tenths + hundredths;
                _pressure.SetValue(new BindingValue(pressure), false);
            }
        }

        private double Parse(string value, double scale)
        {
            if (!double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture.NumberFormat,
                out double scaledValue))
            {
                return scaledValue;
            }

            if (scaledValue < 1.0d)
            {
                scaledValue *= scale * 10d;
            }
            else
            {
                scaledValue = 0d;
            }
            return scaledValue;
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
            _altitude.SetValue(BindingValue.Empty, true);
            _pressure.SetValue(BindingValue.Empty, true);
        }
    }
}
