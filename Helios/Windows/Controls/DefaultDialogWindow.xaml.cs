// Copyright 2020 Ammo Goettsch
// 
// Helios is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// Helios is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Windows;
using System.Windows.Input;

namespace GadrocsWorkshop.Helios.Windows.Controls
{
    /// <summary>
    /// The generic container window for showing a dialog via a show dialog command, such as ShowModalCommand
    /// </summary>
    public partial class DefaultDialogWindow : DialogWindow
    {
        public DefaultDialogWindow()
        {
            InitializeComponent();
        }

        private void CloseWindow_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void CloseWindow_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Close();
        }

        public void ShowModal(object sender, ExecutedRoutedEventArgs e)
        {
            // crash if incorrect parameter type
            ShowModalParameter parameter = (ShowModalParameter)e.Parameter;

            // this is the source of the event, and we will resolve DataTemplate from their position
            FrameworkElement host = (FrameworkElement)e.OriginalSource;

            // resolve the data template
            DataTemplate template = parameter.DataTemplate ?? (DataTemplate)host.TryFindResource(new DataTemplateKey(parameter.Content.GetType()));

            // display the dialog appropriate to the content
            ContentTemplate = template;
            Content = parameter.Content;
            ShowDialog();
        }

        /// <summary>
        /// called when this modal dialog tries to open another
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DialogShowModal_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        /// <summary>
        /// called when this modal dialog tries to open another
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DialogShowModal_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            new DefaultDialogWindow().ShowModal(sender, e);
        }
    }
}