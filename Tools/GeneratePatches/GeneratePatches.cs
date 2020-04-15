using LibGit2Sharp;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace GeneratePatches
{
    class GeneratePatches
    {
        // XXX port to real command line, make dcs root a required parameter
        static private readonly string[] _dcsRoots = new string[] { "C:\\Program Files\\Eagle Dynamics\\DCS World", "c:\\dcs", "e:\\dcs" };

        private class AutoUpdateConfig
        {
            [JsonProperty("version")]
            public string Version { get; private set; }
        }

        static void Main(string[] args)
        {
            string dcsRoot = "NOTFOUND";
            foreach (string candidate in _dcsRoots)
            {
                if (Directory.Exists(candidate))
                {
                    dcsRoot = candidate;
                    break;
                }
            }
            if (args.Length > 0)
            {
                switch (args[0])
                {
                    case "init":
                        InitializeRepo(dcsRoot);
                        break;
                    default:
                        UpdatePatches(dcsRoot, args[0]);
                        break;
                }
            }
        }

        private static void UpdatePatches(string dcsRoot, string patchSet)
        {
            // determine DCS version
            string autoUpdatePath = Path.Combine(dcsRoot, "autoupdate.cfg");
            string versionString = JsonConvert.DeserializeObject<AutoUpdateConfig>(File.ReadAllText(autoUpdatePath)).Version;
            System.Version parsed = System.Version.Parse(versionString);
            string dcsVersion = $"{parsed.Major:000}_{parsed.Minor:000}_{parsed.Build:00000}_{parsed.Revision:00000}";

            // now build patches based on files changed in repo
            string patchesPath = ToolsCommon.FileSystem.FindNearestDirectory("Patches");
            using (Repository repo = new Repository(dcsRoot))
            {
                foreach (StatusEntry item in repo.RetrieveStatus(new StatusOptions()))
                {
                    if (item.State == FileStatus.ModifiedInWorkdir)
                    {
                        string source;
                        string target;
                        Debug.WriteLine(item.FilePath);
                        Blob blob = repo.Head.Tip[item.FilePath].Target as Blob;
                        using (var content = new StreamReader(blob.GetContentStream(), Encoding.UTF8))
                        {
                            source = content.ReadToEnd();
                        }
                        string workingPath = Path.Combine(repo.Info.WorkingDirectory, item.FilePath);
                        using (var content = new StreamReader(workingPath, Encoding.UTF8))
                        {
                            target = content.ReadToEnd();
                        }

                        string outputPath = Path.Combine(patchesPath, "DCS", dcsVersion, patchSet, $"{item.FilePath}.gpatch");
                        string reversePath = Path.Combine(patchesPath, "DCS", dcsVersion, patchSet, $"{item.FilePath}.grevert");
                        string outputDirectoryPath = Path.GetDirectoryName(outputPath);
                        if (!Directory.Exists(outputDirectoryPath))
                        {
                            Directory.CreateDirectory(outputDirectoryPath);
                        }

                        WritePatch(source, target, outputPath);
                        WritePatch(target, source, reversePath);
                    }
                }
            }
        }

        private static void WritePatch(string source, string target, string outputPath)
        {
            // NOTE: do our own diffs so we just do semantic cleanup. 
            // We don't want to optimize for efficiency.
            DiffMatchPatch.diff_match_patch googleDiff = new DiffMatchPatch.diff_match_patch();
            List<DiffMatchPatch.Diff> diffs;
            diffs = googleDiff.diff_main(source, target);
            googleDiff.diff_cleanupSemantic(diffs);

            // write patch
            List<DiffMatchPatch.Patch> patches = googleDiff.patch_make(diffs);
            using (StreamWriter streamWriter = new StreamWriter(outputPath, false, Encoding.UTF8))
            {
                streamWriter.Write(googleDiff.patch_toText(patches));
            }
        }

        private static void InitializeRepo(string dcsRoot)
        {
            Console.WriteLine($"Initializing git repo at '{dcsRoot}' and checking in lua files");
            if (!Directory.Exists(Path.Combine(dcsRoot, ".git")))
            {
                Repository.Init(dcsRoot);
                InitializeGitConfig(dcsRoot, "gitignore");
                InitializeGitConfig(dcsRoot, "gitattributes");
            }
            using (Repository repo = new Repository(dcsRoot))
            {
                Commands.Stage(repo, "*");
                Signature author = new Signature("Helios GeneratePatches", "@anonymous", DateTime.Now);
                try
                {
                    Commit commit = repo.Commit("Initial checkin of clean files", author, author);
                }
                catch (EmptyCommitException)
                {
                    Console.WriteLine("all non-ignored files are already committed");
                }
            }
        }

        private static void InitializeGitConfig(string dcsRoot, string configFile)
        {
            string configPath = Path.Combine(dcsRoot, $".{configFile}");
            if (!File.Exists(configPath))
            {
                string sourcePath = ToolsCommon.FileSystem.FindNearestDirectory("Setup\\DCS") + configFile;
                string config = File.ReadAllText(sourcePath);
                File.WriteAllText(configPath, config);
            }
        }
    }
}
