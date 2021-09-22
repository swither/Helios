-- Exports.Lua additional functions for Template interface

function ProcessHighImportance(mainPanelDevice)
	-- replace all these, as they are nonsense examples
	SendData(4000, string.format("%0.3f", mainPanelDevice:get_argument_value(1)))
end

function ProcessLowImportance(mainPanelDevice)
	-- replace all these, as they are nonsense examples
	local indication = parse_indication(1)
	if indication then
        SendData("4001", string.format("%2s", indication.example))
	end
	-- replace all these, as they are nonsense examples
	SendData(4002, string.format("%0.3f", mainPanelDevice:get_argument_value(2)))
end

