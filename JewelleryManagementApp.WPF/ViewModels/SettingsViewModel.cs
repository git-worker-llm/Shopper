using System.Linq;
using System.Windows.Input;
using JewelleryManagementApp.WPF.Data;
using JewelleryManagementApp.WPF.Models;
using JewelleryManagementApp.WPF.Commands;

namespace JewelleryManagementApp.WPF.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        private readonly JewelleryDbContext _dbContext;
        private Settings _settings;

        public Settings Settings
        {
            get => _settings;
            set { _settings = value; OnPropertyChanged(); }
        }

        public ICommand SaveSettingsCommand { get; }

        public SettingsViewModel(JewelleryDbContext dbContext)
        {
            _dbContext = dbContext;
            _settings = _dbContext.Settings.FirstOrDefault() ?? new Settings();
            SaveSettingsCommand = new RelayCommand(_ => SaveSettings());
        }

        public override void Refresh()
        {
            Settings = _dbContext.Settings.FirstOrDefault() ?? new Settings();
        }

        private void SaveSettings()
        {
            if (_settings.Id == 0)
                _dbContext.Settings.Add(_settings);
            _dbContext.SaveChanges();
        }
    }
}
