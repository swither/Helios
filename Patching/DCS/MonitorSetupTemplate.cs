namespace GadrocsWorkshop.Helios.Patching.DCS
{
    /// <summary>
    /// specific naming and handling that differs between a shared monitor setup file and a separate monitor setup file
    /// </summary>
    internal class MonitorSetupTemplate
    {
        public MonitorSetupTemplate(string combinedMonitorSetupName, string profileName, bool combined)
        {
            CombinedMonitorSetupName = combinedMonitorSetupName;
            ProfileName = profileName;
            Combined = combined;
            BaseName = ProfileName?.Replace(" ", "") ?? "";
        }

        public string CombinedMonitorSetupName { get; }
        public string ProfileName { get; }
        public string BaseName { get; }
        public bool Combined { get; }

        public string MonitorSetupFileBaseName => Combined ? "Helios" : $"H_{BaseName}";
        public string MonitorSetupName => Combined ? CombinedMonitorSetupName : $"H_{BaseName}";
        public string SourcesList => Combined ? "compatible Helios Profiles" : $"Helios Profile {ProfileName ?? "<unsaved>"}";
        public string FileName => $"{MonitorSetupFileBaseName}.lua";
    }
}