Option Explicit
On Error Resume Next
Dim argNum, argCount:argCount = Wscript.Arguments.Count
if argCount = 0  then
   Wscript.Echo "Helios Plugin Installer Post Build changes requires an argument which is the" &_
   vbLf & " msi file to have the releases changed, and also set certain flags." &_
   vbLf & " No argument was passed into the HeliosInstallAdjustments.vbs file." &_
   vbLf & " SELECT queries will display the rows of the result list specified in the query"
    Wscript.Quit 1
else
   Dim msiPackage:msiPackage = Wscript.Arguments(0)
   Dim oShell 
   Dim TypeLib
   Dim version:version = Wscript.Arguments(1)
   Dim infinity:infinity = "65535" & MID(version, 2)
   Wscript.Echo "Starting Post Build Script to set up " & msiPackage & " for version " & version
   Wscript.Echo "Setting file versions to " & infinity

   ' https://docs.microsoft.com/en-us/windows/win32/msi/database-object
   ' https://docs.microsoft.com/en-us/windows/win32/msi/session-object

   ' upgrade codes for Helios 1.4+ (production)
   Dim heliosUpgrade:heliosUpgrade = "{CC2079BB-0910-4F0F-A0C0-98687E84DBAC}"
   Dim upgradeCode:upgradeCode = heliosUpgrade
   
   ' upgrade code for Helios Development Prototypes
   Dim heliosDevUpgrade:heliosDevUpgrade = "{AFCCBF57-8468-42DA-9B23-DC535336FC4E}"

   ' open MSI as database session ourselves, instead of using WiRunSQL wrapper
   Dim installer : Set installer = Wscript.CreateObject("WindowsInstaller.Installer") : CheckError "connect to installer"
   Dim database : Set database = installer.OpenDatabase(msiPackage, 1) : CheckError "open database"
   Dim session : Set session = installer.OpenPackage(database,1) : If Err <> 0 Then Fail "Installer: '" & msiPackage & "' has invalid installer package format"


   ' change product version and file versions
   Execute database, "UPDATE Property SET `Value` = '" & version & "' WHERE `Property` = 'ProductVersion'"
   Execute database, "UPDATE File SET `Version` = '" & infinity & "' WHERE `Version` <> ''"

   ' make all upgrades major upgrades
   Set TypeLib = CreateObject("Scriptlet.TypeLib")
   Dim newGuid:newGuid = TypeLib.Guid
   newGuid = Left(newGuid, Len(newGuid)-2)
   Execute database, "UPDATE Property SET `Value` = '" & newGuid & "' WHERE `Property` = 'ProductCode'"

   ' run custom actions as user instead of system
   Execute database, "UPDATE CustomAction SET `Type` = 1025 WHERE `Type` = 3073"

   '  The msi can get built with all sorts of unnecessary dependencies, so we try to stop them
   '  being layed down on the target system
   if not RemoveProblemComponent (database, "F-15E.DLL|F-15E.dll") then
		Wscript.Echo  "* * * Info: No unnecessary dll Components found"
	end if

     ' fix up installation paths (DIRCA_TARGETDIR) for dev tools (or similar) that install on top of Helios folder
   Execute database, "UPDATE CustomAction SET `Target` = '[ProgramFiles64Folder][Manufacturer]\Helios' WHERE `Target` = '[ProgramFiles64Folder][Manufacturer]\[ProductName]'"
   Execute database, "UPDATE CustomAction SET `Target` = '[ProgramFilesFolder][Manufacturer]\Helios' WHERE `Target` = '[ProgramFilesFolder][Manufacturer]\[ProductName]'"


   ' special handling for development builds
   Dim devBuild
   Set devBuild = New RegExp
   devBuild.Pattern = "[0-9]+\.[0-9]+\.1...\.[0-9]+"
   if devBuild.Test(version) then

     if session.Property("ProductName") = "Helios F-15E Plugin" then
         Wscript.Echo "Changing product to HeliosDev F-15E Plugin"
         upgradeCode = heliosDevUpgrade
         Execute database, "UPDATE Property SET `Value` = '" & heliosDevUpgrade & "' WHERE `Property` = 'UpgradeCode'"
         Execute database, "UPDATE Property SET `Value` = 'HeliosDev F-15E Plugin' WHERE `Property` = 'ProductName'"
     end if

     ' fix up installation paths (DIRCA_TARGETDIR) for dev tools (or similar) that install on top of Helios folder
     Execute database, "UPDATE CustomAction SET `Target` = '[ProgramFiles64Folder][Manufacturer]\\HeliosDev' WHERE `Target` = '[ProgramFiles64Folder][Manufacturer]\Helios'"
     Execute database, "UPDATE CustomAction SET `Target` = '[ProgramFilesFolder][Manufacturer]\\HeliosDev' WHERE `Target` = '[ProgramFilesFolder][Manufacturer]\Helios'"

     ' fix up start menu folder
     Execute database, "UPDATE Directory SET `DefaultDir` = 'HELIOS~1|HeliosDev' WHERE `DefaultDir` = 'HELIOS|Helios'"
   end if

   ' allow any version to upgrade
   Execute database, "DELETE FROM Upgrade"
   Execute database, "INSERT INTO Upgrade(UpgradeCode, VersionMin, Attributes, ActionProperty) VALUES ('" & upgradeCode & "', '" & version & "', '258', 'NEWERPRODUCTFOUND')"
   Execute database, "INSERT INTO Upgrade(UpgradeCode, VersionMax, Attributes, ActionProperty) VALUES ('" & upgradeCode & "', '" & version & "', '0', 'PREVIOUSVERSIONINSTALLED')"

   ' "UPDATE MsiAssemblyName SET Value = '" & version & "' WHERE `Value` = '" & sOldRel & "'"

   ' set disable advertise shortcuts if not already done
   if session.Property("DISABLEADVTSHORTCUTS") < "1" then
      Wscript.Echo "Setting DISABLEADVTSHORTCUTS = 1"
      Execute database, "INSERT INTO Property(Property, Value) VALUES ('DISABLEADVTSHORTCUTS', '1')"
   else
      Wscript.Echo "DISABLEADVTSHORTCUTS " & session.Property("DISABLEADVTSHORTCUTS")
   end if

   ' commit database
   database.Commit() : CheckError "commit"

   ' check results
   ' XXX since we can't correctly close the views, we need to do this in another process
   ' session = nothing
   ' database = nothing
   ' Dim session2 : Set session2 = installer.OpenPackage(msiPackage, 1) : If Err <> 0 ThenFail "Cannot reopen '" & msiPackage & "' to check results"
   ' Wscript.Echo "UpgradeCode " & session2.Property("UpgradeCode")
   ' Wscript.Echo "ProductVersion " & session2.Property("ProductVersion")
   ' Wscript.Echo "ProductCode " & session2.Property("ProductCode")
