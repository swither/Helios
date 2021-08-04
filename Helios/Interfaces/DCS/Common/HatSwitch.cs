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

using System;
using System.Collections.Generic;
using System.Globalization;
using GadrocsWorkshop.Helios.UDPInterface;
using Newtonsoft.Json;

namespace GadrocsWorkshop.Helios.Interfaces.DCS.Common
{
    public class HatSwitch : DCSFunctionWithActions
    {
        public enum HatPosition : int
        {
            Center,
            Up,
            Down,
            Left,
            Right
        }

        protected readonly string _id;
        protected readonly string _format;
        protected bool _everyframe;

        private HatPosition _currentPosition = HatPosition.Center;

        private HeliosValue _positionValue;

        private readonly Dictionary<string, HatPosition> _positionByValue = new Dictionary<string, HatPosition>();
        private readonly Dictionary<HatPosition, string> _sendData = new Dictionary<HatPosition, string>();

        public HatSwitch(BaseUDPInterface sourceInterface, string deviceId, string argId,
                    string leftAction, string leftValue,
                    string upAction, string upValue,
                    string rightAction, string rightValue,
                    string downAction, string downValue,
                    string releaseAction, string releaseValue,
                    string device, string name, string exportFormat)
            : this(sourceInterface, deviceId, argId, leftAction, leftValue, upAction, upValue, rightAction, rightValue, downAction, downValue, releaseAction, releaseValue, device, name, exportFormat, false)
        {
            // all code in referenced constructor
        }

        public HatSwitch(BaseUDPInterface sourceInterface, string deviceId, string argId, 
                            string leftAction, string leftValue, 
                            string upAction, string upValue, 
                            string rightAction, string rightValue, 
                            string downAction, string downValue, 
                            string releaseAction, string releaseValue,
                            string device, string name, string exportFormat, bool everyFrame)
            : base(sourceInterface, device, name, "Current position of the hat switch.")
        {
            _id = argId;
            _format = exportFormat;
            _everyframe = everyFrame;
            SerializedActions.Add("left", new SerializedAction()
            {
                DeviceID = deviceId,
                ActionID = leftAction,
                ActionValue = leftValue
            });
            SerializedActions.Add("up", new SerializedAction()
            {
                DeviceID = deviceId,
                ActionID = upAction,
                ActionValue = upValue
            });
            SerializedActions.Add("right", new SerializedAction()
            {
                DeviceID = deviceId,
                ActionID = rightAction,
                ActionValue = rightValue
            });
            SerializedActions.Add("down", new SerializedAction()
            {
                DeviceID = deviceId,
                ActionID = downAction,
                ActionValue = downValue
            });
            SerializedActions.Add("center", new SerializedAction()
            {
                DeviceID = deviceId,
                ActionID = releaseAction,
                ActionValue = releaseValue
            });
            DoBuild();
        }

        // deserialization constructor
        public HatSwitch(BaseUDPInterface sourceInterface, System.Runtime.Serialization.StreamingContext context)
            : base(sourceInterface, context)
        {
            // no code
        }

        public override void BuildAfterDeserialization()
        {
            DoBuild();
        }

        private void DoBuild()
        {
            foreach (HatPosition position in Enum.GetValues(typeof(HatPosition)))
            {
                string key = position.ToString().ToLowerInvariant();
                _sendData.Add(position,
                    "C" + SerializedActions[key].DeviceID + "," + SerializedActions[key].ActionID + "," +
                    SerializedActions[key].ActionValue);
                _positionByValue.Add(SerializedActions[key].ActionValue, position);
            }

            _positionValue = new HeliosValue(SourceInterface, new BindingValue((double) _currentPosition), SerializedDeviceName,
                SerializedFunctionName, SerializedDescription,
                "Position 0 = center, 1 = up, 2 = down, 3 = left,  or 4 = right.", BindingValueUnits.Numeric);
            _positionValue.Execute += SetPositionAction_Execute;
            Values.Add(_positionValue);
            Actions.Add(_positionValue);
            Triggers.Add(_positionValue);
        }

        [JsonProperty("switchPosition")]
        public HatPosition SwitchPosition
        {
            get
            {
                return _currentPosition;
            }
            set
            {
                if (!_currentPosition.Equals(value))
                {
                    _currentPosition = value;
                    _positionValue.SetValue(new BindingValue(((int)_currentPosition).ToString(CultureInfo.InvariantCulture)), SourceInterface.BypassTriggers);
                }
            }
        }

        void SetPositionAction_Execute(object action, HeliosActionEventArgs e)
        {
            SourceInterface.BeginTriggerBypass(e.BypassCascadingTriggers);
            try
            {
                SwitchPosition = (HatPosition)Enum.Parse(typeof(HatPosition), e.Value.StringValue);
                SourceInterface.SendData(_sendData[SwitchPosition]);
            }
            catch
            {
                // No-op if the parse fails we won't set the position.
            }
            SourceInterface.EndTriggerBypass(e.BypassCascadingTriggers);
        }

        public override void ProcessNetworkData(string id, string value)
        {
            if (_positionByValue.ContainsKey(value))
            {
                SwitchPosition = _positionByValue[value];
            }
        }

        protected override ExportDataElement[] DefaultDataElements =>
            new ExportDataElement[] { new DCSDataElement(_id, _format, _everyframe) };

        public override void Reset()
        {
            SourceInterface.BeginTriggerBypass(true);
            SwitchPosition = HatPosition.Center;
            SourceInterface.EndTriggerBypass(true);
        }
    }
}
