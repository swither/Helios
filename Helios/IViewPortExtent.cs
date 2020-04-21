namespace GadrocsWorkshop.Helios
{
    /// <summary>
    /// Visuals that implement this interface display a Simulator Viewport at their location (Top, Left, Width, Height)
    /// </summary>
    public interface IViewportExtent
    {
        /// <summary>
        /// a string that must match the name of the viewport in the simulator
        /// </summary>
        string ViewportName { get; }

        /// <summary>
        /// true if the viewport is marked as requiring patches rather than being supported by unmodified simulator installation
        /// </summary>
        bool RequiresPatches { get; }
    }
}