using System.Windows;

namespace GadrocsWorkshop.Helios.Controls.RendererCapabilities
{
    public interface IWindowsControl
    {
        /// <summary>
        /// create a FrameWorkElement for display as the first child of the view calling the
        /// renderer implementing this interface
        ///
        /// usually this requires making a new control, because we don't have a mechanism to
        /// remove from the previous tree
        /// </summary>
        /// <returns>a new control that can be inserted in the visual tree</returns>
        FrameworkElement CreateNewFrameworkElement();
    }
}