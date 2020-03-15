Get-Content d:\users\ammo\src\git\HeliosExperimentation\Helios\Interfaces\DCS\Common\HeliosExport16.lua `
 | %{$_ -replace "HELIOS_REPLACE_IPAddress","127.0.0.1"} `
 | %{$_ -replace "HELIOS_REPLACE_Port","9089"} `
 | %{$_ -replace "HELIOS_REPLACE_ExportInterval","0.067"} `
 | out-file -encoding ASCII Scripts\Helios\HeliosExport16.lua
Push-Location d:\users\ammo\src\git\heliostesting\tools\MockExports
d:\lua51\bin\lua runexport.lua FW-190D9 d:\users\ammo\src\git\HeliosExperimentation\lua\dcs\scripts
# d:\lua51\bin\lua runexport.lua SA342Minigun d:\users\ammo\src\git\HeliosExperimentation\lua\dcs\scripts
Pop-Location