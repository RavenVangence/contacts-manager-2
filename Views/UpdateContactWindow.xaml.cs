using System.Windows;
using ContactsManager.Models;

namespace ContactsManager.Views
{
    public partial class UpdateContactWindow : Window
    {
        public Contact Contact { get; private set; }
        public bool IsNewContact { get; private set; }

        public UpdateContactWindow(Contact? contact = null)
        {
            InitializeComponent();

            if (contact != null)
            {
                Contact = contact;
                IsNewContact = false;
                Title = "Update Contact";
            }
            else
            {
                Contact = new Contact();
                IsNewContact = true;
                Title = "Add New Contact";
            }

            DataContext = Contact;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
