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

using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Schema.Generation;

namespace GadrocsWorkshop.Helios.Json.NetworkFunction
{
    public class SchemaGenerationProviderForJsonNet : JSchemaGenerationProvider
    {
        #region Overrides

        public override bool CanGenerateSchema(JSchemaTypeGenerationContext context) =>
            typeof(UDPInterface.NetworkFunction).IsAssignableFrom(context.ObjectType);

        public override JSchema GetSchema(JSchemaTypeGenerationContext context)
        {
            // generate the default schema, without this generation provider
            JSchemaGenerator generator = new JSchemaGenerator();
            InterfaceExport.ConfigureGenerator(generator);
            JSchema schema = generator.Generate(context.ObjectType);

            // now fix the typename
            JSchema constType = new JSchema();
            string typeName = context.ObjectType.FullName;
            if (typeName.StartsWith(UDPInterface.NetworkFunction.OMITTED_PREFIX))
            {
                typeName = typeName.Substring(UDPInterface.NetworkFunction.OMITTED_PREFIX.Length);
            }

            constType.Enum.Add(typeName);
            schema.Properties["Type"] = constType;
            return schema;
        }

        #endregion
    }
}