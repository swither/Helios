using System.ComponentModel;

namespace GadrocsWorkshop.Helios.ProfileEditor
{
    public enum ResetMonitorsScalingMode
    {
        [Description("No scaling")]
        None,
        [Description("Scale exactly to monitor size")]
        ScaleMonitor,
        [Description("Scale to fit content")]
        ScaleToFit,
        [Description("Scale to quarter size (demo)")]
        ScaleToTopRightQuarter
    }
}
