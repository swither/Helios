using System;

namespace GadrocsWorkshop.Helios.Util
{
    public class HeliosUndoBatch : IDisposable
    {
        public HeliosUndoBatch()
        {
            ConfigManager.UndoManager.StartBatch();
        }

        public void Dispose()
        {
            ConfigManager.UndoManager.CloseBatch();
        }
    }
}