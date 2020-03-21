using System;

namespace GadrocsWorkshop.Helios.Patching
{
    class Patch
    {
        public object TargetPath { get; internal set; }

        internal bool TryApply(string source, out string patched, out StatusReportItem status)
        {
            throw new NotImplementedException();
        }

        internal bool IsApplied(string source, out StatusReportItem status)
        {
            throw new NotImplementedException();
        }
    }
}
