namespace GadrocsWorkshop.Helios.Tools.Capabilities
{
    public interface ISelectionAwareTool
    {
        /// <summary>
        /// connect to the current visuals selection set, for example to track changes or just use the
        /// current selection when tool is called
        ///
        /// this function will be called repeatedly as the current editor changes
        /// </summary>
        /// <param name="selectedItems"></param>
        void AttachToSelection(HeliosVisualCollection selectedItems);
    }
}