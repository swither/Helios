using System;

namespace GadrocsWorkshop.Helios.Util.Shadow
{
    public class RawMonitorEventArgs : EventArgs
    {
        #region Private

        public Monitor Raw { get; }

        #endregion

        public RawMonitorEventArgs(Monitor monitor)
        {
            Raw = monitor;
        }
    }
}