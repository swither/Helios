using System;
using System.Windows;
using NLog;
using NLog.Targets;

namespace GadrocsWorkshop.Helios.ControlCenter.StatusViewer
{
    [Target("StatusViewer")]
    public sealed class StatusViewerLogTarget : TargetWithLayout
    {
        public static StatusViewer Parent { get; internal set; }

        protected override void Write(LogEventInfo logEvent)
        {
            if (Parent == null)
            {
                // not hooked up
                return;
            }
            string message = this.Layout.Render(logEvent);

            // note: exceptions here will bring down the application
            Application.Current.Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    Parent.WriteLogMessage(logEvent, message);
                }
                catch (System.Exception ex)
                {
                    // can't log from here
                    Console.WriteLine($@"log consumer failed to write message: {ex}");
                }
            });
        }
    }
}
