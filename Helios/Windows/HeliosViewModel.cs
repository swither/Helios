using GadrocsWorkshop.Helios.ComponentModel;
using System.Windows;

namespace GadrocsWorkshop.Helios.Windows
{
    /// <summary>
    /// a view model class using dependency properties, demonstrating how those
    /// can still be hooking in to Helios' undo mechanism
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class HeliosViewModel<T> : DependencyObject where T : NotificationObject
    {
        protected HeliosViewModel(T data)
        {
            Data = data;
        }

        protected static void GenerateHeliosUndoForProperty(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // pretend this dependency object is a NotificationObject in the tree under our data object and
            // generate an Undo record that will set our property back if called
            HeliosViewModel<T> sourceObject = (HeliosViewModel<T>) d;
            sourceObject.Data.OnPropertyChanged(
                e.Property.Name,
                new PropertyNotificationEventArgs(sourceObject, e.Property.Name, e.OldValue, e.NewValue));
        }

        #region Properties

        public T Data { get; }

        #endregion
    }
}