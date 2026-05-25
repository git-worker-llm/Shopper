using System;
using System.Globalization;
using System.Windows.Data;
using JewelleryManagementApp.WPF.Models;

namespace JewelleryManagementApp.WPF.Helpers
{
    public class MathAddConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is Bill bill)
            {
                return $"₹{(bill.TotalAmount + bill.GSTAmount):N2}";
            }
            return "₹0.00";
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
