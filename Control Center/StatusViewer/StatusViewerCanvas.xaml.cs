using System.Windows.Controls;

namespace GadrocsWorkshop.Helios.ControlCenter.StatusViewer
{
    /// <summary>
    /// A status viewer that shows StatusReportItems as they are received either from status
    /// check or from log messages
    /// </summary>
    public partial class StatusViewerCanvas : Canvas
    {
        public StatusViewerCanvas()
        {
            InitializeComponent();
        }
    }
}
