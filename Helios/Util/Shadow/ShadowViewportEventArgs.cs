using System;

namespace GadrocsWorkshop.Helios.Util.Shadow
{
    public class ShadowViewportEventArgs : EventArgs
    {
        public ShadowVisual Data { get; }

        public ShadowViewportEventArgs(ShadowVisual shadow)
        {
            Data = shadow;
        }
    }
}