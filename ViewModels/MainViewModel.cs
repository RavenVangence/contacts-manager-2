using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using ContactsManager.Infrastructure;
using ContactsManager.Models;
using ContactsManager.Views;

namespace ContactsManager.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private Contact? _selectedContact;
        private string _searchText = string.Empty;
        private string _currentSortProperty = nameof(Contact.LastName);
        private ListSortDirection _currentSortDirection = ListSortDirection.Ascending;

        public ObservableCollection<Contact> Contacts { get; } = new();
        public ICollectionView ContactsView { get; }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText != value)
                {
                    _searchText = value;
                    OnPropertyChanged();
                    ContactsView.Refresh();
                }
            }
        }

        public Contact? SelectedContact
        {
            get => _selectedContact;
            set { if (_selectedContact != value) { _selectedContact = value; OnPropertyChanged(); } }
        }

        public ICommand AddCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand ImportCommand { get; }
        public ICommand ExportCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand SortByNameCommand { get; }
        public ICommand SortByEmailCommand { get; }
        public ICommand SortByCompanyCommand { get; }
        public ICommand SortByUsedCommand { get; }
        public ICommand SortByDateCommand { get; }

        public MainViewModel()
        {
            // Sample data with Used property
            Contacts.Add(new Contact { FirstName = "Ava", LastName = "Johnson", Email = "ava@example.com", Phone = "+1 (555) 010-1234", Company = "Northwind", Notes = "Key stakeholder", Used = true });
            Contacts.Add(new Contact { FirstName = "Ben", LastName = "Kim", Email = "ben.kim@example.com", Phone = "+1 (555) 010-5678", Company = "Adventure Works", Used = false });
            Contacts.Add(new Contact { FirstName = "Carla", LastName = "Nguyen", Email = "carla.n@example.com", Phone = "+1 (555) 010-9999", Company = "Fabrikam", Notes = "Prefers email", Used = true });

            ContactsView = CollectionViewSource.GetDefaultView(Contacts);
            ContactsView.SortDescriptions.Add(new SortDescription(_currentSortProperty, _currentSortDirection));
            ContactsView.Filter = FilterContact;

            AddCommand = new RelayCommand(AddContact);
            DeleteCommand = new RelayCommand(_ => DeleteSelected(), _ => SelectedContact != null);
            SaveCommand = new RelayCommand(_ => SaveSelected(), _ => SelectedContact != null);
            ImportCommand = new RelayCommand(ImportContacts);
            ExportCommand = new RelayCommand(ExportContacts);
            EditCommand = new RelayCommand(_ => EditSelected(), _ => SelectedContact != null);
            SortByNameCommand = new RelayCommand(() => SortBy(nameof(Contact.LastName)));
            SortByEmailCommand = new RelayCommand(() => SortBy(nameof(Contact.Email)));
            SortByCompanyCommand = new RelayCommand(() => SortBy(nameof(Contact.Company)));
            SortByUsedCommand = new RelayCommand(() => SortBy(nameof(Contact.Used)));
            SortByDateCommand = new RelayCommand(() => SortBy(nameof(Contact.ModifiedDate)));
        }

        private bool FilterContact(object obj)
        {
            if (obj is not Contact c) return false;
            if (string.IsNullOrWhiteSpace(SearchText)) return true;
            var q = SearchText.Trim();
            return (c.FullName?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false)
                || (c.Email?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false)
                || (c.Phone?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false)
                || (c.Company?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false);
        }

        private void AddContact()
        {
            var updateWindow = new UpdateContactWindow();
            if (updateWindow.ShowDialog() == true)
            {
                Contacts.Add(updateWindow.Contact);
                SelectedContact = updateWindow.Contact;
                ContactsView.Refresh();
            }
        }

        private void DeleteSelected()
        {
            if (SelectedContact is null) return;
            var toRemove = SelectedContact;
            SelectedContact = null;
            Contacts.Remove(toRemove);
            ContactsView.Refresh();
        }

        private void SaveSelected()
        {
            // Placeholder for persistence. For now just ensure view refresh.
            ContactsView.Refresh();
        }

        private void ImportContacts()
        {
            // Placeholder for Excel import functionality
            // This would open a file dialog and read Excel data using ClosedXML
            // For now, just add a sample imported contact
            var importedContact = new Contact
            {
                FirstName = "Imported",
                LastName = "Contact",
                Email = "imported@example.com",
                Phone = "+1 (555) 000-0000",
                Company = "Import Corp"
            };
            Contacts.Add(importedContact);
            ContactsView.Refresh();
        }

        private void ExportContacts()
        {
            // Placeholder for Excel export functionality
            // This would create an Excel file using ClosedXML with all contacts
            // Columns: Name, Surname, Phone Number, Used, Created Date, Modified Date
        }

        private void EditSelected()
        {
            if (SelectedContact == null) return;

            var updateWindow = new UpdateContactWindow(SelectedContact);
            updateWindow.ShowDialog();
            ContactsView.Refresh();
        }

        private void SortBy(string propertyName)
        {
            if (_currentSortProperty == propertyName)
            {
                // Toggle sort direction if same property
                _currentSortDirection = _currentSortDirection == ListSortDirection.Ascending
                    ? ListSortDirection.Descending
                    : ListSortDirection.Ascending;
            }
            else
            {
                _currentSortProperty = propertyName;
                _currentSortDirection = ListSortDirection.Ascending;
            }

            ContactsView.SortDescriptions.Clear();
            ContactsView.SortDescriptions.Add(new SortDescription(_currentSortProperty, _currentSortDirection));
            ContactsView.Refresh();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
