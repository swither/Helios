namespace GadrocsWorkshop.Helios.Interfaces.Capabilities
{
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
