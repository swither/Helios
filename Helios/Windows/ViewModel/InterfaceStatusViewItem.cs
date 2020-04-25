using System.Windows;

namespace GadrocsWorkshop.Helios.Windows.ViewModel
{
    /// <summary>
    /// A view model for StatusReportItem viewed in the Interface Status view
    ///
    /// Not implemented as HeliosViewModel because StatusReportItem (the model) is not a Helios NotificationObject
    /// </summary>
    public class InterfaceStatusViewItem : DependencyObject
    {
        private StatusReportItem item;

        public InterfaceStatusViewItem(StatusReportItem item)
        {
            this.item = item;
        }

        public bool HasRecommendation => item.Recommendation != null;
        public string Status => item.Severity.ToString();
        public string TextLine1 => item.Status;
        public string TextLine2 => item.Recommendation;
    }
}