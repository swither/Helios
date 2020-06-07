# Notes for Creating a Helios Build

## Building

see wiki on [Building a Release](https://github.com/HeliosVirtualCockpit/Helios/wiki/Building-a-Release)

## Post Build Activities

(to be reviewed and tested)

In VS, do a CTRL-O and open Helios Setup.exe.  Delete the two existing icons, and RMB to add resource for a new icon and import 
helios\Helios Installer\Graphics\helios_icon.ico

If the install dialogues need to be changed, then this is done in the properties of the Helios Installer project with F4 (RMB properties does not show these).

### About
```
Helios is a virtual cockpit simulator system for aircraft in the DCS World.  With Helios, you can create virtual cockpits, which allow you to increase your immersion in your favourite combat aircraft.  Helios profiles can be created to allow you to simulate switches, knobs, gauges and more complex instruments which can then be mapped into DCS to give you a much improved combat pilot experience.  Many people use a touch screen monitor with their virtual cockpits.   It is also possible to run the Helios cockpit on a remote PC.

Helios was originally created by Craig "Gadroc" Courtney.  Gadroc donated his code to the open source community and this code is currently delivered out of the HeliosVirtualCockpit fork on Github. 
```

### Details
```
```

### Eagle Forum
Update my EDForums Signature

## Post Ship 

Create a new entry in the ChangeLog.md file so that new features can be added into this file.
Clean up and GH Issues that have been resolved in the shipped code. 
