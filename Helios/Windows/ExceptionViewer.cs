﻿// Copyright 2020 Ammo Goettsch
// 
// Helios is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// Helios is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Threading;
using NLog;

namespace GadrocsWorkshop.Helios.Windows
{
    /// <summary>
    /// the last resort handler for exceptions, which logs the expection and then shown an ugly message box
    /// with the exception to the user, without assuming we can display any of our custom dialogs via WPF
    /// </summary>
    public class ExceptionViewer
    {
        private static readonly Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public static void DisplayUnhandledException(DispatcherUnhandledExceptionEventArgs e)
        {
            Logger.Error(e.Exception,
                $"Unhandled exception occurred. {Assembly.GetExecutingAssembly()?.GetName()?.Name ?? "Application"} will exit.");
            DisplayException(e.Exception);

            // prepare for exit or crash
            HeliosInit.OnShutdown();
        }

        public static void DisplayException(Exception ex)
        {
            string message = ex.Message;
            Regex buildPathExpression = new Regex("[A-Z]:.*\\\\Helios(Dev)?\\\\");

            string trace = ex.StackTrace;
            Exception scan = ex;

            // get up to 10 inner exceptions
            for (int i = 0; i < 10 && scan.InnerException != null; i++)
            {
                scan = scan.InnerException;
                trace =
                    $"{trace}{Environment.NewLine}{Environment.NewLine}caused by:{Environment.NewLine}{scan.Message}{Environment.NewLine}{scan.StackTrace}";
            }

            Match buildPathMatch = buildPathExpression.Match(trace);
            if (buildPathMatch.Success)
            {
                message += "\n" + trace.Replace(buildPathMatch.Groups[0].Value, "");
            }
            else
            {
                message += "\n" + trace;
            }
            MessageBox.Show(
                $"Unhandled exception occurred.  Please file a bug report:\n{message}",
                $"Unhandled Error in {ex.Source}");
        }
    }
}