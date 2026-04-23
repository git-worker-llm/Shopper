using System;
using System.Windows;

namespace JewelleryManagementApp.WPF.Services
{
    public class ExceptionService : IExceptionService
    {
        public void HandleException(Exception ex)
        {
            // Log to file (simplified implementation)
            System.IO.File.AppendAllText("error.log", $"{DateTime.Now}: {ex.Message}\n{ex.StackTrace}\n\n");

            MessageBox.Show("An unexpected error occurred. Please contact support.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
