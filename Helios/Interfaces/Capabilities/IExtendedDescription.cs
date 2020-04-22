namespace GadrocsWorkshop.Helios.Interfaces.Capabilities
{
    /// <summary>
    /// Helios interfaces that implement this interface can provide extended display information for
    /// use in the user interface.
    /// </summary>
    public interface IExtendedDescription
    {
        /// <summary>
        /// a long, human-readable description
        /// </summary>
        string Description { get; }

        /// <summary>
        /// a human-readable message explaining what happens if this item is removed
        /// </summary>
        string RemovalNarrative { get; }
    }
}
