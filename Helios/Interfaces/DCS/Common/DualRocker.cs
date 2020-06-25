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

namespace GadrocsWorkshop.Helios.Interfaces.DCS.Common
{
    using GadrocsWorkshop.Helios.UDPInterface;

    public class DualRocker : NetworkFunction
    {
        private readonly string _id;
        private readonly string _format;

        private readonly string _pushed1ActionData;
        private readonly string _pushed2ActionData;
        private readonly string _release1ActionData;
        private readonly string _release2ActionData;

        private readonly string _position1Value;
        private readonly string _releaseValue;
        private readonly string _position2Value;

        private readonly HeliosTrigger _pushed1Trigger;
        private readonly HeliosTrigger _pushed2Trigger;
        private readonly HeliosTrigger _releasedTrigger;

        private readonly HeliosValue _value;
        private readonly HeliosValue _argValue;

        private bool _release2 = false;

        public DualRocker(BaseUDPInterface sourceInterface, string deviceId, string button1Id, string button2Id, string releaseButtonId, string releaseButton2Id, string argId, string device, string name, bool vertical)
            : this(sourceInterface, deviceId, button1Id, button2Id, releaseButtonId, releaseButton2Id, argId, device, name, vertical, "1", "-1", "0", "%1d")
        {
        }

        public DualRocker(BaseUDPInterface sourceInterface, string deviceId, string button1Id, string button2Id, string releaseButtonId, string releaseButton2Id, string argId, string device, string name, bool vertical, string push1Value, string push2Value, string releaseValue, string exportFormat)
            : base(sourceInterface)
        {
            string position2Name;
            string position1Name;
            _id = argId;
			
			string argName = "Argument Value of " + name;
			_format = exportFormat;

            _position1Value = push1Value;
            _position2Value = push2Value;
            _releaseValue = releaseValue;

            _pushed1ActionData = "C" + deviceId + "," + button1Id + "," + push1Value;
            _pushed2ActionData = "C" + deviceId + "," + button2Id + "," + push2Value;
            _release1ActionData = "C" + deviceId + "," + releaseButtonId + "," + releaseValue;
            _release2ActionData = "C" + deviceId + "," + releaseButton2Id + "," + releaseValue;

            if (vertical)
            {
                position1Name = "up";
                position2Name = "down";
            }
            else
            {
                position1Name = "left";
                position2Name = "right";
            }


            _value = new HeliosValue(sourceInterface, new BindingValue(false), device, name, "Current position of this rocker.", "1=" + position1Name + ", 2=released" + ", 3=" + position2Name, BindingValueUnits.Numeric);
            Values.Add(_value);
            Triggers.Add(_value);

            _pushed1Trigger = new HeliosTrigger(sourceInterface, device, name, "pushed " + position1Name, "Fired when this rocker is pushed " + position1Name + " in the simulator.");
            Triggers.Add(_pushed1Trigger);
            _pushed2Trigger = new HeliosTrigger(sourceInterface, device, name, "pushed " + position2Name, "Fired when this rocker is pushed " + position2Name + " in the simulator.");
            Triggers.Add(_pushed2Trigger);

            _releasedTrigger = new HeliosTrigger(sourceInterface, device, name, "released", "Fired when this rocker is released in the simulator.");
            Triggers.Add(_releasedTrigger);

            HeliosAction push1Action = new HeliosAction(sourceInterface, device, name, "push " + position1Name, "Pushes this rocker " + position1Name + " in the simulator");
            push1Action.Execute += new HeliosActionHandler(Push1Action_Execute);
            Actions.Add(push1Action);

            HeliosAction push2Action = new HeliosAction(sourceInterface, device, name, "push " + position2Name, "Pushes this rocker " + position2Name + " in the simulator");
            push2Action.Execute += new HeliosActionHandler(Push2Action_Execute);
            Actions.Add(push2Action);

            HeliosAction releaseAction = new HeliosAction(sourceInterface, device, name, "release", "Releases the rocker in the simulator.");
            releaseAction.Execute += new HeliosActionHandler(ReleaseAction_Execute);
            Actions.Add(releaseAction);

			_argValue = new HeliosValue(sourceInterface, BindingValue.Empty, device, argName, "Argument value in DCS", "argument value", BindingValueUnits.Numeric);

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

        public override ExportDataElement[] GetDataElements()
        {
            return new ExportDataElement[] { new DCSDataElement(_id, _format) };
        }

        public override void Reset()
        {
            _value.SetValue(BindingValue.Empty, true);
			_argValue.SetValue(BindingValue.Empty, true);
		}

    }
}
