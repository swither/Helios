using System;

namespace GadrocsWorkshop.Helios.Util
{
    /// <summary>
    /// a design instance for an editor of a helios control, because "Control" is in the paths
    /// in all legacy code instead of making "Control" the data context
    ///
    /// using a concretized descendant of this class as the design instance in a property editor will allow path
    /// checking by XAML Intellisense
    ///
    /// example XAML:
    ///
    ///     d:DataContext="{d:DesignInstance Type={x:Type controls:DesignTimeMyControlName}}"
    ///
    /// with C#:
    ///
    ///     DesignTimeMyControlName: DesignTimeControl&lt;MyControlName&gt;
    ///
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DesignTimeControl<T> where T: HeliosVisual
    {
        public DesignTimeControl()
        {
            Control = Activator.CreateInstance<T>();
        }

        public T Control { get; }
    }
}
