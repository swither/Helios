using System.Collections.Generic;

namespace GadrocsWorkshop.Helios.Interfaces.Capabilities
{
    /// <summary>
    /// Objects that implement this interface are able to generate a list of user-readable
    /// issues that may require review or actions by the user.
    ///
    /// Helios interfaces that implement this interface will be called during "Preflight Check" when
    /// their profile is started, so that they can check their configuration.  Returning any error items
    /// during this check will block the profile from starting unless the user overrides this check.
    /// </summary>
    public interface IReadyCheck
    {
        IEnumerable<StatusReportItem> PerformReadyCheck();
    }
}
