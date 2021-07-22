namespace GadrocsWorkshop.Helios.Controls.Capabilities
{
    /// <summary>
    /// controls that implement this interface have a name, which includes all Helios controls
    /// </summary>
    public interface INamedControl
    {
        string Name { get; }
    }
}
