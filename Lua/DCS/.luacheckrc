std = {
    globals = {
        -- modules
        "lfs",
        "log",

        -- helios epxport API
        helios = {
            fields = {
                "autoLoadDelay",
                "ensureString",
                "parseIndication",
                "round",
                "selfName",
                "send",
                "splitString",
                "textureToString",
                "version"
            }
        },

        -- LUA mutable globals
        "package",

        -- DCS mutable export hooks
        "LuaExportStart",
        "LuaExportBeforeNextFrame",
        "LuaExportAfterNextFrame",
        "LuaExportStop",
        "LuaExportActivityNextEvent",

        -- compatibility with modules
        Helios_Udp = {
            fields = {
                -- API functions
                "Flush",
                "ResetChangeValues",
                "Send"
            }
        },
        Helios_Util = {
            fields = {
                -- API functions
                "Convert_Lamp",
                "Convert_SW",
                "Degrees",
                "GetListIndicator",
                "Split",
                "ValueConvert"
            }
        }
    },
    read_globals = {
        -- LUA std
        "dofile",
        "io",
        "ipairs",
        "math",
        "os",
        "pairs",
        "pcall",
        "require",
        "string",
        "table",
        "tonumber",
        "type",

        -- DCS
        "GetDevice",
        "list_indication",
        "LoGeoCoordinatesToLoCoordinates",
        "LoGetAccelerationUnits",
        "LoGetADIPitchBankYaw",
        "LoGetAltitudeAboveGroundLevel",
        "LoGetAltitudeAboveSeaLevel",
        "LoGetAngleOfAttack",
        "LoGetControlPanel_HSI",
        "LoGetEngineInfo",
        "LoGetGlideDeviation",
        "LoGetIndicatedAirSpeed",
        "LoGetNavigationInfo",
        "LoGetRoute",
        "LoGetSelfData",
        "LoGetSideDeviation",
        "LoGetVerticalVelocity"
    }
}
ignore = {
    "631" -- line too long
}
-- WARNING: following sections do not work when running within vscode via vscode-lua, so you must
-- either: execute "lua\bin\luacheck Scripts" manually on the command line or configure an
-- absolute path to luacheck in settings.json (workspace path does not get expanded on vscode-lua)
-- For ease of local hacking, consider running "git update-index --skip-worktree .vscode\settings.json"
files["Scripts/Helios/Drivers/*.lua"] = {
    std = {
        globals = { },
        read_globals = {
            -- modules
            "log",

            -- helios export API
            helios = {
                fields = {
                    "ensureString",
                    "parseIndication",
                    "round",
                    "selfName",
                    "send",
                    "splitString",
                    "textureToString",
                    "version"
                }
            },

            -- LUA std
            "math",
            "table",
            "string",
            "tonumber"

            -- DCS
            -- nothing for now, let"s try to wrap those things that we need in safe alternatives, like parseIndication
        }
    }
 }
 -- Helios modules from Capt Zeen
 files["Scripts/Helios/Mods/*.lua"] = {
    std = {
        globals = {
            -- bugs
            "max_clamp",
            "rockets_firing_mode",
            "up",
            "ADF_ARN83_Frequency",
            "ADF_band_selector",
            "multi_sw_121",
            "multi_sw_172",

            -- module exports
            PLANE = {
                fields = {
                    -- write backs
                    "Name", -- self name
                    "FlamingCliffsAircraft", -- mode switch to bypass processing
                    "HighImportanceArguments", -- arguments as normal
                    "LowImportanceArguments", -- arguments as normal

                    -- function hooks
                    "HighImportance", -- extra HighImportance hook after arguments, default is empty
                    "LowImportance",  -- extra LowImportance hook after arguments, default is empty
                    "ProcessInput", -- override of all input/command processing, not chained
                    "ProcessExports",   -- override for FlamingCliffsAircraft, default is empty

                    -- locals stored in the module context
                    "ExportArguments", -- used for translation/impersonation
                    "Get_UV26", -- aircraft specific
                    "StationTypes", -- aircraft specific
                    "Trigger", -- aircraft specific
                    "Check" -- aircraft specific copy of standard utility function
                }
            }
        },
        read_globals = {
            -- modules
            "log",

            -- helios module compatibility API
            Helios_Udp = {
                fields = {
                    -- API functions
                    "Send",
                    "Flush",
                    "ResetChangeValues"
                }
            },
            Helios_Util = {
                fields = {
                    -- API functions
                    "Split",
                    "ValueConvert",
                    "Convert_Lamp",
                    "Degrees",
                    "GetListIndicator",
                    "Convert_SW"
                }
            },

            -- LUA std
            "math",
            "table",
            "string",
            "tonumber",
            "type",
            "tostring",

            -- DCS
            "GetDevice",
            "LoSetCommand",

            -- DCS, to talk to flaming cliffs engine
            "LoGeoCoordinatesToLoCoordinates",
            "LoGetAccelerationUnits",
            "LoGetADIPitchBankYaw",
            "LoGetAltitudeAboveGroundLevel",
            "LoGetAltitudeAboveSeaLevel",
            "LoGetAngleOfAttack",
            "LoGetControlPanel_HSI",
            "LoGetEngineInfo",
            "LoGetGlideDeviation",
            "LoGetIndicatedAirSpeed",
            "LoGetNavigationInfo",
            "LoGetRoute",
            "LoGetSideDeviation",
            "LoGetVerticalVelocity"
        }
    },
    ignore = {
        "211", -- unused (bug)
        "212", -- unused arg (bug)
        "231", -- unused (bug)
        "311", -- overwrite (bug)
        "411", -- redefine (bug)
        "421", -- shadowing (bug)
        "611", -- whitespace line
        "612", -- trailing whitespace
        "614" -- trailing whitespace
    }
}

 -- specific plane modules all have different globals and we can"t for loop here
