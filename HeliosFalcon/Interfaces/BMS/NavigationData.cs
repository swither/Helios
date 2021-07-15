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

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using GadrocsWorkshop.Helios.ComponentModel;
using GadrocsWorkshop.Helios.Interfaces.Falcon.BMS;

namespace GadrocsWorkshop.Helios.Interfaces.Falcon
{
    public class NavigationData
    {
        private List<string> _navPoints = new List<string>();

        #region Properties
        public List<string> NavPoints
        {
            get
            {
                return _navPoints;
            }
        }
        #endregion

        public List<string> ParseStringData(StringData stringData)
        {
            foreach (var item in stringData.data)
            {
                Console.WriteLine(item.value);
                if(item.value.Contains("NP:")) { _navPoints.Add(item.value.Replace(";","")); }
            }
            return _navPoints;
        }
    }
}
