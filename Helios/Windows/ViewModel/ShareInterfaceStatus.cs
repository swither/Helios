using System.Collections.Generic;
using GadrocsWorkshop.Helios.Interfaces.Common;

namespace GadrocsWorkshop.Helios.Windows.ViewModel
{
    /// <summary>
    /// specialized for convenience in XAML only
    /// </summary>
    public class ShareInterfaceStatus : Share<InterfaceStatus>
    {
        public ShareInterfaceStatus(IList<InterfaceStatus> report) : base(report)
        {
        }
    }
}