using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace GadrocsWorkshop.Helios.Interfaces.Common
{
    public class InterfaceStatus : NotificationObject, IStatusReportObserver
    {
        private InterfaceStatus(HeliosInterface heliosInterface)
        {
            Interface = heliosInterface;
            ReadyCheck = (IReadyCheck) heliosInterface;
            Name = $"{heliosInterface.Name}";
            HeliosInterfaceDescriptor descriptor =
                ConfigManager.ModuleManager.InterfaceDescriptors[heliosInterface.TypeIdentifier];
            HasEditor = descriptor.InterfaceEditorType != null;
            Subscription = heliosInterface as IStatusReportNotify;
            Subscription?.Subscribe(this);
        }

        public void Dispose()
        {
            Subscription?.Unsubscribe(this);
        }

        public static bool TryManage(HeliosInterface heliosInterface, out InterfaceStatus managed)
        {
            if (!(heliosInterface is IReadyCheck))
            {
                managed = null;
                return false;
            }

            managed = new InterfaceStatus(heliosInterface);
            return true;
        }

        public void ReceiveStatusReport(IEnumerable<StatusReportItem> statusReport)
        {
            Report = statusReport.ToList();
        }

        /// <summary>
        /// the ready check interface of Interface
        /// </summary>
        [JsonIgnore]
        public IReadyCheck ReadyCheck { get; }

        /// <summary>
        /// the Helios interface object being observed and queried for status
        /// </summary>
        [JsonIgnore]
        public HeliosInterface Interface { get; }

        [JsonProperty("Name")]
        public string Name { get; }

        [JsonIgnore]
        public bool HasEditor { get; }

        /// <summary>
        /// backing field for property Report, contains
        /// the most recently received status report from the managed interface
        /// </summary>
        private IList<StatusReportItem> _report = new List<StatusReportItem>();

        /// <summary>
        /// the most recently received status report from the managed interface
        /// </summary>
        [JsonProperty("Report")]
        public IList<StatusReportItem> Report
        {
            get => _report;
            set
            {
                if (_report != null && _report == value) return;
                IList<StatusReportItem> oldValue = _report;
                _report = value;
                OnPropertyChanged("Report", oldValue, value, true);
            }
        }

        /// <summary>
        /// the managed interface's status report subscription interface or null
        /// </summary>
        [JsonIgnore]
        public IStatusReportNotify Subscription { get; }

        internal void PerformCheck()
        {
            // prefer the status report notification
            if (Subscription != null)
            {
                Subscription.InvalidateStatusReport();
                return;
            }

            // as a last resort, get a static report from ready check
            if (ReadyCheck != null)
            {
                Report = ReadyCheck.PerformReadyCheck().ToList();
            }
        }
    }
}