using System.Windows;
using System.Windows.Input;
using GadrocsWorkshop.Helios;
using GadrocsWorkshop.Helios.Util;
using GadrocsWorkshop.Helios.Windows;

namespace net.derammo.Helios.SampleProgram.Sample
{
    // A view model for our sample data, encapsulating the UI state and interaction
    // state that is not persisted to the configuration.  Any persisted values would be part
    // of the model and stored in SampleModel.
    //
    // command handlers are also added to the view model in order to group them with the other
    // "UI flow" logic.  This means commands are NOT routable commands but rather are bound 
    // by Command="{Binding CommandName}" in the UI XAML 
    //
    public partial class SampleViewModel : HeliosViewModel<SampleModel>
    {
        /// <summary>
        /// backing field for property UndoCommand, contains
        /// handlers for undo action
        /// </summary>
        private ICommand _undoCommand;

        public SampleViewModel(SampleModel model)
            : base(model)
        {
            // no code
        }

        private static void OnSomeViewStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // do some stuff with the change, including maybe calling GenerateHeliosUndoForProperty if we want to
            // make this undo-able

            // don't change the related items if this is an undo
            if (ConfigManager.UndoManager?.Working ?? false)
            {
                return;
            }

            // in this example, we will change multiple things in the (data) model with one UI interaction,
            // so we batch their undo events
            using (new HeliosUndoBatch())
            {
                GenerateHeliosUndoForProperty(d, e);
                SampleViewModel model = (SampleViewModel) d;
                model.Data.SomeOtherThing = new object();
                model.Data.SomeData = "some related text";
                model.Data.SomeInteger = (int) e.NewValue;                
            }
        }

        #region Properties

        /// <summary>
        /// some view state that isn't backed up to the model but does generate an undo event if changed
        /// </summary>
        public string SomeOtherViewState
        {
            get => (string) GetValue(SomeOtherViewStateProperty);
            set => SetValue(SomeOtherViewStateProperty, value);
        }

        /// <summary>
        /// some other view state that calls some user-supplied logic if changed
        /// </summary>
        public int SomeViewState
        {
            get => (int) GetValue(SomeViewStateProperty);
            set => SetValue(SomeViewStateProperty, value);
        }

        /// <summary>
        /// handlers for undo action
        /// </summary>
        public ICommand UndoCommand
        {
            get
            {
                _undoCommand = _undoCommand ?? new RelayCommand(
                    // Execute is to undo
                    parameter => { ConfigManager.UndoManager?.Undo(); },

                    // CanExecute if undo is available
                    parameter => ConfigManager.UndoManager?.CanUndo ?? false );
                return _undoCommand;
            }
        }
        #endregion

        #region DependencyProperties

        public static readonly DependencyProperty SomeOtherViewStateProperty =
            DependencyProperty.Register("SomeOtherViewState", typeof(string), typeof(SampleViewModel),
                new PropertyMetadata("hello", GenerateHeliosUndoForProperty));

        public static readonly DependencyProperty SomeViewStateProperty =
            DependencyProperty.Register("SomeViewState", typeof(int), typeof(SampleViewModel),
                new PropertyMetadata(1, OnSomeViewStateChanged));

        #endregion
    }
}