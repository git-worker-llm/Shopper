using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.IO;
using System.Xml;
using JewelleryManagementApp.WPF.Data;
using JewelleryManagementApp.WPF.Models;
using Microsoft.EntityFrameworkCore;

namespace JewelleryManagementApp.WPF.Services
{
    public class InvoiceService : IInvoiceService
    {
        public InvoiceService(JewelleryDbContext dbContext)
        {
            // DI Compatibility
        }

        public async Task PrintInvoiceAsync(Bill bill)
        {
            try
            {
                InvoiceTemplate? template = null;
                Settings? settings = null;

                using (var db = new JewelleryDbContext())
                {
                    template = await db.InvoiceTemplates.FirstOrDefaultAsync();
                    settings = await db.Settings.FirstOrDefaultAsync() ?? new Settings();
                }

                string shopName = template?.ShopName ?? settings.ShopName;
                if (string.IsNullOrWhiteSpace(shopName)) shopName = "AuraJewels Luxury Salon";

                string gstNo = template?.GSTNumber ?? settings.GSTNumber;
                if (string.IsNullOrWhiteSpace(gstNo)) gstNo = "27AAAAA1111A1Z1";

                string address = template?.Address ?? settings.Address;
                if (string.IsNullOrWhiteSpace(address)) address = "Gold Souk, MG Road, Mumbai, India";

                // Standard Indian jewellery tax split is CGST (1.5%) & SGST (1.5%)
                double cgstAmount = bill.GSTAmount / 2;
                double sgstAmount = bill.GSTAmount / 2;
                double grandTotal = bill.TotalAmount + bill.GSTAmount;

                string fontFamilyName = template?.SelectedFontFamily ?? "Segoe UI";
                string accentColorName = template?.AccentColor ?? "Gold";
                double headerFontSize = template?.HeaderFontSize ?? 22.0;
                string footerText = template?.FooterText ?? "Thank you for shopping with us! Terms: Gold sold can be exchanged at current rate less melting loss.";

                // Accent color lookup
                string accentHex = "#D4AF37"; // Gold default
                if (accentColorName == "Charcoal") accentHex = "#333333";
                else if (accentColorName == "RoseGold") accentHex = "#B76E79";
                else if (accentColorName == "DarkBlue") accentHex = "#2980B9";

                var accentBrush = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(accentHex));

                var doc = new FlowDocument
                {
                    FontFamily = new System.Windows.Media.FontFamily(fontFamilyName),
                    PagePadding = new Thickness(40),
                    Background = System.Windows.Media.Brushes.White,
                    Foreground = System.Windows.Media.Brushes.Black
                };

                // Tax Invoice Subtitle
                doc.Blocks.Add(new Paragraph(new Run("TAX INVOICE"))
                {
                    FontSize = 10,
                    FontWeight = FontWeights.Bold,
                    Foreground = System.Windows.Media.Brushes.DarkGray,
                    TextAlignment = TextAlignment.Center,
                    Margin = new Thickness(0, 0, 0, 5)
                });

                // Header Paragraph
                doc.Blocks.Add(new Paragraph(new Run(shopName))
                {
                    FontSize = headerFontSize,
                    FontWeight = FontWeights.Bold,
                    Foreground = accentBrush,
                    TextAlignment = TextAlignment.Center,
                    Margin = new Thickness(0, 0, 0, 5)
                });

                doc.Blocks.Add(new Paragraph(new Run($"Address: {address}\nGSTIN: {gstNo}"))
                {
                    FontSize = 10.5,
                    Foreground = System.Windows.Media.Brushes.DimGray,
                    TextAlignment = TextAlignment.Center,
                    Margin = new Thickness(0, 0, 0, 15),
                    LineHeight = 16
                });

                // Meta Info Section (Vertical layout to guarantee 100% print visibility on all page widths)
                var metaPara = new Paragraph { FontSize = 11, LineHeight = 18, Margin = new Thickness(0, 5, 0, 15) };
                string customerName = bill.Customer?.Name ?? "Walk-in Patron";
                string customerPhone = bill.Customer?.Phone ?? "N/A";

                // Invoice details
                metaPara.Inlines.Add(new Bold(new Run("INVOICE DETAILS:\n")) { Foreground = accentBrush });
                metaPara.Inlines.Add(new Run($"Invoice Number: INV-{bill.Id:D5}   |   Date: {bill.Date:dd MMM yyyy}   |   Time: {bill.Date:hh:mm tt}\n"));

                // Customer details
                metaPara.Inlines.Add(new Bold(new Run("BILLED TO:\n")) { Foreground = accentBrush });
                metaPara.Inlines.Add(new Run($"Customer Name: {customerName}   |   Phone: {customerPhone}\n"));
                metaPara.Inlines.Add(new Run("HSN / SAC Code: 7113 (Jewellery)"));
                doc.Blocks.Add(metaPara);

                // Beautiful Table layout for detailed jewelry specifications
                var table = new Table
                {
                    CellSpacing = 0,
                    BorderThickness = new Thickness(0, 1, 0, 1),
                    BorderBrush = System.Windows.Media.Brushes.LightGray,
                    Margin = new Thickness(0, 10, 0, 10)
                };

                table.Columns.Add(new TableColumn { Width = new GridLength(220) }); // Description
                table.Columns.Add(new TableColumn { Width = new GridLength(70) });  // Metal/Category
                table.Columns.Add(new TableColumn { Width = new GridLength(70) });  // Weight
                table.Columns.Add(new TableColumn { Width = new GridLength(90) });  // Metal Rate / g
                table.Columns.Add(new TableColumn { Width = new GridLength(40) });  // Qty
                table.Columns.Add(new TableColumn { Width = new GridLength(90) });  // Total Value

                var rowGroup = new TableRowGroup();
                
                // Header Row
                var headerRow = new TableRow();
                headerRow.Cells.Add(new TableCell(new Paragraph(new Run("Jewellery Description")) { FontWeight = FontWeights.Bold, FontSize = 11 }) { Padding = new Thickness(0, 8, 0, 8) });
                headerRow.Cells.Add(new TableCell(new Paragraph(new Run("Metal")) { FontWeight = FontWeights.Bold, FontSize = 11 }) { Padding = new Thickness(0, 8, 0, 8) });
                headerRow.Cells.Add(new TableCell(new Paragraph(new Run("Weight")) { FontWeight = FontWeights.Bold, FontSize = 11 }) { Padding = new Thickness(0, 8, 0, 8) });
                headerRow.Cells.Add(new TableCell(new Paragraph(new Run("Rate / g")) { FontWeight = FontWeights.Bold, FontSize = 11 }) { Padding = new Thickness(0, 8, 0, 8) });
                headerRow.Cells.Add(new TableCell(new Paragraph(new Run("Qty")) { FontWeight = FontWeights.Bold, FontSize = 11 }) { Padding = new Thickness(0, 8, 0, 8) });
                headerRow.Cells.Add(new TableCell(new Paragraph(new Run("Amount")) { FontWeight = FontWeights.Bold, FontSize = 11, TextAlignment = TextAlignment.Right }) { Padding = new Thickness(0, 8, 0, 8) });
                rowGroup.Rows.Add(headerRow);

                foreach (var item in bill.BillItems)
                {
                    var row = new TableRow();
                    string itemName = item.Item?.Name ?? "Gold/Silver Jewellery Piece";
                    string category = item.Item?.Category ?? "Gold";
                    double weight = item.Item?.Weight ?? 0.0;
                    double price = item.Item?.Price ?? item.Price;
                    double pricePerGram = weight > 0 ? (price / weight) : price;

                    row.Cells.Add(new TableCell(new Paragraph(new Run(itemName)) { FontSize = 10.5 }) { Padding = new Thickness(0, 6, 0, 6) });
                    row.Cells.Add(new TableCell(new Paragraph(new Run(category)) { FontSize = 10.5 }) { Padding = new Thickness(0, 6, 0, 6) });
                    row.Cells.Add(new TableCell(new Paragraph(new Run(weight > 0 ? $"{weight:F2} g" : "-")) { FontSize = 10.5 }) { Padding = new Thickness(0, 6, 0, 6) });
                    row.Cells.Add(new TableCell(new Paragraph(new Run(weight > 0 ? $"₹{pricePerGram:N2}" : "-")) { FontSize = 10.5 }) { Padding = new Thickness(0, 6, 0, 6) });
                    row.Cells.Add(new TableCell(new Paragraph(new Run(item.Quantity.ToString())) { FontSize = 10.5 }) { Padding = new Thickness(0, 6, 0, 6) });
                    row.Cells.Add(new TableCell(new Paragraph(new Run($"₹{item.Price:N2}")) { FontSize = 10.5, TextAlignment = TextAlignment.Right }) { Padding = new Thickness(0, 6, 0, 6) });
                    rowGroup.Rows.Add(row);
                }

                table.RowGroups.Add(rowGroup);
                doc.Blocks.Add(table);

                // Financial Summary block
                var summaryTable = new Table { CellSpacing = 0, Margin = new Thickness(0, 10, 0, 10) };
                summaryTable.Columns.Add(new TableColumn { Width = new GridLength(350) }); // Left (Amount in Words)
                summaryTable.Columns.Add(new TableColumn { Width = new GridLength(210) }); // Right (Amounts breakdown)

                var summaryRowGroup = new TableRowGroup();
                var summaryRow = new TableRow();

                // Amount in Words cell
                string words = AmountToWords(grandTotal);
                var wordsPara = new Paragraph { FontSize = 10, LineHeight = 16 };
                wordsPara.Inlines.Add(new Run("TOTAL AMOUNT IN WORDS:\n") { FontWeight = FontWeights.Bold, Foreground = System.Windows.Media.Brushes.Gray });
                wordsPara.Inlines.Add(new Run(words) { FontStyle = FontStyles.Italic, FontWeight = FontWeights.SemiBold });
                summaryRow.Cells.Add(new TableCell(wordsPara));

                // Amounts breakup cell
                var summaryPara = new Paragraph { TextAlignment = TextAlignment.Right, LineHeight = 20, FontSize = 11 };
                summaryPara.Inlines.Add(new Run($"Subtotal Taxable Value: ₹{bill.TotalAmount:N2}\n"));
                summaryPara.Inlines.Add(new Run($"CGST (1.5%): ₹{cgstAmount:N2}\n") { Foreground = System.Windows.Media.Brushes.Gray });
                summaryPara.Inlines.Add(new Run($"SGST (1.5%): ₹{sgstAmount:N2}\n") { Foreground = System.Windows.Media.Brushes.Gray });
                summaryPara.Inlines.Add(new Bold(new Run($"Grand Total: ₹{grandTotal:N2}")) { FontSize = 16, Foreground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#111111")) });
                summaryRow.Cells.Add(new TableCell(summaryPara));

                summaryRowGroup.Rows.Add(summaryRow);
                summaryTable.RowGroups.Add(summaryRowGroup);
                doc.Blocks.Add(summaryTable);

                // Hallmark certification card
                var hallmarkPara = new Paragraph
                {
                    FontSize = 9.5,
                    Foreground = System.Windows.Media.Brushes.DarkGoldenrod,
                    Background = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFDF0")),
                    BorderBrush = accentBrush,
                    BorderThickness = new Thickness(1),
                    Padding = new Thickness(10),
                    TextAlignment = TextAlignment.Center,
                    Margin = new Thickness(0, 15, 0, 15)
                };
                hallmarkPara.Inlines.Add(new Bold(new Run("✔ BIS HALLMARK CERTIFIED QUALITY & PURITY\n")) { Foreground = accentBrush });
                hallmarkPara.Inlines.Add(new Run("All gold and silver ornaments sold under this tax invoice carry official BIS Hallmark fineness certification stamps."));
                doc.Blocks.Add(hallmarkPara);

                // Terms and Conditions footer
                doc.Blocks.Add(new Paragraph(new Run($"TERMS & CONDITIONS:\n{footerText}"))
                {
                    FontSize = 8.5,
                    Foreground = System.Windows.Media.Brushes.DarkGray,
                    TextAlignment = TextAlignment.Center,
                    Margin = new Thickness(0, 15, 0, 15)
                });

                // Signature Blocks Table
                var signatureTable = new Table { CellSpacing = 0, Margin = new Thickness(0, 30, 0, 0) };
                signatureTable.Columns.Add(new TableColumn { Width = new GridLength(280) });
                signatureTable.Columns.Add(new TableColumn { Width = new GridLength(280) });

                var sigRowGroup = new TableRowGroup();
                var sigRow = new TableRow();

                var customerSigCell = new TableCell(new Paragraph(new Run("\n\n\n___________________________\nCustomer's Signature")) { FontSize = 9.5, TextAlignment = TextAlignment.Left });
                var ownerSigCell = new TableCell(new Paragraph(new Run($"\n\nFor {shopName}\n\n___________________________\nAuthorized Signatory")) { FontSize = 9.5, TextAlignment = TextAlignment.Right, FontWeight = FontWeights.SemiBold });

                sigRow.Cells.Add(customerSigCell);
                sigRow.Cells.Add(ownerSigCell);
                sigRowGroup.Rows.Add(sigRow);
                signatureTable.RowGroups.Add(sigRowGroup);
                doc.Blocks.Add(signatureTable);

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

        private static string AmountToWords(double amount)
        {
            try
            {
                long number = (long)Math.Round(amount);
                if (number == 0) return "Zero Rupees Only";
                if (number < 0) return "Minus " + AmountToWords(Math.Abs(amount));

                string words = "";

                if ((number / 10000000) > 0)
                {
                    words += AmountToWords(number / 10000000) + " Crore ";
                    number %= 10000000;
                }

                if ((number / 100000) > 0)
                {
                    words += AmountToWords(number / 100000) + " Lakh ";
                    number %= 100000;
                }

                if ((number / 1000) > 0)
                {
                    words += AmountToWords(number / 1000) + " Thousand ";
                    number %= 1000;
                }

                if ((number / 100) > 0)
                {
                    words += AmountToWords(number / 100) + " Hundred ";
                    number %= 100;
                }

                if (number > 0)
                {
                    if (words != "") words += "and ";

                    var unitsMap = new[] { "Zero", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten", "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen", "Eighteen", "Nineteen" };
                    var tensMap = new[] { "Zero", "Ten", "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety" };

                    if (number < 20)
                        words += unitsMap[number];
                    else
                    {
                        words += tensMap[number / 10];
                        if ((number % 10) > 0)
                            words += " " + unitsMap[number % 10];
                    }
                }

                return words.Trim() + " Rupees Only";
            }
            catch
            {
                return "";
            }
        }
    }
}
