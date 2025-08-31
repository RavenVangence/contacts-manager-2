using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
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
        private string _currentSortProperty = nameof(Contact.FirstName);
        private ListSortDirection _currentSortDirection = ListSortDirection.Ascending;
        private bool? _usedFilter = null;
        private bool _isEditMode = false;
        private bool _isLoading = false;
        private bool _hasUnsavedChanges = false;
        private int _originalContactCount = 0;
        private Dictionary<string, Contact> _originalContactStates = new();
        private Contact? _contactBeforeEdit;

        public ObservableCollection<Contact> Contacts { get; } = new();
        public ICollectionView ContactsView { get; }
        public ScrollViewer? ScrollViewer { get; set; }

        public bool HasContacts => Contacts.Any();
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (_isLoading != value)
                {
                    _isLoading = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(ShowLoadingMessage));
                    OnPropertyChanged(nameof(ShowMainContent));
                }
            }
        }

        public bool ShowLoadingMessage => IsLoading;
        public bool ShowMainContent => !IsLoading;

        public bool HasUnsavedChanges
        {
            get => _hasUnsavedChanges;
            set
            {
                if (_hasUnsavedChanges != value)
                {
                    _hasUnsavedChanges = value;
                    OnPropertyChanged();
                }
            }
        }

        public int TotalContacts => Contacts.Count;
        public int TotalUsed => Contacts.Count(c => c.Used);
        public int TotalUnused => Contacts.Count(c => !c.Used);

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

        public string CurrentSortProperty => _currentSortProperty;
        public ListSortDirection CurrentSortDirection => _currentSortDirection;

        public bool? UsedFilter
        {
            get => _usedFilter;
            set
            {
                if (_usedFilter != value)
                {
                    _usedFilter = value;
                    OnPropertyChanged();
                    ContactsView.Refresh();
                }
            }
        }

        public ICommand AddCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand PersistChangesCommand { get; }
        public ICommand ImportCommand { get; }
        public ICommand ExportCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand SortByNameCommand { get; }
        public ICommand SortBySurnameCommand { get; }
        public ICommand SortByUsedCommand { get; }
        public ICommand FilterByUsedCommand { get; }
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
            PersistChangesCommand = new RelayCommand(PersistChanges);
            ImportCommand = new RelayCommand(ImportContacts);
            ExportCommand = new RelayCommand(ExportContacts);
            EditCommand = new RelayCommand(param => EditSelected(param as Contact), _ => true);
            SortByNameCommand = new RelayCommand(() => SortBy(nameof(Contact.FirstName)));
            SortBySurnameCommand = new RelayCommand(() => SortBy(nameof(Contact.LastName)));
            SortByUsedCommand = new RelayCommand(() => SortBy(nameof(Contact.Used)));
            FilterByUsedCommand = new RelayCommand(ToggleUsedFilter);
            ContactDoubleClickCommand = new RelayCommand(OnContactDoubleClick);
            CancelEditCommand = new RelayCommand(CancelEdit);
            ToggleUsedCommand = new RelayCommand(param => ToggleUsed(param as Contact), _ => true);

            // Load data from database file on startup
            LoadDatabaseFile();
        }

        private void ToggleUsedFilter()
        {
            if (UsedFilter == null)
            {
                UsedFilter = true; // All -> Used
            }
            else if (UsedFilter == true)
            {
                UsedFilter = false; // Used -> Unused
            }
            else
            {
                UsedFilter = null; // Unused -> All
            }
        }

        private bool FilterContact(object obj)
        {
            if (obj is not Contact c) return false;

            bool matchesSearch = true;
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var q = SearchText.Trim();
                matchesSearch = (c.FullName?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false)
                             || (c.Phone?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false);
            }

            bool matchesUsedFilter = true;
            if (UsedFilter.HasValue)
            {
                matchesUsedFilter = c.Used == UsedFilter.Value;
            }

            return matchesSearch && matchesUsedFilter;
        }

        private void AddContact()
        {
            _contactBeforeEdit = null; // New contact
            var newContact = new Contact();
            Contacts.Insert(0, newContact);
            OnPropertyChanged(nameof(HasContacts));
            UpdateStatistics();
            SelectedContact = newContact;
            IsEditMode = true;
            HasUnsavedChanges = true;
        }

        private void DeleteSelected(Contact? contact)
        {
            if (contact is null) return;

            // Ask for confirmation before deleting
            var result = MessageBox.Show(
                $"Are you sure you want to delete '{contact.FullName}'?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

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
            UpdateStatistics();
            HasUnsavedChanges = true;

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
            UpdateStatistics();
            HasUnsavedChanges = true;
            // No need to refresh the entire view, binding will handle the update
        }

        private void UpdateStatistics()
        {
            OnPropertyChanged(nameof(TotalContacts));
            OnPropertyChanged(nameof(TotalUsed));
            OnPropertyChanged(nameof(TotalUnused));
        }

        private void SaveSelected()
        {
            if (SelectedContact != null)
            {
                // Determine if this is a new contact or an update
                bool isNewContact = _contactBeforeEdit == null;
                string successMessage = isNewContact
                    ? $"Contact '{SelectedContact.FullName}' has been added successfully!"
                    : $"Contact '{SelectedContact.FullName}' has been updated successfully!";

                // Show success message
                MessageBox.Show(successMessage, "Success", MessageBoxButton.OK, MessageBoxImage.Information);
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

                    int importedCount = 0;
                    int duplicateCount = 0;
                    int totalProcessed = 0;

                    foreach (var row in rows)
                    {
                        totalProcessed++;

                        // Expected columns: FirstName, LastName, Phone, Used
                        var contact = new Contact
                        {
                            FirstName = row.Cell(1).GetValue<string>()?.Trim() ?? string.Empty,
                            LastName = row.Cell(2).GetValue<string>()?.Trim() ?? string.Empty,
                            Phone = row.Cell(3).GetValue<string>()?.Trim() ?? string.Empty,
                            Used = row.Cell(4).TryGetValue(out bool used) && used
                        };

                        // Only process if we have at least a name or phone
                        if (!string.IsNullOrWhiteSpace(contact.FirstName) ||
                            !string.IsNullOrWhiteSpace(contact.LastName) ||
                            !string.IsNullOrWhiteSpace(contact.Phone))
                        {
                            // Check for duplicates based on name and phone combination
                            bool isDuplicate = Contacts.Any(existing =>
                                existing.FirstName?.Equals(contact.FirstName, StringComparison.OrdinalIgnoreCase) == true &&
                                existing.LastName?.Equals(contact.LastName, StringComparison.OrdinalIgnoreCase) == true &&
                                existing.Phone?.Equals(contact.Phone, StringComparison.OrdinalIgnoreCase) == true);

                            if (!isDuplicate)
                            {
                                Contacts.Add(contact);
                                importedCount++;
                            }
                            else
                            {
                                duplicateCount++;
                            }
                        }
                    }

                    // Sort contacts by first name, then last name after import
                    var sortedContacts = Contacts
                        .OrderBy(c => c.FirstName?.Trim() ?? string.Empty)
                        .ThenBy(c => c.LastName?.Trim() ?? string.Empty)
                        .ToList();

                    Contacts.Clear();
                    foreach (var contact in sortedContacts)
                    {
                        Contacts.Add(contact);
                    }

                    ContactsView.Refresh();
                    OnPropertyChanged(nameof(HasContacts));
                    UpdateStatistics();
                    HasUnsavedChanges = true; // Mark as having unsaved changes

                    // Show detailed import results
                    string message = $"Import completed!\n\n" +
                                   $"• {importedCount} contacts imported successfully\n" +
                                   $"• {duplicateCount} duplicates skipped\n" +
                                   $"• {totalProcessed} total rows processed\n\n";

                    MessageBox.Show(message, "Import Complete", MessageBoxButton.OK, MessageBoxImage.Information);
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
                    UpdateStatistics();
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

            OnPropertyChanged(nameof(CurrentSortProperty));
            OnPropertyChanged(nameof(CurrentSortDirection));

            ContactsView.SortDescriptions.Clear();
            ContactsView.SortDescriptions.Add(new SortDescription(_currentSortProperty, _currentSortDirection));
            // Removed ContactsView.Refresh() - sort descriptions automatically trigger update
        }

        private void LoadDatabaseFile()
        {
            IsLoading = true;

            Task.Run(() =>
            {
                try
                {
                    string databaseFilePath = Path.Combine(Environment.CurrentDirectory, "sa_contacts.xlsx");

                    if (!File.Exists(databaseFilePath))
                    {
                        // File doesn't exist, start with empty list
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            _originalContactCount = 0; // Track original count as 0 for new files
                            SaveOriginalContactStates(); // Save empty state
                            IsLoading = false;
                        });
                        return;
                    }

                    using var workbook = new XLWorkbook(databaseFilePath);
                    var worksheet = workbook.Worksheet(1);

                    // Check if we have any data at all
                    if (worksheet.LastRowUsed() == null)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            IsLoading = false;
                        });
                        return;
                    }

                    // Detect column structure
                    var headerRow = worksheet.Row(1);
                    var columnCount = worksheet.LastColumnUsed()?.ColumnNumber() ?? 0;

                    // Find column indices
                    int firstNameCol = 1, lastNameCol = 2, phoneCol = 3, usedCol = 4;
                    bool hasUsedColumn = columnCount >= 4;
                    bool needsUsedColumn = false;

                    // If there's no Used column, we'll need to add it
                    if (!hasUsedColumn)
                    {
                        needsUsedColumn = true;
                        usedCol = columnCount + 1;

                        // Add the Used column header
                        worksheet.Cell(1, usedCol).Value = "Used";
                        worksheet.Cell(1, usedCol).Style.Font.Bold = true;
                    }

                    var loadedContacts = new List<Contact>();
                    var rows = worksheet.RowsUsed().Skip(1); // Skip header row

                    foreach (var row in rows)
                    {
                        try
                        {
                            var contact = new Contact
                            {
                                FirstName = GetCellStringValue(row, firstNameCol),
                                LastName = GetCellStringValue(row, lastNameCol),
                                Phone = GetCellStringValue(row, phoneCol),
                                Used = hasUsedColumn ? ParseBooleanValue(row.Cell(usedCol)) : false
                            };

                            // Only add if not empty
                            if (!string.IsNullOrWhiteSpace(contact.FirstName) ||
                                !string.IsNullOrWhiteSpace(contact.LastName) ||
                                !string.IsNullOrWhiteSpace(contact.Phone))
                            {
                                loadedContacts.Add(contact);

                                // If we added the Used column, set default value in Excel
                                if (needsUsedColumn)
                                {
                                    row.Cell(usedCol).Value = false;
                                }
                            }
                        }
                        catch (Exception)
                        {
                            // Skip problematic rows silently
                        }
                    }

                    // Save the file if we added the Used column
                    if (needsUsedColumn)
                    {
                        // Auto-fit columns
                        worksheet.Columns().AdjustToContents();
                        workbook.Save();
                    }

                    // Sort contacts by first name (ascending)
                    loadedContacts = loadedContacts
                        .OrderBy(c => c.FirstName?.Trim() ?? string.Empty)
                        .ThenBy(c => c.LastName?.Trim() ?? string.Empty)
                        .ToList();

                    // Update UI on main thread
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Contacts.Clear();
                        foreach (var contact in loadedContacts)
                        {
                            Contacts.Add(contact);
                        }

                        OnPropertyChanged(nameof(HasContacts));
                        UpdateStatistics();
                        _originalContactCount = Contacts.Count; // Track original count for change detection
                        SaveOriginalContactStates(); // Save original states for change detection
                        HasUnsavedChanges = false; // Clear unsaved changes after loading data
                        IsLoading = false;
                    });
                }
                catch (Exception)
                {
                    // Handle errors silently and just finish loading
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        IsLoading = false;
                    });
                }
            });
        }

        private string GetCellStringValue(IXLRow row, int columnNumber)
        {
            try
            {
                return row.Cell(columnNumber).GetString().Trim();
            }
            catch
            {
                return string.Empty;
            }
        }

        private bool ParseBooleanValue(IXLCell cell)
        {
            try
            {
                // Try to get as boolean first
                return cell.GetBoolean();
            }
            catch
            {
                try
                {
                    // Try as string and parse common boolean representations
                    var stringValue = cell.GetString().Trim().ToLower();
                    return stringValue == "true" || stringValue == "yes" || stringValue == "1" || stringValue == "on";
                }
                catch
                {
                    try
                    {
                        // Try as number (1 = true, 0 = false)
                        var numValue = cell.GetDouble();
                        return numValue > 0;
                    }
                    catch
                    {
                        // Default to false if can't parse
                        return false;
                    }
                }
            }
        }
        private void PersistChanges()
        {
            string databaseFilePath = Path.Combine(Environment.CurrentDirectory, "sa_contacts.xlsx");

            try
            {
                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Contacts");

                // Add header row
                worksheet.Cell(1, 1).Value = "First Name";
                worksheet.Cell(1, 2).Value = "Last Name";
                worksheet.Cell(1, 3).Value = "Phone";
                worksheet.Cell(1, 4).Value = "Used";

                // Make header bold
                worksheet.Row(1).Style.Font.Bold = true;

                // Add contact data
                int row = 2;
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

                workbook.SaveAs(databaseFilePath);

                // Calculate detailed changes before clearing HasUnsavedChanges
                int currentCount = Contacts.Count;
                int addedCount = Math.Max(0, currentCount - _originalContactCount);
                int removedCount = Math.Max(0, _originalContactCount - currentCount);
                int modifiedCount = CountModifiedContacts();

                List<string> changeParts = new List<string>();

                if (addedCount > 0)
                    changeParts.Add($"{addedCount} contact{(addedCount == 1 ? "" : "s")} added");

                if (removedCount > 0)
                    changeParts.Add($"{removedCount} contact{(removedCount == 1 ? "" : "s")} removed");

                if (modifiedCount > 0)
                    changeParts.Add($"{modifiedCount} contact{(modifiedCount == 1 ? "" : "s")} modified");

                string changeMessage;
                if (changeParts.Any())
                {
                    changeMessage = $"Successfully saved {currentCount} contacts to sa_contacts.xlsx\n({string.Join(", ", changeParts)})";
                }
                else
                {
                    changeMessage = $"Successfully saved {currentCount} contacts to sa_contacts.xlsx\n(No changes detected)";
                }

                // Update tracking for next comparison
                _originalContactCount = currentCount;
                SaveOriginalContactStates(); // Save new state as baseline
                HasUnsavedChanges = false; // Clear unsaved changes after successful save

                MessageBox.Show(changeMessage, "Database Saved", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving to database file: {ex.Message}", "Save Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public bool CanClose()
        {
            if (!HasUnsavedChanges)
                return true;

            var result = MessageBox.Show(
                "You have unsaved changes. Do you want to save them before closing?",
                "Unsaved Changes",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Question);

            switch (result)
            {
                case MessageBoxResult.Yes:
                    PersistChanges();
                    return !HasUnsavedChanges; // Only close if save was successful
                case MessageBoxResult.No:
                    return true; // Close without saving
                case MessageBoxResult.Cancel:
                default:
                    return false; // Don't close
            }
        }

        private void SaveOriginalContactStates()
        {
            _originalContactStates.Clear();
            foreach (var contact in Contacts)
            {
                var key = $"{contact.FirstName}|{contact.LastName}|{contact.Phone}"; // Unique identifier
                _originalContactStates[key] = new Contact
                {
                    FirstName = contact.FirstName,
                    LastName = contact.LastName,
                    Phone = contact.Phone,
                    Used = contact.Used
                };
            }
        }

        private int CountModifiedContacts()
        {
            int modifiedCount = 0;

            foreach (var contact in Contacts)
            {
                var currentKey = $"{contact.FirstName}|{contact.LastName}|{contact.Phone}";

                // Look for this contact in original states by trying different key combinations
                // (in case name or phone was changed)
                var originalContact = _originalContactStates.Values
                    .FirstOrDefault(original =>
                        (original.FirstName == contact.FirstName && original.LastName == contact.LastName) ||
                        (original.Phone == contact.Phone && !string.IsNullOrEmpty(contact.Phone)));

                if (originalContact != null)
                {
                    // Check if any field was modified
                    if (originalContact.FirstName != contact.FirstName ||
                        originalContact.LastName != contact.LastName ||
                        originalContact.Phone != contact.Phone ||
                        originalContact.Used != contact.Used)
                    {
                        modifiedCount++;
                    }
                }
            }

            return modifiedCount;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
