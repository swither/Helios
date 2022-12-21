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

        [JsonProperty("exitValue", NullValueHandling = NullValueHandling.Ignore)]
        public string ExitValue { get; private set; }

        [JsonProperty("argumentValue")]
        public string ArgValue { get; private set; }

        [JsonProperty("name")]
        public string Name { get; private set; }

        [JsonProperty("actionID", NullValueHandling = NullValueHandling.Ignore)]
        public string Action { get; private set; }

        [JsonProperty("stopActionID", NullValueHandling = NullValueHandling.Ignore)]
        public string StopAction { get; private set; }

        [JsonProperty("stopActionValue", NullValueHandling = NullValueHandling.Ignore)]
        public string StopActionValue { get; private set; }

        #endregion

        public override string ToString()
        {            
            return ArgValue + "=" + Name;
        }
    }

    /// <summary>
    /// Creates an array of SwitchPosition, mainly for use in rotary switches
    /// </summary>
    public static class SwitchPositions
    {
        /// <summary>
        /// Creates an array of SwitchPosition, mainly for use in rotary switches
        /// </summary>
        /// <param name="numberOfPositions">Number of positions to be placed into the array</param>
        /// <param name="startValue"> The first value for a switch position</param>
        /// <param name="incrementalValue">The value to be added for each switch position</param>
        /// <param name="arg">The arg used to report the switch position</param>
        /// <param name="positionLabels">String array of the labels to be given to each switch position</param>
        /// <param name="valueFormat">This is used as the parameter of ToString to truncate the numerical value which identifies the switch position</param>
        /// <returns></returns>

        internal static SwitchPosition[] Create(int numberOfPositions, double startValue, double incrementalValue, string arg, string[] positionLabels, string valueFormat = "N1")
        {
            return Create(numberOfPositions, startValue, incrementalValue, arg, positionLabels, null, valueFormat);
        }
        /// <summary>
        /// Creates an array of SwitchPosition, mainly for use in rotary switches with incrementing labels
        /// </summary>
        /// <param name="numberOfPositions">Number of positions to be placed into the array</param>
        /// <param name="startValue"> The first value for a switch position</param>
        /// <param name="incrementalValue">The value to be added for each switch position</param>        /// <param name="arg">The arg used to report the switch position</param>
        /// <param name="arg">The arg used to report the switch position</param>
        /// <param name="positionName">Prefix of the label for each switch position</param>
        /// <param name="valueFormat">This is used as the parameter of ToString to truncate the numerical value which identifies the switch position</param>
        /// <returns></returns>

        internal static SwitchPosition[] Create(int numberOfPositions, double startValue, double incrementalValue, string arg, string positionName = "position", string valueFormat = "N1")
        {
            return Create(numberOfPositions, startValue, incrementalValue, arg, new string[] { }, positionName, valueFormat);
        }
        /// <summary>
        /// Creates an array of SwitchPosition, mainly for use in rotary switches
        /// </summary>
        /// <param name="numberOfPositions">Number of positions to be placed into the array</param>
        /// <param name="startValue"> The first value for a switch position</param>
        /// <param name="incrementalValue">The value to be added for each switch position</param>
        /// <param name="arg">The arg used to report the switch position</param>
        /// <param name="positionLabels">String array of the labels to be given to each switch position</param>
        /// <param name="positionName">Prefix of the label for each switch position</param>
        /// <param name="valueFormat">This is used as the parameter of ToString to truncate the numerical value which identifies the switch position</param>
        /// <returns></returns>

        internal static SwitchPosition[] Create(int numberOfPositions, double startValue, double incrementalValue, string arg, string[] positionLabels, string positionName, string valueFormat = "N1")
        {
            SwitchPosition[] positions = new SwitchPosition[numberOfPositions];
            for (int i = 1; i <= numberOfPositions; i++)
            {
                if (positionLabels.Length == numberOfPositions)
                {
                    positions[i - 1] = new SwitchPosition(PositionValue(i, startValue, incrementalValue, valueFormat).ToString(valueFormat), positionLabels[i - 1], arg);

                }
                else
                {
                    positions[i - 1] = new SwitchPosition(PositionValue(i, startValue, incrementalValue, valueFormat).ToString(valueFormat), $"{positionName} {i}", arg);
                }
            }
            return positions;
        }

        private static double PositionValue(int i, double startValue, double incrementValue, string valueFormat)
        {
            int roundDigits = System.Int32.Parse(valueFormat.Substring(valueFormat.Length - 1, 1));
            return System.Math.Round(startValue + ((i - 1) * incrementValue), roundDigits);
        }
    }

}
