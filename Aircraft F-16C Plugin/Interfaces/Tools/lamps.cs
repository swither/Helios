﻿
namespace GadrocsWorkshop.Helios.Interfaces.DCS.F16C
{
#pragma warning disable IDE1006 // Naming Standard issues

    internal enum CautionLights 
    {
        // Caution Light Panel
        FLCS_FAULT = 630,
        ENGINE_FAULT = 631,
        AVIONICS_FAULT = 632,
        SEAT_NOT_ARMED = 633,
        ELEC_SYS = 634,
        SEC = 635,
        EQUIP_HOT = 636,
        NWS_FAIL = 637,
        PROBE_HEAT = 638,
        FUEL_OIL_HOT = 639,
        RADAR_ALT = 640,
        ANTI_SKID = 641,
        CADC = 642,
        INLET_ICING = 643,
        IFF = 644,
        HOOK = 645,
        STORES_CONFIG = 646,
        OVERHEAT = 647,
        NUCLEAR = 648,
        OBOGS = 649,
        ATF_NOT_ENGAGED = 650,
        EEC = 651,
        LIGHTS_MAX = 652,
        CABIN_PRESS = 653,
        FWD_FUEL_LOW = 654,
        BUC = 655,
        LIGHTS_MAX_A = 656,
        LIGHTS_MAX_B = 657,
        AFT_FUEL_LOW = 658,
        LIGHTS_MAX_C = 659,
        LIGHTS_MAX_D = 660,
        LIGHTS_MAX_E = 661,
        MASTER_CAUTION = 117,
        TF_FAIL = 121,
        ENG_FIRE = 126,
        ENGINE = 127,
        HYD_OIL_PRESS = 129,
        FLCS = 130,
        DBU_ON = 131,
        TO_LDG_CONFIG = 133,
        CANOPY = 134,
        OXY_LOW = 135,
        AOA_RED = 110,
        AOA_GREEN = 111,
        AOA_YELLOW = 112,
        GEAR_NOSE = 350,
        GEAR_LEFT = 351,
        GEAR_RIGHT = 352,
        GEAR_WARNING = 369,
        RDY = 113,
        AR_NWS = 114,
        DISC = 115,
        MARKER_BEACON = 157,
        JFS_RUN = 446,
        HYDRAZN = 524,
        AIR = 523,
        EPU = 526,
        FLCS_PMG = 513,
        MAIN_GEN = 512,
        STBY_GEN = 515,
        EPU_GEN = 517,
        EPU_PMG = 516,
        TO_FLCS = 519,
        FLCS_RLY = 518,
        ACFT_BATT_FAIL = 521,
        ACTIVE = 106,
        STBY = 107,
        FL_RUN = 570,
        FL_FAIL = 571,
        FLCS_PWR_A = 581,
        FLCS_PWR_B = 582,
        FLCS_PWR_C = 583,
        FLCS_PWR_D = 584,
        Two_Red_Lines = 119,
    }
    internal enum RWRLights
{
        rwr_Search = 396,
        rwr_Activity = 398,
        rwr_ActPower = 423,
        rwr_Alt_Low = 400,
        rwr_Alt = 424,
        rwr_Power = 402,
        rwr_Hand_Up = 142,
        rwr_Hand_H = 136,
        rwr_Launch = 144,
        rwr_Mode_Pri = 146,
        rwr_Mode_Open = 137,
        rwr_Ship_unkn = 148,
        rwr_Sys_On = 150,
        rwr_Sep_Up = 152,
        rwr_Sep_Down = 138,
    }
    internal enum CmdsLights
{
        cmds_nogo = 370,
        cmds_go = 372,
        cmds_disp = 376,
        cmds_rdy = 379,
    }
    internal enum InternalLights 
{
        Consoles_lt = 788,
        InstPnl_lt = 787,
        ConsolesFlood_lt = 790,
        InstPnlFlood_lt = 791,
    }
    internal enum ECM_Button_Lights 
    {
        Btn_1_S = 461,
        Btn_1_A = 462,
        Btn_1_F = 463,
        Btn_1_T = 464,
        Btn_2_S = 466,
        Btn_2_A = 467,
        Btn_2_F = 468,
        Btn_2_T = 469,
        Btn_3_S = 471,
        Btn_3_A = 472,
        Btn_3_F = 473,
        Btn_3_T = 474,
        Btn_4_S = 476,
        Btn_4_A = 477,
        Btn_4_F = 478,
        Btn_4_T = 479,
        Btn_5_S = 481,
        Btn_5_A = 482,
        Btn_5_F = 483,
        Btn_5_T = 484,
        Btn_6_S = 486,
        Btn_6_A = 487,
        Btn_6_F = 488,
        Btn_6_T = 489,
        Btn_FRM_S = 491,
        Btn_FRM_A = 492,
        Btn_FRM_F = 493,
        Btn_FRM_T = 494,
        Btn_SPL_S = 496,
        Btn_SPL_A = 497,
        Btn_SPL_F = 498,
        Btn_SPL_T = 499,
    }
    internal enum controllers
    {
        brtRwrLights = 792,
        BrightnessCMDS = 789,
        BrightnessWCI = 793,
        ECM_Light = 102,
        BrightnessECM = 803
    }
#pragma warning restore IDE1006 // Naming Standard issues

}
