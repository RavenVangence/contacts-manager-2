using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.ComponentModel;
using ContactsManager.ViewModels;

namespace ContactsManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainWindowViewModel ViewModel => (MainWindowViewModel)DataContext;

        public MainWindow()
        {
            InitializeComponent();

            // Set the DataContext to the new MainWindowViewModel
            DataContext = new MainWindowViewModel();

            // Handle window closing event
            Closing += MainWindow_Closing;

            Loaded += (_, __) =>
            {
                // Preload contacts data when the window loads, but stay on Home page
                if (ViewModel.ContactsViewModel.Contacts.Any())
                {
                    ViewModel.ContactsViewModel.SelectedContact = ViewModel.ContactsViewModel.Contacts.First();
                }

                // Ensure we stay on the Home page after loading
                ViewModel.NavigateToHome();
            };
        }

        private void MainWindow_Closing(object? sender, CancelEventArgs e)
        {
            if (!ViewModel.CanClose())
            {
                e.Cancel = true; // Cancel the closing
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void OpenContactsManager_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.NavigateToContacts();
        }

        private void ContactsNavButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.NavigateToContacts();
        }

        private void HomeNavButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.NavigateToHome();
        }
    }
}