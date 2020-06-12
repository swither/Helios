@echo off
REM output location (you will need to change this for your local folder structure)
set HELIOS_SHARE_FOLDER=d:\google\derammo.github\Helios

REM arguments and validation
set HELIOS_BUILT_VERSION=%1
set HELIOS_REFERENCE_TAG=%2
git rev-parse %HELIOS_BUILT_VERSION%
if %errorlevel% neq 0 (
	echo Failed to check version git tag.  First parameter should be tag of build to publish
	exit /b %errorlevel%
)
git rev-parse %HELIOS_REFERENCE_TAG%
if %errorlevel% neq 0 (
	echo Failed to check reference git tag.  Second parameter should be tag of build relative to which we collect logs
	exit /b %errorlevel%
)

REM publish tag
git push origin %HELIOS_BUILT_VERSION%

REM publish installer file
mkdir %HELIOS_SHARE_FOLDER%\%HELIOS_BUILT_VERSION%
copy "Helios Installer\Release\*.msi" %HELIOS_SHARE_FOLDER%\%HELIOS_BUILT_VERSION%\ 
copy "Helios Installer\Release32\*.msi" %HELIOS_SHARE_FOLDER%\%HELIOS_BUILT_VERSION%\ 

REM collect and format log
FOR /F %%i IN ('git rev-parse %HELIOS_BUILT_VERSION%') DO @set COMMIT=%%i
git log --date=short --ancestry-path --decorate-refs="1.*" --format="##### [%%h](https://github.com/HeliosVirtualCockpit/Helios/commit/%%H) by %%an on %%ad %%d%%n%%w(0,4,4)%%B  %%n" %HELIOS_REFERENCE_TAG%..%HELIOS_BUILT_VERSION% > %HELIOS_SHARE_FOLDER%\%HELIOS_BUILT_VERSION%\changes_%COMMIT%.md
 
REM clean up (except HELIOS_BUILT_VERSION)
set HELIOS_REFERENCE_TAG=
set HELIOS_SHARE_FOLDER=