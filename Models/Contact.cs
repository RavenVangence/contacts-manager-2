using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ContactsManager.Models
{
    public class Contact : INotifyPropertyChanged
    {
        private string _firstName = string.Empty;
        private string _lastName = string.Empty;
        private string? _email;
        private string? _phone;
        private string? _company;
        private string? _notes;
        private bool _used;
        private DateTime _createdDate = DateTime.Now;
        private DateTime _modifiedDate = DateTime.Now;

        public string FirstName
        {
            get => _firstName;
            set { if (_firstName != value) { _firstName = value; OnPropertyChanged(); OnPropertyChanged(nameof(FullName)); UpdateModifiedDate(); } }
        }

        public string LastName
        {
            get => _lastName;
            set { if (_lastName != value) { _lastName = value; OnPropertyChanged(); OnPropertyChanged(nameof(FullName)); UpdateModifiedDate(); } }
        }

        public string FullName => ($"{FirstName} {LastName}").Trim();

        public string? Email
        {
            get => _email;
            set { if (_email != value) { _email = value; OnPropertyChanged(); UpdateModifiedDate(); } }
        }

        public string? Phone
        {
            get => _phone;
            set { if (_phone != value) { _phone = value; OnPropertyChanged(); UpdateModifiedDate(); } }
        }

        public string? Company
        {
            get => _company;
            set { if (_company != value) { _company = value; OnPropertyChanged(); UpdateModifiedDate(); } }
        }

        public string? Notes
        {
            get => _notes;
            set { if (_notes != value) { _notes = value; OnPropertyChanged(); UpdateModifiedDate(); } }
        }

        public bool Used
        {
            get => _used;
            set { if (_used != value) { _used = value; OnPropertyChanged(); UpdateModifiedDate(); } }
        }

        public DateTime CreatedDate
        {
            get => _createdDate;
            set { if (_createdDate != value) { _createdDate = value; OnPropertyChanged(); } }
        }

        public DateTime ModifiedDate
        {
            get => _modifiedDate;
            set { if (_modifiedDate != value) { _modifiedDate = value; OnPropertyChanged(); } }
        }

        private void UpdateModifiedDate()
        {
            ModifiedDate = DateTime.Now;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
