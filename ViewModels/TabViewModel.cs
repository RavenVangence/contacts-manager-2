using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using ContactsManager.Infrastructure;
using ContactsManager.Models;

namespace ContactsManager.ViewModels
{
    public class TabViewModel : INotifyPropertyChanged
    {
        private TabItem? _activeTab;

        public ObservableCollection<TabItem> Tabs { get; } = new();

        public TabItem? ActiveTab
        {
            get => _activeTab;
            set
            {
                if (_activeTab != value)
                {
                    // Deactivate previous tab
                    if (_activeTab != null)
                        _activeTab.IsActive = false;

                    _activeTab = value;

                    // Activate new tab
                    if (_activeTab != null)
                        _activeTab.IsActive = true;

                    OnPropertyChanged();
                    OnPropertyChanged(nameof(HasActiveTabs));
                }
            }
        }

        public bool HasActiveTabs => Tabs.Any();

        public ICommand ActivateTabCommand { get; }
        public ICommand CloseTabCommand { get; }

        public TabViewModel()
        {
            ActivateTabCommand = new RelayCommand(param => ActivateTab(param as TabItem));
            CloseTabCommand = new RelayCommand(param => CloseTab(param as TabItem));
        }

        public TabItem AddTab(string title, string icon, object content, bool isCloseable = true)
        {
            var tab = new TabItem
            {
                Title = title,
                Icon = icon,
                Content = content,
                IsCloseable = isCloseable,
                ActivateCommand = ActivateTabCommand,
                CloseCommand = CloseTabCommand
            };

            Tabs.Add(tab);
            ActiveTab = tab;
            OnPropertyChanged(nameof(HasActiveTabs));

            return tab;
        }

        public void ActivateTab(TabItem? tab)
        {
            if (tab != null && Tabs.Contains(tab))
            {
                ActiveTab = tab;
            }
        }

        public void CloseTab(TabItem? tab)
        {
            if (tab == null || !tab.IsCloseable || !Tabs.Contains(tab))
                return;

            var tabIndex = Tabs.IndexOf(tab);
            Tabs.Remove(tab);

            // If the closed tab was active, activate another tab
            if (ActiveTab == tab)
            {
                if (Tabs.Count > 0)
                {
                    // Try to activate the tab that was to the right, or the last tab if we closed the rightmost
                    var newActiveIndex = Math.Min(tabIndex, Tabs.Count - 1);
                    ActiveTab = Tabs[newActiveIndex];
                }
                else
                {
                    ActiveTab = null;
                }
            }

            OnPropertyChanged(nameof(HasActiveTabs));
        }

        public TabItem? FindTabByContent(object content)
        {
            return Tabs.FirstOrDefault(t => ReferenceEquals(t.Content, content));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
