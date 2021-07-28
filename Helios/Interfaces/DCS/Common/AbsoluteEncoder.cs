//  Copyright 2014 Craig Courtney
//  Copyright 2020 Ammo Goettsch
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
using System.Globalization;
using GadrocsWorkshop.Helios.UDPInterface;
using Newtonsoft.Json;

namespace GadrocsWorkshop.Helios.Interfaces.DCS.Common
{
    public class AbsoluteEncoder : DCSFunctionWithActions
    {
        private string _id;
        private string _format;

        private string _incrementData;
        private string _decrementData;

        [JsonProperty("argumentValue")]
        private double _argValue;

        [JsonProperty("argumentMin")]
        private double _argMin;

        [JsonProperty("argumentMax")]
        private double _argMax;

        [JsonProperty("loop")]
        private bool _loop;

        private HeliosValue _value;

        private HeliosAction _incrementAction;
        private HeliosAction _decrementAction;

        public AbsoluteEncoder(BaseUDPInterface sourceInterface, string deviceId, string buttonId, string button2Id, string argId, double argValue, double argMin, double argMax, string device, string name, bool loop, string exportFormat)
            : base(sourceInterface, device, name, "Current value of this encoder.")
        {
            _id = argId;
            _format = exportFormat;
            _loop = loop;
            _argValue = argValue;
            _argMin = argMin;
            _argMax = argMax;
            SerializedActions.Add("increment", new SerializedAction()
            {
                DeviceID = deviceId,
                ActionID = button2Id
            });
            SerializedActions.Add("decrement", new SerializedAction()
            {
                DeviceID = deviceId,
                ActionID = buttonId
            });
            DoBuild();
        }

        // deserialization constructor
        public AbsoluteEncoder(BaseUDPInterface sourceInterface, System.Runtime.Serialization.StreamingContext context)
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
            _incrementData = "C" + SerializedActions["increment"].DeviceID + "," + SerializedActions["increment"].ActionID +
                             ",";
            _decrementData = "C" + SerializedActions["decrement"].DeviceID + "," + SerializedActions["decrement"].ActionID +
                             ",";

            _value = new HeliosValue(SourceInterface, new BindingValue(0.0d), SerializedDeviceName, SerializedFunctionName,
                SerializedDescription, _argMin.ToString() + "-" + _argMax.ToString(), BindingValueUnits.Numeric);
            _value.Execute += new HeliosActionHandler(Value_Execute);
            Values.Add(_value);
            Triggers.Add(_value);

            _incrementAction = new HeliosAction(SourceInterface, SerializedDeviceName, SerializedFunctionName, "increment",
                "Increments this encoder value.");
            _incrementAction.Execute += new HeliosActionHandler(IncrementAction_Execute);
            Actions.Add(_incrementAction);

            _decrementAction = new HeliosAction(SourceInterface, SerializedDeviceName, SerializedFunctionName, "decrement",
                "Decrement this encoder value.");
            _decrementAction.Execute += new HeliosActionHandler(DecrementAction_Execute);
            Actions.Add(_decrementAction);
        }

        void DecrementAction_Execute(object action, HeliosActionEventArgs e)
        {
            double newValue = _value.Value.DoubleValue - _argValue;
            if (_loop)
            {
                while (newValue < _argMin)
                {
                    newValue = _argMax;
                }
            }
            else
            {
                newValue = Math.Max(_argMin, newValue);
            }
            _value.SetValue(new BindingValue(newValue), e.BypassCascadingTriggers);
            SourceInterface.SendData(_decrementData + _value.Value.DoubleValue.ToString(CultureInfo.InvariantCulture));
        }

        void IncrementAction_Execute(object action, HeliosActionEventArgs e)
        {
            double newValue = _value.Value.DoubleValue + _argValue;
            if (_loop)
            {
                while (newValue > _argMax)
                {
                    newValue = _argMin;
                }
            }
            else
            {
                newValue = Math.Min(_argMax, newValue);
            }
            _value.SetValue(new BindingValue(newValue), e.BypassCascadingTriggers);
            SourceInterface.SendData(_incrementData + _value.Value.DoubleValue.ToString(CultureInfo.InvariantCulture));
        }

        void Value_Execute(object action, HeliosActionEventArgs e)
        {
            _value.SetValue(e.Value, e.BypassCascadingTriggers);
            SourceInterface.SendData(_incrementData + _value.Value.DoubleValue.ToString(CultureInfo.InvariantCulture));
        }

        public override void ProcessNetworkData(string id, string value)
        {
            double newValue;
            if (double.TryParse(value, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out newValue))
            {
                if (newValue > _argMax) newValue = _argMax;
                if (newValue < _argMin) newValue = _argMin;
                _value.SetValue(new BindingValue(newValue), false);
            }          
        }

        protected override ExportDataElement[] DefaultDataElements => new ExportDataElement[] { new DCSDataElement(_id, _format) };

        public override void Reset()
        {
            _value.SetValue(BindingValue.Empty, true);
        }
    }
}
