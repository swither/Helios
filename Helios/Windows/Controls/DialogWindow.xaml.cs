using System.Windows;
using System.Windows.Input;

namespace GadrocsWorkshop.Helios.Windows.Controls
{
    /// <summary>
    /// The generic container window for showing a dialog via a show dialog command, such as ShowModalCommand
    /// </summary>
    public partial class DialogWindow : Window
    {
        public DialogWindow()
        {
            InitializeComponent();
        }

        private void CloseWindow_CanExec(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void CloseWindow_Exec(object sender, ExecutedRoutedEventArgs e)
        {
            Close();
        }
    }
}
