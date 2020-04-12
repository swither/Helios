//  Copyright 2014 Craig Courtney
//    
//  Helios is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  Helios is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Newtonsoft.Json;

namespace GadrocsWorkshop.Helios.Windows.ViewModel
{
    public class ChecklistViewModel : HeliosViewModel<InterfaceStatusScanner>
    {
        public static RoutedUICommand GoThereCommand { get; } =
            new RoutedUICommand("Opens an associated editor.", "GoThere", typeof(ChecklistViewModel));

        /// <summary>
        /// backing field for ReloadCommand
        /// </summary>
        private ICommand _reloadCommand;

        public ChecklistViewModel(InterfaceStatusScanner data)
            : base(data)
        {
            Items = new ViewModelCollection<InterfaceStatus, ChecklistSection>(data.InterfaceStatuses);
            Items.CollectionChanged += Items_CollectionChanged;
        }

        private void Items_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // configure each new item
            if (e.NewItems != null)
            {
                foreach (ChecklistSection section in e.NewItems)
                {
                    section.Initialize(DisplayThreshold);
                }
            }
        }

        private void OnDisplayThresholdChanged()
        {
            // broadcast to all sections
            foreach (ChecklistSection section in Items)
            {
                section.ChangeDisplayThreshold(DisplayThreshold);
            }
        }

        public ViewModelCollection<InterfaceStatus, ChecklistSection> Items { get; }

        /// <summary>
        /// backing field for property ShareCommand, contains
        /// handler for Share button
        /// </summary>
        private ICommand _shareCommand;

        /// <summary>
        /// handler for Share button
        /// </summary>
        public ICommand ShareCommand
        {
            get
            {
                _shareCommand = _shareCommand ?? new RelayCommand(parameter =>
                {
                    // execute a modal dialog
                    Dialog.ShowModalCommand.Execute(
                        new ShowModalParameter
                        {
                            Content = new ShareInterfaceStatus(Data.InterfaceStatuses)
                        }, 
                        parameter as IInputElement);
                });
                return _shareCommand;
            }
        }

        public ICommand ReloadCommand
        {
            get
            {
                _reloadCommand = _reloadCommand ?? new RelayCommand(parameter => { Data.PerformChecks(); });
                return _reloadCommand;
            }
        }

        public StatusReportItem.SeverityCode DisplayThreshold
        {
            get => (StatusReportItem.SeverityCode) GetValue(DisplayThresholdProperty);
            set => SetValue(DisplayThresholdProperty, value);
        }

        public static readonly DependencyProperty DisplayThresholdProperty =
            DependencyProperty.Register("DisplayThreshold", typeof(StatusReportItem.SeverityCode),
                typeof(ChecklistViewModel), new PropertyMetadata(
                    StatusReportItem.SeverityCode.Warning,
                    (d, e) => ((ChecklistViewModel) d).OnDisplayThresholdChanged()));
    }
}