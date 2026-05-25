using System;
using System.Linq;
using System.Windows.Input;
using JewelleryManagementApp.WPF.Data;
using JewelleryManagementApp.WPF.Models;
using JewelleryManagementApp.WPF.Commands;

namespace JewelleryManagementApp.WPF.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        private Settings _settings = null!;

        public Settings Settings
        {
            get => _settings;
            set { _settings = value; OnPropertyChanged(); }
        }

        public ICommand SaveSettingsCommand { get; }

        public SettingsViewModel(JewelleryDbContext dbContext)
        {
            // DI compatibility
            LoadSettings();
            SaveSettingsCommand = new RelayCommand(_ => SaveSettings());
        }

        private void LoadSettings()
        {
            using (var db = new JewelleryDbContext())
            {
                Settings = db.Settings.FirstOrDefault() ?? new Settings
                {
                    ShopName = "AuraJewels Luxury Salon",
                    GSTNumber = "27AAAAA1111A1Z1",
                    Address = "Gold Souk, MG Road, Mumbai, India",
                    GSTRate = 3.0
                };
            }
        }

        public override void Refresh()
        {
            LoadSettings();
        }

        private void SaveSettings()
        {
            try
            {
                using (var db = new JewelleryDbContext())
                {
                    var existing = db.Settings.FirstOrDefault();
                    if (existing == null)
                    {
                        db.Settings.Add(Settings);
                    }
                    else
                    {
                        existing.ShopName = Settings.ShopName;
                        existing.GSTNumber = Settings.GSTNumber;
                        existing.Address = Settings.Address;
                        existing.LogoPath = Settings.LogoPath;
                        existing.GSTRate = Settings.GSTRate;
                        existing.IsLightTheme = Settings.IsLightTheme;
                        db.Settings.Update(existing);
                    }
                    db.SaveChanges();
                }
                System.Windows.MessageBox.Show("Configuration updated successfully!", "Settings Saved", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Failed to save settings: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }
    }
}
