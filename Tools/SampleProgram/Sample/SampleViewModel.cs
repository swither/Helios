using System.Windows;
using GadrocsWorkshop.Helios.Util;
using GadrocsWorkshop.Helios.Windows;

namespace net.derammo.Helios.SampleProgram.Sample
{
    public class SampleViewModel : HeliosViewModel<SampleModel>
    {
        public SampleViewModel(SampleModel model)
            : base(model)
        {
            // no code
        }

        private static void OnSomeViewStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // do some stuff with the change, including maybe calling GenerateHeliosUndoForProperty if we want to
            // make this undo-able

            // in this example, we will change multiple things in the (data) model with one UI interaction, so we batch
            // their undo events
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

        public static readonly DependencyProperty SomeOtherViewStateProperty =
            DependencyProperty.Register("SomeOtherViewState", typeof(string), typeof(SampleViewModel),
                new PropertyMetadata("hello", GenerateHeliosUndoForProperty));

        /// <summary>
        /// some other view state that calls some user-supplied logic if changed
        /// </summary>
        public int SomeViewState
        {
            get => (int) GetValue(SomeViewStateProperty);
            set => SetValue(SomeViewStateProperty, value);
        }

        public static readonly DependencyProperty SomeViewStateProperty =
            DependencyProperty.Register("SomeViewState", typeof(int), typeof(SampleViewModel),
                new PropertyMetadata(1, OnSomeViewStateChanged));

        #endregion
    }
}