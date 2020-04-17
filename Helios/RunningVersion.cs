using System.Reflection;

namespace GadrocsWorkshop.Helios
{
    /// <summary>
    /// This utility exists because VersionChecker can only be static initalized after Helios is started.
    /// REVISIT: After we fix VersionChecker, this class can be integrated into VersionChecker
    /// </summary>
    public class RunningVersion
    {
        public static bool IsDevelopmentPrototype
        {
            get
            {
                System.Version running = Assembly.GetEntryAssembly().GetName().Version;
                return running.Build >= 1000 && running.Build < 2000;
            }
        }
    }
}
