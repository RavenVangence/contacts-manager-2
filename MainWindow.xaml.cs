using System.Linq;
using System.Windows;
// ...existing using directives removed for brevity...

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
            Loaded += (_, __) =>
            {
                if (DataContext is ViewModels.MainViewModel vm && vm.Contacts.Any())
                {
                    vm.SelectedContact = vm.Contacts.First();
                }
            };
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}