using net.derammo.Helios.SampleProgram.Sample;
using System.ComponentModel;
using System.Windows;
using GadrocsWorkshop.Helios;
using GadrocsWorkshop.Helios.ComponentModel;

namespace net.derammo.Helios.SampleProgram
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow: Window
    {
        public MainWindow()
        {
            // create some sample model we have
            SampleModel sampleModel = new SampleModel();
            sampleModel.PropertyChanged += SampleModel_PropertyChanged;

            // we explicitly create the view model for now instead of automatically selecting it
            SampleData = new SampleViewModel(sampleModel);

            // we are our own data context, because our child will just bind to "SampleData"
            DataContext = this;

            InitializeComponent();
        }

        private void SampleModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // this is what Profile Editor does for us, but we aren't in the Profile Editor,
            // so we implement it here
            if (e is PropertyNotificationEventArgs args)
            {
                ConfigManager.UndoManager?.AddPropertyChange(sender, args);
            }
        }

        public SampleViewModel SampleData { get; }
    }
}
