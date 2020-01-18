using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GadrocsWorkshop.Helios
{
    /// <summary>
    /// Objects that implement this interface are able to generate a list of user-readable
    /// issues that may require review or actions by the user.
    /// </summary>
    public interface IReadyCheck
    {
        IEnumerable<StatusReportItem> PerformReadyCheck();
    }
}
