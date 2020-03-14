set VERSION=%1

REM build with version stamps
MSBuild.exe -p:Configuration=Release -t:Clean
MSBuild.exe /p:version=%VERSION% -p:Configuration=Release -t:Rebuild
if %errorlevel% neq 0 exit /b %errorlevel%

REM build installers; this requires https://stackoverflow.com/questions/8648428/an-error-occurred-while-validating-hresult-8000000a/45580775#45580775
devenv Helios.sln /Build Release

REM fix up installer
pushd "Helios Installer\Release"
cscript //nologo ..\HeliosInstallAdjustments.vbs "Helios Installer.msi" %VERSION%
popd