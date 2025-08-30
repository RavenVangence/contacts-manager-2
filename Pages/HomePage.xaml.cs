using System.Windows;
using System.Windows.Controls;

namespace ContactsManager.Pages
{
    /// <summary>
    /// Interaction logic for HomePage.xaml
    /// </summary>
    public partial class HomePage : Page
    {
        public HomePage()
        {
            InitializeComponent();
        }

        private void GoToContacts_Click(object sender, RoutedEventArgs e)
        {
            // Navigate to contacts page using the main window's navigation method
            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                mainWindow.NavigateToContacts();
            }
        }
    }
}
