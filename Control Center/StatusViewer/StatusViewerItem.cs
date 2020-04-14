using GadrocsWorkshop.Helios.Windows;

namespace GadrocsWorkshop.Helios.ControlCenter.StatusViewer
{
    public class StatusViewerItem : HeliosStaticViewModel<StatusReportItem>
    {
        public StatusViewerItem(StatusReportItem data) : base(data)
        {
        }

        public string Recommendation
        {
            get
            {
                if (Data.Link == null || Data.Link.Scheme != StatusReportItem.HELIOS_SCHEME)
                {
                    // no link, we have nothing to edit
                    return Data.Recommendation;
                }

                if (string.IsNullOrEmpty(Data.Recommendation))
                {
                    // empty recommendation
                    return Data.Recommendation;
                }

                if (Data.Recommendation.Length < 2)
                {
                    // recommendation is too short to be a sentence, so avoid boundary case and do nothing
                    return Data.Recommendation;
                }

                string lowerCase;
                
                if (!char.IsUpper(Data.Recommendation[0]))
                {
                    // already starts wither lower case
                    lowerCase = Data.Recommendation;
                }
                else
                {
                    lowerCase = $"{char.ToLower(Data.Recommendation[0])}{Data.Recommendation.Substring(1)}";
                }

                switch (Data.Link.Host)
                {
                    case StatusReportItem.CONTROL_CENTER_HOST:
                        return $"Using Control Center, {lowerCase}";
                    case StatusReportItem.PROFILE_EDITOR_HOST:
                        return $"Using Profile Editor, {lowerCase}";
                    default:
                        return Data.Recommendation;
                }
            }
        }
    }
}