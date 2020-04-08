using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Input;
using GadrocsWorkshop.Helios;
using GadrocsWorkshop.Helios.ComponentModel;

namespace net.derammo.Helios.SampleProgram.Sample
{
    /// <summary>
    /// Interaction logic for SampleView.xaml
    /// </summary>
    public partial class SampleView : Grid
    {
        public SampleView()
        {
            InitializeComponent();
            SampleModel sampleModel = new SampleModel();
            sampleModel.PropertyChanged += SampleModel_PropertyChanged;
            DataContext = new SampleViewModel(sampleModel);
        }

        private void SampleModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // this is what profile editor does
            PropertyNotificationEventArgs args = e as PropertyNotificationEventArgs;
            if (args != null)
            {
                ConfigManager.UndoManager.AddPropertyChange(sender, args);
            }
        }

        private void Undo_Executed(object target, ExecutedRoutedEventArgs e)
        {
            ConfigManager.UndoManager.Undo();
        }
    }
}