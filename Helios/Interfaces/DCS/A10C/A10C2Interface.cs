﻿// Copyright 2020 Helios Contributors
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

namespace GadrocsWorkshop.Helios.Interfaces.DCS.A10C
{
    using ComponentModel;
    using Common;
    using static Microsoft.WindowsAPICodePack.Shell.PropertySystem.SystemProperties.System;
    using GadrocsWorkshop.Helios.Interfaces.DCS.A10C.Functions;

    /// <summary>
    /// Interface for DCS A-10C II including any changes made that are not in DCS A-10C
    /// </summary>
    [HeliosInterface("Helios.A10C2", "DCS A-10C II", typeof(DCSInterfaceEditor), typeof(UniqueHeliosInterfaceFactory), UniquenessKey = "Helios.DCSInterface")]
    public class A10C2Interface : A10CInterface
    {
        #region Devices
        // REVISIT: TISL does not exist in the A-10C II and needs to be removed 
        private const string UFC = "8";
        private const string VHF_AM_RADIO = "55";
        private const string TISL = "57";
        private const string SCORPION_HMCS = "75";
        #endregion

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
        private const string BUTTON_10 = "3010";
        private const string BUTTON_11 = "3011";
        private const string BUTTON_12 = "3012";
        private const string BUTTON_13 = "3013";
        private const string BUTTON_14 = "3014";
        private const string BUTTON_15 = "3015";
        private const string BUTTON_16 = "3016";
        private const string BUTTON_17 = "3017";
        private const string BUTTON_18 = "3018";
        private const string BUTTON_19 = "3019";
        private const string BUTTON_20 = "3020";
        private const string BUTTON_21 = "3021";
        private const string BUTTON_22 = "3022";
        private const string BUTTON_23 = "3023";
        private const string BUTTON_24 = "3024";
        private const string BUTTON_25 = "3025";
        private const string BUTTON_26 = "3026";
        private const string BUTTON_27 = "3027";
        private const string BUTTON_28 = "3028";
        private const string BUTTON_29 = "3029";
        private const string BUTTON_30 = "3030";
        private const string BUTTON_31 = "3031";
        private const string BUTTON_32 = "3032";
        private const string BUTTON_33 = "3033";
        private const string BUTTON_34 = "3034";
        private const string BUTTON_35 = "3035";
        private const string BUTTON_36 = "3036";
        private const string BUTTON_37 = "3037";
        private const string BUTTON_38 = "3038";
        private const string BUTTON_39 = "3039";
        private const string BUTTON_40 = "3040";
        private const string BUTTON_41 = "3041";
        private const string BUTTON_42 = "3042";
        private const string BUTTON_43 = "3043";
        private const string BUTTON_44 = "3044";
        private const string BUTTON_45 = "3045";
        private const string BUTTON_46 = "3046";
        private const string BUTTON_47 = "3047";
        private const string BUTTON_48 = "3048";
        private const string BUTTON_49 = "3049";
        private const string BUTTON_50 = "3050";
        private const string BUTTON_51 = "3051";
        private const string BUTTON_52 = "3052";
        private const string BUTTON_53 = "3053";
        private const string BUTTON_54 = "3054";
        private const string BUTTON_55 = "3055";
        private const string BUTTON_56 = "3056";
        private const string BUTTON_57 = "3057";
        private const string BUTTON_58 = "3058";
        private const string BUTTON_59 = "3059";
        private const string BUTTON_60 = "3060";
        private const string BUTTON_61 = "3061";
        private const string BUTTON_62 = "3062";
        private const string BUTTON_63 = "3063";
        private const string BUTTON_64 = "3064";
        private const string BUTTON_65 = "3065";
        private const string BUTTON_66 = "3066";
        private const string BUTTON_67 = "3067";
        #endregion

