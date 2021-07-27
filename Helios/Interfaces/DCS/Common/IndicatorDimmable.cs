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

using System.Globalization;
using GadrocsWorkshop.Helios.UDPInterface;

namespace GadrocsWorkshop.Helios.Interfaces.DCS.Common
{
    public class IndicatorDimmable : FlagValue
    {
        private readonly string _id;
        private readonly string _format;
        private HeliosValue _brightness;

        public IndicatorDimmable(BaseUDPInterface sourceInterface, string id, string device, string name,
            string description, string exportFormat)
            : base(sourceInterface, id, device, name, description, exportFormat)
        {
            _id = id;
            _format = exportFormat;
            DoBuild();
        }

        // deserialization constructor
        public IndicatorDimmable(BaseUDPInterface sourceInterface, System.Runtime.Serialization.StreamingContext context)
            : base(sourceInterface, context)
        {
            // no code
        }

        private void DoBuild()
        {
            _brightness = new HeliosValue(SourceInterface, BindingValue.Empty, SerializedDeviceName,
                $"{SerializedFunctionName} brightness", $"{SerializedDescription} brightness percentage", "", BindingValueUnits.Numeric);
            Values.Add(_brightness);
            Triggers.Add(_brightness);
        }

        public override void BuildAfterDeserialization()
        {
            base.BuildAfterDeserialization();
            DoBuild();
        }

        public override void ProcessNetworkData(string id, string value)
        {
            if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture.NumberFormat,
                out double parsedValue))
            {
                _brightness.SetValue(
                    new BindingValue((parsedValue > 1 ? 1d : parsedValue < 0 ? 0d : parsedValue) * 100), false);
            }

            base.ProcessNetworkData(id, value);
        }

        protected override ExportDataElement[] DefaultDataElements => new ExportDataElement[] {new DCSDataElement(_id, _format, true)};

        public override void Reset()
        {
            base.Reset();
            _brightness.SetValue(BindingValue.Empty, true);
        }
    }
}