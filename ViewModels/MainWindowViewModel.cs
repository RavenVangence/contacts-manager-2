using System.ComponentModel;
using System.Runtime.CompilerServices;
using ContactsManager.ViewModels;

namespace ContactsManager.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private string _currentPageType = "Home";
        private MainViewModel? _contactsViewModel;

        public string CurrentPageType
        {
            get => _currentPageType;
            set
            {
                if (_currentPageType != value)
                {
                    _currentPageType = value;
                    OnPropertyChanged();
                }
            }
        }

        public MainViewModel? ContactsViewModel
        {
            get => _contactsViewModel;
            set
            {
                _contactsViewModel = value;
                OnPropertyChanged();
            }
        }

        public MainWindowViewModel()
        {
            // Start with Home page - don't initialize ContactsViewModel until needed
            CurrentPageType = "Home";
        }

        public void NavigateToContacts()
        {
            CurrentPageType = "Contacts";
        }

        public void NavigateToHome()
        {
            CurrentPageType = "Home";
        }

        public bool CanClose()
        {
            // Only check ContactsViewModel if it exists
            return ContactsViewModel?.CanClose() ?? true;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
