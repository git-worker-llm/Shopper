using System.Windows;

namespace JewelleryManagementApp.WPF.Views
{
    public partial class LoginView : Window
    {
        public LoginView()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            // Simple hardcoded check for now
            if (UsernameBox.Text == "admin" && PasswordBox.Password == "admin")
            {
                var mainWindow = new MainWindow();
                mainWindow.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("Invalid credentials");
            }
        }
    }
}
