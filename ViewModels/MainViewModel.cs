using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Controls;
using Microsoft.Win32;
using ClosedXML.Excel;
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
        private bool _isEditMode = false;
        private Contact? _contactBeforeEdit;

        public ObservableCollection<Contact> Contacts { get; } = new();
        public ICollectionView ContactsView { get; }
        public ScrollViewer? ScrollViewer { get; set; }

        public bool HasContacts => Contacts.Any();

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
            set
            {
                if (_selectedContact != value)
                {
                    _selectedContact = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsEditMode
        {
            get => _isEditMode;
            set
            {
                if (_isEditMode != value)
                {
                    _isEditMode = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand AddCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand ImportCommand { get; }
        public ICommand ExportCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand SortByNameCommand { get; }
        public ICommand SortBySurnameCommand { get; }
        public ICommand SortByUsedCommand { get; }
        public ICommand ContactDoubleClickCommand { get; }
        public ICommand CancelEditCommand { get; }
        public ICommand ToggleUsedCommand { get; }

        public MainViewModel()
        {
            ContactsView = CollectionViewSource.GetDefaultView(Contacts);
            ContactsView.SortDescriptions.Add(new SortDescription(_currentSortProperty, _currentSortDirection));
            ContactsView.Filter = FilterContact;

            AddCommand = new RelayCommand(AddContact);
            DeleteCommand = new RelayCommand(param => DeleteSelected(param as Contact), _ => true);
            SaveCommand = new RelayCommand(_ => SaveSelected(), _ => SelectedContact != null);
            ImportCommand = new RelayCommand(ImportContacts);
            ExportCommand = new RelayCommand(ExportContacts);
            EditCommand = new RelayCommand(param => EditSelected(param as Contact), _ => true);
            SortByNameCommand = new RelayCommand(() => SortBy(nameof(Contact.FirstName)));
            SortBySurnameCommand = new RelayCommand(() => SortBy(nameof(Contact.LastName)));
            SortByUsedCommand = new RelayCommand(() => SortBy(nameof(Contact.Used)));
            ContactDoubleClickCommand = new RelayCommand(OnContactDoubleClick);
            CancelEditCommand = new RelayCommand(CancelEdit);
            ToggleUsedCommand = new RelayCommand(param => ToggleUsed(param as Contact), _ => true);
        }

        private bool FilterContact(object obj)
        {
            if (obj is not Contact c) return false;
            if (string.IsNullOrWhiteSpace(SearchText)) return true;
            var q = SearchText.Trim();
            return (c.FullName?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false)
                || (c.Phone?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false);
        }

        private void AddContact()
        {
            _contactBeforeEdit = null; // New contact
            var newContact = new Contact();
            Contacts.Insert(0, newContact);
            OnPropertyChanged(nameof(HasContacts));
            SelectedContact = newContact;
            IsEditMode = true;
        }

        private void DeleteSelected(Contact? contact)
        {
            if (contact is null) return;

            // Store current scroll position before deletion
            var currentScrollPosition = ScrollViewer?.VerticalOffset ?? 0;

            // Store current selection state before deletion
            var wasSelected = SelectedContact == contact;

            // Remove from collection first - this immediately updates UI
            Contacts.Remove(contact);

            // Only clear selection if the deleted contact was selected
            if (wasSelected)
            {
                SelectedContact = null;
                IsEditMode = false; // Hide edit mode when deleting
            }

            OnPropertyChanged(nameof(HasContacts));

            // Compensate for unwanted scroll behavior
            if (ScrollViewer != null)
            {
                // Scroll up slightly to compensate for the automatic scroll down
                var compensatedPosition = Math.Max(0, currentScrollPosition - 30);
                ScrollViewer.Dispatcher.BeginInvoke(() =>
                {
                    ScrollViewer.ScrollToVerticalOffset(compensatedPosition);
                });
            }
        }

        private void ToggleUsed(Contact? contact)
        {
            if (contact is null) return;
            contact.Used = !contact.Used;
            // No need to refresh the entire view, binding will handle the update
        }

        private void SaveSelected()
        {
            if (SelectedContact != null)
            {
                if (string.IsNullOrWhiteSpace(SelectedContact.FullName.Trim()))
                {
                    MessageBox.Show("Contact name cannot be empty.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (string.IsNullOrWhiteSpace(SelectedContact.Phone))
                {
                    MessageBox.Show("Phone number cannot be empty.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            // Placeholder for persistence. For now just ensure view refresh.
            IsEditMode = false;
            SelectedContact = null;
            _contactBeforeEdit = null;
            // Removed ContactsView.Refresh() - binding updates automatically handle changes
        }

        private void ImportContacts()
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "Import Contacts from Excel",
                Filter = "Excel files (*.xlsx;*.xls)|*.xlsx;*.xls|All files (*.*)|*.*",
                DefaultExt = "xlsx"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    using var workbook = new XLWorkbook(openFileDialog.FileName);
                    var worksheet = workbook.Worksheet(1); // Use first worksheet

                    // Assume first row contains headers
                    var rows = worksheet.RowsUsed().Skip(1); // Skip header row

                    foreach (var row in rows)
                    {
                        // Expected columns: FirstName, LastName, Phone, Used
                        var contact = new Contact
                        {
                            FirstName = row.Cell(1).GetValue<string>(),
                            LastName = row.Cell(2).GetValue<string>(),
                            Phone = row.Cell(3).GetValue<string>(),
                            Used = row.Cell(4).TryGetValue(out bool used) && used
                        };

                        // Only add if we have at least a name
                        if (!string.IsNullOrWhiteSpace(contact.FirstName) || !string.IsNullOrWhiteSpace(contact.LastName))
                        {
                            Contacts.Add(contact);
                        }
                    }

                    ContactsView.Refresh();
                    OnPropertyChanged(nameof(HasContacts));
                    MessageBox.Show($"Successfully imported {rows.Count()} contacts.", "Import Complete",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error importing contacts: {ex.Message}", "Import Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ExportContacts()
        {
            if (!Contacts.Any())
            {
                MessageBox.Show("No contacts to export.", "Export", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var saveFileDialog = new SaveFileDialog
            {
                Title = "Export Contacts to Excel",
                Filter = "Excel files (*.xlsx)|*.xlsx|All files (*.*)|*.*",
                DefaultExt = "xlsx",
                FileName = $"Contacts_{DateTime.Now:yyyy-MM-dd}.xlsx"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    using var workbook = new XLWorkbook();
                    var worksheet = workbook.Worksheets.Add("Contacts");

                    // Headers
                    worksheet.Cell(1, 1).Value = "First Name";
                    worksheet.Cell(1, 2).Value = "Last Name";
                    worksheet.Cell(1, 3).Value = "Phone";
                    worksheet.Cell(1, 4).Value = "Used";

                    // Format headers
                    var headerRange = worksheet.Range(1, 1, 1, 4);
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

                    // Data
                    var row = 2;
                    foreach (var contact in Contacts)
                    {
                        worksheet.Cell(row, 1).Value = contact.FirstName;
                        worksheet.Cell(row, 2).Value = contact.LastName;
                        worksheet.Cell(row, 3).Value = contact.Phone;
                        worksheet.Cell(row, 4).Value = contact.Used;
                        row++;
                    }

                    // Auto-fit columns
                    worksheet.Columns().AdjustToContents();

                    workbook.SaveAs(saveFileDialog.FileName);
                    MessageBox.Show($"Successfully exported {Contacts.Count} contacts to {Path.GetFileName(saveFileDialog.FileName)}",
                        "Export Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error exporting contacts: {ex.Message}", "Export Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void EditSelected(Contact? contact)
        {
            if (contact == null) return;

            _contactBeforeEdit = new Contact
            {
                FirstName = contact.FirstName,
                LastName = contact.LastName,
                Phone = contact.Phone,
                Used = contact.Used
            };

            SelectedContact = contact;
            IsEditMode = true;
        }

        private void OnContactDoubleClick()
        {
            if (SelectedContact != null)
            {
                IsEditMode = true;
            }
        }

        private void CancelEdit()
        {
            if (SelectedContact != null)
            {
                if (_contactBeforeEdit == null) // New contact was being added
                {
                    Contacts.Remove(SelectedContact);
                    OnPropertyChanged(nameof(HasContacts));
                }
                else // Existing contact was being edited
                {
                    // Restore original values
                    SelectedContact.FirstName = _contactBeforeEdit.FirstName;
                    SelectedContact.LastName = _contactBeforeEdit.LastName;
                    SelectedContact.Phone = _contactBeforeEdit.Phone;
                    SelectedContact.Used = _contactBeforeEdit.Used;
                }
            }

            IsEditMode = false;
            SelectedContact = null;
            _contactBeforeEdit = null;
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
            // Removed ContactsView.Refresh() - sort descriptions automatically trigger update
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
