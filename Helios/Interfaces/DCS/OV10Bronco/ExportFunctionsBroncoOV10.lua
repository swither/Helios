-- Exports.Lua from Helios Bronco OV-10 Interface
function driver.processHighImportance(mainPanelDevice)
	-- Send Altimeter Values	
	helios.send(2051, string.format("%0.4f;%0.4f;%0.5f", mainPanelDevice:get_argument_value(111), mainPanelDevice:get_argument_value(112), mainPanelDevice:get_argument_value(113)))
	--helios.send(2059, string.format("%0.2f;%0.2f;%0.2f;%0.3f", mainPanelDevice:get_argument_value(56), mainPanelDevice:get_argument_value(57), mainPanelDevice:get_argument_value(58), mainPanelDevice:get_argument_value(59)))		
end

function driver.processLowImportance(mainPanelDevice)
	-- TACAN Channel
	helios.send(2263, string.format("%0.2f;%0.2f;%0.2f", mainPanelDevice:get_argument_value(119), mainPanelDevice:get_argument_value(120), mainPanelDevice:get_argument_value(121)))
end