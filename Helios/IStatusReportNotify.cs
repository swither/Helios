using System.Collections.Generic;

namespace GadrocsWorkshop.Helios
{
    /// <summary>
    /// callback interface for changes that happen without a call to PerformReadyCheck
    /// </summary>
    public interface IStatusReportObserver
    {
        void ReceiveNameChange(string newStatusName);
        void ReceiveStatusReport(IEnumerable<StatusReportItem> statusReport);
    }

    /// <summary>
    /// Objects that implement this interface may generate a list of user-readable
    /// issues without being called on IReadyCheck, i.e. due to UI interaction or 
    /// other events
    /// </summary>
    public interface IStatusReportNotify
    {
        /// <summary>
        /// the name this object uses for status reporting
        ///
        /// when this name changes, all observers are notified with ReceiveNameChange
        /// </summary>
        string StatusName { get; }

        /// <param name="observer"></param>
        // REVISIT: once on language 8.0, these can all be default implemented?
        void Subscribe(IStatusReportObserver observer);
        void Unsubscribe(IStatusReportObserver observer);

        /// <summary>
        /// alternative to InvalidateStatusReport: send this status report to all subscribers,
        /// asserting that it is fresh
        /// </summary>
        /// <param name="statusReport"></param>
        void PublishStatusReport(IEnumerable<StatusReportItem> statusReport);

        /// <summary>
        /// asynchronously invalidate the status report, so that a fresh one will be generated
        /// and sent to all subscribers
        /// </summary>
        void InvalidateStatusReport();
    }
}
