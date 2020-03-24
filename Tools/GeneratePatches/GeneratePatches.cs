using GadrocsWorkshop.Helios.Patching.DCS;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibGit2Sharp;
using System;
using System.Diagnostics;

namespace GeneratePatches
{
    class GeneratePatches
    {
        static void Main(string[] args)
        {
            string dcsRoot = new PatchDestination().RootFolder;
            if (args.Length > 0)
            {
                switch (args[0])
                {
                    case "init":
                        InitializeRepo(dcsRoot);
                        break;
                }
            }
            else
            {
                UpdatePatches(dcsRoot);
            }
        }

        private static void UpdatePatches(string dcsRoot)
        {
            // XXX determine DCS version
            string dcsVersion = "002_005_005_41371";
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

                        string patchesPath = Path.Combine("..", "..", "..", "..", "Patches");
                        if (!Directory.Exists(patchesPath))
                        {
                            throw new System.Exception($"relative path for patch output '{patchesPath}' does not exist");
                        }
                        string outputPath = Path.Combine(patchesPath, "DCS", dcsVersion, $"{item.FilePath}.gpatch");
                        string reversePath = Path.Combine(patchesPath, "DCS", dcsVersion, $"{item.FilePath}.grevert");
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
                catch (LibGit2Sharp.EmptyCommitException)
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
                string config = File.ReadAllText($"..\\..\\..\\Setup\\DCS\\{configFile}");
                File.WriteAllText(configPath, config);
            }
        }
    }
}
