using System;
using System.IO;

namespace GadrocsWorkshop.Helios.Patching.DCS
{
    public class PatchDestination : IPatchDestination
    {
        // XXX create a Helios utility to locate and remember DCS root folders
        static private readonly string[] dcsRoots = new string[] { "C:\\Program Files\\Eagle Dynamics\\DCS World", "c:\\dcs", "e:\\dcs" };
        private string _dcsRoot = "NOTFOUND";

        public PatchDestination()
        {
            foreach (string candidate in dcsRoots)
            {
                if (Directory.Exists(candidate))
                {
                    _dcsRoot = candidate;
                    break;
                }
            }
        }

        public object Description => $"DCS installation in '{_dcsRoot}'";

        public bool TryGetSource(string targetPath, out string source)
        {
            string path = LocateFile(targetPath);
            if (!File.Exists(path))
            {
                source = null;
                return false;
            }
            using (StreamReader streamReader = new StreamReader(path))
            {
                source = streamReader.ReadToEnd();
                return true;
            }
        }

        private string LocateFile(string targetPath)
        {
            return Path.Combine(_dcsRoot, targetPath);
        }

        public bool TryLock()
        {
            // XXX implement
            return true;
        }

        public bool TryUnlock()
        {
            // XXX implement
            return true;
        }

        public bool TryWritePatched(string targetPath, string patched)
        {
            string path = LocateFile(targetPath);
            if (!File.Exists(path)) {
                ConfigManager.LogManager.LogError($"attempt to write back a patched file to a non-existing location: '{path}'");
                return false;
            }
            using (StreamWriter streamWriter = new StreamWriter(path))
            {
                streamWriter.Write(patched);
                return true;
            }
        }
    }
}