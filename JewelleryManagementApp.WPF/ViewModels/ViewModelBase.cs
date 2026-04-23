using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using JewelleryManagementApp.WPF.Commands;

namespace JewelleryManagementApp.WPF.ViewModels
{
    public abstract class ViewModelBase : INotifyPropertyChanged, IDataErrorInfo
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // IDataErrorInfo Implementation
        private readonly Dictionary<string, string> _errors = new Dictionary<string, string>();
        public string Error => null!;
        public string this[string columnName] => _errors.ContainsKey(columnName) ? _errors[columnName] : string.Empty;

        protected void AddError(string propertyName, string error)
        {
            _errors[propertyName] = error;
            OnPropertyChanged("Item[]");
        }

        protected void ClearError(string propertyName)
        {
            if (_errors.Remove(propertyName))
            {
                OnPropertyChanged("Item[]");
            }
        }

        // Refresh Mechanism
        public ICommand RefreshCommand { get; }
        protected ViewModelBase()
        {
            RefreshCommand = new RelayCommand(_ => Refresh());
        }

        public abstract void Refresh();
    }
}
