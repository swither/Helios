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

using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;

namespace GadrocsWorkshop.Helios.UDPInterface
{
    [DebuggerDisplay("{SerializedTypeName} {DebugIdentity}")]
    public abstract class NetworkFunction : IBuildableFunction
    {
        public const string HELIOS_TYPE_PROPERTY = "heliosType";
        public const string OMITTED_PREFIX = "GadrocsWorkshop.Helios.Interfaces.";

        protected NetworkFunction(BaseUDPInterface sourceInterface)
        {
            SourceInterface = sourceInterface;
        }

        #region Hooks

        /// <summary>
        /// debug display helper
        /// </summary>
        [JsonIgnore]
        public virtual string DebugIdentity => LocalKey;

        /// <summary>
        /// a string key that can be used to identify this item among the functions in a particular interface
        /// </summary>
        [JsonIgnore]
        public virtual string LocalKey => string.Join(",", DataElements.Select(d => $"#{d.ID}"));

        [JsonProperty(HELIOS_TYPE_PROPERTY, Order = -10, Required = Required.Always)]
        protected virtual string SerializedTypeName
        {
            get
            {
                string typeName = GetType().FullName;
                if (typeName?.StartsWith(OMITTED_PREFIX) ?? false)
                {
                    typeName = typeName.Substring(OMITTED_PREFIX.Length);
                }

                return typeName;
            }
        }

        /// <summary>
        /// create from stored parameters, called from parameterized constructor and
        /// after deserialization
        /// </summary>
        public abstract void BuildAfterDeserialization();

        public abstract void ProcessNetworkData(string id, string value);

        public abstract void Reset();

        #endregion

        #region Properties

        /// <summary>
        /// if false, then this item does not get saved to JSON and restored from JSON
        /// </summary>
        [JsonIgnore]
        public bool Serializable { get; set; } = true;

        [JsonIgnore] public BaseUDPInterface SourceInterface { get; }

        [JsonIgnore] public HeliosTriggerCollection Triggers { get; } = new HeliosTriggerCollection();

        [JsonIgnore] public HeliosActionCollection Actions { get; } = new HeliosActionCollection();

        [JsonIgnore] public HeliosValueCollection Values { get; } = new HeliosValueCollection();

        public abstract ExportDataElement[] DataElements { get; }

        protected abstract ExportDataElement[] DefaultDataElements { get; }

        /// <summary>
        /// if true, this function is missing some implementation and will not operate correctly
        /// </summary>
        [JsonProperty("unimplemented", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [DefaultValue(false)]
        public bool Unimplemented { get; set; }

        #endregion
    }
}