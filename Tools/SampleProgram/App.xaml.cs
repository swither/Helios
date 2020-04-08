using System.ComponentModel;
using System.Windows;
using GadrocsWorkshop.Helios;
using GadrocsWorkshop.Helios.ComponentModel;
using net.derammo.Helios.SampleProgram.Sample;

namespace net.derammo.Helios.SampleProgram
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            HeliosInit.Initialize("d:\\temp\\SampleProgram", "sample.log", LogLevel.Error);
        }
    }
}