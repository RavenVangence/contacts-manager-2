# WPF Contacts Manager

A WPF application built in .NET for managing contact records with browsing, sorting, updating, and Excel import/export functionality.

## Features

- **Main Menu**  
  - Provides navigation to the main screen.

- **Browse Box**  
  - Scroll through a list of records.  
  - Sort records in multiple orders (e.g., ascending/descending by name, date, or status).  
  - Records are displayed with dynamic colors based on their `Used` value.

- **Record Management**  
  - Add new records.  
  - Edit existing records.  
  - Delete records with confirmation.

- **Update Screen**  
  - Dedicated view for adding or modifying records.

- **Excel Import & Export** 
  - Export data back to Excel for external use.

## Color Scheme

The application uses the following color palette:

- `#010B12`  
- `#0C8900`  
- `#2BC20E`  
- `#9CFF00`  
- `#39FF13`  
- `#1E1F21`  
- `#444AFF`  

## Getting Started

### Prerequisites
- .NET 8   
- Visual Studio or Visual Studio Code with WPF support  
- SQL Server  

### Setup
1. Clone this repository.
2. Open the solution in Visual Studio.
3. Restore NuGet packages.
4. Build and run the project.

### Importing Data
- Use the **Import** button in the application to load contacts.

### Exporting Data
- Click the **Export** button to save database records to Excel.

### Excel Operations
- ClosedXML for reading and writing Excel (.xlsx)
- Flexible header mapping for Name, Surname, Phone/Phone Number, and optional Used column (defaults to false when absent)
- Export columns: Name, Surname, Phone Number, Used, Created Date, Modified Date

## License
This project is licensed under the MIT License.
