// Copyright 2021 Ammo Goettsch
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

using Newtonsoft.Json;

namespace GadrocsWorkshop.Helios.Json.BindingValueUnit
{
    public class ConverterForJsonNet : JsonConverter
    {
        #region Overrides

        public override bool CanConvert(System.Type objectType) =>
            typeof(Helios.BindingValueUnit).IsAssignableFrom(objectType);

        public override object ReadJson(JsonReader reader, System.Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.String)
            {
                throw new JsonException("BindingValueUnit must be encoded as string in this implementation");
            }

            string stringValue = reader.Value?.ToString();
            if (stringValue == null)
            {
                throw new JsonException(
                    $"Unable to convert null to \"{objectType}\".");
            }

            object value = BindingValueUnits.FetchUnitByName(stringValue);
            if (value == null)
            {
                throw new JsonException(
                    $"Unable to convert \"{stringValue}\" to \"{objectType}\".");
            }

            return value;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(BindingValueUnits.FetchUnitName(value as Helios.BindingValueUnit));
        }

        #endregion
    }
}