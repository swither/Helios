using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using GadrocsWorkshop.Helios;
using GadrocsWorkshop.Helios.Patching;
using GadrocsWorkshop.Helios.Patching.DCS;
using GadrocsWorkshop.Helios.Util;
using GadrocsWorkshop.Helios.Util.DCS;

namespace HeliosVirtualCockpit.Helios.HeliosPatching
{
    class HeliosPatching
    {
        /// <summary>
        /// elevated executable to apply patches
        ///
        /// currently only supports DCS locations for output
        /// does not use CommandLine to avoid running third party code as root when possible
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            try
            {
                if (args.Length < 2)
                {
                    // tell users to go away and not try to use this executable even though it is in the installation folder
                    throw new Exception("Do not execute this program.  It is a system process used by Helios Profile Editor.");
                }

                if (args.Length < 7)
                {
                    throw new Exception(
                        "must have at least the following arguments: -o PIPENAME -h HELIOSDOCUMENTS -d DCSROOT COMMAND");
                }

                if (args[0] != "-o")
                {
                    throw new Exception(
                        "must have the following arguments: -o PIPENAME -h HELIOSDOCUMENTS -d DCSROOT COMMAND");
                }

                if (args[2] != "-h")
                {
                    throw new Exception(
                        "must have the following arguments: -o PIPENAME -h HELIOSDOCUMENTS -d DCSROOT COMMAND");
                }

                if (args[4] != "-d")
                {
                    throw new Exception(
                        "must have the following arguments: -o PIPENAME -h HELIOSDOCUMENTS -d DCSROOT COMMAND");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "HeliosPatching Utility Executable");
                return;
            }

            // start only enough of Helios to support logging and settings file
            string documentPath = args[3];
            HeliosInit.InitializeLogging(documentPath, LogLevel.Info);
            ConfigManager.DocumentPath = documentPath;
            HeliosInit.InitializeSettings(false);

            string pipeName = args[1];
            string dcsRoot = args[5];
            string verb = args[6];
            try
            {
                switch (verb)
                {
                    case "apply":
                        using (ElevatedProcessResponsePipe response = new ElevatedProcessResponsePipe(pipeName))
                        {
                            DcsApply(response, dcsRoot, args.Skip(5));
                            response.WaitToDeliver();
                        }

                        break;
                    case "revert":
                        using (ElevatedProcessResponsePipe response = new ElevatedProcessResponsePipe(pipeName))
                        {
                            DcsRevert(response, dcsRoot, args.Skip(5));
                            response.WaitToDeliver();
                        }

                        break;
                    default:
                        throw new Exception(
                            "must have arguments matching: -o PIPENAME -h HELIOSDOCUMENTS -d DCSROOT apply|revert PATCHFOLDERS...");
                }
            }
            catch (Exception ex)
            {
                ConfigManager.LogManager.LogError("fatal error while performing patch operations as administrator", ex);
#if DEBUG
                // during debugging, we may be running as a console application, so print to console and wait
                Console.WriteLine(ex);
                Console.WriteLine(ex.StackTrace);

                new ManualResetEvent(false).WaitOne(TimeSpan.FromSeconds(30));
#else
                _ = ex;
#endif
                throw;
            }
        }

        private static PatchList LoadPatches(IEnumerable<string> patchFolders)
        {
            PatchList patches = new PatchList();
            foreach (string patchFolder in patchFolders)
            {
                patches.Merge(PatchList.Load(patchFolder));
            }
            return patches;
        }

        private static void DcsApply(ElevatedProcessResponsePipe response, string outputRoot, IEnumerable<string> patchFolders)
        {
            PatchList patches = LoadPatches(patchFolders);
            if (!InstallationLocation.TryLoadLocation(outputRoot, true, out InstallationLocation location))
            {
                ReportBadLocation(response, outputRoot);
                return;
            }
            PatchDestination dcs = new PatchDestination(location);
            HashSet<string> patchExclusions = PatchInstallation.LoadPatchExclusions();
            IList<StatusReportItem> results = patches.Apply(dcs, patchExclusions).ToList();
            response.SendReport(results);
        }

        private static void ReportBadLocation(ElevatedProcessResponsePipe response, string outputRoot)
        {
            response.SendReport(new StatusReportItem
            {
                Status = $"Elevated HeliosPatching utility could not open DCS installation {outputRoot}",
                Severity = StatusReportItem.SeverityCode.Error
            }.AsReport());
        }

        private static void DcsRevert(ElevatedProcessResponsePipe response, string outputRoot, IEnumerable<string> patchFolders)
        {
            PatchList patches = LoadPatches(patchFolders);
            if (!InstallationLocation.TryLoadLocation(outputRoot, true, out InstallationLocation location))
            {
                ReportBadLocation(response, outputRoot);
                return;
            }
            PatchDestination dcs = new PatchDestination(location);
            HashSet<string> patchExclusions = PatchInstallation.LoadPatchExclusions();
            IList<StatusReportItem> results = patches.Revert(dcs, patchExclusions).ToList();
            response.SendReport(results);
        }
    }
}
