using System;
using System.ComponentModel;
using System.Windows;

namespace GadrocsWorkshop.Helios.Util
{
    public class DesignTimeEditorResources: ResourceDictionary
    {
        public DesignTimeEditorResources()
        {
            if ((bool)DesignerProperties.IsInDesignModeProperty.GetMetadata(typeof(DependencyObject)).DefaultValue)
            {
                Source = new Uri("pack://application:,,,/Helios;component/Styles/Editor.xaml");
            }
        }
    }
}
