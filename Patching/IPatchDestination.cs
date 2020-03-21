namespace GadrocsWorkshop.Helios.Patching
{
    internal interface IPatchDestination
    {
        object Description { get; }

        bool TryLock();
        bool TryUnlock();
        bool TryGetSource(object targetPath, out string source);
        bool TryWritePatched(object targetPath, string patched);
    }
}