using System.Windows.Input;

namespace GadrocsWorkshop.Helios.Windows
{
    public class Dialog
    {
        public static RoutedUICommand ShowModalCommand { get; } =
            new RoutedUICommand("Opens a modal dialog.", "ShowModal", typeof(Dialog));
    }
}
