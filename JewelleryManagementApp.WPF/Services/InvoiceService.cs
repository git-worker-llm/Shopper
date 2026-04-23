using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using JewelleryManagementApp.WPF.Data;
using JewelleryManagementApp.WPF.Models;

namespace JewelleryManagementApp.WPF.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly JewelleryDbContext _dbContext;

        public InvoiceService(JewelleryDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task PrintInvoiceAsync(Bill bill)
        {
            try
            {
                var settings = _dbContext.Settings.FirstOrDefault() ?? new Settings();

                var doc = new FlowDocument();
                doc.PagePadding = new Thickness(50);

                doc.Blocks.Add(new Paragraph(new Run(settings.ShopName)) { FontSize = 24, FontWeight = FontWeights.Bold });
                doc.Blocks.Add(new Paragraph(new Run($"GST: {settings.GSTNumber}")));
                doc.Blocks.Add(new Paragraph(new Run($"Address: {settings.Address}")));
                doc.Blocks.Add(new Paragraph(new Run($"Date: {bill.Date}")));
                doc.Blocks.Add(new Paragraph(new Run("--------------------------------------------------")));

                var table = new Table();
                table.Columns.Add(new TableColumn());
                table.Columns.Add(new TableColumn());
                table.Columns.Add(new TableColumn());

                var rowGroup = new TableRowGroup();
                var headerRow = new TableRow();
                headerRow.Cells.Add(new TableCell(new Paragraph(new Run("Item"))));
                headerRow.Cells.Add(new TableCell(new Paragraph(new Run("Qty"))));
                headerRow.Cells.Add(new TableCell(new Paragraph(new Run("Price"))));
                rowGroup.Rows.Add(headerRow);

                foreach (var item in bill.BillItems)
                {
                    var row = new TableRow();
                    row.Cells.Add(new TableCell(new Paragraph(new Run(item.Item?.Name ?? "N/A"))));
                    row.Cells.Add(new TableCell(new Paragraph(new Run(item.Quantity.ToString()))));
                    row.Cells.Add(new TableCell(new Paragraph(new Run(item.Price.ToString("C")))));
                    rowGroup.Rows.Add(row);
                }
                table.RowGroups.Add(rowGroup);
                doc.Blocks.Add(table);

                doc.Blocks.Add(new Paragraph(new Run("--------------------------------------------------")));
                doc.Blocks.Add(new Paragraph(new Run($"GST Amount: {bill.GSTAmount:C}")));
                doc.Blocks.Add(new Paragraph(new Run($"Total Amount: {bill.TotalAmount:C}")) { FontSize = 18, FontWeight = FontWeights.Bold });

                PrintDialog printDialog = new PrintDialog();
                if (printDialog.ShowDialog() == true)
                {
                    IDocumentPaginatorSource idpSource = doc;
                    printDialog.PrintDocument(idpSource.DocumentPaginator, $"Invoice_{bill.Id}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Printing failed: {ex.Message}");
            }
        }
    }
}
