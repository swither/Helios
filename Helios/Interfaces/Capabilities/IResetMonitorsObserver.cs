namespace GadrocsWorkshop.Helios.Interfaces.Capabilities
{
    /// <summary>
    /// Helios interfaces that implement this interface are notified when a "Reset Monitors" operation completes
    /// </summary>
    public interface IResetMonitorsObserver
    {
        /// <summary>
        /// indicates a reset of profile monitors has occurred, without indicating whether it was successful
        /// 
        /// called on main thread after reset monitors thread completes and all change events on Profile.Monitors collection
        /// and properties on Monitor objects have been delivered
        /// </summary>
        void NotifyResetMonitorsComplete();
    }
}
