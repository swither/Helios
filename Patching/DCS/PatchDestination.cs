using GadrocsWorkshop.Helios.Util.DCS;
using System;
using System.IO;

namespace GadrocsWorkshop.Helios.Patching.DCS
{
    public class PatchDestination : IPatchDestination
    {
        private string _dcsRoot = "NOTFOUND";
        private static System.Text.Encoding _utf8WithoutBom = new System.Text.UTF8Encoding(false);

        public PatchDestination(InstallationLocation location)
        {
            _dcsRoot = location.Path;
            if (!System.Version.TryParse(location.Version, out Version parsed)) {
                throw new System.Exception("invalid version format read from DCS autoupdate file; update Helios to support current DCS version");
            }
            Version = $"{parsed.Major:000}_{parsed.Minor:000}_{parsed.Build:00000}_{parsed.Revision:00000}";
            DisplayVersion = location.Version;
        }

        public string Description => $"DCS {DisplayVersion}";
        public string LongDescription => $"DCS installation in '{_dcsRoot}'";

        public string RootFolder => _dcsRoot;

        public string Version { get; private set; } = "002_005_00005_41371";
        public string DisplayVersion { get; private set; } = "2.5.5.41371";

        public bool TryGetSource(string targetPath, out string source)
        {
            string path = LocateFile(targetPath);
            if (!File.Exists(path))
            {
                source = null;
                return false;
            }
            using (StreamReader streamReader = new StreamReader(path, _utf8WithoutBom))
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
            using (StreamWriter streamWriter = new StreamWriter(path, false, _utf8WithoutBom))
            {
                streamWriter.Write(patched);
                return true;
            }
        }

        public PatchList SelectPatches(string patchesPath, string patchSet)
        {
            PatchList patches = new PatchList();
            if (!Directory.Exists(patchesPath))
            {
                return patches;
            }
            string candidateVersion = "";
            string candidatePatchSetPath = "";
            foreach (string versionPath in Directory.EnumerateDirectories(patchesPath, "???_???_?????_*", SearchOption.TopDirectoryOnly))
            {
                string directoryVersion = Path.GetFileName(versionPath);
                string patchSetPath = Path.Combine(versionPath, patchSet);
                if (!Directory.Exists(patchSetPath))
                {
                    // patch set not included in this update
                    continue;
                }
                if (directoryVersion.CompareTo(Version) > 0)
                {
                    // patches are only for later version of DCS
                    continue;
                }
                if (directoryVersion.CompareTo(candidateVersion) > 0)
                {
                    candidateVersion = directoryVersion;
                    candidatePatchSetPath = patchSetPath;
                }
            }
            if (candidatePatchSetPath == "")
            {
                ConfigManager.LogManager.LogWarning($"current version of DCS {DisplayVersion} is not supported by any installed {patchSet} patch set");
                return patches;
            }
            foreach (string patchPath in Directory.EnumerateFiles(candidatePatchSetPath, "*.gpatch", SearchOption.AllDirectories))
            {
                PatchFile patch = new PatchFile
                {
                    TargetPath = patchPath.Substring(candidatePatchSetPath.Length + 1, patchPath.Length - (candidatePatchSetPath.Length + 8))
                };
                patch.Load(patchPath);
                patches.Add(patch);
            }
            return patches;
        }
    }
}