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

using System.Collections.Generic;
using GadrocsWorkshop.Helios.UDPInterface;
using Newtonsoft.Json;

namespace GadrocsWorkshop.Helios.Interfaces.DCS.Common
{
    public class GuardedSwitch : Switch
    {
        private enum GuardPositionValue {
            guardUp = 1,
            guardDown = 2
        };
        private GuardPositionValue _guardPosition = GuardPositionValue.guardDown;

        // this ends up in our serialized ExportDataElement array, so we don't need to persist it
        private readonly string _guardArgId;

        [JsonProperty("actions")]
        protected Dictionary<string, DCSFunctionWithActions.SerializedAction> SerializedActions { get; private set; } = new Dictionary<string, DCSFunctionWithActions.SerializedAction>();

        private HeliosValue _guardValue;
        private HeliosAction _autoguardPositionAction;

        #region Static Factories

        public static GuardedSwitch CreateToggleSwitch(BaseUDPInterface sourceInterface, string deviceId, string action, string argId,
                                                    string guardAction, string guardArgId, string guardUpValue, string guardDownValue,
                                                    string position1Value, string position1Name,
                                                    string position2Value, string position2Name,
                                                    string device, string name, string exportFormat)
        {
            return new GuardedSwitch(sourceInterface, deviceId, argId, guardAction, guardArgId, guardUpValue, guardDownValue, new SwitchPosition[] { new SwitchPosition(position1Value, position1Name, action), new SwitchPosition(position2Value, position2Name, action) }, device, name, exportFormat);
        }

        public static GuardedSwitch CreateThreeWaySwitch(BaseUDPInterface sourceInterface, string deviceId, string action, string argId,
                                                    string guardAction, string guardArgId, string guardUpValue, string guardDownValue,
                                                    string position1Value, string position1Name,
                                                    string position2Value, string position2Name,
                                                    string position3Value, string position3Name,
                                                    string device, string name, string exportFormat)
        {
            return new GuardedSwitch(sourceInterface, deviceId, argId, guardAction, guardArgId, guardUpValue, guardDownValue,
                    new SwitchPosition[] { new SwitchPosition(position1Value, position1Name, action), new SwitchPosition(position2Value, position2Name, action), new SwitchPosition(position3Value, position3Name, action) },
                    device, name, exportFormat);
        }

        #endregion

        public GuardedSwitch(BaseUDPInterface sourceInterface, string deviceId, string argId, string guardAction, string guardArgId, string guardUpValue, string guardDownValue, SwitchPosition[] positions, string device, string name, string exportFormat)
            : this(sourceInterface, deviceId, argId, guardAction, guardArgId, guardUpValue, guardDownValue, positions, device, name, exportFormat, false)
        {
        }

        public GuardedSwitch(BaseUDPInterface sourceInterface, string deviceId, string argId, string guardAction, string guardArgId, string guardUpValue, string guardDownValue, SwitchPosition[] positions, string device, string name, string exportFormat, bool everyFrame)
            : base(sourceInterface, deviceId, argId, positions, device, name, exportFormat, everyFrame, false)
        {
            // base does not call its DoBuild, because we are using a special constructor
            _guardArgId = guardArgId;
            SerializedActions.Add(GuardPositionValue.guardUp.ToString(), new DCSFunctionWithActions.SerializedAction()
            {
                DeviceID = deviceId,
                ActionID = guardAction,
                ActionValue = guardUpValue
            });
            SerializedActions.Add(GuardPositionValue.guardDown.ToString(), new DCSFunctionWithActions.SerializedAction()
            {
                DeviceID = deviceId,
                ActionID = guardAction,
                ActionValue = guardDownValue
            });

            // now build everything with the additional actions
            base.DoBuild();
            DoBuild();
        }

        // deserialization constructor
        public GuardedSwitch(BaseUDPInterface sourceInterface, System.Runtime.Serialization.StreamingContext context)
            : base(sourceInterface, context)
        {
            // no code
        }

        public override void BuildAfterDeserialization()
        {
            base.BuildAfterDeserialization();
            DoBuild();
        }

        private new void DoBuild()
        {
            _guardValue = new HeliosValue(SourceInterface, BindingValue.Empty, SerializedDeviceName,
                SerializedFunctionName + " guard", "Current position of the guard for this switch.", "1 = Up, 2 = Down",
                BindingValueUnits.Numeric);
            _guardValue.Execute += new HeliosActionHandler(GuardValue_Execute);
            Actions.Add(_guardValue);
            Triggers.Add(_guardValue);
            Values.Add(_guardValue);

            _autoguardPositionAction = new HeliosAction(SourceInterface, SerializedDeviceName, SerializedFunctionName,
                "autoguard set", "Sets the position of this switch, and automatically switches the guard up if necessary.",
                ValueDescriptions, BindingValueUnits.Numeric);
            _autoguardPositionAction.Execute += new HeliosActionHandler(AutoguardPositionAction_Execute);
            Actions.Add(_autoguardPositionAction);
        }

        [JsonProperty("guardPosition")]
        public int GuardPosition
        {
            get => (int)_guardPosition;
            private set
            {
                if (value >= 1 && value <= 2 && value != (int)_guardPosition)
                {
                    switch (value)
                    {
                        case 1:
                            _guardPosition = GuardPositionValue.guardUp;
                            break;
                        case 2:
                            _guardPosition = GuardPositionValue.guardDown;
                            break;
                    }

                    _guardValue.SetValue(new BindingValue(_guardPosition.ToString()), SourceInterface.BypassTriggers);
                }
            }
        }

        void AutoguardPositionAction_Execute(object action, HeliosActionEventArgs e)
        {
            if (_guardPosition == GuardPositionValue.guardDown)
            {
                SourceInterface.BeginTriggerBypass(e.BypassCascadingTriggers);
                _guardPosition = GuardPositionValue.guardUp;
                SourceInterface.SendData(SerializedActions[_guardPosition.ToString()].CommandString);
                SourceInterface.EndTriggerBypass(e.BypassCascadingTriggers);
            }
        }

        protected void GuardValue_Execute(object action, HeliosActionEventArgs e)
        {
            SourceInterface.BeginTriggerBypass(e.BypassCascadingTriggers);
            GuardPosition = (int)e.Value.DoubleValue;
            SourceInterface.SendData(SerializedActions[_guardPosition.ToString()].CommandString);
            SourceInterface.EndTriggerBypass(e.BypassCascadingTriggers);
        }

        public override void ProcessNetworkData(string id, string value)
        {
            if (DataElements[1].ID.Equals(id))
            {
                GuardPositionValue newGuardPosition = _guardPosition;
                if (SerializedActions["guardUp"].ActionValue.Equals(value))
                {
                    newGuardPosition = GuardPositionValue.guardUp;
                }
                else if (SerializedActions["guardDown"].ActionValue.Equals(value))
                {
                    newGuardPosition = GuardPositionValue.guardDown;
                }
                _guardPosition = newGuardPosition;
            }
            else
            {
                base.ProcessNetworkData(id, value);
            }
        }

        protected override ExportDataElement[] DefaultDataElements =>
            new ExportDataElement[] { new DCSDataElement(_id, _format, _everyframe), new DCSDataElement(_guardArgId, _format, _everyframe)  };
    }
}
