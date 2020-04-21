@set VERSION=%1

@REM build with version stamps
MSBuild.exe -binaryLogger:LogFile=clean.binlog -clp:WarningsOnly -warnAsMessage:MSB4078 -p:Configuration=Release;Platform=x64 -t:Clean Helios.sln
@rmdir /s /q bin
@MSBuild.exe -binaryLogger:LogFile=prebuild.binlog -clp:WarningsOnly -warnAsMessage:MSB4078 -p:version=%VERSION% -p:Configuration=Release;Platform=x64 BuildMeFirst.sln
@MSBuild.exe -binaryLogger:LogFile=build.binlog  -clp:WarningsOnly -warnAsMessage:MSB4078 -p:version=%VERSION% -p:Configuration=NoInstallers;Platform=x64 Helios.sln
@if %errorlevel% neq 0 (
	echo build of "NoInstallers" configuration failed.  Installer will not be built.
	exit /b %errorlevel%
)

@REM build installers; this requires https://stackoverflow.com/questions/8648428/an-error-occurred-while-validating-hresult-8000000a/45580775#45580775
@devenv Helios.sln /Build "JustInstallers|x64"

@REM fix up installer
@pushd "Helios Installer\Release"
@cscript //nologo ..\HeliosInstallAdjustments.vbs "Helios Installer.msi" %VERSION%
@popd