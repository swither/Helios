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

using GadrocsWorkshop.Helios.UDPInterface;
using Newtonsoft.Json;

// ReSharper disable once CheckNamespace needs to be in this namespace for JSON naming
namespace GadrocsWorkshop.Helios.Interfaces
{
    /// <summary>
    /// This function is never actually instantiated, as it represents an ignored
    /// entry in the function table of an interface file.  The class exists only to let
    /// the schema generator know that this is a valid choice.
    /// </summary>
    public class Ignored : NetworkFunction
    {
        public Ignored(BaseUDPInterface sourceInterface) : base(sourceInterface)
        {
            // no code
        }

        #region Overrides

        public override void BuildAfterDeserialization()
        {
            // no code
        }

        public override void ProcessNetworkData(string id, string value)
        {
            // no code
        }

        public override void Reset()
        {
            // no code
        }

        protected override ExportDataElement[] DefaultDataElements => new ExportDataElement[0];

        [JsonIgnore]
        public override ExportDataElement[] DataElements => DefaultDataElements;

        #endregion
    }
}