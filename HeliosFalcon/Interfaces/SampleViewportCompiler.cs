using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using GadrocsWorkshop.Helios.ComponentModel;
using GadrocsWorkshop.Helios.Interfaces.Common;
using GadrocsWorkshop.Helios.Interfaces.Falcon.Interfaces.RTT;
using GadrocsWorkshop.Helios.Util.Shadow;

namespace GadrocsWorkshop.Helios.Interfaces.Falcon
{
    [HeliosInterface("Helios.Tools.SampleProgram.SampleViewportCompiler", "Sample Viewport Compiler", null,
        Factory = typeof(UniqueHeliosInterfaceFactory))]
    public class SampleViewportCompiler : ViewportCompilerInterface<SampleMonitor, SampleMonitorEventArgs>
    {
        public SampleViewportCompiler() : base("Sample Viewport Compiler")
        {
            // don't do any work here; we may not be getting initialized
        }

        public override string Description => "Sample implementation of a viewport compiler that should not be used in production.";

        public override string RemovalNarrative => "This is only a sample and should not be used in production, so removing it is expected.";

        public override InstallationResult Install(IInstallationCallbacks callbacks)
        {
            // TODO: perform compilation and write files
            return InstallationResult.Canceled;
        }

        public override IEnumerable<StatusReportItem> PerformReadyCheck()
        {
            return CreateStatusReport();
        }

        public override void ReadXml(XmlReader reader)
        {
            if (reader.Name == "RTT")
            {
                Rtt = HeliosXmlModel.ReadXml<ConfigGenerator>(reader);
            }
        }

        private void Rtt_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // bubble up any undoable events without invalidating Rtt, to generate an Undo record that will set our child's property back if called
            OnPropertyChanged(new PropertyNotificationEventArgs(this, "ChildProperty", e as PropertyNotificationEventArgs));
        }

        public override void WriteXml(XmlWriter writer)
        {
            if (null == Rtt)
            {
                return;
            }
            HeliosXmlModel.WriteXml(writer, Rtt);
        }

        protected override SampleMonitor CreateShadowMonitor(Monitor monitor) => new SampleMonitor(this, monitor, monitor, false);

        protected override List<StatusReportItem> CreateStatusReport()
        {
            // TODO: write a real status report
            return Viewports
                .Select(v => new StatusReportItem
                {
                    // XXX remove this debug stuff
                    Severity = StatusReportItem.SeverityCode.Warning,
                    Status = $"({v.Visual.Left + v.Monitor.Left}, {v.Visual.Top + v.Monitor.Top}) {v.Visual.Width}x{v.Visual.Height} {v.Viewport.ViewportName}",
                    Recommendation = "Sample output to show this is up to date"
                })
                .Concat(Rtt?.CreateStatusReport(Viewports) ?? new [] { new StatusReportItem
                {
                    Severity = StatusReportItem.SeverityCode.Error,
                    Status = "RTT configuration generator is not initialized"
                }})
                .ToList();
        }

        protected override void UpdateAllGeometry()
        {
            Rtt?.Update(Viewports);
        }

        #region Overrides of ViewportCompilerInterface<SampleMonitor,SampleMonitorEventArgs>

        protected override void AttachToProfileOnMainThread()
        {
            base.AttachToProfileOnMainThread();
            Profile.ProfileStarted += Profile_ProfileStarted;
            Profile.ProfileStopped += Profile_ProfileStopped;
            if (null == Rtt)
            {
                Rtt = new ConfigGenerator();
            }
            Rtt.PropertyChanged += Rtt_PropertyChanged;
        }

        public ConfigGenerator Rtt { get; private set; }

        private void Profile_ProfileStarted(object sender, EventArgs e)
        {
            // TODO: run the thing
        }

        private void Profile_ProfileStopped(object sender, EventArgs e)
        {
            // TODO: stop the thing if running
        }

        protected override void DetachFromProfileOnMainThread(HeliosProfile oldProfile)
        {
            oldProfile.ProfileStarted -= Profile_ProfileStarted;
            oldProfile.ProfileStopped -= Profile_ProfileStopped;
            base.DetachFromProfileOnMainThread(oldProfile);

            if (Rtt != null)
            {
                Rtt.PropertyChanged -= Rtt_PropertyChanged;
                Rtt.Dispose();
                Rtt = null;
            }
        }

        #endregion
    }

    /// <summary>
    /// specific monitor class used in this implementation; add any additional information that is required on a per-monitor basis, if any
    /// </summary>
    public class SampleMonitor: ShadowMonitorBase<SampleMonitorEventArgs>
    {
        public SampleMonitor(IShadowVisualParent parent, Monitor monitor, HeliosVisual visual, bool recurse = true) : base(parent, monitor, visual, recurse)
        {
            // TODO: load any saved per-monitor configuration
        }

        public override bool AddViewport()
        {
            return true;
        }

        public override SampleMonitorEventArgs CreateEvent() => new SampleMonitorEventArgs(this);

        public override void Instrument()
        {
            // build the tree from this monitor
            Instrument(Monitor, Monitor);
        }

        public override bool RemoveViewport()
        {
            return true;
        }
    }

    /// <summary>
    /// don't modify this class; it is just a fully typed event for the specific monitor class used here
    /// </summary>
    public class SampleMonitorEventArgs : EventArgs
    {
        public SampleMonitorEventArgs(SampleMonitor sampleMonitor)
        {
            SampleMonitor = sampleMonitor;
        }

        public SampleMonitor SampleMonitor { get; }
    }
}
