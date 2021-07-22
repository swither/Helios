using System.Xml.Serialization;

namespace GadrocsWorkshop.Helios.Interfaces.Falcon.Interfaces.RTT
{
    [XmlRoot("Local", Namespace = XML_NAMESPACE)]
    public class LocalOptions: NotificationObject
    {
        public const string XML_NAMESPACE = ConfigGenerator.XML_NAMESPACE + "/Local";

        /// <summary>
        /// backing field for property FramesPerSecond, contains
        /// RTT updates per second
        /// </summary>
        private int _framesPerSecond;

        /// <summary>
        /// RTT updates per second
        /// </summary>
        [XmlElement("FramesPerSecond")]
        public int FramesPerSecond
        {
            get => _framesPerSecond;
            set
            {
                if (_framesPerSecond == value) return;
                int oldValue = _framesPerSecond;
                _framesPerSecond = value;
                OnPropertyChanged(nameof(FramesPerSecond), oldValue, value, true);
            }
        }
    }
}
