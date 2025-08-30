using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.ComponentModel;

namespace ContactsManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Handle window closing event
            Closing += MainWindow_Closing;

            Loaded += (_, __) =>
            {
                if (DataContext is ViewModels.MainViewModel vm)
                {
                    // Set scroll compensation reference
                    vm.ScrollViewer = MainScrollViewer;

                    if (vm.Contacts.Any())
                    {
                        vm.SelectedContact = vm.Contacts.First();
                    }
                }
            };
        }

        private void MainWindow_Closing(object? sender, CancelEventArgs e)
        {
            if (DataContext is ViewModels.MainViewModel vm)
            {
                if (!vm.CanClose())
                {
                    e.Cancel = true; // Cancel the closing
                }
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void OpenContactsManager_Click(object sender, RoutedEventArgs e)
        {
            // Since this is already the Contacts Manager application, 
            // we'll bring the current window to front and focus it
            this.Activate();
            this.Focus();
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