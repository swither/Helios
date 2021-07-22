using System.Windows;

namespace GadrocsWorkshop.Helios.Util.Shadow
{
    /// <summary>
    /// callbacks from objects shadowing visuals (viewports, monitors) in the Helios Profile
    /// to implement our model
    /// </summary>
    public interface IShadowVisualParent
    {
        Vector GlobalOffset { get; }
        double Scale { get; }
        void AddViewport(ShadowVisual viewport);
        void RemoveViewport(ShadowVisual viewport);
    }
}