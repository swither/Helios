namespace GadrocsWorkshop.Helios.Patching
{
    public interface IPatchDestination
    {
        /// <summary>
        /// a description that allows a human to identify this patch destination
        /// </summary>
        string Description { get; }

        /// <summary>
        /// a longer description for debugging purposes, including file paths
        /// </summary>
        string LongDescription { get; }

        bool TryLock();
        bool TryUnlock();
        bool TryGetSource(string targetPath, out string source);
        bool TryWritePatched(string targetPath, string patched);

        PatchList SelectPatches(string patchesPath, string patchSet);
    }
}