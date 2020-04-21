using System.Collections.Generic;

namespace GadrocsWorkshop.Helios.Interfaces.Capabilities
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
