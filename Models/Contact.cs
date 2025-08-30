using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace ContactsManager.Models
{
    public class Contact : INotifyPropertyChanged, IDataErrorInfo
    {
        private string _firstName = string.Empty;
        private string _lastName = string.Empty;
        private string _phone = string.Empty;
        private bool _used;

        public string FirstName
        {
            get => _firstName;
            set { if (_firstName != value) { _firstName = value; OnPropertyChanged(); OnPropertyChanged(nameof(FullName)); OnPropertyChanged(nameof(IsValid)); } }
        }

        public string LastName
        {
            get => _lastName;
            set { if (_lastName != value) { _lastName = value; OnPropertyChanged(); OnPropertyChanged(nameof(FullName)); OnPropertyChanged(nameof(IsValid)); } }
        }

        public string FullName => ($"{FirstName} {LastName}").Trim();

        public string Phone
        {
            get => _phone;
            set { if (_phone != value) { _phone = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsValid)); } }
        }

        public bool Used
        {
            get => _used;
            set { if (_used != value) { _used = value; OnPropertyChanged(); } }
        }

        public bool IsValid => string.IsNullOrEmpty(this["FirstName"]) && string.IsNullOrEmpty(this["LastName"]) && string.IsNullOrEmpty(this["Phone"]);

        public string Error => string.Empty;

        public string this[string columnName]
        {
            get
            {
                var error = string.Empty;
                switch (columnName)
                {
                    case "FirstName":
                        if (string.IsNullOrWhiteSpace(FirstName))
                            error = "First name is required.";
                        else if (!Regex.IsMatch(FirstName, @"^[a-zA-Z.'\- ]+$"))
                            error = "First name contains invalid characters.";
                        break;
                    case "LastName":
                        if (string.IsNullOrWhiteSpace(LastName))
                            error = "Last name is required.";
                        else if (!Regex.IsMatch(LastName, @"^[a-zA-Z.'\- ]+$"))
                            error = "Last name contains invalid characters.";
                        break;
                    case "Phone":
                        if (string.IsNullOrWhiteSpace(Phone))
                            error = "Phone is required.";
                        else if (Phone.Length < 10 || Phone.Length > 15)
                            error = "Phone number must be between 10 and 15 digits.";
                        else if (!Regex.IsMatch(Phone, @"^\+?[0-9]+$"))
                            error = "Phone number can only contain digits and a leading '+'.";
                        break;
                }
                return error;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
