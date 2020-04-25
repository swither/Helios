@set VERSION=%1

@REM make sure clean working directory

@REM step 1: untracked files
@REM result code not set correctly on windows it seems, so we have to check output
@set LSFILES=
@set gituntracked=git ls-files --exclude-standard --others
@FOR /F %%i IN ('%gituntracked%') DO @set LSFILES=%%i
@if "%LSFILES%" NEQ "" (
	@git status --porcelain
	echo Untracked files in working directory.  Update your gitignore or commit files to git before building.
	exit /b %errorlevel%
)

@REM step 2: staged files
@git diff-index --quiet --cached HEAD --
@if %errorlevel% neq 0 (
	@git status --porcelain
	echo Unfinished commit in index.  Complete commit or stash changes before building.
	exit /b %errorlevel%
)

@REM step 3: unstaged modifications 
@git diff-files --quiet
@if %errorlevel% neq 0 (
	@git status --porcelain
	echo Working directory is not clean.  Stash changes before building.
	exit /b %errorlevel%
)

@REM create tag, make sure it is unique
git tag %1
@if %errorlevel% neq 0 (
	echo Failed to place git tag.  Ensure you used a unique major.minor.build.revision number
	exit /b %errorlevel%
)

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