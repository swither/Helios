using GadrocsWorkshop.Helios.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace GadrocsWorkshop.Helios.Patching.DCS
{
    public class ViewportSetupFileViewModel : HeliosViewModel<ViewportSetupFile>
    {
        public ViewportSetupFileViewModel(string profileName, ViewportSetupFile data) : base(data)
        {
            ProfileName = profileName;
        }

        /// <summary>
        /// the short name of the profile for which this monitor setup was generated
        /// </summary>
        public string ProfileName { get; }

        /// <summary>
        /// a suitable label for this item in the UI
        /// </summary>
        public string DisplayName => $"Helios Profile '{ProfileName}'";

        /// <summary>
        /// a label for the separate monitor setup file that will be used if this item is excluded
        /// WARNING: this value is escaped for use in a label
        /// </summary>
        public string MonitorSetupDisplayName => $"Monitor Setup 'H__{ProfileName}'";

        /// <summary>
        /// the current status of this monitor setup file
        /// </summary>
        public ViewportSetupFileStatus Status
        {
            get => (ViewportSetupFileStatus) GetValue(StatusProperty);
            set => SetValue(StatusProperty, value);
        }

        public static readonly DependencyProperty StatusProperty =
            DependencyProperty.Register("Status", typeof(ViewportSetupFileStatus), typeof(ViewportSetupFileViewModel),
                new PropertyMetadata(ViewportSetupFileStatus.Unknown));

        /// <summary>
        /// if the status is not OK, then this string has a short human-readable decsription of the problem suitable for display
        /// next to the profile name
        /// </summary>
        public string ProblemShortDescription
        {
            get => (string) GetValue(ProblemShortDescriptionProperty);
            set => SetValue(ProblemShortDescriptionProperty, value);
        }

        public static readonly DependencyProperty ProblemShortDescriptionProperty =
            DependencyProperty.Register("ProblemShortDescription", typeof(string), typeof(ViewportSetupFileViewModel),
                new PropertyMetadata(null));

        /// <summary>
        /// if the status is not OK, then this string has a human-readable narrative describing the problem
        /// </summary>
        public string ProblemNarrative
        {
            get => (string) GetValue(ProblemNarrativeProperty);
            set => SetValue(ProblemNarrativeProperty, value);
        }

        public static readonly DependencyProperty ProblemNarrativeProperty =
            DependencyProperty.Register("ProblemNarrative", typeof(string), typeof(ViewportSetupFileViewModel),
                new PropertyMetadata(null));


        /// <summary>
        /// true if this monitor setup file is associated with the profile currently open for editing
        /// </summary>
        public bool IsCurrentProfile
        {
            get => (bool) GetValue(IsCurrentProfileProperty);
            set => SetValue(IsCurrentProfileProperty, value);
        }

        public static readonly DependencyProperty IsCurrentProfileProperty =
            DependencyProperty.Register("IsCurrentProfile", typeof(bool), typeof(ViewportSetupFileViewModel),
                new PropertyMetadata(false));

        internal void SetData(ViewportSetupFile localViewports)
        {
            // copy the viewports data without replacing the Data object
            Data.Clear();
            Data.MonitorLayoutKey = localViewports.MonitorLayoutKey;
            IList<StatusReportItem> _ = Data.Merge(ProfileName, localViewports).ToList();
        }
    }
}