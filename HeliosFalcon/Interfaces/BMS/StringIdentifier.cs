//  Copyright 2021 Todd Kennedy
//  Copyright 2020 Helios Contributors
//    
//  Helios is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  Helios is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;

namespace GadrocsWorkshop.Helios.Interfaces.Falcon.BMS
{
    [Serializable]
    public enum StringIdentifier : uint
    {
        BmsExe = 0,             // BMS exe name, full path
        KeyFile,                // Key file name in use, full path

        BmsBasedir,             // BmsBasedir to BmsPictureDirectory:
        BmsBinDirectory,        // - BMS directories in use
        BmsDataDirectory,
        BmsUIArtDirectory,
        BmsUserDirectory,
        BmsAcmiDirectory,
        BmsBriefingsDirectory,
        BmsConfigDirectory,
        BmsLogsDirectory,
        BmsPatchDirectory,
        BmsPictureDirectory,

        ThrName,                 // Current theater name
        ThrCampaigndir,          // ThrCampaigndir to ThrTacrefpicsdir:
        ThrTerraindir,           // - Current theater directories in use
        ThrArtdir,
        ThrMoviedir,
        ThrUisounddir,
        ThrObjectdir,
        Thr3ddatadir,
        ThrMisctexdir,
        ThrSounddir,
        ThrTacrefdir,
        ThrSplashdir,
        ThrCockpitdir,
        ThrSimdatadir,
        ThrSubtitlesdir,
        ThrTacrefpicsdir,

        AcName,                  // Current AC name
        AcNCTR,                  // Current AC NCTR

        ButtonsFile,             // Current 3dbuttons.dat file full path
        CockpitFile,             // Current 3dckpit.dat file full path

// VERSION 3
        NavPoint,                // Multiple entries, one for each NavPoint. Format for each entry is (NP, O1, O2, PT can be concatenated):
                                 // (NavPoint, mandatory) NP:<index>,<type>,<x>,<y>,<z>,<grnd_elev>;
                                 //     <index>        int            NavPoint number, 1-99
                                 //     <type>         two chars      GM (GMPOINT), PO (POSPOINT), WP (WAYPOINT), MK (MARKPOINT), DL (DATALINK)
                                 //                                   CB (CAMPBULLSEYE), L1 (LINE1), L2 (LINE2), L3 (LINE3), L4 (LINE4), PT (PREPLANNEDTHREAD)
                                 //     <x>,<y>        float          position in sim coordinates
                                 //     <z>            float          altitude in 10s of feet
                                 //     <grnd_elev>    float          ground elevation in 10s of feet
                                 // (OA1/OA2, optional) O1:<bearing>,<range>,<alt>; (and/or) O2:<bearing>,<range>,<alt>;
                                 //     <bearing>      float
                                 //     <range>        unsigned int
                                 //     <alt>          unsigned int
                                 // (PPT, optional) PT:<str_id>,<range>,<declutter>;
                                 //     <str_id>       "string"
                                 //     <range>        float
                                 //     <declutter>    int            0 = false, 1 = true


        StringIdentifier_DIM     // (number of identifiers; add new IDs only *above* this one)
    };    
}
