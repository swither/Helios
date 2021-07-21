using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using GadrocsWorkshop.Helios.ComponentModel;
using GadrocsWorkshop.Helios.Interfaces.Common;
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
            // TODO: read config
        }

        public override void WriteXml(XmlWriter writer)
        {
            // TODO: write config
        }

        protected override SampleMonitor CreateShadowMonitor(Monitor monitor) => new SampleMonitor(this, monitor, monitor, false);

        protected override List<StatusReportItem> CreateStatusReport()
        {
            // TODO: write a status report
            return Viewports
                .Select(v => new StatusReportItem
                {
                    Severity = StatusReportItem.SeverityCode.Warning,
                    Status = $"({v.Visual.Left + v.Monitor.Left}, {v.Visual.Top + v.Monitor.Top}) {v.Visual.Width}x{v.Visual.Height}",
                    Recommendation = "Sample output to show this is up to date"
                })
                .ToList();
        }

        protected override void UpdateAllGeometry()
        {
            // TODO: perform any calculations based on viewports and/or monitors having changed, which could change the status report
        }
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
