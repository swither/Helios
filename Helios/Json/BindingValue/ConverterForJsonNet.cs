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

namespace GadrocsWorkshop.Helios.Json.BindingValue
{
    public class ConverterForJsonNet : JsonConverter
    {
        #region Overrides

        public override bool CanConvert(System.Type objectType) =>
            typeof(Helios.BindingValue).IsAssignableFrom(objectType);

        public override object ReadJson(JsonReader reader, System.Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault we intend to throw in the default case
            switch (reader.TokenType)
            {
                // ReSharper disable PossibleNullReferenceException we are checking token types
                case JsonToken.Boolean:
                    return new Helios.BindingValue((bool) reader.Value);
                case JsonToken.Float:
                    return new Helios.BindingValue((double) reader.Value);
                case JsonToken.Integer:
                    return new Helios.BindingValue((int) reader.Value);
                case JsonToken.String:
                    return new Helios.BindingValue((string) reader.Value);
                // ReSharper restore PossibleNullReferenceException
                default:
                    throw new JsonException("BindingValue must be encoded as native type token in this implementation");
            }
        }

        #endregion

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            Helios.BindingValue bindingValue = ((Helios.BindingValue) value);
            switch (bindingValue.NaitiveType)
            {
                case BindingValueType.Boolean:
                    writer.WriteValue(bindingValue.BoolValue);
                    break;
                case BindingValueType.Double:
                    writer.WriteValue(bindingValue.DoubleValue);
                    break;
                case BindingValueType.String:
                    writer.WriteValue(bindingValue.StringValue);
                    break;
                default:
                    throw new JsonException(
                        $"BindingValueType {bindingValue.NaitiveType.ToString()} unimplemented for serialization");
            }
        }
    }
}