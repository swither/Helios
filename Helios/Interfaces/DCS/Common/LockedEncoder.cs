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

using System.Globalization;
using GadrocsWorkshop.Helios.UDPInterface;

namespace GadrocsWorkshop.Helios.Interfaces.DCS.Common
{
    public class LockedEncoder : DCSFunctionWithActions
    {
        private HeliosAction _incrementAction;
        private HeliosAction _decrementAction;

        public LockedEncoder(BaseUDPInterface sourceInterface, string deviceId, string buttonId, string button2Id, string argId, double argValue, string device, string name)
            : base(sourceInterface, device, name, null)
        {
            SerializedActions.Add("increment", new SerializedAction()
            {
                DeviceID = deviceId,
                ActionID = buttonId,
                ActionValue = argValue.ToString(CultureInfo.InvariantCulture)
            });
            SerializedActions.Add("decrement", new SerializedAction()
            {
                DeviceID = deviceId,
                ActionID = button2Id,
                ActionValue = (-argValue).ToString(CultureInfo.InvariantCulture)
            }); 
            DoBuild();
        }

        // deserialization constructor
        public LockedEncoder(BaseUDPInterface sourceInterface, System.Runtime.Serialization.StreamingContext context)
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
            _incrementAction = new HeliosAction(SourceInterface, SerializedDeviceName, SerializedFunctionName, "increment",
                "Increments this setting.");
            _incrementAction.Execute += IncrementAction_Execute;
            Actions.Add(_incrementAction);

            _decrementAction = new HeliosAction(SourceInterface, SerializedDeviceName, SerializedFunctionName, "decrement",
                "Decrement this setting.");
            _decrementAction.Execute += DecrementAction_Execute;
            Actions.Add(_decrementAction);
        }

        void DecrementAction_Execute(object action, HeliosActionEventArgs e)
        {
            SourceInterface.SendData(SerializedActions["decrement"].CommandString);
        }

        void IncrementAction_Execute(object action, HeliosActionEventArgs e)
        {
            SourceInterface.SendData(SerializedActions["increment"].CommandString);
        }

        public override void ProcessNetworkData(string id, string value)
        {
            // No-Op
        }

        protected override ExportDataElement[] DefaultDataElements => new ExportDataElement[0];

        public override void Reset()
        {
            // No-Op
        }
    }
}
