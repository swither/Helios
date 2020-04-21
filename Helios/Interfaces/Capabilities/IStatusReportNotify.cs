using System.Collections.Generic;

namespace GadrocsWorkshop.Helios.Interfaces.Capabilities
{
    /// <summary>
    /// callback interface for changes that happen without a call to PerformReadyCheck
    /// </summary>
    public interface IStatusReportObserver
    {
        /// <summary>
        /// called when interface status changes
        /// </summary>
        /// <param name="statusName">the human-readable name the interface would like to use in status reporting</param>
        /// <param name="description">a longer, human-readable description of the interface</param>
        /// <param name="statusReport">the current status report</param> 
        void ReceiveStatusReport(string statusName, string description, IList<StatusReportItem> statusReport);
    }

    /// <summary>
    /// Objects that implement this interface may generate a list of user-readable
    /// issues without being called on IReadyCheck, i.e. due to UI interaction or 
    /// other events
    /// </summary>
    public interface IStatusReportNotify
    {
        /// <param name="observer"></param>
        // REVISIT: once on language 8.0, these can all be default implemented?
        void Subscribe(IStatusReportObserver observer);
        void Unsubscribe(IStatusReportObserver observer);

        /// <summary>
        /// alternative to InvalidateStatusReport: send this status report to all subscribers,
        /// asserting that it is fresh
        /// </summary>
        /// <param name="statusReport"></param>
        void PublishStatusReport(IList<StatusReportItem> statusReport);

        /// <summary>
        /// invalidate the status report, so that a fresh one will be generated
        /// and sent to all subscribers
        ///
        /// optionally, this functionality may be implemented asynchronously, so that the new status report
        /// is not sent immediately
        /// </summary>
        void InvalidateStatusReport();
    }
}
