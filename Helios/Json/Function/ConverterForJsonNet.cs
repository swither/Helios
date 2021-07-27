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

using System;
using GadrocsWorkshop.Helios.Interfaces.Capabilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GadrocsWorkshop.Helios.Json.Function
{
    public class ConverterForJsonNet<F, I> : JsonConverter
        where F : class, IBuildableFunction where I : class, ISoftInterface
    {
        #region Overrides

        public override bool CanConvert(Type objectType) => typeof(F).IsAssignableFrom(objectType);

        public override object ReadJson(JsonReader reader,
            Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject json = JObject.Load(reader);
            I parent = serializer.Context.Context as I;

            string typeName = json.Value<string>("Type");
            Type classType = parent.ResolveFunctionType(typeName);

            System.Reflection.ConstructorInfo deserializationConstructor =
                classType.GetConstructor(new[] {typeof(I), typeof(System.Runtime.Serialization.StreamingContext)});
            F item;
            if (deserializationConstructor != null)
            {
                // create an instance of the correct type, using the preferred constructor
                item = (F) Activator.CreateInstance(classType, parent, serializer.Context);
            }
            else
            {
                System.Reflection.ConstructorInfo parentOnlyConstructor = classType.GetConstructor(new[] {typeof(I)});
                if (parentOnlyConstructor == null)
                {
                    throw new JsonException(
                        $"{classType} does not define a constructor from {typeof(I)} and cannot be deserialized from JSON");
                }

                // create an instance of the correct type
                item = (F) Activator.CreateInstance(classType, serializer.Context.Context as I);
            }


            // populate all the fields/properties that are persisted
            serializer.Populate(json.CreateReader(), item);

            // now run the equivalent of the constructor work
            item.BuildAfterDeserialization();
            return item;
        }

        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer,
            object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}