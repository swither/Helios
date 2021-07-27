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

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using GadrocsWorkshop.Helios.Interfaces.DCS.Common;
using GadrocsWorkshop.Helios.Json.BindingValue;
using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Schema.Generation;

namespace GadrocsWorkshop.Helios.Json
{
    public class InterfaceExport
    {
        public static List<System.Type> KnownTypes = new List<System.Type>
        {
            // REVISIT: could we walk the assemblies and figure out which types are concrete?
            // or are there concrete types that aren't intended for use?
            typeof(Interfaces.DCS.A10C.Functions.Altimeter),
            typeof(Interfaces.DCS.A10C.Functions.HSIMiles),
            typeof(Interfaces.DCS.A10C.Functions.ILSFrequency),
            typeof(Interfaces.DCS.A10C.Functions.TACANChannel),
            typeof(Interfaces.DCS.A10C.Functions.TotalFuel),
            typeof(Interfaces.DCS.A10C.Functions.VHFPresetSelector),
            typeof(Interfaces.DCS.A10C.Functions.VHFRadioEncoder),
            typeof(Interfaces.DCS.A10C.Functions.VHFRadioEncoder1),
            typeof(Interfaces.DCS.A10C.Functions.VHFRadioEncoder3),
            typeof(Interfaces.DCS.A10C.Functions.VHFRadioEncoder4),
            typeof(Interfaces.DCS.AV8B.Functions.Altimeter),
            typeof(Interfaces.DCS.AV8B.Functions.Digits2Display),
            typeof(Interfaces.DCS.AV8B.Functions.Digits3Display),
            typeof(Interfaces.DCS.AV8B.Functions.Digits4Display),
            typeof(Interfaces.DCS.AV8B.Functions.FuelTotalDisplay),
            typeof(Interfaces.DCS.AV8B.Functions.SMCFuzeDisplay),
            typeof(Interfaces.DCS.AV8B.Functions.SMCMultipleDisplay),
            typeof(Interfaces.DCS.BlackShark.Functions.HSIRange),
            typeof(Interfaces.DCS.BlackShark.Functions.LatitudeEntry),
            typeof(Interfaces.DCS.BlackShark.Functions.MagVariation),
            typeof(Interfaces.DCS.BlackShark.Functions.VHF2Rotary1),
            typeof(Interfaces.DCS.BlackShark.Functions.VHF2Rotary23),
            typeof(Interfaces.DCS.BlackShark.Functions.VHF2Rotary4),
            typeof(AbsoluteEncoder),
            typeof(Axis),
            typeof(DualNetworkValue),
            typeof(DualRocker),
            typeof(FlagValue),
            typeof(GuardedSwitch),
            typeof(HatSwitch),
            typeof(IndicatorPushButton),
            typeof(LockedEncoder),
            typeof(NetworkValue),
            typeof(PushButton),
            typeof(Rocker),
            typeof(RotaryEncoder),
            typeof(ScaledNetworkValue),
            typeof(SilentValueConsumer),
            typeof(Switch),
            typeof(Text),
            typeof(Interfaces.DCS.FA18C.Functions.Altimeter)
        };

        /// <summary>
        /// configures all our generator options except the special handling for NetworkFunction, so we can disable it
        /// </summary>
        /// <param name="generator"></param>
        public static void ConfigureGenerator(JSchemaGenerator generator)
        {
            generator.SchemaReferenceHandling = SchemaReferenceHandling.Objects;
            // generator.SchemaIdGenerationHandling = SchemaIdGenerationHandling.TypeName;
            generator.DefaultRequired = Required.DisallowNull;
            generator.GenerationProviders.Add(new BindingValueUnit.SchemaGenerationProviderForJsonNet());
        }

        public void GenerateInterfaceJson()
        {
            UDPInterface.BaseUDPInterface.IsWritingFunctionsToJson = true;
            foreach (HeliosInterfaceDescriptor descriptor in ConfigManager.ModuleManager.InterfaceDescriptors)
            {
                if (descriptor.ParentTypeIdentifier != null)
                {
                    // don't serialize child interfaces
                    continue;
                }

                HeliosInterface newInterface = descriptor.CreateInstance();
                if (newInterface is UDPInterface.BaseUDPInterface udpInterface)
                {
                    string name = udpInterface.TypeIdentifier;
                    if ((name == null) || (name.Length < 1))
                    {
                        name = udpInterface.Name;
                    }

                    string moduleName = name;
                    IEnumerable<string> vehicles = new string[0];
                    if (udpInterface is DCSInterface dcs)
                    {
                        moduleName = dcs.ModuleName;
                        vehicles = dcs.Vehicles;
                    }

                    InterfaceFile<UDPInterface.NetworkFunction> jsonObject =
                        new InterfaceFile<UDPInterface.NetworkFunction>
                        {
                            VersionString =
                                VersionChecker.VersionToString(Assembly.GetEntryAssembly().GetName().Version),
                            Module = moduleName,
                            Functions = udpInterface.Functions.Where(func => func.Serializable),
                            Vehicles = vehicles
                        };
                    string jsonPath = Path.Combine(ConfigManager.DocumentPath, "Interfaces", $"{name}.hif.json");
                    Directory.CreateDirectory(Path.GetDirectoryName(jsonPath));
                    File.WriteAllText(
                        jsonPath,
                        JsonConvert.SerializeObject(
                            jsonObject,
                            new JsonSerializerSettings
                            {
                                Formatting = Formatting.Indented,
                                Converters = new List<JsonConverter>
                                {
                                    new BindingValueUnit.ConverterForJsonNet(),
                                    new ConverterForJsonNet()
                                }
                            })
                    );
                }
            }

            UDPInterface.BaseUDPInterface.IsWritingFunctionsToJson = false;
        }

        public void GenerateInterfaceSchema()
        {
            JsonLicenses.LoadLicenses();
            JSchemaGenerator generator = new JSchemaGenerator();
            ConfigureGenerator(generator);
            generator.GenerationProviders.Add(new NetworkFunction.SchemaGenerationProviderForJsonNet());

            JSchema functions = new JSchema();
            foreach (System.Type knownType in KnownTypes)
            {
                functions.OneOf.Add(generator.Generate(knownType));
            }

            JSchema schema = generator.Generate(typeof(InterfaceFile<UDPInterface.NetworkFunction>));
            schema.Properties["functions"].Items.Clear();
            schema.Properties["functions"].Items.Add(functions);

            string schemaPath = Path.Combine(ConfigManager.DocumentPath, "Interfaces", "hif.schema.json");
            Directory.CreateDirectory(Path.GetDirectoryName(schemaPath));
            using (StreamWriter file = File.CreateText(schemaPath))
            using (JsonTextWriter writer = new JsonTextWriter(file)
            {
                Formatting = Formatting.Indented
            })
            {
                schema.WriteTo(writer, new JSchemaWriterSettings());
            }
        }
    }
}