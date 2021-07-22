using System.Xml.Serialization;

namespace GadrocsWorkshop.Helios.Interfaces.Falcon.Interfaces.RTT
{
    [XmlRoot("Network", Namespace = XML_NAMESPACE)]
    public class NetworkOptions: NotificationObject
    {
        public const string XML_NAMESPACE = ConfigGenerator.XML_NAMESPACE + "/Network";
    }
}
