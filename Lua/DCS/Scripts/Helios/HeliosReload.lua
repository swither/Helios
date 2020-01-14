-- luacheck: globals loadfile
local helios_reload_private = {};
local helios_reload = {
    -- seconds between checks on the file modification
    checkInterval = 5.0
}

-- enable hot reloading of the script on changes
function helios_reload.hotload(fullPath)
    local attributes = lfs.attributes(fullPath)
    if attributes == nil then
        log.write("HELIOS.RELOAD", log.ERROR, string.format("hot reloading will not work because the specified file '%s' does not exist", fullPath))
        return
    end

    -- build a new global scope that does not get written when we reload
    helios_reload_private.fullPath = fullPath;
    helios_reload_private.modified = attributes.modification;

    -- start Export script in no-hook mode, so it won't register global callbacks
    log.write("HELIOS.RELOAD", log.INFO, string.format("enabling hot reloading for script '%s'", helios_reload_private.fullPath))
    helios_reload.impl = loadfile(lfs.writedir().."Scripts\\Helios\\HeliosExport16.lua")("nohooks")
end

-- shut down and reload.  NOTE: we will get a new dynamic local port, so Helios will see this like a DCS restart
function helios_reload.reload()
    log.write("HELIOS.RELOAD", log.INFO, string.format("hot reloading script '%s'", helios_reload_private.fullPath))
    helios_reload.impl.unload()
    local success, result = pcall(loadfile, helios_reload_private.fullPath)
    if success then
        -- run it
        success, result = pcall(result, "nohooks")
    end
    if success then
        -- install new version
        helios_reload.impl = result
    else
        log.write("HELIOS.RELOAD", log.ERROR, string.format("hot reload of script '%s' failed", helios_reload_private.fullPath))
        if type(result) == "string" then
            log.write("HELIOS.RELOAD", log.ERROR, result)
        end
    end
    -- NOTE: this either starts the new version or restarts the previous one if we failed to load
    helios_reload.impl.init()
end

-- check for file change on main script
function helios_reload_private.scriptChanged()
    local previous = helios_reload_private.modified or 0
    local attributes = lfs.attributes(helios_reload_private.fullPath)
    if attributes == nil then
        if helios_reload_private.modified ~= nil then
            -- log this once
            log.write("HELIOS.RELOAD", log.ERROR, string.format("script '%s' is no longer accessible; hot reload will not work", helios_reload_private.fullPath))
            helios_reload_private.modified = nil
        end
        return false
    end
    helios_reload_private.modified = attributes.modification
    log.write("HELIOS.RELOAD", log.DEBUG, string.format("checking script '%s' modified at %d; previous version from %d",
        helios_reload_private.fullPath,
        helios_reload_private.modified,
        previous))
    return helios_reload_private.modified > (1 + previous)
end

-- called on our reload check timer, returns true if reloaded
function helios_reload_private.checkReload()
    -- check for file change
    if helios_reload_private.scriptChanged() then
        -- reload everything
        helios_reload.reload()
        return true
    end
    return false
end

-- save and chain any previous exports
helios_reload_private.previousHooks = {}
helios_reload_private.previousHooks.LuaExportStart = LuaExportStart
helios_reload_private.previousHooks.LuaExportStop = LuaExportStop
helios_reload_private.previousHooks.LuaExportActivityNextEvent = LuaExportActivityNextEvent
helios_reload_private.previousHooks.LuaExportBeforeNextFrame = LuaExportBeforeNextFrame
helios_reload_private.previousHooks.LuaExportAfterNextFrame = LuaExportAfterNextFrame

-- utility to chain one DCS hook without arguments
function helios_reload_private.chainHook(functionName)
    _G[functionName] = function() -- luacheck: no global
        -- try execute Helios version of hook
        local success, result = pcall(helios_reload.impl[functionName])
        if not success then
            log.write("HELIOS.RELOAD", log.ERROR, string.format("error return from Helios implementation of '%s'", functionName))
            if type(result) == "string" then
                log.write("HELIOS.RELOAD", log.ERROR, result)
            end
        end
        -- chain to next
        local nextHandler = helios_reload_private.previousHooks[functionName]
        if nextHandler ~= nil then
            nextHandler()
        end
    end
end

-- hook all the basic functions without arguments
helios_reload_private.chainHook("LuaExportStart")
helios_reload_private.chainHook("LuaExportStop")
helios_reload_private.chainHook("LuaExportAfterNextFrame")
helios_reload_private.chainHook("LuaExportBeforeNextFrame")

-- specialized chain for next event hook
function LuaExportActivityNextEvent(timeNow)
    local timeNext;

    -- check for reload
    if helios_reload_private.lastCheck ~= nil then
        local nextCheck = helios_reload_private.lastCheck + helios_reload.checkInterval;
        if timeNow >= nextCheck then
            log.write("HELIOS.RELOAD", log.DEBUG, string.format("checking for reload at time '%f'", timeNow))
            helios_reload_private.checkReload()
            helios_reload_private.lastCheck = timeNow;
            timeNext = timeNow + helios_reload.checkInterval;
        else
            timeNext = nextCheck;
        end
    else
        helios_reload_private.lastCheck = timeNow;
        timeNext = timeNow + helios_reload.checkInterval;
    end

    -- try execute Helios version of hook
    local success, result = pcall(helios_reload.impl.LuaExportActivityNextEvent, timeNow)
    if success then
        if result < timeNext then
            timeNext = result
        end
    else
        log.write("HELIOS.RELOAD", log.ERROR, string.format("error return from Helios implementation of 'LuaExportActivityNextEvent'"))
        if type(result) == "string" then
            log.write("HELIOS.RELOAD", log.ERROR, result)
        end
    end

    -- chain to next and keep closest event time that requires wake up
    local nextHandler = helios_reload_private.previousHooks.LuaExportActivityNextEvent
    if nextHandler ~= nil then
        local timeOther = nextHandler(timeNow)
        if timeOther < timeNext then
            timeNext = timeOther
        end
    end
    return timeNext
end

return helios_reload