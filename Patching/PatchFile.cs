using DiffMatchPatch;
using System;
using System.Collections.Generic;
using System.IO;

namespace GadrocsWorkshop.Helios.Patching
{
    public class PatchFile
    {
        private static diff_match_patch _googleDiff = new diff_match_patch();
        private static System.Text.Encoding _utf8WithoutBom = new System.Text.UTF8Encoding(false);
        private List<Patch> _patches;
        private List<Patch> _reversePatches;

        public string TargetPath { get; internal set; }

        internal bool TryApply(string source, out string patched, out string status)
        {
            status = null;
            object[] results = _googleDiff.patch_apply(_patches, source);
            patched = (string)results[0];
            bool[] applied = (bool[])results[1];
            diff_match_patch.PatchResult[] resultCodes = (diff_match_patch.PatchResult[])results[2];
            for (int i = 0; i < applied.Length; i++)
            {
                if (!applied[i])
                {
                    status = ($"failed to apply patch to {TargetPath}: {resultCodes[i].ToString()}");
                    return false;
                }
                switch (resultCodes[i])
                {
                    case diff_match_patch.PatchResult.UNKNOWN:
                        throw new System.Exception($"invalid result code from application of {_patches[i].ToString()}");
                    case diff_match_patch.PatchResult.APPLIED_PERFECT:
                        break;
                    case diff_match_patch.PatchResult.APPLIED_IMPERFECT:
                        status = "patch imperfectly matched input, but was applied";
                        break;
                    default:
                        throw new System.Exception($"patch should not have returned success with result code {resultCodes[i].ToString()}");
                }
            }
            return true;
        }

        internal bool TryRevert(string source, out string patched, out string status)
        {
            if (_reversePatches == null)
            {
                status = $"no reverse patch available for {TargetPath}";
                patched = source;
                return false;
            }
            status = null;
            object[] results = _googleDiff.patch_apply(_reversePatches, source);
            patched = (string)results[0];
            bool[] applied = (bool[])results[1];
            diff_match_patch.PatchResult[] resultCodes = (diff_match_patch.PatchResult[])results[2];
            for (int i = 0; i < applied.Length; i++)
            {
                if (!applied[i])
                {
                    status = ($"failed to revert patch against {TargetPath}: {resultCodes[i].ToString()}");
                    return false;
                }
                switch (resultCodes[i])
                {
                    case diff_match_patch.PatchResult.UNKNOWN:
                        throw new System.Exception($"invalid result code from application of reverse patch {_patches[i].ToString()}");
                    case diff_match_patch.PatchResult.APPLIED_PERFECT:
                        break;
                    case diff_match_patch.PatchResult.APPLIED_IMPERFECT:
                        status = "reverse patch imperfectly matched input, but was applied";
                        break;
                    default:
                        throw new System.Exception($"reverse patch should not have returned success with result code {resultCodes[i].ToString()}");
                }
            }
            return true;
        }

        internal bool IsApplied(string patched)
        {
            // check if applied by just seeing if all the inserts are present (we don't generate patches that insert and then delete the same thing)
            foreach (Patch patch in _patches)
            {
                string matchText = "";
                foreach (Diff chunk in patch.diffs)
                {
                    switch (chunk.operation)
                    {
                        case Operation.DELETE:
                            break;
                        case Operation.INSERT:
                        case Operation.EQUAL:
                            matchText += chunk.text;
                            break;
                    }
                }
                if (!patched.Contains(matchText))
                {
                    return false;
                }
            }
            return true;
        }

        internal void Load(string patchPath)
        {
            using (StreamReader streamReader = new StreamReader(patchPath, _utf8WithoutBom))
            {
                string patchText = streamReader.ReadToEnd();
                _patches = _googleDiff.patch_fromText(patchText);
            }

            string revertPath = Path.ChangeExtension(patchPath, ".grevert");
            if (!File.Exists(revertPath))
            {
                return;
            }

            using (StreamReader streamReader = new StreamReader(revertPath, _utf8WithoutBom))
            {
                string patchText = streamReader.ReadToEnd();
                _reversePatches = _googleDiff.patch_fromText(patchText);
            }
        }
    }
}
