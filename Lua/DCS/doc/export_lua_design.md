# Introduction 

This document explains the design goals for the DCS Export Lua scripts.  It is intended to do two things:
* to facilitate discussion
* to serve as long-lived documentation for future developers

# API Background for Beginners
In any software system, if you allow two files (scripts, DLLs, programs) to talk to each other, you have created an API.  This is because you can separately replace one of the two files and things will stop working if the other file no longer understands what is being sent between them.

In strongly typed languages (C#, C, Swift, etc.) you can define this API as an interface and make sure you never change it.  The language will then make sure you never violate this contract.

Lua doesn't have types and it doesn't have any way to check this sort of compliance.  Code will just run and blow up if a certain function that used to be there is gone or starts acting otherwise.  That's why we need to be careful how many APIs we create and how we manage them.  People will mess up their Export Scripts in the field and stuff will mysteriously fail and we cannot afford to support them.

# Requirement 1: Support for multiple planes / vehicles
From this point, we will say 'vehicles' in this document, because DCS isn't limited to just planes.  We have a requirement that Helios support different vehicles.

# Requirement 2: Helios needs to know what plane is being exported
In Helios, different vehicles will potentially use the same export code numbers to mean different things.  This is in contrast to, for example, DCS-BIOS, which has unique numbers for every code point of every aircraft.  Helios made the other choice, because there is no translation of code numbers for basic exported values.  We just send the actual number that DCS uses internally, if the value is from "Device 0", which means from the basic set of vehicle properties.  This means that Helios really needs to know what vehicle is being eported in order to operate safely.   Otherwise, DCS could send a value differently for a certain aircraft and Helios could malfunction.

Some profiles work around this situation by mapping all aircraft to the A-10C interface.  We should not need to do that, if Helios can correctly handle vehicle changes.  

# Requirement 3: Helios must have a stable API between the export script and vehicle-specific code
In the real world, people will have customized Export scripts.  We need to do what we can to make these continue to work from Helios version to Helios version.  Therefore, we need to make an effort to create a stable API between these pieces, so that an older vehicle script will continue to work.

Since Lua does not have a real API system, we use [luacheck](https://github.com/mpeterv/luacheck) to keep the API stable.  This way, we can detect anything being added or removed from the Helios export API and we can do a careful review.  There is no way to enforce the types of arguments passed to the API (in any reasonable way), so this is the only reasonable level of enforcement we have found so far. 

# Requirement 4: Helios export core must not be split across files
The parts of the Helios export script that are not vehicle-specfic must not be split across files.  We do want to clearly separate the private code from the exported API, but these must be stored in the same file.  This is required because users will copy these files around, and we must therefore disallow having one file from one version and another file from another version.  This follows from the API Background explained above, because the private code and the public API are tightly coupled and have no API between them, so they must be locked together.

# ~~Non-Requirement 5: Helios must continue to support only exporting the minimum required data~~
This requirement was based on a misunderstanding of how Helios works.  This is not required.

# Requirement 6: Helios must also support users who don't use the generated export scripts
There is significant investment into export scripts (e.g. CaptZeen's work) that do a lot of custom work in the export.  These must continue to be supported.  That means Helios needs to know not to overwrite this export scripts and they must continue to work as they do today.  This places special requirements on any new protocols between Helios and the Export script, since they have to operate safely even if they are not supported by the Export script.

# Requirement 7: Helios must support vehicle impersonation
It must continue to be possible to use the A-10C Helios Interface (for example) with an FA-18C_hornet vehicle (for example.)  This is actually required for the Helios Generic DCS interface also, since it doesn't natively know what planes it is used for. 

# Requirement 8: Helios private code must not be accessible to vehicle export scripts
Since the internal code in the Export core can change from version to version, it must not be visible to the scripts that our users may modify and maintain themselves.  This requirement is somewhat obvious from the API discussion above, but is placed here as a check list item.

# Nice-to-have 9: Helios export scripts should be executable outside DCS
For ease of development and testing, it should be possible to run the Export scripts outside DCS, such as in a Lua debugger.  It probably isn't necessary to mock up the vehicle data, but we should be able to test protocol sequences.

# Requirement 10: Export.lua should be a tiny stub that supports chaining
Export.lua itself should just be a stub that loads the Helios code from another file.  It should also support the chaining of exports via chaining the DCS hook calls. 

# Nice-to-have 11: During development, it should be possible to hot-reload the Helios Export script without restarting DCS
If we have Requirement 10, then we can have a version of Export.lua that reloads Helios if the files change, without restarting.  That would really help with the development cycle time.

# Requirement 12: File naming should allow incompatible versions to coexist (not run)
When we release a major version of Helios that introduces incompatible changes (if we have to) then there needs to be a way to tell that one file is the Export.lua for the previous version and one is for the new one.  Users will need this to migrate their changes they may have made to the Export script.  For example, if Helios 1.7 introduces a new Export.lua, then people can hit the "update Exports" button in Profile Editor and get a new HeliosExports17.lua that sits next to HeliosExports16.lua (modified).  Then they can switch to calling the new one (from Export.lua) after they migrate their changes.

# Maybe Requirement 13:
DCS Export can be tuned (via Export Frequency in Helios UI, which becones Export Interval in the scripts).   If anyone is actually using this to performance tune, then this should be per module/driver/plane and not one setting for all airplanes supported by the same Export.lua? 