        public A10C2Interface() : base(
            "DCS A-10C II", 
            "A-10C_2",
            "pack://application:,,,/Helios;component/Interfaces/DCS/A10C/ExportFunctionsA10C2.lua")
        {
            // see if we can restore from JSON
#if (!DEBUG)
                        if (LoadFunctionsFromJson())
                        {
                            return;
                        }
#endif
            base.AddFunctions();
            #region Scorpion HMCS
            AddFunction(Switch.CreateThreeWaySwitch(this, SCORPION_HMCS, BUTTON_1, "550", "1.0", "On", "0.0", "Off", "-1.0", "Bat", "Scorpion HMCS", "Power", "%0.1f"));
            #endregion

            // REVISIT: TISL does not exist in the A-10C II and needs to be removed 
            #region TISL Panel
            AddFunction(new Switch(this, TISL, "622", new SwitchPosition[] { new SwitchPosition("0.0", "Off", BUTTON_1), new SwitchPosition("0.1", "Cage", BUTTON_1), new SwitchPosition("0.2", "Dive", BUTTON_1), new SwitchPosition("0.3", "Level Narrow Nar", BUTTON_1), new SwitchPosition("0.4", "Level Wide", BUTTON_1) }, "TISL", "Mode Select", null));
            AddFunction(Switch.CreateThreeWaySwitch(this, TISL, BUTTON_2, "623", "1", "Over 10", "0", "10-5", "-1", "Under 5", "TISL", "Slant Range", null));
            AddFunction(new Axis(this, TISL, BUTTON_3, "624", 0.1d, 0.0d, 1.0d, "TISL", "Altitude above target tens of thousands of feet",true,null));
            AddFunction(new Axis(this, TISL, BUTTON_4, "626", 0.1d, 0.0d, 1.0d, "TISL", "Altitude above target thousands of feet", true, null));
            AddFunction(new Axis(this, TISL, BUTTON_5, "636", 0.05d, 0.0d, 1.0d, "TISL", "TISL Code Wheel 1", true, null));
            AddFunction(new Axis(this, TISL, BUTTON_6, "638", 0.05d, 0.0d, 1.0d, "TISL", "TISL Code Wheel 2", true, null));
            AddFunction(new Axis(this, TISL, BUTTON_7, "640", 0.05d, 0.0d, 1.0d, "TISL", "TISL Code Wheel 3", true, null));
            AddFunction(new Axis(this, TISL, BUTTON_8, "642", 0.05d, 0.0d, 1.0d, "TISL", "TISL Code Wheel 4", true, null));
            AddFunction(Switch.CreateThreeWaySwitch(this, TISL, BUTTON_9, "644", "1", "TISL", "0", "Both", "-1", "Aux", "TISL", "Code Select", null));
            AddFunction(new PushButton(this, TISL, BUTTON_10, "628", "TISL", "Enter", null));
            AddFunction(new PushButton(this, TISL, BUTTON_11, "630", "TISL", "OverTemp", null));
            AddFunction(new PushButton(this, TISL, BUTTON_12, "632", "TISL", "Bite", null));
            AddFunction(new PushButton(this, TISL, BUTTON_13, "634", "TISL", "Track", null));
            AddFunction(new NetworkValue(this, "629", "TISL", "TISL-AUX indicator", "Current value of the indicator", "0 to 1", BindingValueUnits.Numeric, null));
            AddFunction(new NetworkValue(this, "631", "TISL", "OVER TEMP indicator", "Current value of the indicator", "0 to 1", BindingValueUnits.Numeric, null));
            AddFunction(new NetworkValue(this, "633", "TISL", "DET-ACD indicator", "Current value of the indicator", "0 to 1", BindingValueUnits.Numeric, null));
            AddFunction(new NetworkValue(this, "635", "TISL", "TRACK indicator", "Current value of the indicator", "0 to 1", BindingValueUnits.Numeric, null));
            #endregion

            #region VHF AM Radio (ARC-210)
            AddFunction(new PushButton(this, UFC, BUTTON_33, "534", "UFC", "Toggle ARC-210 RT2 Status (long press)", "%0.1f"));
            AddFunction(new PushButton(this, UFC, BUTTON_34, "535", "UFC", "Toggle ECCM", "%0.1f"));
            AddFunction(new PushButton(this, UFC, BUTTON_35, "536", "UFC", "Toggle IDM R/T", "%0.1f"));

            AddFunction(new Switch(this, VHF_AM_RADIO, "551", SwitchPositions.Create(7, 0d, 0.1d, BUTTON_43, new string[] { "OFF", "TR G", "TR", "ADF", "CHG PRST", "TEST", "ZERO (PULL)"}, "%0.1f"), "ARC-210", "Master switch", "%0.1f"));
            AddFunction(new RotaryEncoder(this, VHF_AM_RADIO, BUTTON_27, "552", 0.1d, "ARC-210", "Channel select knob"));
            AddFunction(new Switch(this, VHF_AM_RADIO, "553", SwitchPositions.Create(7, 0d, 0.1d, BUTTON_44, new string[] { "ECCM MASTER", "ECCM", "PRST", "MAN", "MAR", "243", "121 (PULL)" }, "%0.1f"), "ARC-210", "Secondary switch", "%0.1f"));

            AddFunction(new Switch(this, VHF_AM_RADIO, "554", SwitchPositions.Create(4, 0d, 0.1d, BUTTON_25, "Posn", "%0.1f"), "ARC-210", "100 MHz Selector", "%0.1f"));
            AddFunction(new Switch(this, VHF_AM_RADIO, "555", SwitchPositions.Create(10, 0d, 0.1d, BUTTON_23, "Posn", "%0.1f"), "ARC-210", "10 MHz Selector", "%0.1f"));
            AddFunction(new Switch(this, VHF_AM_RADIO, "556", SwitchPositions.Create(10, 0d, 0.1d, BUTTON_21, "Posn", "%0.1f"), "ARC-210", "1 MHz Selector", "%0.1f"));
            AddFunction(new Switch(this, VHF_AM_RADIO, "557", SwitchPositions.Create(10, 0d, 0.1d, BUTTON_19, "Posn", "%0.1f"), "ARC-210", "100 KHz Selector", "%0.1f"));
            AddFunction(new Switch(this, VHF_AM_RADIO, "558", SwitchPositions.Create(4, 0d, 0.1d, BUTTON_17, "Posn", "%0.1f"), "ARC-210", "25 KHz Selector", "%0.1f"));

            AddFunction(new PushButton(this, VHF_AM_RADIO, BUTTON_14, "573", "ARC-210", "Enter", "%0.1f"));
            AddFunction(new PushButton(this, VHF_AM_RADIO, BUTTON_13, "572", "ARC-210", "Offset frequency", "%0.1f"));
            AddFunction(new PushButton(this, VHF_AM_RADIO, BUTTON_12, "571", "ARC-210", "Transmit / receive function toggle", "%0.1f"));
            AddFunction(new PushButton(this, VHF_AM_RADIO, BUTTON_11, "570", "ARC-210", "Amplitude modulation / frequency modulation select", "%0.1f"));
            AddFunction(new PushButton(this, VHF_AM_RADIO, BUTTON_10, "569", "ARC-210", "Menu pages", "%0.1f"));
            AddFunction(new Switch(this, VHF_AM_RADIO, "568", new SwitchPosition[] { new SwitchPosition("0.0", "Off", BUTTON_15), new SwitchPosition("1.0", "On", BUTTON_15) }, "ARC-210", "Squelch on/off", "%0.1f"));

            AddFunction(new PushButton(this, VHF_AM_RADIO, BUTTON_4, "567", "ARC-210", "Select receiver - transmitter", "%0.1f"));
            AddFunction(new PushButton(this, VHF_AM_RADIO, BUTTON_3, "566", "ARC-210", "Global positioning system", "%0.1f"));
            AddFunction(new PushButton(this, VHF_AM_RADIO, BUTTON_2, "565", "ARC-210", "Time of day receive", "%0.1f"));
            AddFunction(new PushButton(this, VHF_AM_RADIO, BUTTON_1, "564", "ARC-210", "Time of day send", "%0.1f"));
            AddFunction(new PushButton(this, VHF_AM_RADIO, BUTTON_5, "563", "ARC-210", "Upper FSK", "%0.1f"));
            AddFunction(new PushButton(this, VHF_AM_RADIO, BUTTON_6, "562", "ARC-210", "Middle FSK", "%0.1f"));
            AddFunction(new PushButton(this, VHF_AM_RADIO, BUTTON_7, "561", "ARC-210", "Lower FSK", "%0.1f"));

            AddFunction(new PushButton(this, VHF_AM_RADIO, BUTTON_8, "560", "ARC-210", "Brightness increase", "%0.1f"));
            AddFunction(new PushButton(this, VHF_AM_RADIO, BUTTON_9, "559", "ARC-210", "Brightness decrease", "%0.1f"));

            AddFunction(new ARC210Display(this)); // 2422
            #endregion

        }
    }
}
