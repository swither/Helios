using GadrocsWorkshop.Helios;

namespace net.derammo.Helios.SampleProgram.Sample
{
    /// <summary>
    /// Helios notification object supports IPropertyNotification and automatically creates undo
    /// items for any property changes
    /// </summary>
    public class SampleModel : NotificationObject
    {
        #region Private

        /// <summary>
        /// backing field for property SomeData, contains
        /// some sort of string thingy that gets undo
        /// </summary>
        private string _someData = "some text";

        /// <summary>
        /// backing field for property SomeInteger, contains
        /// an example integer value with undo support
        /// </summary>
        private int _someInteger;

        /// <summary>
        /// backing field for property SomeOtherThing, contains
        /// yet another thing
        /// </summary>
        private object _someOtherThing = new object();

        #endregion

        #region Properties

        /// <summary>
        /// some sort of string thingy that gets undo
        /// </summary>
        public string SomeData
        {
            get => _someData;
            set
            {
                if (_someData != null && _someData == value)
                {
                    return;
                }

                string oldValue = _someData;
                _someData = value;
                OnPropertyChanged("SomeData", oldValue, value, true);
            }
        }

        /// <summary>
        /// an example integer value with undo support
        /// </summary>
        public int SomeInteger
        {
            get => _someInteger;
            set
            {
                if (_someInteger == value)
                {
                    return;
                }

                int oldValue = _someInteger;
                _someInteger = value;
                OnPropertyChanged("SomeInteger", oldValue, value, true);
            }
        }

        /// <summary>
        /// yet another thing
        /// </summary>
        public object SomeOtherThing
        {
            get => _someOtherThing;
            set
            {
                if (_someOtherThing != null && _someOtherThing == value)
                {
                    return;
                }

                object oldValue = _someOtherThing;
                _someOtherThing = value;
                OnPropertyChanged("SomeOtherThing", oldValue, value, true);
            }
        }

        #endregion
    }
}