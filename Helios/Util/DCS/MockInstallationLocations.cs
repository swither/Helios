
using System.Collections.Generic;

namespace GadrocsWorkshop.Helios.Util.DCS
{
    class MockInstallationLocation
    {
        public string SavedGamesName { get; set; }
        public string Path { get; set; }
        public string Version { get; set; }
        public bool IsEnabled { get; set; }
    }

    class MockInstallationLocations
    {
        public IEnumerable<MockInstallationLocation> Items { get; } = new List<MockInstallationLocation>
        {
            new MockInstallationLocation
            {
                SavedGamesName = "FCS.OpenBeta",
                Path = "C:\\Program Files\\Igloo Divonics\\FCS Wuerg\\",
                Version = "3.4.5.23423",
                IsEnabled = true
            }
        };
    }
}
