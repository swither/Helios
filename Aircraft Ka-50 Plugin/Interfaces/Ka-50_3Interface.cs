//  Copyright 2014 Craig Courtney
//    
//  Helios is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  Helios is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.

#pragma warning disable IDE0051 // Remove unused private member: interfaces contain definitions we don't implement
namespace GadrocsWorkshop.Helios.Interfaces.DCS.BlackShark
{
    using GadrocsWorkshop.Helios.ComponentModel;
    using GadrocsWorkshop.Helios.Interfaces.DCS.Common;
    using GadrocsWorkshop.Helios.UDPInterface;

    [HeliosInterface("Helios.KA50-3", "DCS Ka-50 3", typeof(DCSInterfaceEditor), typeof(UniqueHeliosInterfaceFactory), UniquenessKey = "Helios.DCSInterface")]
    public class KA50_3Interface : BlackSharkInterface
    {
        private const string L_370E50 = "64";
        private const string IGLA_INTERFACE = "65";
        private const string INS_EMBEDDED_KA50 = "66";

        #region Buttons
        private const string BUTTON_1 = "3001";
        private const string BUTTON_2 = "3002";
        private const string BUTTON_3 = "3003";
        private const string BUTTON_4 = "3004";
        private const string BUTTON_5 = "3005";
        private const string BUTTON_6 = "3006";
        private const string BUTTON_7 = "3007";
        private const string BUTTON_8 = "3008";
        private const string BUTTON_9 = "3009";
        #endregion

        public KA50_3Interface()
            : base("DCS Black Shark 3", "Ka-50_3", "pack://application:,,,/Ka-50;component/Interfaces/ExportFunctionsKa-50_3.lua")
        {
            // see if we can restore from JSON
#if (!DEBUG)
            if (LoadFunctionsFromJson())
            {
                return;
            }
#endif
            AddFunction(Switch.CreateToggleSwitch(this, L_370E50, BUTTON_1, "49", "1", "On", "0", "Off", "Missile Warning System", "ODS operation mode", "%1d"));
        }
    }
}
