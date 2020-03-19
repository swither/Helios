dofile(LockOn_Options.script_path.."UHF_Radio/indicator/common_init.lua")

purposes = {render_purpose.GENERAL}

dofile(LockOn_Options.script_path.."UHF_Radio/indicator/indicators.lua")
indicator = indicators.PRESET_CHANNEL

dofile(LockOn_Options.common_script_path.."ViewportHandling.lua")
try_find_assigned_viewport("A10C_UHFP", "UHF_PRESET_CHANNEL")
