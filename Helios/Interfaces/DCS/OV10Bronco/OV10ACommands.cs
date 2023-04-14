//  Copyright 2023 Helios Contributors
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

namespace GadrocsWorkshop.Helios.Interfaces.DCS.OV10Bronco
{
    internal class OV10ACommands
    {
        //    Add these from input
        internal enum Keys
        {
        }
        internal enum device_commands
        {
            BAT = 3201,
            RGEN = 3202,
            LGEN = 3203,
            RSTART = 3204,
            LSTART = 3205,
            INV = 3206,
            RCRANK = 3207,
            LCRANK = 3208,
            LTS = 3209,
            NORM_TRIM = 3210,
            TRIM = 3211,
            FLAPS_ALT = 3212,
            SEL_TRIM = 3213,
            YAW_DAMPER = 3214,
            ALT_TRIM = 3215,
            UHT_MAN_XMIT = 3216,
            UHF_Mode = 3217,
            UHF_DIX = 3218,
            UHF_UNI = 3219,
            UHF_CEN = 3220,
            UHF_VOl = 3221,
            UHF_Preset = 3222,
            MASTER_ARM = 3223,
            INTERVAL = 3224,
            MK_4_POD = 3225,
            GUNS_R = 3226,
            GUNS_L = 3227,
            SPA_1 = 3228,
            SPA_2 = 3229,
            SPA_3 = 3230,
            SPA_4 = 3231,
            SPA_5 = 3232,
            SPA_wingR = 3233,
            SPA_wingL = 3234,
            EMERJET = 3235,
            Light_NAV = 3236,
            Light_ANTICOL = 3237,
            Light_FORM = 3238,
            Light_PHARE = 3239,
            Light_FLOOD = 3240,
            Light_CONSOL = 3241,
            Light_INSTRUMENT = 3242,
            HUD_BRT = 3243,
            HUD_Film = 3244,
            HUD_Warn = 3245,
            HUD_FUMI = 3246,
            HUD_Teinte = 3247,
            HUD_Elevation = 3248,
            TCN_Mode = 3249,
            TCN_Chan_U = 3250,
            TCN_Chan_D = 3251,
            TCN_Vol = 3252,
            CUTOFFR = 3253,
            CUTOFFL = 3254,
            LANDINGGEAR = 3255,
            FLAPSLEVER = 3256,
        }
        internal enum EFM_commands
        {
        }
    }

}
