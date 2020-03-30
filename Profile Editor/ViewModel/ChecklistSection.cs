using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace GadrocsWorkshop.Helios.ProfileEditor.ViewModel
{
    public class ChecklistSection : DependencyObject, IStatusReportObserver
    {
        private List<StatusReportItem> _report = new List<StatusReportItem>();
        private StatusReportItem.SeverityCode _displayThreshold;

        private ChecklistSection()
        {
            Items = new ObservableCollection<ChecklistItem>();
            Recommendations = new ObservableCollection<string>();
        }

        public static bool TryManage(HeliosInterface heliosInterface, out ChecklistSection managed)
        {
            if (!(heliosInterface is IReadyCheck readyCheck))
            {
                managed = null;
                return false;
            }
            ChecklistSection section = new ChecklistSection();
            section.Interface = heliosInterface;
            section.ReadyCheck = readyCheck;
            section.Name = $"{heliosInterface.Name}";
            HeliosInterfaceDescriptor descriptor = ConfigManager.ModuleManager.InterfaceDescriptors[heliosInterface.TypeIdentifier];
            section.HasEditor = descriptor.InterfaceEditorType != null;

            // sign up for status changes
            if (heliosInterface is IStatusReportNotify subscription)
            {
                section.Subscription = subscription;
                section.Subscription.Subscribe(section);
            }

            // allow this object to be used in UI
            managed = section;
            return true;
        }

        public void Dispose()
        {
            Subscription?.Unsubscribe(this);
        }

        public void PerformCheck(StatusReportItem.SeverityCode displayThreshold)
        {
            DisplayThreshold = displayThreshold;
            IEnumerable<StatusReportItem> collection = ReadyCheck.PerformReadyCheck();
            Update(collection);
        }

        private void Update(IEnumerable<StatusReportItem> freshReport)
        {
            _report.Clear();
            _report.AddRange(freshReport);
            Items.Clear();
            Recommendations.Clear();
            RecommendationsVisibility = Visibility.Collapsed;

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
                System.StringComparer.OrdinalIgnoreCase);
            foreach (string recommendation in uniqueRecommendations)
            {
                Recommendations.Add(recommendation);
                RecommendationsVisibility = Visibility.Visible;
            }

            // create full resolution status items
            StatusReportItem.SeverityCode newStatus = _report.Select(item => item.Severity).Max();
            IEnumerable<ChecklistItem> checklistItems = _report.Where(item => item.Severity >= DisplayThreshold).Select(item => new ChecklistItem(item));
            foreach (ChecklistItem checklistItem in checklistItems)
            {
                Items.Add(checklistItem);
            }
            setStatus(newStatus);
            UpdateDetailsVisibility();
        }

        public void ReceiveStatusReport(IEnumerable<StatusReportItem> statusReport)
        {
            Update(statusReport);
        }

        private void UpdateDetailsVisibility()
        {
            DetailsVisibility = ((Items.Count > 0) && (_displayThreshold < StatusReportItem.SeverityCode.None)) ? Visibility.Visible : Visibility.Collapsed;
        }

        #region Properties
        public IReadyCheck ReadyCheck { get; private set; }
        public IStatusReportNotify Subscription { get; private set; }
        public String Name { get; private set; }
        public bool HasEditor { get; private set; }
        public HeliosInterface Interface { get; private set; }
        public StatusReportItem.SeverityCode DisplayThreshold
        {
            get => _displayThreshold;
            set
            {
                _displayThreshold = value;
                UpdateDetailsVisibility();
            }
        }
        public Visibility GoThereVisibility => HasEditor ? Visibility.Visible : Visibility.Hidden;

        public Visibility DetailsVisibility
        {
            get { return (Visibility)GetValue(DetailsVisibilityProperty); }
            set { SetValue(DetailsVisibilityProperty, value); }
        }
        public static readonly DependencyProperty DetailsVisibilityProperty =
            DependencyProperty.Register("DetailsVisibility", typeof(Visibility), typeof(ChecklistSection), new PropertyMetadata(Visibility.Visible));

        public Visibility RecommendationsVisibility
        {
            get { return (Visibility)GetValue(RecommendationsVisibilityProperty); }
            set { SetValue(RecommendationsVisibilityProperty, value); }
        }
        public static readonly DependencyProperty RecommendationsVisibilityProperty =
            DependencyProperty.Register("RecommendationsVisibility", typeof(Visibility), typeof(ChecklistSection), new PropertyMetadata(Visibility.Visible));

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
        public string Status
        {
            get { return (string)GetValue(StatusProperty); }
        }
        public static readonly DependencyProperty StatusProperty =
            DependencyProperty.Register("Status", typeof(string), typeof(ChecklistSection), new PropertyMetadata("None"));

        public string StatusNarrative
        {
            get { return (string)GetValue(StatusNarrativeProperty); }
            set { SetValue(StatusNarrativeProperty, value); }
        }
        public static readonly DependencyProperty StatusNarrativeProperty =
            DependencyProperty.Register("StatusNarrative", typeof(string), typeof(ChecklistSection), new PropertyMetadata(""));

        public ObservableCollection<ChecklistItem> Items
        {
            get { return (ObservableCollection<ChecklistItem>)GetValue(itemsProperty); }
            set { SetValue(itemsProperty, value); }
        }
        public static readonly DependencyProperty itemsProperty =
            DependencyProperty.Register("Items", typeof(ObservableCollection<ChecklistItem>), typeof(ChecklistSection), new PropertyMetadata(null));

        public ObservableCollection<string> Recommendations
        {
            get { return (ObservableCollection<string>)GetValue(RecommendationsProperty); }
            set { SetValue(RecommendationsProperty, value); }
        }
        public static readonly DependencyProperty RecommendationsProperty =
            DependencyProperty.Register("Recommendations", typeof(ObservableCollection<string>), typeof(ChecklistSection), new PropertyMetadata(null));

        #endregion
    }
}
