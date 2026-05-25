using System.Linq;
using System.Windows;
using JewelleryManagementApp.WPF.Data;

namespace JewelleryManagementApp.WPF.Views
{
    public partial class LoginView : Window
    {
        public LoginView()
        {
            InitializeComponent();
            this.MouseDown += (s, e) => { if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed) DragMove(); };

            // Load and apply initial theme
            try
            {
                using (var db = new JewelleryDbContext())
                {
                    var settings = db.Settings.FirstOrDefault();
                    if (settings != null)
                    {
                        ThemeModeCheckbox.IsChecked = settings.IsLightTheme;
                        App.ApplyTheme(settings.IsLightTheme);
                    }
                }
            }
            catch { }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (UsernameBox.Text == "admin" && PasswordBox.Password == "admin")
            {
                var mainWindow = new MainWindow();
                mainWindow.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("Invalid credentials. Try admin / admin", "Authorization Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ThemeToggle_Checked(object sender, RoutedEventArgs e)
        {
            SetTheme(true);
        }

        private void ThemeToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            SetTheme(false);
        }

        private void SetTheme(bool isLight)
        {
            App.ApplyTheme(isLight);

            // Persist the choice immediately to Settings table in SQLite
            try
            {
                using (var db = new JewelleryDbContext())
                {
                    var existing = db.Settings.FirstOrDefault();
                    if (existing != null)
                    {
                        existing.IsLightTheme = isLight;
                        db.Settings.Update(existing);
                        db.SaveChanges();
                    }
                }
            }
            catch { }
        }
    }
}
