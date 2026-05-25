using System.Windows;
using System.Windows.Controls;
using JewelleryManagementApp.WPF.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace JewelleryManagementApp.WPF.Views
{
    public partial class PrintTemplateView : UserControl
    {
        public PrintTemplateView()
        {
            InitializeComponent();
            DataContext = App.ServiceProvider.GetRequiredService<PrintTemplateViewModel>();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (DataContext is PrintTemplateViewModel vm)
            {
                vm.OnXamlChanged();
            }
        }

        private void InsertField_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string field)
            {
                int selectionStart = TemplateXamlBox.SelectionStart;
                TemplateXamlBox.Text = TemplateXamlBox.Text.Insert(selectionStart, field);
                TemplateXamlBox.SelectionStart = selectionStart + field.Length;
                TemplateXamlBox.Focus();
            }
        }
    }
}
