using System.Collections.Generic;

namespace GadrocsWorkshop.Helios
{
    /// <summary>
    /// callback interface for changes that happen without a call to PerformReadyCheck
    /// </summary>
    public interface IStatusReportObserver
    {
        void ReceiveStatusReport(IEnumerable<StatusReportItem> statusReport);
    }

    /// <summary>
    /// Objects that implement this interface may generate a list of user-readable
    /// issues without being called on IReadyCheck, i.e. due to UI interaction or 
    /// other events
    /// </summary>
    public interface IStatusReportNotify
    {
        void Subscribe(IStatusReportObserver observer);
        void Unsubscribe(IStatusReportObserver observer);
    }
}
