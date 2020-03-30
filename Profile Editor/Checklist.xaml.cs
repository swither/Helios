using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GadrocsWorkshop.Helios.ProfileEditor
{
    /// <summary>
    /// Interaction logic for Checklist.xaml
    /// </summary>
    public partial class Checklist : Grid
    {
        #region Commands
        public static RoutedUICommand GoThereCommand { get; } = new RoutedUICommand("Opens an associated editor.", "GoThere", typeof(Checklist));
        #endregion

        public Checklist()
        {
            DataContext = this;
            InitializeComponent();
            Sections = new ObservableCollection<ViewModel.ChecklistSection>();
        }

        public void Load(HeliosProfile profile)
        {
            Sections.Clear();
            foreach (HeliosInterface heliosInterface in profile.Interfaces)
            {
                if (ViewModel.ChecklistSection.TryManage(heliosInterface, out ViewModel.ChecklistSection section))
                {
                    Sections.Add(section);
                }
            }
            PerformChecks();

            // sign up for interface changes add/remove
            profile.Interfaces.CollectionChanged += Profile_InterfacesChanged;
        }

        private void Profile_InterfacesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                List<ViewModel.ChecklistSection> remove = new List<ViewModel.ChecklistSection>();
                foreach (HeliosInterface heliosInterface in e.OldItems)
                {
                    foreach (ViewModel.ChecklistSection section in Sections)
                    {
                        if (section.Interface == heliosInterface)
                        {
                            remove.Add(section);
                        }
                    }
                }
                // now that we are done iterating this collection, remove all the marked items
                foreach (ViewModel.ChecklistSection section in remove)
                {
                    Sections.Remove(section);
                    section.Dispose();
                }
            }

            if (e.NewItems != null)
            {
                foreach (HeliosInterface heliosInterface in e.NewItems)
                {
                    if (ViewModel.ChecklistSection.TryManage(heliosInterface, out ViewModel.ChecklistSection section))
                    {
                        Sections.Add(section);
                    }
                }
            }

            PerformChecks();
        }

        private void PerformChecks()
        {
            foreach (ViewModel.ChecklistSection section in Sections)
            {
                section.PerformCheck(DisplayThreshold);
            }
        }

        public StatusReportItem.SeverityCode DisplayThreshold { get; private set; } = StatusReportItem.SeverityCode.Info;
        public ObservableCollection<ViewModel.ChecklistSection> Sections
        {
            get { return (ObservableCollection<ViewModel.ChecklistSection>)GetValue(SectionsProperty); }
            set { SetValue(SectionsProperty, value); }
        }
        public static readonly DependencyProperty SectionsProperty =
            DependencyProperty.Register("Sections", typeof(ObservableCollection<ViewModel.ChecklistSection>), typeof(Checklist), new PropertyMetadata(null));

        private void Reload_Click(object sender, RoutedEventArgs e)
        {
            PerformChecks();
        }

        private void Share_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