files["Scripts/Helios/Mods/*.lua"].std.globals.Helios_A10C = files["Scripts/Helios/Mods/*.lua"].std.globals.PLANE
files["Scripts/Helios/Mods/*.lua"].std.globals.Helios_F14 = files["Scripts/Helios/Mods/*.lua"].std.globals.PLANE
files["Scripts/Helios/Mods/*.lua"].std.globals.Helios_F16C = files["Scripts/Helios/Mods/*.lua"].std.globals.PLANE
files["Scripts/Helios/Mods/*.lua"].std.globals.Helios_F18C = files["Scripts/Helios/Mods/*.lua"].std.globals.PLANE
files["Scripts/Helios/Mods/*.lua"].std.globals.Helios_FC = files["Scripts/Helios/Mods/*.lua"].std.globals.PLANE
files["Scripts/Helios/Mods/*.lua"].std.globals.Helios_Harrier = files["Scripts/Helios/Mods/*.lua"].std.globals.PLANE
files["Scripts/Helios/Mods/*.lua"].std.globals.Helios_Huey = files["Scripts/Helios/Mods/*.lua"].std.globals.PLANE
files["Scripts/Helios/Mods/*.lua"].std.globals.Helios_KA50 = files["Scripts/Helios/Mods/*.lua"].std.globals.PLANE
files["Scripts/Helios/Mods/*.lua"].std.globals.Helios_L39 = files["Scripts/Helios/Mods/*.lua"].std.globals.PLANE
files["Scripts/Helios/Mods/*.lua"].std.globals.Helios_MI8 = files["Scripts/Helios/Mods/*.lua"].std.globals.PLANE
files["Scripts/Helios/Mods/*.lua"].std.globals.Helios_Mig21Bis = files["Scripts/Helios/Mods/*.lua"].std.globals.PLANE
files["Scripts/Helios/Mods/*.lua"].std.globals.Helios_P51 = files["Scripts/Helios/Mods/*.lua"].std.globals.PLANE
files["Scripts/Helios/Mods/*.lua"].std.globals.Helios_SA342 = files["Scripts/Helios/Mods/*.lua"].std.globals.PLANE

