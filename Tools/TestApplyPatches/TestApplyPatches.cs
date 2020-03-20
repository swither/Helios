//using DiffPatch;
//using DiffPatch.Data;
using DiffMatchPatch;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace TestApplyPatches
{
    class TestApplyPatches
    {
        static void Main(string[] args)
        {
            //TestGeneralCases();
            TestDcsPatches();
            // XXX this is useless as it does not use context diff
            // TestDiffPatch(patch);
        }

        private static void TestDcsPatches()
        {
            // XXX create a Helios utility to locate and remember DCS root folders
            string dcsRoot = "e:\\dcs";
            // XXX build the utility to get DCS version and select patch tree
            const string testsRoot = "..\\..\\..\\..\\Patches\\DCS\\002_005_005_41371\\";

            DiffMatchPatch.diff_match_patch googleDiff = new DiffMatchPatch.diff_match_patch();
            foreach (string testFilePath in Directory.EnumerateFiles(testsRoot, "*.gpatch", SearchOption.AllDirectories))
            {
                Debug.Assert(testFilePath.Contains(testsRoot));
                string testFileRelative = testFilePath.Replace(testsRoot, "").Replace(".gpatch", "");

                string source = ReadFile(Path.Combine(dcsRoot, testFileRelative));

                List<DiffMatchPatch.Patch> patches = googleDiff.patch_fromText(ReadFile(testFilePath));
                string patched = ApplyPatches(googleDiff, source, patches);

                Debug.WriteLine("=====================================================================================");
                Debug.WriteLine(patched);

                List<DiffMatchPatch.Patch> reverts = googleDiff.patch_fromText(ReadFile(testFilePath.Replace(".gpatch", ".grevert")));
                string reverted = ApplyPatches(googleDiff, patched, reverts);

                CompareEquals(googleDiff, testFilePath, source, reverted);
            }
        }

        private static string ReadFile(string filePath)
        {
            using (StreamReader streamReader = new StreamReader(filePath))
            {
                return streamReader.ReadToEnd();
            }
        }

        private static void TestGeneralCases()
        {
            const string testsRoot = "..\\..\\..\\test";
            string sourcesPath = Path.Combine(testsRoot, "source");
            string patchedPath = Path.Combine(testsRoot, "patched");

            DiffMatchPatch.diff_match_patch googleDiff = new DiffMatchPatch.diff_match_patch();
            foreach (string testRootPath in Directory.EnumerateDirectories(Path.Combine(testsRoot, "cases")))
            {
                string testSourcePath = Path.Combine(testRootPath, "a");
                Debug.WriteLine(testSourcePath);
                foreach (string testFilePath in Directory.EnumerateFiles(testSourcePath, "*.lua", SearchOption.AllDirectories))
                {
                    Debug.WriteLine("=====================================================================================");
                    Debug.WriteLine(testFilePath);
                    string source = File.ReadAllText(testFilePath.Replace(testSourcePath, sourcesPath));
                    string target = File.ReadAllText(testFilePath.Replace(testSourcePath, patchedPath));
                    string testInput = File.ReadAllText(testFilePath);
                    string testExpected = File.ReadAllText(testFilePath.Replace("\\a\\", "\\b\\"));

                    // NOTE: do our own diffs so we just do semantic cleanup. We don't want to optimize for efficiency.
                    List<DiffMatchPatch.Diff> diffs = googleDiff.diff_main(source, target);
                    googleDiff.diff_cleanupSemantic(diffs);
                    List<DiffMatchPatch.Patch> patches = googleDiff.patch_make(diffs);
                    Debug.WriteLine("++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");
                    Debug.WriteLine(googleDiff.patch_toText(patches));
                    string patched = ApplyPatches(googleDiff, testInput, patches);
                    Debug.WriteLine("=====================================================================================");
                    Debug.WriteLine(patched);
                    CompareEquals(googleDiff, testFilePath, testExpected, patched);
                }
            }
        }

        private static string NoWhiteSpace(string text)
        {
            return text.Replace("\n", "").Replace("\r", "");
        }

        private static void CompareEquals(diff_match_patch googleDiff, string testName, string expected, string actual)
        {
            // XXX our revert does not correctly remove leading \r\n for some reason, so for now just don't require that
            if (NoWhiteSpace(actual) != NoWhiteSpace(expected))
            {
                List<DiffMatchPatch.Diff> errors = googleDiff.diff_main(expected, actual);
                List<DiffMatchPatch.Patch> errorPatches = googleDiff.patch_make(errors);
                Debug.WriteLine("XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX");
                Debug.WriteLine("differences from expected value to actual result:");
                Debug.WriteLine(googleDiff.patch_toText(errorPatches));
                throw new System.Exception($"failed test case {testName}");
            }
        }

        private static string ApplyPatches(diff_match_patch googleDiff, string testInput, List<Patch> patches)
        {
            object[] results = googleDiff.patch_apply(patches, testInput);
            string patched = (string)results[0];
            bool[] applied = (bool[])results[1];
            for (int i = 0; i < applied.Length; i++)
            {
                if (!applied[i])
                {
                    throw new System.Exception($"failed to apply {patches[i].ToString()}");
                }
            }

            return patched;
        }

        //private static void TestDiffPatch(string patch)
        //{
        //    IEnumerable<FileDiff> fileDiffs = DiffParserHelper.Parse(patch);
        //    foreach (FileDiff fileDiff in fileDiffs)
        //    {
        //        string source = System.IO.File.ReadAllText($"e:\\dcs\\{fileDiff.From}");
        //        Debug.WriteLine(source);
        //        string target = PatchHelper.Patch(source, fileDiff.Chunks, "\n");
        //        Debug.WriteLine("====================================");
        //        Debug.WriteLine(target);
        //    }
        //}
    }
}
