using System.ComponentModel;
using System.Runtime.CompilerServices;
using ContactsManager.ViewModels;

namespace ContactsManager.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private bool _isContactsViewActive = false;

        public MainViewModel ContactsViewModel { get; }

        public bool IsContactsViewActive
        {
            get => _isContactsViewActive;
            set
            {
                if (_isContactsViewActive != value)
                {
                    _isContactsViewActive = value;
                    OnPropertyChanged();
                }
            }
        }

        public MainWindowViewModel()
        {
            ContactsViewModel = new MainViewModel();
            // Ensure we always start with Home page
            _isContactsViewActive = false;
        }

        public void NavigateToContacts()
        {
            IsContactsViewActive = true;
        }

        public void NavigateToHome()
        {
            IsContactsViewActive = false;
        }

        public bool CanClose()
        {
            return ContactsViewModel.CanClose();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
