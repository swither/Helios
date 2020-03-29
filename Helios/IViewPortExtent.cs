namespace GadrocsWorkshop.Helios
{
    public interface IViewportExtent
    {
        string ViewportName { get; }
        bool RequiresPatches { get; }
    }
}