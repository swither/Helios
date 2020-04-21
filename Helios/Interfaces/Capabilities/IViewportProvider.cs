namespace GadrocsWorkshop.Helios.Interfaces.Capabilities
{
    /// <summary>
    /// This interface is supported by HeliosInterface instances that provide 
    /// additional simulator viewports.  It is used to make sure that viewport
    /// extentss that require patches can be checked against the included patches.  
    /// </summary>
    public interface IViewportProvider
    {
        bool IsViewportAvailable(string viewportName);
    }
}