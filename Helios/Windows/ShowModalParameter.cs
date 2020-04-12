using System.Windows;

namespace GadrocsWorkshop.Helios.Windows
{
    /// <summary>
    /// the Dialog.ShowModal command requires an instance of this to be the parameter
    /// </summary>
    public class ShowModalParameter: FrameworkElement
    {
        /// <summary>
        /// the content which will be displayed in the dialog
        /// </summary>
        public object Content
        {
            get { return (object)GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }
        public static readonly DependencyProperty ContentProperty =
            DependencyProperty.Register("Content", typeof(object), typeof(ShowModalParameter), new PropertyMetadata(null));

        /// <summary>
        /// data template to use, or null if data template should be
        /// resolved via search for a DataTemplate resource from the
        /// point of origin of the ShowModal event, based on the 
        /// type of Content
        /// </summary>
        public DataTemplate DataTemplate
        {
            get { return (DataTemplate)GetValue(DataTemplateProperty); }
            set { SetValue(DataTemplateProperty, value); }
        }
        public static readonly DependencyProperty DataTemplateProperty =
            DependencyProperty.Register("DataTemplate", typeof(DataTemplate), typeof(ShowModalParameter), new PropertyMetadata(null));
    }
}
