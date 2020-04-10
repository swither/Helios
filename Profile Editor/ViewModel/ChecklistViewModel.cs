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

using System.Windows;

namespace GadrocsWorkshop.Helios.ProfileEditor.ViewModel
{
    internal class ChecklistViewModel : DependencyObject
    {
        public InterfaceStatusScanner Data { get; }
        public ViewModelCollection<InterfaceStatus, ChecklistSection> Items { get; }

        public ChecklistViewModel(InterfaceStatusScanner model)
        {
            Data = model;
            Items = new ViewModelCollection<InterfaceStatus, ChecklistSection>(model.InterfaceStatuses);
            Items.CollectionChanged += Items_CollectionChanged;
        }

        private void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
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