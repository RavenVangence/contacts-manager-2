using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.ComponentModel;
using ContactsManager.ViewModels;
using ContactsManager.Pages;

namespace ContactsManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainWindowViewModel ViewModel => (MainWindowViewModel)DataContext;
        private ContactsPage? _contactsPageInstance;

        public MainWindow()
        {
            InitializeComponent();

            // Set the DataContext to the new MainWindowViewModel
            DataContext = new MainWindowViewModel();

            // Handle window closing event
            Closing += MainWindow_Closing;

            // Navigate to home page on startup
            Loaded += (_, __) =>
            {
                NavigateToHome();
            };
        }

        public void NavigateToHome()
        {
            ViewModel.NavigateToHome();
            MainFrame.Navigate(new HomePage());
        }

        public void NavigateToContacts()
        {
            ViewModel.NavigateToContacts();

            // Create ContactsPage instance only when needed (lazy loading)
            if (_contactsPageInstance == null)
            {
                _contactsPageInstance = new ContactsPage();
            }

            MainFrame.Navigate(_contactsPageInstance);
        }

        private void MainWindow_Closing(object? sender, CancelEventArgs e)
        {
            // Check if we can close based on the contacts page state (if it exists and has been loaded)
            if (_contactsPageInstance?.DataContext is MainViewModel contactsVM && !contactsVM.CanClose())
            {
                e.Cancel = true; // Cancel the closing
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Import_Click(object sender, RoutedEventArgs e)
        {
            NavigateToContacts();
            if (_contactsPageInstance?.DataContext is MainViewModel contactsVM)
            {
                contactsVM.ImportCommand.Execute(null);
            }
        }

        private void Export_Click(object sender, RoutedEventArgs e)
        {
            NavigateToContacts();
            if (_contactsPageInstance?.DataContext is MainViewModel contactsVM)
            {
                contactsVM.ExportCommand.Execute(null);
            }
        }

        private void AddContact_Click(object sender, RoutedEventArgs e)
        {
            NavigateToContacts();
            if (_contactsPageInstance?.DataContext is MainViewModel contactsVM)
            {
                contactsVM.AddCommand.Execute(null);
            }
        }

        private void EditContact_Click(object sender, RoutedEventArgs e)
        {
            NavigateToContacts();
            if (_contactsPageInstance?.DataContext is MainViewModel contactsVM)
            {
                contactsVM.EditCommand.Execute(null);
            }
        }

        private void DeleteContact_Click(object sender, RoutedEventArgs e)
        {
            NavigateToContacts();
            if (_contactsPageInstance?.DataContext is MainViewModel contactsVM)
            {
                contactsVM.DeleteCommand.Execute(null);
            }
        }

        private void SortByName_Click(object sender, RoutedEventArgs e)
        {
            NavigateToContacts();
            if (_contactsPageInstance?.DataContext is MainViewModel contactsVM)
            {
                contactsVM.SortByNameCommand.Execute(null);
            }
        }

        private void SortByUsed_Click(object sender, RoutedEventArgs e)
        {
            NavigateToContacts();
            if (_contactsPageInstance?.DataContext is MainViewModel contactsVM)
            {
                contactsVM.SortByUsedCommand.Execute(null);
            }
        }

        private void OpenContactsManager_Click(object sender, RoutedEventArgs e)
        {
            NavigateToContacts();
        }

        private void ContactsNavButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateToContacts();
        }

        private void HomeNavButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateToHome();
        }
    }
}