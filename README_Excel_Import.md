# Excel Import Format

## Expected Excel File Format

When importing contacts from Excel, the application expects the following column structure:

| Column | Header | Data Type | Description |
|--------|--------|-----------|-------------|
| A (1) | First Name | Text | Contact's first name |
| B (2) | Last Name | Text | Contact's last name |
| C (3) | Email | Text | Email address |
| D (4) | Phone | Text | Phone number |
| E (5) | Company | Text | Company name |
| F (6) | Notes | Text | Additional notes |
| G (7) | Used | Boolean | TRUE/FALSE or 1/0 |

## Import Instructions

1. Click **File → Import** from the menu
2. Select your Excel file (.xlsx or .xls)
3. The first row should contain headers (will be skipped)
4. At minimum, provide either First Name or Last Name
5. The Used column should contain TRUE/FALSE or 1/0 values

## Sample Excel Content

```
First Name | Last Name | Email | Phone | Company | Notes | Used
John | Smith | john@example.com | +1-555-1234 | Acme Corp | Important client | TRUE
Jane | Doe | jane@example.com | +1-555-5678 | Beta Inc | Follow up needed | FALSE
```

## Notes

- Empty rows will be skipped
- Contacts without both First Name and Last Name will be skipped
- Invalid Used column values will default to FALSE
- The application supports both .xlsx and .xls file formats
