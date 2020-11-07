using System.Windows;

namespace GadrocsWorkshop.Helios.ProfileEditor
{
    /// <summary>
    /// Modal dialog window for configuration of Profile Editor Global Options
    /// </summary>
    public partial class GlobalOptionsWindow
    {
        public GlobalOptionsWindow()
        {
            InitializeComponent();
        }

        private void OK_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
