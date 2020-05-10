using System.Collections.ObjectModel;

namespace GadrocsWorkshop.Helios.Patching.DCS
{
    public partial class CombinedMonitorSetupViewModel
    {
        public CombinedMonitorSetupViewModel()
            : base(new MonitorSetup())
        {
            Combined = new ObservableCollection<ViewportSetupFileViewModel>()
            {
                new ViewportSetupFileViewModel("F-A1_existing_profile", new ViewportSetupFile
                {
                })
                {
                    Status = ViewportSetupFileStatus.OK
                },
                new ViewportSetupFileViewModel("F-A2_conflicting_profile", new ViewportSetupFile
                {
                })
                {
                    Status = ViewportSetupFileStatus.Conflict,
                    ProblemShortDescription = "conflicts with F-A1_existing_profile",
                    ProblemNarrative = "the viewport LEFT_MFCD has a different screen position"
                },
                new ViewportSetupFileViewModel("F-A3_not_generated_profile", new ViewportSetupFile
                {
                })
                {
                Status = ViewportSetupFileStatus.NotGenerated,
                ProblemShortDescription = "has no current viewport data",
                ProblemNarrative =  $"DCS Monitor Setup has to be configured in profile 'F-A3_not_generated_profile' before it can be combined"

                }
            };
            Excluded = new ObservableCollection<ViewportSetupFileViewModel>()
            {
                new ViewportSetupFileViewModel("A-B1_separate_profile", new ViewportSetupFile
                {
                })
                {
                    Status = ViewportSetupFileStatus.OK
                }
            };

        }
    }
}