end if

Wscript.Quit 0

Sub Execute(database, sql)
   Dim view: Set view = database.OpenView(sql) : CheckError sql
   view.Execute : CheckError sql
   ' this appears to be async, so it breaks if we release view, and there is no way
   ' to wait for result because Fetch is not allowed
   ' Dim record: Set record = view.Fetch
   ' view.Close
   ' view = nothing

end Sub

Function RemoveProblemComponent(database, fileName)
	RemoveProblemComponent = false
    '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
    ''' REVISIT:  If the Windows Installer starts processing dependencies more reliably, then the following code to '''
    '''           delete a unnecessary and problematic dependencies can probably be removed.                        '''
    '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
    ' This is to remove a problematic copies of all dll's which are the not our intended ones.
	' sql for msi is very limited :-(
	Dim sql: sql = "SELECT Component.KeyPath, Component.ComponentId, File.FileName FROM Component, File " & _
										   "WHERE File.Component_ = Component.Component AND File.FileName <> '"& fileName &"'"
	Dim componentView: Set componentView = database.OpenView(sql)
	componentView.Execute  : CheckError sql
	Dim Record: Set Record = componentView.Fetch : CheckError sql
	do until Record Is Nothing
		if instr(Record.StringData(3),".dll") > 0 then
          Execute database, "DELETE FROM Component WHERE `KeyPath` = '" & Record.StringData(1) & "' " 
          Wscript.Echo "Delete Component: " & Record.StringData(2) & " for File " & Record.StringData(3)
          RemoveProblemComponent = true
		end if 
		set Record = componentView.Fetch
	loop
end Function

Sub CheckError(context)
    Dim message, errRec
    If Err = 0 Then Exit Sub
    message = context & ": " & Err.Source & " " & Hex(Err) & ": " & Err.Description
    If Not installer Is Nothing Then
        Set errRec = installer.LastErrorRecord
        If Not errRec Is Nothing Then message = message & vbNewLine & errRec.FormatText
    End If
    Fail message
End Sub

Sub Fail(message)
    Wscript.Echo message
    Wscript.Quit 2
End Sub

 
