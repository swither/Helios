namespace GadrocsWorkshop.Helios.Patching
{
    internal interface IPatchDestination
    {
        string Description { get; }

        bool TryLock();
        bool TryUnlock();
        bool TryGetSource(string targetPath, out string source);
        bool TryWritePatched(string targetPath, string patched);
    }
}