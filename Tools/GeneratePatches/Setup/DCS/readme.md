These files are using by GeneratePatches to set up a local git repo for DCS' Lua files. They can also be used manually:

- in these instructions, "DCSROOOT" shall be the root directory of the DCS program installation
- copy gitttributes to DCSROOT/.gitattributes
- copy gitignore to DCSROOT/.gitignore
- cd DCSROOT
- git init
- git add .
- git commit -a -m initial

After a potentially long time, git will finish putting all the correct files under git control.  

Now you can test running patches/mods etc and then:

- git diff
  to see the changes you have made
  
- git reset --hard
  to discard all changes and return to the previously clean DCS installation
 
... plus create branches to commit your mods and changes so you can merge them into future DCS upgrades
