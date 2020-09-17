// Copyright 2020 Ammo Goettsch
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
using GadrocsWorkshop.Helios.Interfaces.DCS.Common;

namespace GadrocsWorkshop.Helios.ProfileEditorTools.DCSInterfaceLoadTester
{
    /// <summary>
    /// default implementation to use when no compatible tester can be determined
    /// </summary>
    internal class UnsupportedTester : TesterBase
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public UnsupportedTester(DCSDataElement dataElement) : base(dataElement)
        {
            Logger.Warn("Interface tester does not recognize format {Format} of {Id}", dataElement.Format, dataElement.ID);
        }

        public override string Update(DateTime now, TimeSpan elapsed) => null;
    }
}