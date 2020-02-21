using System.Windows.Media;

namespace GadrocsWorkshop.Helios.Controls
{
    internal class ViewportExtentRenderer : TextDecorationRenderer
    {
        protected override void OnRender(DrawingContext drawingContext)
        {
            // only in design mode
            if (ConfigManager.Application.ShowDesignTimeControls)
            {
                base.OnRender(drawingContext);
            }
        }
    }
}