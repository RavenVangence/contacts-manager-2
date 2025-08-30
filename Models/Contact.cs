using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ContactsManager.Models
{
    public class Contact : INotifyPropertyChanged
    {
        private string _firstName = string.Empty;
        private string _lastName = string.Empty;
        private string _phone = string.Empty;
        private bool _used;

        public string FirstName
        {
            get => _firstName;
            set { if (_firstName != value) { _firstName = value; OnPropertyChanged(); OnPropertyChanged(nameof(FullName)); } }
        }

        public string LastName
        {
            get => _lastName;
            set { if (_lastName != value) { _lastName = value; OnPropertyChanged(); OnPropertyChanged(nameof(FullName)); } }
        }

        public string FullName => ($"{FirstName} {LastName}").Trim();

        public string Phone
        {
            get => _phone;
            set { if (_phone != value) { _phone = value; OnPropertyChanged(); } }
        }

        public bool Used
        {
            get => _used;
            set { if (_used != value) { _used = value; OnPropertyChanged(); } }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
