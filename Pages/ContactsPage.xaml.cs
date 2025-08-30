using System.Windows.Controls;
using ContactsManager.ViewModels;

namespace ContactsManager.Pages
{
    /// <summary>
    /// Interaction logic for ContactsPage.xaml
    /// </summary>
    public partial class ContactsPage : Page
    {
        public ContactsPage()
        {
            InitializeComponent();
            // Set the DataContext to the contacts view model only when this page is loaded
            DataContext = new MainViewModel();
            ContactsView.DataContext = DataContext;
        }
    }
}
