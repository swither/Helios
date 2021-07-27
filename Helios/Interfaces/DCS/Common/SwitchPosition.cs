// Copyright 2020 Helios Contributors
// 
// Helios is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// Helios is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
// 

using Newtonsoft.Json;

namespace GadrocsWorkshop.Helios.Interfaces.DCS.Common
{
    public class SwitchPosition
    {
        // used by deserialization
        [JsonConstructor]
        private SwitchPosition()
        {
            // no code
        }

        public SwitchPosition(string argValue, string name, string action)
            : this(argValue, name, action, null, null)
        {
        }

        public SwitchPosition(string argValue, string name, string action, string stopAction, string stopActionValue)
            : this(argValue, name, action, stopAction, stopActionValue, null)
        {
        }

        public SwitchPosition(string argValue, string name, string action, string stopAction, string stopActionValue, string exitValue)
        {
            ArgValue = argValue;
            Name = name;
            Action = action;
            StopAction = stopAction;
            StopActionValue = stopActionValue;
            ExitValue = exitValue;
        }

        #region Properties

        [JsonProperty("ExitValue", NullValueHandling = NullValueHandling.Ignore)]
        public string ExitValue { get; private set; }

        [JsonProperty("ArgumentValue")]
        public string ArgValue { get; private set; }

        [JsonProperty("Name")]
        public string Name { get; private set; }

        [JsonProperty("ActionID", NullValueHandling = NullValueHandling.Ignore)]
        public string Action { get; private set; }

        [JsonProperty("StopActionID", NullValueHandling = NullValueHandling.Ignore)]
        public string StopAction { get; private set; }

        [JsonProperty("StopActionValue", NullValueHandling = NullValueHandling.Ignore)]
        public string StopActionValue { get; private set; }

        #endregion

        public override string ToString()
        {            
            return ArgValue + "=" + Name;
        }
    }
}
