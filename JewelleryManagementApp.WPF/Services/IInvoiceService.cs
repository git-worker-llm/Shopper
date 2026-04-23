using System.Threading.Tasks;
using JewelleryManagementApp.WPF.Models;

namespace JewelleryManagementApp.WPF.Services
{
    public interface IInvoiceService
    {
        Task PrintInvoiceAsync(Bill bill);
    }
}
