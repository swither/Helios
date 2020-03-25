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

        public string TargetPath { get; internal set; }

        internal bool TryApply(string source, out string patched, out string status)
        {
            throw new NotImplementedException();
        }

        internal bool IsApplied(string patched, out string status)
        {
            // check if applied by just seeing if all the inserts are present (we don't generate patches that insert and then delete the same thing)
            foreach (Patch patch in _patches)
            {
                foreach (Diff chunk in patch.diffs)
                {
                    if (chunk.operation == Operation.INSERT)
                    {
                        if (!patched.Contains(chunk.text))
                        {
                            status = $"a required patch to the file '{TargetPath}' is not installed";
                            return false;
                        }
                    }
                }
            }
            status = $"file '{TargetPath}' is correctly patched";
            return true;
        }

        internal void Load(string patchPath)
        {
            using (StreamReader streamReader = new StreamReader(patchPath, _utf8WithoutBom))
            {
                string patchText = streamReader.ReadToEnd();
                _patches = _googleDiff.patch_fromText(patchText);
            }
        }
    }
}
