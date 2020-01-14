using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GadrocsWorkshop.Helios.UDPInterface
{
    // A network function that presents as an event that is fired when a message is received.  Any
    // attached value is only available in the event.
    public class NetworkTrigger : NetworkFunction
    {
        private string _id;
        private HeliosTrigger _receivedTrigger;

        // without a received value, this presents as an event without value
        public NetworkTrigger(BaseUDPInterface sourceInterface, string id, string name, string description)
            : base(sourceInterface)
        {
            _id = id;
            _receivedTrigger = new HeliosTrigger(sourceInterface, "", name, "", description);
            Triggers.Add(_receivedTrigger);
        }
        
        public override void ProcessNetworkData(string id, string value)
        {
            BindingValue bound = new BindingValue(value);
            _receivedTrigger.FireTrigger(bound);
        }

        public override ExportDataElement[] GetDataElements()
        {
            return new ExportDataElement[] { new ExportDataElement(_id) };
        }

        public override void Reset()
        {
        }
    }
}
