using System.Windows.Input;

namespace GadrocsWorkshop.Helios.Windows
{
    /// <summary>
    /// commands that are used across plugins and therefore must be declared in the SDK
    /// </summary>
    public static class SharedCommands
    {
        public static RoutedUICommand ResetMonitors { get; } = new RoutedUICommand("Resets the monitors in this profile to those of the current system.", "ResetMonitors", typeof(SharedCommands));
    }
}
