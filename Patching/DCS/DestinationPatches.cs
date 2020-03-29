//  Copyright 2014 Craig Courtney
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

namespace GadrocsWorkshop.Helios.Patching.DCS
{
    class DestinationPatches
    {
        public PatchDestination Destination;
        public StatusCodes Status;
        public PatchList Patches;
        public bool Enabled;

        public DestinationPatches(InstallationLocation location, string patchSet)
        {
            Destination = new PatchDestination(location);
            Status = StatusCodes.Unknown;
            Patches = PatchList.LoadPatches(Destination, patchSet);
            Enabled = location.IsEnabled;
        }

        internal void CheckApplied()
        {
            foreach (StatusReportItem result in Patches.Verify(Destination))
            {
                // don't log these results, because Verify considers being out of date to be an error
                if (result.Severity > StatusReportItem.SeverityCode.Warning)
                {
                    Status = StatusCodes.OutOfDate;
                    return;
                }
            }
            // if we survive verifying all patches, it is up to date
            Status = StatusCodes.UpToDate;
        }
    }
}
