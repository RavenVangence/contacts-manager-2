using ContactsManager.Server.Models;
using ClosedXML.Excel;

namespace ContactsManager.Server.Services;

public class ExcelContactRepository : IContactRepository
{
    private readonly string _filePath;
    private readonly List<Contact> _contacts = new();
    private int _nextId = 1;
    private readonly ILogger<ExcelContactRepository>? _logger;

    public ExcelContactRepository(ILogger<ExcelContactRepository>? logger = null)
    {
        _logger = logger;

        // Use local file in the application directory for single-server deployment
        _filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sa_contacts.xlsx");

        // If running in development, use the source directory instead of bin folder
        if (!File.Exists(_filePath))
        {
            var sourceFilePath = Path.Combine(Directory.GetCurrentDirectory(), "sa_contacts.xlsx");
            if (File.Exists(sourceFilePath))
            {
                _filePath = sourceFilePath;
            }
        }

        LoadFromExcel();
    }

    private void LoadFromExcel()
    {
        _logger?.LogInformation($"Attempting to load Excel file from: {_filePath}");

        if (!File.Exists(_filePath))
        {
            _logger?.LogWarning("Excel file not found, creating empty file");
            CreateEmptyExcelFile();
            return;
        }

        try
        {
            _logger?.LogInformation("Excel file found, loading data...");
            using var workbook = new XLWorkbook(_filePath);
            var worksheet = workbook.Worksheet(1);

            // Find the last row with data
            var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 1;
            _logger?.LogInformation($"Last row with data: {lastRow}");

            // Skip header row (row 1)
            for (int row = 2; row <= lastRow; row++)
            {
                var firstName = worksheet.Cell(row, 1).GetString();
                var lastName = worksheet.Cell(row, 2).GetString();
                var phone = worksheet.Cell(row, 3).GetString();
                var usedValue = worksheet.Cell(row, 4).GetString().ToLower();

                _logger?.LogDebug($"Row {row}: {firstName} {lastName} {phone} {usedValue}");

                if (string.IsNullOrWhiteSpace(firstName) && string.IsNullOrWhiteSpace(lastName))
                    continue;

                var contact = new Contact
                {
                    Id = _nextId++,
                    FirstName = firstName,
                    LastName = lastName,
                    Phone = phone,
                    Used = usedValue == "true" || usedValue == "yes" || usedValue == "1"
                };

                _contacts.Add(contact);
            }

            _logger?.LogInformation($"Loaded {_contacts.Count} contacts from Excel");
        }
        catch (Exception ex)
        {
            // Log error and create empty file
            _logger?.LogError(ex, $"Error loading Excel file: {ex.Message}");
            CreateEmptyExcelFile();

            // Add some sample data if Excel loading failed
            if (_contacts.Count == 0)
            {
                _logger?.LogInformation("Adding sample data since Excel loading failed");
                _contacts.Add(new Contact { Id = _nextId++, FirstName = "John", LastName = "Doe", Phone = "555-1234", Used = true });
                _contacts.Add(new Contact { Id = _nextId++, FirstName = "Jane", LastName = "Smith", Phone = "555-5678", Used = false });
                SaveToExcel(); // Try to save sample data
            }
        }
    }

    private void CreateEmptyExcelFile()
    {
        try
        {
            _logger?.LogInformation($"Creating empty Excel file at: {_filePath}");

            // Ensure directory exists
            var directory = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Contacts");

            // Add headers
            worksheet.Cell(1, 1).Value = "First Name";
            worksheet.Cell(1, 2).Value = "Last Name";
            worksheet.Cell(1, 3).Value = "Phone";
            worksheet.Cell(1, 4).Value = "Used";

            // Format header row
            var headerRange = worksheet.Range(1, 1, 1, 4);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

            workbook.SaveAs(_filePath);
            _logger?.LogInformation("Empty Excel file created successfully");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, $"Error creating Excel file: {ex.Message}");
        }
    }

    private void SaveToExcel()
    {
        try
        {
            _logger?.LogInformation($"Saving {_contacts.Count} contacts to Excel file: {_filePath}");

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Contacts");

            // Add headers
            worksheet.Cell(1, 1).Value = "First Name";
            worksheet.Cell(1, 2).Value = "Last Name";
            worksheet.Cell(1, 3).Value = "Phone";
            worksheet.Cell(1, 4).Value = "Used";

            // Format header row
            var headerRange = worksheet.Range(1, 1, 1, 4);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

            // Add data
            for (int i = 0; i < _contacts.Count; i++)
            {
                var contact = _contacts[i];
                var row = i + 2; // Start from row 2 (after headers)

                worksheet.Cell(row, 1).Value = contact.FirstName;
                worksheet.Cell(row, 2).Value = contact.LastName;
                worksheet.Cell(row, 3).Value = contact.Phone;
                worksheet.Cell(row, 4).Value = contact.Used ? "TRUE" : "FALSE";
            }

            // Auto-fit columns
            worksheet.Columns().AdjustToContents();

            workbook.SaveAs(_filePath);
            _logger?.LogInformation("Excel file saved successfully");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, $"Error saving Excel file: {ex.Message}");
            throw;
        }
    }

    public Contact Add(Contact c)
    {
        c.Id = _nextId++;
        _contacts.Add(c);
        SaveToExcel();
        return c;
    }

    public bool Delete(int id)
    {
        var existing = Get(id);
        if (existing == null) return false;
        _contacts.Remove(existing);
        SaveToExcel();
        return true;
    }

    public Contact? Get(int id) => _contacts.FirstOrDefault(c => c.Id == id);

    public IEnumerable<Contact> GetAll() => _contacts;

    public bool Update(int id, Contact c)
    {
        var existing = Get(id);
        if (existing == null) return false;
        existing.FirstName = c.FirstName;
        existing.LastName = c.LastName;
        existing.Phone = c.Phone;
        existing.Used = c.Used;
        SaveToExcel();
        return true;
    }

    public void SaveAll(List<Contact> contacts)
    {
        _logger?.LogInformation($"Saving all {contacts.Count} contacts to replace current data");

        // Replace all contacts with the provided list
        _contacts.Clear();
        _contacts.AddRange(contacts);

        // Update next ID to be one higher than the highest ID
        _nextId = contacts.Any() ? contacts.Max(c => c.Id) + 1 : 1;

        // Save to Excel
        SaveToExcel();
    }
}
