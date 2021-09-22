local fc2_flags = {}

function driver.processExports(selfData)
	-- Flaming Cliffs model only
	local engine = LoGetEngineInfo()
	local hsi    = LoGetControlPanel_HSI()
	local route = LoGetRoute()

	if pitch == nil then
		-- this is a problem for FC, because this is the primary source of instrument data
		-- log only once
		if fc2_flags.pbyLogged == nil then
			log.write("HELIOS.EXPORT", log.DEBUG, "FC2 export received nothing from LoGetADIPitchBankYaw; cannot send telemetry")
			fc2_flags.pbyLogged = true
		end
	end
	if hsi ~= nil then
		helios.send(6, 360 - ((hsi.ADF or hsi.ADF_raw or 0.0) * 57.3), "%.2f")
		helios.send(7, 360 - ((hsi.RMI or hsi.RMI_raw or 0.0) * 57.3), "%.2f")
		helios.send(8, ((hsi.Compass or selfData.Heading or 0.0) * 57.3), "%.2f")
	end
	if engine ~= nil and engine.RPM ~= nil then
		helios.send(9, engine.RPM.left, "%.2f")
		helios.send(10, engine.RPM.right, "%.2f")
	end
	if engine ~= nil and engine.Temperature ~= nil then
		helios.send(11, engine.Temperature.left, "%.2f")
		helios.send(12, engine.Temperature.right, "%.2f")
	end
	if selfData ~= nil then
		local distanceToWay = 999
		local myLoc = LoGeoCoordinatesToLoCoordinates(selfData.LatLongAlt.Long, selfData.LatLongAlt.Lat)
		if myLoc ~= nil and route ~= nil and route.goto_point ~= nil and route.goto_point.world_point ~= nil then
			distanceToWay = math.sqrt((myLoc.x - route.goto_point.world_point.x)^2 + (myLoc.y -  route.goto_point.world_point.y)^2)
			helios.send(15, distanceToWay, "%.2f")
		end
	else
		-- log only once
		if (fc2_flags.selfLogged == nil) then
			log.write("HELIOS.EXPORT", log.DEBUG, "FC2 export received nothing from LoGetSelfData; cannot send location")
			fc2_flags.selfLogged = true
		end
	end
end
