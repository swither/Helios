// Copyright 2021 Helios Contributors
// 
// HeliosFalcon is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// HeliosFalcon is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
// 

using System;
using System.Diagnostics;
using System.IO;

namespace GadrocsWorkshop.Helios.Interfaces.Falcon.Interfaces.RTT
{
    internal class ProcessControl
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public static bool StartRTTClient(string executablePath)
        {
            if (!File.Exists(executablePath))
            {
                return false;
            }

            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = Path.GetFileName(executablePath);
            psi.WorkingDirectory = Path.GetDirectoryName(executablePath);
            psi.UseShellExecute = true;
            psi.RedirectStandardOutput = false;
            Process.Start(psi);
            return true;
        }

        public static void KillRTTCllient(string processName)
        {
            try
            {
                Process[] localProcessesByName = Process.GetProcessesByName(processName);
                foreach (Process proc in localProcessesByName)
                {
                    Logger.Info("Killing process image name {ProcessImageName}", processName);
                    proc.Kill();
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error caught during kill processing for process image name {ProcessImageName}",
                    processName);
            }
        }
    }
}