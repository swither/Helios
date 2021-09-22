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

using System.Diagnostics;
using System.Runtime.Serialization;
using GadrocsWorkshop.Helios.UDPInterface;
using Newtonsoft.Json;

namespace GadrocsWorkshop.Helios.Interfaces.DCS.Common
{
    public class DualRocker : DCSFunctionWithButtons
    {
        private readonly string _id;
        private readonly string _format;

        private string _pushed1ActionData;
        private string _pushed2ActionData;
        private string _release1ActionData;
        private string _release2ActionData;
        private string _position1Value;
        private string _releaseValue;
        private string _position2Value;

        private HeliosTrigger _pushed1Trigger;
        private HeliosTrigger _pushed2Trigger;
        private HeliosTrigger _releasedTrigger;

        private HeliosValue _value;
        private HeliosValue _argValue;
        private string _argName;

        private bool _release2;

        [JsonProperty("vertical")]
        private bool _vertical;

        public DualRocker(BaseUDPInterface sourceInterface, string deviceId, string button1Id, string button2Id, string releaseButtonId, string releaseButton2Id, string argId, string device, string name, bool vertical)
            : this(sourceInterface, deviceId, button1Id, button2Id, releaseButtonId, releaseButton2Id, argId, device, name, vertical, "1", "-1", "0", "%1d")
        {
        }

        public DualRocker(BaseUDPInterface sourceInterface, string deviceId, string button1Id, string button2Id, string releaseButtonId, string releaseButton2Id, string argId, string device, string name, bool vertical, string push1Value, string push2Value, string releaseValue, string exportFormat)
            : base(sourceInterface, device, name, "Current position of this rocker.")
        {
            _id = argId;
            _format = exportFormat;
            _vertical = vertical;
            SerializedButtons.Add(new SerializedButton { DeviceID = deviceId, PushID = button1Id, PushValue = push1Value, ReleaseID = releaseButtonId, ReleaseValue = releaseValue });
            SerializedButtons.Add(new SerializedButton { DeviceID = deviceId, PushID = button2Id, PushValue = push2Value, ReleaseID = releaseButton2Id, ReleaseValue = releaseValue });
            DoBuild();
        }

        // deserialization constructor
        public DualRocker(BaseUDPInterface sourceInterface, StreamingContext context)
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
            string position1Name;
            string position2Name;
            _argName = "Argument Value of " + SerializedFunctionName;

            _position1Value = SerializedButtons[0].PushValue;
            _position2Value = SerializedButtons[1].PushValue;
            _releaseValue = SerializedButtons[0].ReleaseValue;

            // limitation of this implementation
            Debug.Assert(SerializedButtons[0].ReleaseValue == SerializedButtons[1].ReleaseValue);

            _pushed1ActionData = "C" + SerializedButtons[0].DeviceID + "," + SerializedButtons[0].PushID + "," +
                                 SerializedButtons[0].PushValue;
            _pushed2ActionData = "C" + SerializedButtons[1].DeviceID + "," + SerializedButtons[1].PushID + "," +
                                 SerializedButtons[1].PushValue;
            _release1ActionData = "C" + SerializedButtons[0].DeviceID + "," + SerializedButtons[0].ReleaseID + "," +
                                  SerializedButtons[0].ReleaseValue;
            _release2ActionData = "C" + SerializedButtons[1].DeviceID + "," + SerializedButtons[1].ReleaseID + "," +
                                  SerializedButtons[1].ReleaseValue;

            if (_vertical)
            {
                position1Name = "up";
                position2Name = "down";
            }
            else
            {
                position1Name = "left";
                position2Name = "right";
            }


            _value = new HeliosValue(SourceInterface, new BindingValue(false), SerializedDeviceName, SerializedFunctionName,
                SerializedDescription, "1=" + position1Name + ", 2=released" + ", 3=" + position2Name,
                BindingValueUnits.Numeric);
            Values.Add(_value);
            Triggers.Add(_value);

            _pushed1Trigger = new HeliosTrigger(SourceInterface, SerializedDeviceName, SerializedFunctionName,
                "pushed " + position1Name, "Fired when this rocker is pushed " + position1Name + " in the simulator.");
            Triggers.Add(_pushed1Trigger);
            _pushed2Trigger = new HeliosTrigger(SourceInterface, SerializedDeviceName, SerializedFunctionName,
                "pushed " + position2Name, "Fired when this rocker is pushed " + position2Name + " in the simulator.");
            Triggers.Add(_pushed2Trigger);

            _releasedTrigger = new HeliosTrigger(SourceInterface, SerializedDeviceName, SerializedFunctionName, "released",
                "Fired when this rocker is released in the simulator.");
            Triggers.Add(_releasedTrigger);

            HeliosAction push1Action = new HeliosAction(SourceInterface, SerializedDeviceName, SerializedFunctionName,
                "push " + position1Name, "Pushes this rocker " + position1Name + " in the simulator");
            push1Action.Execute += Push1Action_Execute;
            Actions.Add(push1Action);

            HeliosAction pushAction2 = new HeliosAction(SourceInterface, SerializedDeviceName, SerializedFunctionName,
                "push " + position2Name, "Pushes this rocker " + position2Name + " in the simulator");
            pushAction2.Execute += Push2Action_Execute;
            Actions.Add(pushAction2);

            HeliosAction releaseAction = new HeliosAction(SourceInterface, SerializedDeviceName, SerializedFunctionName,
                "release", "Releases the rocker in the simulator.");
            releaseAction.Execute += ReleaseAction_Execute;
            Actions.Add(releaseAction);

            _argValue = new HeliosValue(SourceInterface, BindingValue.Empty, SerializedDeviceName, _argName,
                "Argument value in DCS", "argument value", BindingValueUnits.Numeric);

            Values.Add(_argValue);
            Triggers.Add(_argValue);
        }

        void ReleaseAction_Execute(object action, HeliosActionEventArgs e)
        {
            if (_release2)
            {
                SourceInterface.SendData(_release2ActionData);
            }
            else
            {
                SourceInterface.SendData(_release1ActionData);
            }
        }

        void Push1Action_Execute(object action, HeliosActionEventArgs e)
        {
            _release2 = false;
            SourceInterface.SendData(_pushed1ActionData);
        }

        void Push2Action_Execute(object action, HeliosActionEventArgs e)
        {
            _release2 = true;
            SourceInterface.SendData(_pushed2ActionData);
        }

        public override void ProcessNetworkData(string id, string value)
        {
            if (value.Equals(_position1Value))
            {
                _value.SetValue(new BindingValue(1), false);
                _pushed1Trigger.FireTrigger(BindingValue.Empty);
            }
            else if (value.Equals(_position2Value))
            {
                _value.SetValue(new BindingValue(3), false);
                _pushed2Trigger.FireTrigger(BindingValue.Empty);
            }
            else if (value.Equals(_releaseValue))
            {
                _value.SetValue(new BindingValue(2), false);
                _releasedTrigger.FireTrigger(BindingValue.Empty);
            }
			_argValue.SetValue(new BindingValue(value), false);
		}

        protected override ExportDataElement[] DefaultDataElements => new ExportDataElement[] { new DCSDataElement(_id, _format) };
        
        public override void Reset()
        {
            _value.SetValue(BindingValue.Empty, true);
			_argValue.SetValue(BindingValue.Empty, true);
		}
    }
}
