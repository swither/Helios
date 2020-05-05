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

        /// <summary>
        /// tries to acquire a lock on the patch destination preventing other write operations
        /// </summary>
        /// <returns></returns>
        bool TryLock();

        /// <summary>
        /// tries to release the lock on the destination
        /// </summary>
        /// <returns></returns>
        bool TryUnlock();

        /// <summary>
        /// tries to read the full text of the target file
        /// </summary>
        /// <param name="targetPath"></param>
        /// <param name="source"></param>
        /// <returns>true if the contents were read successfully</returns>
        bool TryGetSource(string targetPath, out string source);

        /// <summary>
        /// tries to save the current version of the file as an archived original for this version
        /// </summary>
        /// <param name="targetPath"></param>
        /// <returns>true if the file was saved</returns>
        bool TrySaveOriginal(string targetPath);

        /// <summary>
        /// tries to write the patched file
        /// </summary>
        /// <param name="targetPath"></param>
        /// <param name="patched"></param>
        /// <returns>true if the file could be written</returns>
        bool TryWritePatched(string targetPath, string patched);

        /// <summary>
        /// tries to restore the file as it was before patching, if it is available
        /// </summary>
        /// <param name="targetPath"></param>
        /// <returns>true if the file was restored</returns>
        bool TryRestoreOriginal(string targetPath);

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