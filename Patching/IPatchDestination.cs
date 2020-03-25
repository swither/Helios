namespace GadrocsWorkshop.Helios.Patching
{
    public interface IPatchDestination
    {
        string Description { get; }

        bool TryLock();
        bool TryUnlock();
        bool TryGetSource(string targetPath, out string source);
        bool TryWritePatched(string targetPath, string patched);

        PatchList SelectPatches(string patchesPath, string patchSet);
    }
}