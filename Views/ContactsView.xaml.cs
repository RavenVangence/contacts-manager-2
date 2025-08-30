using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ContactsManager.ViewModels;

namespace ContactsManager.Views
{
    /// <summary>
    /// Interaction logic for ContactsView.xaml
    /// </summary>
    public partial class ContactsView : UserControl
    {
        public ContactsView()
        {
            InitializeComponent();

            Loaded += (_, __) =>
            {
                if (DataContext is MainViewModel vm)
                {
                    // Set scroll compensation reference
                    vm.ScrollViewer = MainScrollViewer;
                }
            };
        }

        private void ListBox_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            // Prevent ListBox from handling mouse wheel events
            e.Handled = true;

            // Redirect mouse wheel events to the parent ScrollViewer
            var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
            {
                RoutedEvent = UIElement.MouseWheelEvent,
                Source = sender
            };
            MainScrollViewer.RaiseEvent(eventArg);
        }
    }
}
