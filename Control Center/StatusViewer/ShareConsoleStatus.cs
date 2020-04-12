using System.Collections.Generic;
using GadrocsWorkshop.Helios.Windows.ViewModel;

namespace GadrocsWorkshop.Helios.ControlCenter.StatusViewer
{
    /// <summary>
    /// specialized for use in XAML
    /// </summary>
    class ShareConsoleStatus : Share<ConsoleStatus>
    {
        public ShareConsoleStatus(IList<ConsoleStatus> report) : base(report)
        {
        }
    }
}
