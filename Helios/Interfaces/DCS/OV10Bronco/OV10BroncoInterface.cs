// Copyright 2023 Helios Contributors
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
//#define CREATEINTERFACE
namespace GadrocsWorkshop.Helios.Interfaces.DCS.OV10Bronco
{
    using ComponentModel;
    using Common;
    using GadrocsWorkshop.Helios.Interfaces.DCS.OV10Bronco.Tools;
    using GadrocsWorkshop.Helios.UDPInterface;
    using System;
    using System.IO;
    using System.Collections.Generic;
    using static System.Net.WebRequestMethods;
    using System.Windows.Controls;
    using GadrocsWorkshop.Helios.Interfaces.DCS.OV10Bronco.Functions;

    /// <summary>
    /// Interface for DCS OV-10 Bronco Community Module
    /// </summary>
    [HeliosInterface("Helios.OV10Bronco", "DCS OV-10A Bronco", typeof(DCSInterfaceEditor), typeof(UniqueHeliosInterfaceFactory), UniquenessKey = "Helios.DCSInterface")]
    public class OV10BroncoInterface : DCSInterface
    {
        private readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private string _dcsPath = $@"{Environment.GetEnvironmentVariable("userprofile")}\DCS World.openbeta\Mods\Aircraft";
        private NetworkFunctionCollection _functions = new NetworkFunctionCollection();
        public OV10BroncoInterface() : base(
            "DCS OV-10A Bronco",
            "OV10A",
            "pack://application:,,,/Helios;component/Interfaces/DCS/OV10Bronco/ExportFunctionsBroncoOV10.lua")
        {


#if (CREATEINTERFACE && DEBUG)
            DcsPath = Path.Combine(Environment.GetEnvironmentVariable("userprofile"), "Saved Games", "DCS World.openbeta", "mods", "Aircraft");
            DcsPath = Path.Combine(Environment.GetEnvironmentVariable("userprofile"), "Documents", "DCSLua");

            OV10AInterfaceCreator ic = new OV10AInterfaceCreator("OV10A");
            OV10ACommandsCreator commands = new OV10ACommandsCreator(Path.Combine(DcsPath, "OV-10 Bronco"),ic.DocumentPath);
            OV10ADevicesCreator dev = new OV10ADevicesCreator(Path.Combine(DcsPath, "OV-10 Bronco"), ic.DocumentPath);

            OV10AMainPanelCreator mpc = new OV10AMainPanelCreator(Path.Combine(DcsPath, "OV-10 Bronco"), ic.DocumentPath);

            _functions.AddRange(mpc.GetNetworkFunctions(this));
            MakeFunctionsFromDcsModule(ic);
            AddFunctionsToDcs();
            return;
#else
            if (LoadFunctionsFromJson())
            {
                return;
            }
#endif
#pragma warning disable CS0162 // Unreachable code detected
            #region Electric system
            AddFunction(new Switch(this, devices.ELECTRIC.ToString("d"), "00", new SwitchPosition[] { new SwitchPosition("-1.0", "Posn 1", OV10ACommands.device_commands.BAT.ToString("d")), new SwitchPosition("0.0", "Posn 2", OV10ACommands.device_commands.BAT.ToString("d")), new SwitchPosition("1.0", "Posn 3", OV10ACommands.device_commands.BAT.ToString("d")) }, "Electric system", "Batterie", "%0.1f"));
            AddFunction(new Switch(this, devices.ELECTRIC.ToString("d"), "01", new SwitchPosition[] { new SwitchPosition("-1.0", "Posn 1", OV10ACommands.device_commands.RGEN.ToString("d")), new SwitchPosition("0.0", "Posn 2", OV10ACommands.device_commands.RGEN.ToString("d")), new SwitchPosition("1.0", "Posn 3", OV10ACommands.device_commands.RGEN.ToString("d")) }, "Electric system", "R GEN", "%0.1f"));
            AddFunction(new Switch(this, devices.ELECTRIC.ToString("d"), "02", new SwitchPosition[] { new SwitchPosition("-1.0", "Posn 1", OV10ACommands.device_commands.LGEN.ToString("d")), new SwitchPosition("0.0", "Posn 2", OV10ACommands.device_commands.LGEN.ToString("d")), new SwitchPosition("1.0", "Posn 3", OV10ACommands.device_commands.LGEN.ToString("d")) }, "Electric system", "L GEN", "%0.1f"));
            AddFunction(new Switch(this, devices.ELECTRIC.ToString("d"), "03", new SwitchPosition[] { new SwitchPosition("-1.0", "Posn 1", OV10ACommands.device_commands.RSTART.ToString("d")), new SwitchPosition("0.0", "Posn 2", OV10ACommands.device_commands.RSTART.ToString("d")), new SwitchPosition("1.0", "Posn 3", OV10ACommands.device_commands.RSTART.ToString("d")) }, "Electric system", "R START", "%0.1f"));
            AddFunction(new Switch(this, devices.ELECTRIC.ToString("d"), "04", new SwitchPosition[] { new SwitchPosition("-1.0", "Posn 1", OV10ACommands.device_commands.LSTART.ToString("d")), new SwitchPosition("0.0", "Posn 2", OV10ACommands.device_commands.LSTART.ToString("d")), new SwitchPosition("1.0", "Posn 3", OV10ACommands.device_commands.LSTART.ToString("d")) }, "Electric system", "L START", "%0.1f"));
            AddFunction(new Switch(this, devices.ELECTRIC.ToString("d"), "05", new SwitchPosition[] { new SwitchPosition("-1.0", "Posn 1", OV10ACommands.device_commands.INV.ToString("d")), new SwitchPosition("0.0", "Posn 2", OV10ACommands.device_commands.INV.ToString("d")), new SwitchPosition("1.0", "Posn 3", OV10ACommands.device_commands.INV.ToString("d")) }, "Electric system", "Inverter", "%0.1f"));
            AddFunction(new Switch(this, devices.FUEL.ToString("d"), "06", new SwitchPosition[] { new SwitchPosition("1.0", "Posn 1", OV10ACommands.device_commands.RCRANK.ToString("d")), new SwitchPosition("0.0", "Posn 2", OV10ACommands.device_commands.RCRANK.ToString("d")) }, "Electric system", "R Crank", "%0.1f"));
            AddFunction(new Switch(this, devices.FUEL.ToString("d"), "07", new SwitchPosition[] { new SwitchPosition("1.0", "Posn 1", OV10ACommands.device_commands.LCRANK.ToString("d")), new SwitchPosition("0.0", "Posn 2", OV10ACommands.device_commands.LCRANK.ToString("d")) }, "Electric system", "L Crank", "%0.1f"));
            #endregion Electric system
            #region FLAPS
            AddFunction(new Switch(this, devices.ELECTRIC.ToString("d"), "10", new SwitchPosition[] { new SwitchPosition("-1.0", "Posn 1", OV10ACommands.device_commands.LTS.ToString("d")), new SwitchPosition("0.0", "Posn 2", OV10ACommands.device_commands.LTS.ToString("d")), new SwitchPosition("1.0", "Posn 3", OV10ACommands.device_commands.LTS.ToString("d")) }, "FLAPS", "LTS", "%0.1f"));
            AddFunction(new Switch(this, devices.ELECTRIC.ToString("d"), "11", new SwitchPosition[] { new SwitchPosition("-1.0", "Posn 1", OV10ACommands.device_commands.NORM_TRIM.ToString("d")), new SwitchPosition("0.0", "Posn 2", OV10ACommands.device_commands.NORM_TRIM.ToString("d")), new SwitchPosition("1.0", "Posn 3", OV10ACommands.device_commands.NORM_TRIM.ToString("d")) }, "FLAPS", "Norm Trim", "%0.1f"));
            AddFunction(new Switch(this, devices.ELECTRIC.ToString("d"), "12", new SwitchPosition[] { new SwitchPosition("-1.0", "Posn 1", OV10ACommands.device_commands.TRIM.ToString("d")), new SwitchPosition("0.0", "Posn 2", OV10ACommands.device_commands.TRIM.ToString("d")), new SwitchPosition("1.0", "Posn 3", OV10ACommands.device_commands.TRIM.ToString("d")) }, "FLAPS", "Trim", "%0.1f"));
            AddFunction(new Switch(this, devices.ELECTRIC.ToString("d"), "13", new SwitchPosition[] { new SwitchPosition("-1.0", "Posn 1", OV10ACommands.device_commands.FLAPS_ALT.ToString("d")), new SwitchPosition("0.0", "Posn 2", OV10ACommands.device_commands.FLAPS_ALT.ToString("d")), new SwitchPosition("1.0", "Posn 3", OV10ACommands.device_commands.FLAPS_ALT.ToString("d")) }, "FLAPS", "Flaps Alt", "%0.1f"));
            AddFunction(new Switch(this, devices.ELECTRIC.ToString("d"), "14", new SwitchPosition[] { new SwitchPosition("-1.0", "Posn 1", OV10ACommands.device_commands.SEL_TRIM.ToString("d")), new SwitchPosition("0.0", "Posn 2", OV10ACommands.device_commands.SEL_TRIM.ToString("d")), new SwitchPosition("1.0", "Posn 3", OV10ACommands.device_commands.SEL_TRIM.ToString("d")) }, "FLAPS", "Select Trim", "%0.1f"));
            AddFunction(new Switch(this, devices.ELECTRIC.ToString("d"), "15", new SwitchPosition[] { new SwitchPosition("-1.0", "Posn 1", OV10ACommands.device_commands.YAW_DAMPER.ToString("d")), new SwitchPosition("0.0", "Posn 2", OV10ACommands.device_commands.YAW_DAMPER.ToString("d")), new SwitchPosition("1.0", "Posn 3", OV10ACommands.device_commands.YAW_DAMPER.ToString("d")) }, "FLAPS", "Yaw Damper", "%0.1f"));
            AddFunction(new Switch(this, devices.ELECTRIC.ToString("d"), "16", new SwitchPosition[] { new SwitchPosition("-1.0", "Posn 1", OV10ACommands.device_commands.ALT_TRIM.ToString("d")), new SwitchPosition("0.0", "Posn 2", OV10ACommands.device_commands.ALT_TRIM.ToString("d")), new SwitchPosition("1.0", "Posn 3", OV10ACommands.device_commands.ALT_TRIM.ToString("d")) }, "FLAPS", "Trim Alt", "%0.1f"));
            #endregion FLAPS
            #region RADIO
            AddFunction(new Switch(this, devices.ELECTRIC.ToString("d"), "20", new SwitchPosition[] { new SwitchPosition("1.0", "MAN", OV10ACommands.device_commands.UHT_MAN_XMIT.ToString("d")), new SwitchPosition("0.0", "XMIT", OV10ACommands.device_commands.UHT_MAN_XMIT.ToString("d")) }, "RADIO", "MAN/XMIT", "%0.1f"));
            AddFunction(new Switch(this, devices.ELECTRIC.ToString("d"), "21", SwitchPositions.Create(4, 0d, 0d, OV10ACommands.device_commands.UHF_Mode.ToString("d"), "Posn", "%0.3f"), "RADIO", "UHF Mode", "%0.3f"));
            AddFunction(new Axis(this, devices.ELECTRIC.ToString("d"), OV10ACommands.device_commands.UHF_UNI.ToString("d"), "23", 0.1d, 0.0d, 100d, "RADIO", "UHF frequency (units)", false, "%0.3f"));
            AddFunction(new Axis(this, devices.ELECTRIC.ToString("d"), OV10ACommands.device_commands.UHF_DIX.ToString("d"), "22", 0.1d, 0.0d, 100d, "RADIO", "UHF frequency (tens)", false, "%0.3f"));
            AddFunction(new Axis(this, devices.ELECTRIC.ToString("d"), OV10ACommands.device_commands.UHF_CEN.ToString("d"), "24", 0.1d, 0.0d, 100d, "RADIO", "UHF frequency (hundreds)", false, "%0.3f"));
            AddFunction(new Axis(this, devices.ELECTRIC.ToString("d"), OV10ACommands.device_commands.UHF_VOl.ToString("d"), "25", 0.1d, 0.0d, 1.0d, "RADIO", "Volume", false, "%0.1f"));
            AddFunction(new RotaryEncoder(this, devices.ELECTRIC.ToString("d"), OV10ACommands.device_commands.UHF_Preset.ToString("d"), "26", 0.1d, "RADIO", "Channel preset"));
            //AddFunction(new Axis(this, devices.ELECTRIC.ToString("d"), OV10ACommands.device_commands.UHF_Preset.ToString("d"), "26", 0.1d, 0.0d, 1.0d, "RADIO", "Preset Chan", false, "%0.1f"));
            #endregion RADIO
            #region HUD
            AddFunction(new Axis(this, devices.GUNSIGHT.ToString("d"), OV10ACommands.device_commands.HUD_BRT.ToString("d"), "30", 0.1d, 0.0d, 100d, "HUD", "HUD BRT", false, "%0.1f"));
            AddFunction(new Switch(this, devices.GUNSIGHT.ToString("d"), "31", new SwitchPosition[] { new SwitchPosition("1.0", "Posn 1", OV10ACommands.device_commands.HUD_Film.ToString("d")), new SwitchPosition("0.0", "Posn 2", OV10ACommands.device_commands.HUD_Film.ToString("d")) }, "HUD", "Film", "%0.1f"));
            AddFunction(new Switch(this, devices.GUNSIGHT.ToString("d"), "32", new SwitchPosition[] { new SwitchPosition("1.0", "Posn 1", OV10ACommands.device_commands.HUD_Warn.ToString("d")), new SwitchPosition("0.0", "Posn 2", OV10ACommands.device_commands.HUD_Warn.ToString("d")) }, "HUD", "Warn", "%0.1f"));
            AddFunction(new Switch(this, devices.GUNSIGHT.ToString("d"), "33", new SwitchPosition[] { new SwitchPosition("1.0", "Posn 1", OV10ACommands.device_commands.HUD_FUMI.ToString("d")), new SwitchPosition("0.0", "Posn 2", OV10ACommands.device_commands.HUD_FUMI.ToString("d")) }, "HUD", "Smoke", "%0.1f"));
            AddFunction(new Switch(this, devices.GUNSIGHT.ToString("d"), "34", new SwitchPosition[] { new SwitchPosition("1.0", "Posn 1", OV10ACommands.device_commands.HUD_Teinte.ToString("d")), new SwitchPosition("0.0", "Posn 2", OV10ACommands.device_commands.HUD_Teinte.ToString("d")) }, "HUD", "HUD glass tint", "%0.1f"));
            AddFunction(new Axis(this, devices.GUNSIGHT.ToString("d"), OV10ACommands.device_commands.HUD_Elevation.ToString("d"), "35", 0.1d, 0.0d, 1.0d, "HUD", "HUD Elevation", false, "%0.1f"));
            #endregion HUD
            #region TACAN
            AddFunction(new Switch(this, devices.TACAN_OV10.ToString("d"), "40", SwitchPositions.Create(3, 0d, 0d, OV10ACommands.device_commands.TCN_Mode.ToString("d"), "Posn", "%0.3f"), "TACAN", "TACAN Mode", "%0.3f"));
            AddFunction(new Switch(this, devices.TACAN_OV10.ToString("d"), "41", SwitchPositions.Create(9, 0d, 0d, OV10ACommands.device_commands.TCN_Chan_U.ToString("d"), "Posn", "%0.3f"), "TACAN", "TACAN Channel (Units)", "%0.3f"));
            AddFunction(new Switch(this, devices.TACAN_OV10.ToString("d"), "42", SwitchPositions.Create(20, 0d, 0d, OV10ACommands.device_commands.TCN_Chan_D.ToString("d"), "Posn", "%0.3f"), "TACAN", "TACAN Channel (Tens)", "%0.3f"));
            AddFunction(new Axis(this, devices.TACAN_OV10.ToString("d"), OV10ACommands.device_commands.TCN_Vol.ToString("d"), "43", 0.1d, 0.0d, 100d, "TACAN", "TACAN Volume", false, "%0.1f"));
            #endregion TACAN            
            //AddFunction(new Switch(this, devices.EXT_LIGHTS.ToString("d"), "100", new SwitchPosition[] { new SwitchPosition("-1.0", "Posn 1", OV10ACommands.device_commands.Light_NAV.ToString("d")), new SwitchPosition("0.0", "Posn 2", OV10ACommands.device_commands.Light_NAV.ToString("d")), new SwitchPosition("1.0", "Posn 3", OV10ACommands.device_commands.Light_NAV.ToString("d")) }, "Light EXT", "NAV light", "%0.1f"));
            //AddFunction(new Switch(this, devices.EXT_LIGHTS.ToString("d"), "101", new SwitchPosition[] { new SwitchPosition("1.0", "Posn 1", OV10ACommands.device_commands.Light_ANTICOL.ToString("d")), new SwitchPosition("0.0", "Posn 2", OV10ACommands.device_commands.Light_ANTICOL.ToString("d")) }, "Light EXT", "AntiCol light", "%0.1f"));
            //AddFunction(new Switch(this, devices.EXT_LIGHTS.ToString("d"), "102", new SwitchPosition[] { new SwitchPosition("1.0", "Posn 1", OV10ACommands.device_commands.Light_FORM.ToString("d")), new SwitchPosition("0.0", "Posn 2", OV10ACommands.device_commands.Light_FORM.ToString("d")) }, "Light EXT", "Formation light", "%0.1f"));
            //AddFunction(new Switch(this, devices.EXT_LIGHTS.ToString("d"), "103", new SwitchPosition[] { new SwitchPosition("1.0", "Posn 1", OV10ACommands.device_commands.Light_PHARE.ToString("d")), new SwitchPosition("0.0", "Posn 2", OV10ACommands.device_commands.Light_PHARE.ToString("d")) }, "Light EXT", "Landing Phare light", "%0.1f"));
            //#endregion Light EXT
            //#region Light INT
            //AddFunction(new Axis(this, devices.INT_LIGHTS.ToString("d"), OV10ACommands.device_commands.Light_FLOOD.ToString("d"), "110", 0.1d, 0.0d, 100d, "Light INT", "FLOOD", false, "%0.1f"));
            //AddFunction(new Axis(this, devices.INT_LIGHTS.ToString("d"), OV10ACommands.device_commands.Light_CONSOL.ToString("d"), "111", 0.1d, 0.0d, 100d, "Light INT", "CONSOL", false, "%0.1f"));
            //AddFunction(new Axis(this, devices.INT_LIGHTS.ToString("d"), OV10ACommands.device_commands.Light_INSTRUMENT.ToString("d"), "112", 0.1d, 0.0d, 100d, "Light INT", "INSTRUMENT", false, "%0.1f"));
            #region Light EXT

            #endregion Light INT
            #region WEAPON
            AddFunction(new Switch(this, devices.WEAPON_SYSTEM.ToString("d"), "300", new SwitchPosition[] { new SwitchPosition("1.0", "Posn 1", OV10ACommands.device_commands.MASTER_ARM.ToString("d")), new SwitchPosition("0.0", "Posn 2", OV10ACommands.device_commands.MASTER_ARM.ToString("d")) }, "WEAPON", "MASTER ARM", "%0.1f"));
            AddFunction(new Switch(this, devices.WEAPON_SYSTEM.ToString("d"), "301", new SwitchPosition[] { new SwitchPosition("-1.0", "Posn 1", OV10ACommands.device_commands.INTERVAL.ToString("d")), new SwitchPosition("0.0", "Posn 2", OV10ACommands.device_commands.INTERVAL.ToString("d")), new SwitchPosition("1.0", "Posn 3", OV10ACommands.device_commands.INTERVAL.ToString("d")) }, "WEAPON", "Bomb Interval", "%0.1f"));
            AddFunction(new Switch(this, devices.WEAPON_SYSTEM.ToString("d"), "302", new SwitchPosition[] { new SwitchPosition("-1.0", "Posn 1", OV10ACommands.device_commands.MK_4_POD.ToString("d")), new SwitchPosition("0.0", "Posn 2", OV10ACommands.device_commands.MK_4_POD.ToString("d")), new SwitchPosition("1.0", "Posn 3", OV10ACommands.device_commands.MK_4_POD.ToString("d")) }, "WEAPON", "MK 4 POD", "%0.1f"));
            AddFunction(new Switch(this, devices.WEAPON_SYSTEM.ToString("d"), "303", new SwitchPosition[] { new SwitchPosition("1.0", "Posn 1", OV10ACommands.device_commands.GUNS_R.ToString("d")), new SwitchPosition("0.0", "Posn 2", OV10ACommands.device_commands.GUNS_R.ToString("d")) }, "WEAPON", "Copilot GUNS Right", "%0.1f"));
            AddFunction(new Switch(this, devices.WEAPON_SYSTEM.ToString("d"), "304", new SwitchPosition[] { new SwitchPosition("1.0", "Posn 1", OV10ACommands.device_commands.GUNS_L.ToString("d")), new SwitchPosition("0.0", "Posn 2", OV10ACommands.device_commands.GUNS_L.ToString("d")) }, "WEAPON", "GUNS Left", "%0.1f"));
            AddFunction(new Switch(this, devices.WEAPON_SYSTEM.ToString("d"), "305", new SwitchPosition[] { new SwitchPosition("-1.0", "Posn 1", OV10ACommands.device_commands.SPA_1.ToString("d")), new SwitchPosition("0.0", "Posn 2", OV10ACommands.device_commands.SPA_1.ToString("d")), new SwitchPosition("1.0", "Posn 3", OV10ACommands.device_commands.SPA_1.ToString("d")) }, "WEAPON", "SPA 1", "%0.1f"));
            AddFunction(new Switch(this, devices.WEAPON_SYSTEM.ToString("d"), "306", new SwitchPosition[] { new SwitchPosition("-1.0", "Posn 1", OV10ACommands.device_commands.SPA_2.ToString("d")), new SwitchPosition("0.0", "Posn 2", OV10ACommands.device_commands.SPA_2.ToString("d")), new SwitchPosition("1.0", "Posn 3", OV10ACommands.device_commands.SPA_2.ToString("d")) }, "WEAPON", "SPA 2", "%0.1f"));
            AddFunction(new Switch(this, devices.WEAPON_SYSTEM.ToString("d"), "307", new SwitchPosition[] { new SwitchPosition("-1.0", "Posn 1", OV10ACommands.device_commands.SPA_3.ToString("d")), new SwitchPosition("0.0", "Posn 2", OV10ACommands.device_commands.SPA_3.ToString("d")), new SwitchPosition("1.0", "Posn 3", OV10ACommands.device_commands.SPA_3.ToString("d")) }, "WEAPON", "SPA 3", "%0.1f"));
            AddFunction(new Switch(this, devices.WEAPON_SYSTEM.ToString("d"), "308", new SwitchPosition[] { new SwitchPosition("-1.0", "Posn 1", OV10ACommands.device_commands.SPA_4.ToString("d")), new SwitchPosition("0.0", "Posn 2", OV10ACommands.device_commands.SPA_4.ToString("d")), new SwitchPosition("1.0", "Posn 3", OV10ACommands.device_commands.SPA_4.ToString("d")) }, "WEAPON", "SPA 4", "%0.1f"));
            AddFunction(new Switch(this, devices.WEAPON_SYSTEM.ToString("d"), "309", new SwitchPosition[] { new SwitchPosition("-1.0", "Posn 1", OV10ACommands.device_commands.SPA_5.ToString("d")), new SwitchPosition("0.0", "Posn 2", OV10ACommands.device_commands.SPA_5.ToString("d")), new SwitchPosition("1.0", "Posn 3", OV10ACommands.device_commands.SPA_5.ToString("d")) }, "WEAPON", "SPA 5", "%0.1f"));
            AddFunction(new Switch(this, devices.WEAPON_SYSTEM.ToString("d"), "310", new SwitchPosition[] { new SwitchPosition("-1.0", "Posn 1", OV10ACommands.device_commands.SPA_wingR.ToString("d")), new SwitchPosition("0.0", "Posn 2", OV10ACommands.device_commands.SPA_wingR.ToString("d")), new SwitchPosition("1.0", "Posn 3", OV10ACommands.device_commands.SPA_wingR.ToString("d")) }, "WEAPON", "Wing R", "%0.1f"));
            AddFunction(new Switch(this, devices.WEAPON_SYSTEM.ToString("d"), "311", new SwitchPosition[] { new SwitchPosition("-1.0", "Posn 1", OV10ACommands.device_commands.SPA_wingL.ToString("d")), new SwitchPosition("0.0", "Posn 2", OV10ACommands.device_commands.SPA_wingL.ToString("d")), new SwitchPosition("1.0", "Posn 3", OV10ACommands.device_commands.SPA_wingL.ToString("d")) }, "WEAPON", "Wing L", "%0.1f"));
            AddFunction(new PushButton(this, devices.WEAPON_SYSTEM.ToString("d"), OV10ACommands.device_commands.EMERJET.ToString("d"), "312", "WEAPON", "Emergency jettison", "%1d"));
            #endregion WEAPON
            #region Engine
            AddFunction(new Switch(this, devices.FUEL.ToString("d"), "600", new SwitchPosition[] { new SwitchPosition("1.0", "Posn 1", OV10ACommands.device_commands.CUTOFFR.ToString("d")), new SwitchPosition("0.0", "Posn 2", OV10ACommands.device_commands.CUTOFFR.ToString("d")) }, "Engine", "Engine Cutoff Right", "%0.1f"));
            AddFunction(new Switch(this, devices.FUEL.ToString("d"), "601", new SwitchPosition[] { new SwitchPosition("1.0", "Posn 1", OV10ACommands.device_commands.CUTOFFL.ToString("d")), new SwitchPosition("0.0", "Posn 2", OV10ACommands.device_commands.CUTOFFL.ToString("d")) }, "Engine", "Engine Cutoff Left", "%0.1f"));
            AddFunction(new Switch(this, devices.INSTRUMENT.ToString("d"), "602", new SwitchPosition[] { new SwitchPosition("1.0", "Posn 1", OV10ACommands.device_commands.LANDINGGEAR.ToString("d")), new SwitchPosition("0.0", "Posn 2", OV10ACommands.device_commands.LANDINGGEAR.ToString("d")) }, "Engine", "Landing GEAR", "%0.1f"));
            AddFunction(new Switch(this, devices.INSTRUMENT.ToString("d"), "603", new SwitchPosition[] { new SwitchPosition("-1.0", "Posn 1", OV10ACommands.device_commands.FLAPSLEVER.ToString("d")), new SwitchPosition("0.0", "Posn 2", OV10ACommands.device_commands.FLAPSLEVER.ToString("d")), new SwitchPosition("1.0", "Posn 3", OV10ACommands.device_commands.FLAPSLEVER.ToString("d")) }, "Engine", "FLAPS Lever", "%0.1f"));
            #endregion Engine
            #region Instruments
            AddFunction(new ScaledNetworkValue(this, "100", new CalibrationPointCollectionDouble(-1, 0, 1, 400), "Instruments", "IAS", "Indicated Airspeed", "Value 0 to 400", BindingValueUnits.Knots, "%0.3f"));
            AddFunction(new ScaledNetworkValue(this, "101", new CalibrationPointCollectionDouble(1, 0, -1, 360), "Instruments", "Heading", "Heading of aircraft", "Value 0 to 360", BindingValueUnits.Degrees, "%0.3f"));
            CalibrationPointCollectionDouble calibration = new CalibrationPointCollectionDouble(-1, -5, 1, 10);
            calibration.Add(new CalibrationPointDouble(0, 1));
            calibration.Add(new CalibrationPointDouble(0.5, 5));
            AddFunction(new ScaledNetworkValue(this, "102", calibration, "Instruments", "G meter", "G loading on aircraft", "Value -5 to 10", BindingValueUnits.G, "%0.3f"));
            AddFunction(new ScaledNetworkValue(this, "103", new CalibrationPointCollectionDouble(-1, 1, 1, 10), "Instruments", "G Meter Maximum", "Maximum G loading on aircraft", "Value 1 to 10", BindingValueUnits.G, "%0.3f"));
            AddFunction(new ScaledNetworkValue(this, "104", new CalibrationPointCollectionDouble(1, -5, -1, 0), "Instruments", "G Meter Minimum", "Minimum G loading on aircraft", "Value 1 to 10", BindingValueUnits.G, "%0.3f"));
            AddFunction(new ScaledNetworkValue(this, "105", new CalibrationPointCollectionDouble(-1, -180, 1, 180), "Instruments", "ADI Roll", "Roll in degrees", "Value -180 to 180", BindingValueUnits.Degrees, "%0.3f"));
            AddFunction(new ScaledNetworkValue(this, "106", new CalibrationPointCollectionDouble(-1, -90, 1, 90), "Instruments", "ADI Pitch", "Pitch in degrees", "Value -90 to 90", BindingValueUnits.Degrees, "%0.3f"));
            AddFunction(new ScaledNetworkValue(this, "107", new CalibrationPointCollectionDouble(-1, -180, 1, 180), "Instruments", "ADI Roll Secu?", "Roll in degrees", "Value -180 to 180", BindingValueUnits.Degrees, "%0.3f"));
            AddFunction(new ScaledNetworkValue(this, "108", new CalibrationPointCollectionDouble(-1, -90, 1, 90), "Instruments", "ADI Pitch Secu?", "Pitch in degrees", "Value -90 to 90", BindingValueUnits.Degrees, "%0.3f"));
            calibration = new CalibrationPointCollectionDouble(-1, -6000, 1, 6000);
            calibration.Add(new CalibrationPointDouble(-0.8, -4000));
            calibration.Add(new CalibrationPointDouble(-0.2, -1000));
            calibration.Add(new CalibrationPointDouble(0, 0));
            calibration.Add(new CalibrationPointDouble(0.2, 1000));
            calibration.Add(new CalibrationPointDouble(0.8, 4000));
            AddFunction(new ScaledNetworkValue(this, "109", calibration, "Instruments", "VVI", "Vertical velocity of the aircraft", "Value -6000 to 6000", BindingValueUnits.FeetPerSecond, "%0.3f"));
            calibration = new CalibrationPointCollectionDouble(-1, 0, 1, 2000);
            calibration.Add(new CalibrationPointDouble(0,1000));
            AddFunction(new ScaledNetworkValue(this, "110", calibration, "Instruments", "Total Fuel", "Total weight of fuel in the aircraft", "Value 0 to 2000", BindingValueUnits.Pounds, "%0.4f"));
            //AddFunction(new ScaledNetworkValue(this, "111", new CalibrationPointCollectionDouble(-1, 0, 1, 100000), "Instruments", "Barometric altitude (ten thousands digit)", "Barometric altitude of aircraft (ten thousands digit)", "Value 0 to 10", BindingValueUnits.Feet, "%0.3f"));
            //AddFunction(new ScaledNetworkValue(this, "112", new CalibrationPointCollectionDouble(-1, 0, 1, 10000), "Instruments", "Barometric altitude (thousands digit)", "Barometric altitude of aircraft (thousands digit)", "Value 0 to 10", BindingValueUnits.Feet, "%0.3f"));
            //AddFunction(new ScaledNetworkValue(this, "113", new CalibrationPointCollectionDouble(-1, 0, 1, 1000), "Instruments", "Barometric altitude (hundreds digit)", "Barometric altitude of aircraft (hundreds digit)", "Value 0 to 10", BindingValueUnits.Feet, "%0.3f"));
            AddFunction(new ScaledNetworkValue(this, "114", new CalibrationPointCollectionDouble(-1, 0, 1, 12), "Instruments", "Chronometer (hours)", "Hours display on the chronometer", "Value 0 to 12", BindingValueUnits.Hours, "%0.3f"));
            AddFunction(new ScaledNetworkValue(this, "115", new CalibrationPointCollectionDouble(-1, 0, 1, 60), "Instruments", "Chronometer (minutes)", "Minutes display on the chronometer", "Value 0 to 60", BindingValueUnits.Minutes, "%0.3f"));
            AddFunction(new ScaledNetworkValue(this, "116", new CalibrationPointCollectionDouble(-1, 0, 1, 60), "Instruments", "Chronometer (seconds)", "seconds display on the chronometer", "Value 0 to 60", BindingValueUnits.Seconds, "%0.3f"));
            AddFunction(new ScaledNetworkValue(this, "117", new CalibrationPointCollectionDouble(-1, -10, 1, 10), "Instruments", "Slide angle", "slide angle of aircraft in degrees", "Value -10 to 10", BindingValueUnits.Degrees, "%0.3f"));
            AddFunction(new ScaledNetworkValue(this, "118", new CalibrationPointCollectionDouble(-1, -45, 1, 45), "Instruments", "Turn rate indicator", "Turn rate of aircraft in degrees", "Value -45 to 45", BindingValueUnits.Degrees, "%0.3f"));
            AddFunction(new TACANChannel(this));
            //AddFunction(new ScaledNetworkValue(this, "119", new CalibrationPointCollectionDouble(-1, 0, 1, 1000), "TACAN", "TACAN channel (hundreds digit)", "TACAN channel (hundreds digit)", "Value 0 to 1000", BindingValueUnits.Numeric, "%0.3f"));
            //AddFunction(new ScaledNetworkValue(this, "120", new CalibrationPointCollectionDouble(-1, 0, 1, 100), "TACAN", "TACAN channel (tens digit)", "TACAN channel (tens digit)", "Value 0 to 100", BindingValueUnits.Numeric, "%0.3f"));
            //AddFunction(new ScaledNetworkValue(this, "121", new CalibrationPointCollectionDouble(-1, 0, 1, 10), "TACAN", "TACAN channel (units digit)", "TACAN channel (units digit)", "Value 0 to 10", BindingValueUnits.Numeric, "%0.3f"));
            AddFunction(new ScaledNetworkValue(this, "122", new CalibrationPointCollectionDouble(-1, 0, 1, 360), "TACAN", "TACAN bearing", "TACAN bearing to station", "Value 0 to 360", BindingValueUnits.Degrees, "%0.3f"));
            //AddFunction(new ScaledNetworkValue(this, "123", new CalibrationPointCollectionDouble(-1, 0, 1, 360), "TACAN", "TACAN bearing", "TACAN bearing to station", "Value 0 to 360", BindingValueUnits.Degrees, "%0.3f"));
            AddFunction(new ScaledNetworkValue(this, "124", new CalibrationPointCollectionDouble(-1, 0, 1, 10), "TACAN", "TACAN range (units)", "TACAN range to station (units)", "Value 0 to 360", BindingValueUnits.Degrees, "%0.3f"));


            //            TCN_RNG_V = CreateGauge("parameter")
            //TCN_RNG_V.parameter_name = "TACANRNGV"
            //TCN_RNG_V.arg_number = 123
            //TCN_RNG_V.input = { 0.0, 1.0}
            //            --m
            //TCN_RNG_V.output = { -1.0, 1.0}
            //            TCN_RNG_U = CreateGauge("parameter")
            //TCN_RNG_U.parameter_name = "TACANRNGU"
            //TCN_RNG_U.arg_number = 124
            //TCN_RNG_U.input = { 0.0, 10.0}
            //            --m
            //TCN_RNG_U.output = { -1.0, 1.0}
            //            TCN_RNG_D = CreateGauge("parameter")
            //TCN_RNG_D.parameter_name = "TACANRNGD"
            //TCN_RNG_D.arg_number = 125
            //TCN_RNG_D.input = { 0.0, 100.0}
            //            --m
            //TCN_RNG_D.output = { -1.0, 1.0}

            //            LightFLOOD = CreateGauge("parameter")
            //LightFLOOD.parameter_name = "FLOODLIGHT"
            //LightFLOOD.arg_number = 140
            //LightFLOOD.input = { 0,1}
            //            LightFLOOD.output = { 0,1}
            //            LightCONSOL = CreateGauge("parameter")
            //LightCONSOL.parameter_name = "CONSOLLIGHT"
            //LightCONSOL.arg_number = 141
            //LightCONSOL.input = { 0,1}
            //            LightCONSOL.output = { 0,1}
            //            LightINSTRU = CreateGauge("parameter")
            //LightINSTRU.parameter_name = "INSTRULIGHT"
            //LightINSTRU.arg_number = 142
            //LightINSTRU.input = { 0,1}
            //            LightINSTRU.output = { 0,1}

            #endregion Instruments
#pragma warning restore CS0162 // Unreachable code detected
        }
        virtual protected void AddFunctionsToDcs()
        {
            Dictionary<string, string> idValidator = new Dictionary<string, string>();
            foreach (NetworkFunction nf in _functions)
            {
                if (!idValidator.ContainsKey(nf.DataElements[0].ID))
                {
                    idValidator.Add(nf.DataElements[0].ID, nf.LocalKey);
                    AddFunction(nf);
                }
                else
                {
                    Logger.Warn($"Duplicate Function Found for ID {nf.DataElements[0].ID}: {nf.LocalKey} and {idValidator[nf.DataElements[0].ID]}");
                }
            }
        }
        virtual internal void MakeFunctionsFromDcsModule(IDCSInterfaceCreator ic)
        {
            foreach (string path in new string[] { Path.Combine(DcsPath, "OV-10 Bronco", "Cockpit", "Scripts", "clickabledata.lua") })
            {
                _functions.AddRange(ic.CreateFunctionsFromDcsModule(this, path));
            }
        }
        virtual protected string DcsPath { get => _dcsPath; set => _dcsPath = value; }
    }
}
