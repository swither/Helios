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
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization;
using GadrocsWorkshop.Helios.Interfaces.Capabilities;
using GadrocsWorkshop.Helios.Interfaces.DCS.Soft;
using Newtonsoft.Json;

namespace GadrocsWorkshop.Helios.Json
{
    public class InterfaceHeader
    {
        public static InterfaceHeader LoadHeader(string jsonPath)
        {
            using (StreamReader file = File.OpenText(jsonPath))
            {
                // XXX check schema first and see if we can get decent readable error messages from that
                JsonSerializer serializer = new JsonSerializer();

                InterfaceHeader loaded = (InterfaceHeader) serializer.Deserialize(file, typeof(InterfaceHeader));
                if (null == loaded)
                {
                    throw new Exception("failed to load any header from interface JSON");
                }

                return loaded;
            }
        }

        /// <summary>
        /// types of interfaces that can be declared in JSON
        /// </summary>
        public enum InterfaceType
        {
            [Description(
                "the default, contains only parameters and functions for an existing interface type and does not create a new type")]
            Existing,

            [Description("DCS interface")] DCS
        }

        internal static HeliosInterfaceDescriptor Load(string specFilePath)
        {
            InterfaceHeader header = LoadHeader(specFilePath);
            switch (header.Type)
            {
                case InterfaceType.Existing:
                {
                    // don't create a new type, this spec will be read by their owner
                    return null;
                }
                case InterfaceType.DCS:
                {
                    // XXX this should not know about DCS code; fix this by making a plugin attribute for interface descriptor factories
                    // remove .hif.json extension by removing extension twice
                    string typeIdentifier = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(specFilePath));
                    return new SoftInterfaceDescriptor(header.Name, typeIdentifier, header.Module, specFilePath);
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #region Properties

        [JsonProperty("source", Order = -16)] public string Source { get; set; } = "Helios";

        [JsonProperty("version", Order = -15)] public string VersionString { get; set; }

        [JsonProperty("commit", Order = -14)] public string Commit { get; set; } = "";

        [JsonProperty("type", Order = -13)]
        [JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        public InterfaceType Type { get; set; } = InterfaceType.Existing;

        /// <summary>
        /// display name of the interface
        /// </summary>
        [JsonProperty("name", Order = -12)]
        public string Name { get; set; }

        [JsonProperty("module", Order = -11)] public string Module { get; set; }

        #endregion
    }

    public class InterfaceFile<TFunction> : InterfaceHeader where TFunction : class, IBuildableFunction
    {
        public static InterfaceFile<TFunction> LoadFunctions<TInterface>(TInterface functionHost, string jsonPath)
            where TInterface : class, ISoftInterface
        {
            using (StreamReader file = File.OpenText(jsonPath))
            {
                // XXX check schema first and see if we can get decent readable error messages from that
                JsonSerializer serializer = new JsonSerializer();
                serializer.Converters.Add(new Function.ConverterForJsonNet<TFunction, TInterface>());
                serializer.Converters.Add(new BindingValueUnit.ConverterForJsonNet());

                // don't append to arrays
                serializer.ObjectCreationHandling = ObjectCreationHandling.Replace;

                // this isn't a cross process capable context, so we are zeroing all the bits of the valid states
                serializer.Context = new StreamingContext(0, functionHost);
                InterfaceFile<TFunction> loaded = (InterfaceFile<TFunction>) serializer.Deserialize(file,
                    typeof(InterfaceFile<TFunction>));
                if (null == loaded)
                {
                    throw new Exception("failed to load any functions from interface JSON");
                }

                return loaded;
            }
        }

        #region Properties

        [JsonProperty("vehicles", Order = -2)] public IEnumerable<string> Vehicles { get; set; }

        [JsonProperty("functions", Order = -1)] public IEnumerable<TFunction> Functions { get; set; }

        #endregion
    }
}