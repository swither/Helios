using System.Windows;
using GadrocsWorkshop.Helios.ComponentModel;

namespace GadrocsWorkshop.Helios.Windows
{
    public class HeliosViewModel<T> : DependencyObject where T : NotificationObject
    {
        protected HeliosViewModel(T data)
        {
            Data = data;
        }

        protected static void GenerateHeliosUndoForProperty(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // pretend this dependency object is a notificationobject in the tree under our data object and
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