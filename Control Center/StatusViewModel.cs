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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace GadrocsWorkshop.Helios.ControlCenter
{
    public class StatusViewModel: DependencyObject
    {
        private Queue<StatusReportItem> _items = new Queue<StatusReportItem>();
        private LinkedList<StatusReportItem> _shown = new LinkedList<StatusReportItem>();
        private int _windowBase = 0;
        private int _windowSize = 200;
        private int _capacity = 200;

        public class StatusTemplateSelector : DataTemplateSelector
        {
            public override DataTemplate SelectTemplate(object item, DependencyObject container)
            {
                StatusReportItem listItem = item as StatusReportItem;
                FrameworkElement element = container as FrameworkElement;
                if (listItem == null)
                {
                    return null;
                }
                return element.FindResource(listItem.Severity.ToString()) as DataTemplate;
            }
        }
    
        public StatusViewModel()
        {
            // don't use default generated dependency property, we need our own copy
            Items = new ObservableCollection<StatusReportItem>();
        }

        public void AddItem(StatusReportItem item)
        {
            _items.Enqueue(item);
            if (_windowBase + _windowSize >= _items.Count)
            {
                // new item is visible
                _shown.AddLast(item);
                Items.Add(item);

                // may have scrolled something off
                while (_shown.Count > _windowSize)
                {
                    Items.Remove(_shown.First.Value);
                    _shown.RemoveFirst();
                }
            }

            // finally, check if we exceeded our capacity
            while (_items.Count > _capacity)
            {
                StatusReportItem discard = _items.Dequeue();
                if (_windowBase == 0)
                {
                    Items.Remove(discard);
                    _shown.RemoveFirst();
                }
                _windowBase--;
            }
        }

        public void Clear()
        {
            _items.Clear();
            _shown.Clear();
            _windowBase = 0;
            Items = new ObservableCollection<StatusReportItem>();
        }

        public StatusTemplateSelector TemplateSelector { get; } = new StatusTemplateSelector();

        public ObservableCollection<StatusReportItem> Items
        {
            get { return (ObservableCollection<StatusReportItem>)GetValue(ItemsProperty); }
            set { SetValue(ItemsProperty, value); }
        }
        public static readonly DependencyProperty ItemsProperty =
            DependencyProperty.Register("observableCollection", typeof(ObservableCollection<StatusReportItem>), typeof(StatusViewModel), new PropertyMetadata(null));

    }
}