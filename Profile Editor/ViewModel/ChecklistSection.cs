using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using GadrocsWorkshop.Helios.Windows;

namespace GadrocsWorkshop.Helios.ProfileEditor.ViewModel
{
    /// <summary>
    /// a view model for the status of an interface, as viewed in the configuration check (Checklist) view
    /// </summary>
    public class ChecklistSection : HeliosViewModel<InterfaceStatus>
    {
        #region Private

        private readonly List<StatusReportItem> _report = new List<StatusReportItem>();

        private StatusReportItem.SeverityCode _displayThreshold;

        #endregion

        public ChecklistSection(InterfaceStatus data)
            : base(data)
        {
            Items = new ObservableCollection<ChecklistItem>();
            Recommendations = new ObservableCollection<string>();
            Update(data.Report);
            data.PropertyChanged += Data_PropertyChanged;
        }

        private void Data_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Report":
                    Update(((InterfaceStatus) sender).Report);
                    break;
            }
        }

        private void Update(IEnumerable<StatusReportItem> freshReport)
        {
            _report.Clear();
            Items.Clear();
            Recommendations.Clear();
            RecommendationsVisibility = Visibility.Collapsed;

            _report.AddRange(freshReport.Where(i => i.Severity >= DisplayThreshold));
            if (_report.Count < 1)
            {
                // done
                setStatus(StatusReportItem.SeverityCode.None);
                UpdateDetailsVisibility();
                return;
            }

            // create a unique set of recommendations
            HashSet<string> uniqueRecommendations = new HashSet<string>(
                _report.Select(item => item.Recommendation).Where(text => !string.IsNullOrEmpty(text)),
                StringComparer.OrdinalIgnoreCase);
            foreach (string recommendation in uniqueRecommendations)
            {
                Recommendations.Add(recommendation);
                RecommendationsVisibility = Visibility.Visible;
            }

            // create full resolution status items
            StatusReportItem.SeverityCode newStatus = _report.Select(item => item.Severity).Max();
            IEnumerable<ChecklistItem> checklistItems = _report.Where(item => item.Severity >= DisplayThreshold)
                .Select(item => new ChecklistItem(item));
            foreach (ChecklistItem checklistItem in checklistItems)
            {
                Items.Add(checklistItem);
            }

            setStatus(newStatus);
            UpdateDetailsVisibility();
        }

        private void UpdateDetailsVisibility()
        {
            DetailsVisibility = Items.Count > 0 && _displayThreshold < StatusReportItem.SeverityCode.None
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        /// <summary>
        /// Instead of a setter for the corresponding string, we use this method to
        /// ensure the underlying enum can be renamed and otherwise changed safely.
        /// </summary>
        /// <param name="code"></param>
        public void setStatus(StatusReportItem.SeverityCode code)
        {
            SetValue(StatusProperty, code.ToString());
            switch (code)
            {
                case StatusReportItem.SeverityCode.None:
                    StatusNarrative = "This component is not reporting any status.";
                    break;
                case StatusReportItem.SeverityCode.Info:
                    StatusNarrative = "This component is configured correctly.";
                    break;
                case StatusReportItem.SeverityCode.Warning:
                    StatusNarrative = "This component is reporting warnings that should be corrected.";
                    break;
                case StatusReportItem.SeverityCode.Error:
                    StatusNarrative = "This component is reporting errors and requires attention.";
                    break;
            }
        }

        #region Properties

        public StatusReportItem.SeverityCode DisplayThreshold
        {
            get => _displayThreshold;
            set
            {
                _displayThreshold = value;
                UpdateDetailsVisibility();
            }
        }

        public Visibility GoThereVisibility => Data.HasEditor ? Visibility.Visible : Visibility.Hidden;

        public Visibility DetailsVisibility
        {
            get => (Visibility) GetValue(DetailsVisibilityProperty);
            set => SetValue(DetailsVisibilityProperty, value);
        }

        public ObservableCollection<ChecklistItem> Items
        {
            get => (ObservableCollection<ChecklistItem>) GetValue(ItemsProperty);
            set => SetValue(ItemsProperty, value);
        }

        public ObservableCollection<string> Recommendations
        {
            get => (ObservableCollection<string>) GetValue(RecommendationsProperty);
            set => SetValue(RecommendationsProperty, value);
        }

        public Visibility RecommendationsVisibility
        {
            get => (Visibility) GetValue(RecommendationsVisibilityProperty);
            set => SetValue(RecommendationsVisibilityProperty, value);
        }

        public string Status => (string) GetValue(StatusProperty);

        public string StatusNarrative
        {
            get => (string) GetValue(StatusNarrativeProperty);
            set => SetValue(StatusNarrativeProperty, value);
        }

        #endregion

        #region DependencyProperties

        public static readonly DependencyProperty DetailsVisibilityProperty =
            DependencyProperty.Register("DetailsVisibility", typeof(Visibility), typeof(ChecklistSection),
                new PropertyMetadata(Visibility.Visible));

        public static readonly DependencyProperty ItemsProperty =
            DependencyProperty.Register("Items", typeof(ObservableCollection<ChecklistItem>), typeof(ChecklistSection),
                new PropertyMetadata(null));

        public static readonly DependencyProperty RecommendationsProperty =
            DependencyProperty.Register("Recommendations", typeof(ObservableCollection<string>),
                typeof(ChecklistSection), new PropertyMetadata(null));

        public static readonly DependencyProperty RecommendationsVisibilityProperty =
            DependencyProperty.Register("RecommendationsVisibility", typeof(Visibility), typeof(ChecklistSection),
                new PropertyMetadata(Visibility.Visible));

        public static readonly DependencyProperty StatusProperty =
            DependencyProperty.Register("Status", typeof(string), typeof(ChecklistSection),
                new PropertyMetadata("None"));

        public static readonly DependencyProperty StatusNarrativeProperty =
            DependencyProperty.Register("StatusNarrative", typeof(string), typeof(ChecklistSection),
                new PropertyMetadata(""));

        #endregion
    }
}