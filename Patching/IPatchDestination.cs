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

        /// <summary>
        /// finds all applicable patches from a specfic patches folder
        /// </summary>
        /// <param name="patchesPath">the root folder for Patches</param>
        /// <param name="patchSet">the patch set, such as 'Viewports'</param>
        /// <param name="selectedVersion">returns the selected version, if any.  If set before calling this method, only that version will be considered.</param>
        /// <returns>a patchlist that may be empty if no matches were found</returns>
        PatchList SelectPatches(string patchesPath, string patchSet, ref string selectedVersion);
    }
}