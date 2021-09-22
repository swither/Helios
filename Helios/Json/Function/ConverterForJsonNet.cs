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
    public class ConverterForJsonNet<TFunction, TInterface> : JsonConverter
        where TFunction : class, IBuildableFunction where TInterface : class, ISoftInterface
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        public const string IGNORE_TYPE = "Ignored";

        #region Overrides

        public override bool CanConvert(Type objectType) => typeof(TFunction).IsAssignableFrom(objectType);

        public override object ReadJson(JsonReader reader,
            Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject json = JObject.Load(reader);
            TInterface parent = (TInterface)serializer.Context.Context;

            // let the container class dereference this type name
            string typeName = json.Value<string>(UDPInterface.NetworkFunction.HELIOS_TYPE_PROPERTY);
            if (typeName == null)
            {
                // not a recognized type, continue but log
                Logger.Warn("unsupported function {id} with key {key} detected in interface file",
                    json.Value<string>("id"), json.Value<string>("key"));
                return null;
            }

            if (string.Equals(typeName, IGNORE_TYPE, StringComparison.InvariantCultureIgnoreCase))
            {
                // silently ignore this record
                return null;
            }

            Type classType = parent.ResolveFunctionType(typeName);
            if (null == classType)
            {
                string unsafeString = typeName;
                if (unsafeString.Length > 256)
                {
                    unsafeString = unsafeString.Substring(0, 256);
                }
                throw new JsonException($"{unsafeString} is not a valid type that may appear in a Helios interface");
            }
             
            // now instantiate the function
            System.Reflection.ConstructorInfo deserializationConstructor =
                classType.GetConstructor(new[] {typeof(TInterface), typeof(System.Runtime.Serialization.StreamingContext)});
            TFunction item;
            if (deserializationConstructor != null)
            {
                // create an instance of the correct type, using the preferred constructor
                item = (TFunction) Activator.CreateInstance(classType, parent, serializer.Context);
            }
            else
            {
                System.Reflection.ConstructorInfo parentOnlyConstructor = classType.GetConstructor(new[] {typeof(TInterface)});
                if (parentOnlyConstructor == null)
                {
                    throw new JsonException(
                        $"{classType} does not define a constructor from {typeof(TInterface)} and cannot be deserialized from JSON");
                }

                // create an instance of the correct type
                item = (TFunction) Activator.CreateInstance(classType, serializer.Context.Context as TInterface);
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
            // this converter is only for reading
            throw new NotImplementedException();
        }

        #endregion
    }
}