using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace ContactsManager.Models
{
    public class TabItem : INotifyPropertyChanged
    {
        private string _title = string.Empty;
        private string _icon = string.Empty;
        private bool _isActive = false;
        private bool _isCloseable = true;
        private object? _content;
        private string _id = Guid.NewGuid().ToString();

        public string Id
        {
            get => _id;
            set
            {
                if (_id != value)
                {
                    _id = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Title
        {
            get => _title;
            set
            {
                if (_title != value)
                {
                    _title = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Icon
        {
            get => _icon;
            set
            {
                if (_icon != value)
                {
                    _icon = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsActive
        {
            get => _isActive;
            set
            {
                if (_isActive != value)
                {
                    _isActive = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsCloseable
        {
            get => _isCloseable;
            set
            {
                if (_isCloseable != value)
                {
                    _isCloseable = value;
                    OnPropertyChanged();
                }
            }
        }

        public object? Content
        {
            get => _content;
            set
            {
                if (_content != value)
                {
                    _content = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand? ActivateCommand { get; set; }
        public ICommand? CloseCommand { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
