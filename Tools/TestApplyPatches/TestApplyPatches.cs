//using DiffPatch;
//using DiffPatch.Data;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace TestApplyPatches
{
    class TestApplyPatches
    {
        static void Main(string[] args)
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
                    Debug.WriteLine( "=====================================================================================");
                    Debug.WriteLine(patched);

                    if (patched != testExpected)
                    {
                        List<DiffMatchPatch.Diff> errors = googleDiff.diff_main(testExpected, patched);
                        List<DiffMatchPatch.Patch> errorPatches = googleDiff.patch_make(errors);
                        Debug.WriteLine("XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX");
                        Debug.WriteLine("differences from expected output to patched result:");
                        Debug.WriteLine(googleDiff.patch_toText(errorPatches));
                        throw new System.Exception($"failed test case {testFilePath}");
                    }
                }
            }

            // XXX this is useless as it does not use context diff
            // testDiffPatch(patch);
        }

        //private static void testDiffPatch(string patch)
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
