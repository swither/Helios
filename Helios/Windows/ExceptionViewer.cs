using System;
using System.Text.RegularExpressions;
using System.Windows.Threading;

namespace GadrocsWorkshop.Helios.Windows
{
    public class ExceptionViewer
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public static void DisplayUnhandledException(DispatcherUnhandledExceptionEventArgs e)
        {
            DisplayException(e.Exception);
            // now we exit
            Logger.Error(e.Exception, $"Unhandled exception occurred.  {e.Exception.Source} will exit.");
            HeliosInit.OnShutdown();
        }

        public static void DisplayException(Exception ex)
        {
            string message;
            Regex buidPathExpression = new Regex("[A-Z]:\\\\.*\\\\Helios\\\\");
            string trace = ex.StackTrace;
            Match buildPathMatch = buidPathExpression.Match(trace);
            if (buildPathMatch.Success)
            {
                message = trace.Replace(buildPathMatch.Groups[0].Value, "");
            }
            else
            {
                message = ex.Message;
            }

            System.Windows.MessageBox.Show(
                $"Unhandled exception occurred.  Please file a bug:\n{message}",
                $"Unhandled Error in {ex.Source}");
        }
    }
}