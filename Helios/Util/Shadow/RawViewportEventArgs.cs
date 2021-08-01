using System;

namespace GadrocsWorkshop.Helios.Util.Shadow
{
    public class RawViewportEventArgs : EventArgs
    {
        public HeliosVisual Raw { get; }

        public RawViewportEventArgs(HeliosVisual visual)
        {
            Raw = visual;
        }
    }
}