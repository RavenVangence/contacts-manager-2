# Contacts Manager

A lightweight WPF desktop application (\.NET 8) for managing contacts. It supports quick search, sorting, filtering by usage state, inline editing, and Excel import/export using ClosedXML.

## Features
- Add, edit, delete contacts
- Toggle "Used" status and filter by All / Used / Unused
- Sort by First Name, Last Name, or Used (ascending/descending)
- Fast search by name or phone
- Excel import/export (\*.xlsx) powered by ClosedXML
- Automatically loads and saves to `sa_contacts.xlsx` in the app folder
- Change detection with a clear save prompt before closing

## Tech stack
- .NET 8
- WPF (MVVM)
- ClosedXML for Excel IO

## Getting started
### Prerequisites
- Windows 10/11
- .NET SDK 8.0+
- Optional: Visual Studio 2022 (or VS Code with C# Dev Kit)

### Run (Visual Studio)
1. Open `ContactsManager.sln`
2. Set the startup project to `ContactsManager` if needed
3. Press F5 to run

### Run (CLI)
```pwsh
# In the repo root
 dotnet restore
 dotnet build -c Debug
 dotnet run --project .\ContactsManager.csproj
```

## Data file (Excel)
- On startup, the app looks for `sa_contacts.xlsx` in the working directory and loads it if present.
- When you save, it writes the full contact list back to `sa_contacts.xlsx`.
- Expected columns (row 1 = headers):
  1. First Name
  2. Last Name
  3. Phone
  4. Used (TRUE/FALSE)

You can also use the Import/Export actions to load from or save to any `.xlsx` file. Duplicates are skipped on import (match on First, Last, and Phone).

## Project structure
- `App.xaml`, `MainWindow.xaml` — application shell
- `Views/` — UI views (e.g., `ContactsView`, dialogs)
- `Pages/` — page-based navigation content
- `ViewModels/` — MVVM view models (e.g., `MainViewModel`)
- `Models/` — data models (`Contact`, `TabItem`)
- `Converters/` — value converters (e.g., `BoolToVisibilityConverter`)
- `Controls/` — custom controls (e.g., `ValidationTextBox`)
- `Infrastructure/` — helpers (e.g., `RelayCommand`)
- `Assets/` — static assets

## Troubleshooting
- If saving fails, ensure `sa_contacts.xlsx` (or the target export file) is closed in Excel.
- If import yields no rows, confirm the first row contains headers and data starts at row 2.
- The `Used` column accepts TRUE/FALSE, Yes/No, 1/0.

## License
No license specified.

## Acknowledgements
- [ClosedXML](https://github.com/ClosedXML/ClosedXML) for easy Excel integration.
