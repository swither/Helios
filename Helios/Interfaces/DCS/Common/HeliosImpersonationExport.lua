local driver = {}
local selfName = debug.getinfo(1,'S').short_src:match('^.+[/\\](.+).[Ll][Uu][Aa]\"]$')
local driverPath = string.format("%sScripts\\Helios\\Drivers\\%s.lua", lfs.writedir(), impersonatorSelfName)
success, driver = pcall(dofile, driverPath)
	if not success then
		log.write("HELIOS.EXPORT", log.ERROR, string.format("error return from Helios driver impersonator '%s'", driverPath))
	else
		log.write("HELIOS.EXPORT", log.INFO, string.format('Driver %s impersonating %s',impersonatorSelfName,selfName))
	end
driver.selfName = selfName