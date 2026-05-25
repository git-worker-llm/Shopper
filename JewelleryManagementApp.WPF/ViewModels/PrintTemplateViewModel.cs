using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;
using System.IO;
using System.Xml;
using JewelleryManagementApp.WPF.Commands;
using JewelleryManagementApp.WPF.Data;
using JewelleryManagementApp.WPF.Models;
using Microsoft.EntityFrameworkCore;

namespace JewelleryManagementApp.WPF.ViewModels
{
    public class PrintTemplateViewModel : ViewModelBase
    {
        private readonly JewelleryDbContext _dbContext;
        private InvoiceTemplate _template = null!;

        public InvoiceTemplate Template
        {
            get => _template;
            set 
            { 
                _template = value; 
                OnPropertyChanged(); 
                OnPropertyChanged(nameof(AccentColor));
                OnPropertyChanged(nameof(HeaderFontSize));
                OnPropertyChanged(nameof(SelectedFontFamily));
                OnPropertyChanged(nameof(FooterText));
                UpdatePreview(); 
            }
        }

        // Simpler designer bindings
        public string AccentColor
        {
            get => Template.AccentColor;
            set { Template.AccentColor = value; OnPropertyChanged(); UpdatePreview(); }
        }

        public double HeaderFontSize
        {
            get => Template.HeaderFontSize;
            set { Template.HeaderFontSize = value; OnPropertyChanged(); UpdatePreview(); }
        }

        public string SelectedFontFamily
        {
            get => Template.SelectedFontFamily;
            set { Template.SelectedFontFamily = value; OnPropertyChanged(); UpdatePreview(); }
        }

        public string FooterText
        {
            get => Template.FooterText;
            set { Template.FooterText = value; OnPropertyChanged(); UpdatePreview(); }
        }

        public ObservableCollection<string> AvailableColors { get; } = new ObservableCollection<string> { "Gold", "Charcoal", "RoseGold", "DarkBlue" };
        public ObservableCollection<string> AvailableFonts { get; } = new ObservableCollection<string> { "Segoe UI", "Georgia", "Consolas", "Trebuchet MS", "Times New Roman" };

        private FlowDocument _previewDocument = new FlowDocument();
        public FlowDocument PreviewDocument
        {
            get => _previewDocument;
            set { _previewDocument = value; OnPropertyChanged(); }
        }

        public ICommand SaveTemplateCommand { get; }
        public ICommand ResetTemplateCommand { get; }

        public PrintTemplateViewModel(JewelleryDbContext dbContext)
        {
            _dbContext = dbContext;
            LoadTemplate();
            SaveTemplateCommand = new RelayCommand(_ => SaveTemplate());
            ResetTemplateCommand = new RelayCommand(_ => ResetTemplate());
        }

        private void LoadTemplate()
        {
            using (var db = new JewelleryDbContext())
            {
                Template = db.InvoiceTemplates.FirstOrDefault() ?? CreateDefaultTemplate();
            }
        }

        public void UpdatePreview()
        {
            try
            {
                var doc = new FlowDocument
                {
                    FontFamily = new FontFamily(SelectedFontFamily),
                    PagePadding = new System.Windows.Thickness(30),
                    Background = Brushes.White,
                    Foreground = Brushes.Black
                };

                // Accent color lookup
                string accentHex = "#D4AF37"; // Gold
                if (AccentColor == "Charcoal") accentHex = "#333333";
                else if (AccentColor == "RoseGold") accentHex = "#B76E79";
                else if (AccentColor == "DarkBlue") accentHex = "#2980B9";

                var accentBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(accentHex));

                // Tax Invoice Subtitle
                doc.Blocks.Add(new Paragraph(new Run("TAX INVOICE"))
                {
                    FontSize = 10,
                    FontWeight = System.Windows.FontWeights.Bold,
                    Foreground = Brushes.DarkGray,
                    TextAlignment = System.Windows.TextAlignment.Center,
                    Margin = new System.Windows.Thickness(0, 0, 0, 5)
                });

                doc.Blocks.Add(new Paragraph(new Run(Template.ShopName))
                {
                    FontSize = HeaderFontSize,
                    FontWeight = System.Windows.FontWeights.Bold,
                    Foreground = accentBrush,
                    TextAlignment = System.Windows.TextAlignment.Center,
                    Margin = new System.Windows.Thickness(0, 0, 0, 4)
                });

                doc.Blocks.Add(new Paragraph(new Run($"Address: {Template.Address}\nGSTIN: {Template.GSTNumber}"))
                {
                    FontSize = 10,
                    Foreground = Brushes.DimGray,
                    TextAlignment = System.Windows.TextAlignment.Center,
                    Margin = new System.Windows.Thickness(0, 0, 0, 15)
                });

                // Meta Info Section (Vertical layout to guarantee 100% preview visibility on all panel widths)
                var metaPara = new Paragraph { FontSize = 10.5, LineHeight = 16, Margin = new System.Windows.Thickness(0, 5, 0, 15) };
                
                // Invoice details
                metaPara.Inlines.Add(new Bold(new Run("INVOICE DETAILS:\n")) { Foreground = accentBrush });
                metaPara.Inlines.Add(new Run("Invoice Number: INV-00124   |   Date: Monday, 25 May 2026   |   Time: 09:30 PM\n"));

                // Customer details
                metaPara.Inlines.Add(new Bold(new Run("BILLED TO:\n")) { Foreground = accentBrush });
                metaPara.Inlines.Add(new Run("Customer Name: Rajesh Mehta   |   Phone: +91 98765 43210\n"));
                metaPara.Inlines.Add(new Run("HSN / SAC Code: 7113 (Jewellery)"));
                doc.Blocks.Add(metaPara);

                // Dummy table for preview (6 columns)
                var table = new Table { CellSpacing = 0, BorderThickness = new System.Windows.Thickness(0, 1, 0, 1), BorderBrush = Brushes.LightGray };
                table.Columns.Add(new TableColumn { Width = new System.Windows.GridLength(140) }); // Description
                table.Columns.Add(new TableColumn { Width = new System.Windows.GridLength(45) });  // Metal
                table.Columns.Add(new TableColumn { Width = new System.Windows.GridLength(45) });  // Weight
                table.Columns.Add(new TableColumn { Width = new System.Windows.GridLength(55) });  // Rate
                table.Columns.Add(new TableColumn { Width = new System.Windows.GridLength(25) });  // Qty
                table.Columns.Add(new TableColumn { Width = new System.Windows.GridLength(70) });  // Total

                var group = new TableRowGroup();
                var headerRow = new TableRow();
                headerRow.Cells.Add(new TableCell(new Paragraph(new Run("Jewellery Description")) { FontWeight = System.Windows.FontWeights.Bold, FontSize = 10 }));
                headerRow.Cells.Add(new TableCell(new Paragraph(new Run("Metal")) { FontWeight = System.Windows.FontWeights.Bold, FontSize = 10 }));
                headerRow.Cells.Add(new TableCell(new Paragraph(new Run("Weight")) { FontWeight = System.Windows.FontWeights.Bold, FontSize = 10 }));
                headerRow.Cells.Add(new TableCell(new Paragraph(new Run("Rate / g")) { FontWeight = System.Windows.FontWeights.Bold, FontSize = 10 }));
                headerRow.Cells.Add(new TableCell(new Paragraph(new Run("Qty")) { FontWeight = System.Windows.FontWeights.Bold, FontSize = 10 }));
                headerRow.Cells.Add(new TableCell(new Paragraph(new Run("Amount")) { FontWeight = System.Windows.FontWeights.Bold, FontSize = 10, TextAlignment = System.Windows.TextAlignment.Right }));
                group.Rows.Add(headerRow);

                var itemRow = new TableRow();
                itemRow.Cells.Add(new TableCell(new Paragraph(new Run("22K Gold Antique Ring")) { FontSize = 9.5 }));
                itemRow.Cells.Add(new TableCell(new Paragraph(new Run("Gold")) { FontSize = 9.5 }));
                itemRow.Cells.Add(new TableCell(new Paragraph(new Run("15.20 g")) { FontSize = 9.5 }));
                itemRow.Cells.Add(new TableCell(new Paragraph(new Run("₹5,592.10")) { FontSize = 9.5 }));
                itemRow.Cells.Add(new TableCell(new Paragraph(new Run("1")) { FontSize = 9.5 }));
                itemRow.Cells.Add(new TableCell(new Paragraph(new Run("₹85,000.00")) { FontSize = 9.5, TextAlignment = System.Windows.TextAlignment.Right }));
                group.Rows.Add(itemRow);

                table.RowGroups.Add(group);
                doc.Blocks.Add(table);

                // Financial Summary
                var summaryTable = new Table { CellSpacing = 0, Margin = new System.Windows.Thickness(0, 5, 0, 5) };
                summaryTable.Columns.Add(new TableColumn { Width = new System.Windows.GridLength(210) });
                summaryTable.Columns.Add(new TableColumn { Width = new System.Windows.GridLength(150) });

                var sumRowGroup = new TableRowGroup();
                var sumRow = new TableRow();

                var wordsPara = new Paragraph { FontSize = 9.5, LineHeight = 15 };
                wordsPara.Inlines.Add(new Run("TOTAL AMOUNT IN WORDS:\n") { FontWeight = System.Windows.FontWeights.Bold, Foreground = Brushes.Gray });
                wordsPara.Inlines.Add(new Run("Rupees Eighty-Seven Thousand Five Hundred Fifty Only") { FontStyle = System.Windows.FontStyles.Italic, FontWeight = System.Windows.FontWeights.SemiBold });
                sumRow.Cells.Add(new TableCell(wordsPara));

                var summary = new Paragraph { TextAlignment = System.Windows.TextAlignment.Right, LineHeight = 18, FontSize = 10.5 };
                summary.Inlines.Add(new Run("Subtotal Taxable: ₹85,000.00\n"));
                summary.Inlines.Add(new Run("CGST (1.5%): ₹1,275.00\n") { Foreground = Brushes.Gray });
                summary.Inlines.Add(new Run("SGST (1.5%): ₹1,275.00\n") { Foreground = Brushes.Gray });
                summary.Inlines.Add(new Bold(new Run("Grand Total: ₹87,550.00")) { FontSize = 14, Foreground = Brushes.Black });
                sumRow.Cells.Add(new TableCell(summary));

                sumRowGroup.Rows.Add(sumRow);
                summaryTable.RowGroups.Add(sumRowGroup);
                doc.Blocks.Add(summaryTable);

                // Hallmark certification card
                var hallmarkPara = new Paragraph
                {
                    FontSize = 9,
                    Foreground = Brushes.DarkGoldenrod,
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFDF0")),
                    BorderBrush = accentBrush,
                    BorderThickness = new System.Windows.Thickness(1),
                    Padding = new System.Windows.Thickness(8),
                    TextAlignment = System.Windows.TextAlignment.Center,
                    Margin = new System.Windows.Thickness(0, 10, 0, 10)
                };
                hallmarkPara.Inlines.Add(new Bold(new Run("✔ BIS HALLMARK CERTIFIED QUALITY & PURITY\n")) { Foreground = accentBrush });
                hallmarkPara.Inlines.Add(new Run("All gold and silver ornaments sold carry official BIS Hallmark fineness certification."));
                doc.Blocks.Add(hallmarkPara);

                // Footer
                doc.Blocks.Add(new Paragraph(new Run($"TERMS & CONDITIONS:\n{FooterText}"))
                {
                    FontSize = 8,
                    Foreground = Brushes.DarkGray,
                    TextAlignment = System.Windows.TextAlignment.Center,
                    Margin = new System.Windows.Thickness(0, 10, 0, 10)
                });

                // Signature Blocks
                var sigTable = new Table { CellSpacing = 0, Margin = new System.Windows.Thickness(0, 20, 0, 0) };
                sigTable.Columns.Add(new TableColumn { Width = new System.Windows.GridLength(180) });
                sigTable.Columns.Add(new TableColumn { Width = new System.Windows.GridLength(180) });

                var sigRowGroup = new TableRowGroup();
                var sigRow = new TableRow();
                sigRow.Cells.Add(new TableCell(new Paragraph(new Run("\n\n___________________________\nCustomer's Signature")) { FontSize = 9, TextAlignment = System.Windows.TextAlignment.Left }));
                sigRow.Cells.Add(new TableCell(new Paragraph(new Run($"\n\nFor {Template.ShopName}\n\n___________________________\nAuthorized Signatory")) { FontSize = 9, TextAlignment = System.Windows.TextAlignment.Right, FontWeight = System.Windows.FontWeights.SemiBold }));
                sigRowGroup.Rows.Add(sigRow);
                sigTable.RowGroups.Add(sigRowGroup);
                doc.Blocks.Add(sigTable);

                PreviewDocument = doc;
            }
            catch { }
        }

        public void OnXamlChanged() => UpdatePreview();

        private InvoiceTemplate CreateDefaultTemplate()
        {
            return new InvoiceTemplate
            {
                ShopName = "AuraJewels Luxury Salon",
                GSTNumber = "27AAAAA1111A1Z1",
                Address = "Gold Souk, MG Road, Mumbai, India",
                TemplateXaml = "<FlowDocument/>",
                AccentColor = "Gold",
                HeaderFontSize = 22.0,
                SelectedFontFamily = "Segoe UI",
                FooterText = "Thank you for shopping with us! Terms: Gold sold can be exchanged at current rate less melting loss."
            };
        }

        private void ResetTemplate()
        {
            Template = CreateDefaultTemplate();
            System.Windows.MessageBox.Show("Template configurations reset to defaults!", "Reset Completed", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
        }

        private void SaveTemplate()
        {
            try
            {
                using (var db = new JewelleryDbContext())
                {
                    var existing = db.InvoiceTemplates.FirstOrDefault();
                    if (existing == null)
                    {
                        db.InvoiceTemplates.Add(Template);
                    }
                    else
                    {
                        existing.ShopName = Template.ShopName;
                        existing.GSTNumber = Template.GSTNumber;
                        existing.Address = Template.Address;
                        existing.TemplateXaml = Template.TemplateXaml;
                        existing.AccentColor = AccentColor;
                        existing.HeaderFontSize = HeaderFontSize;
                        existing.SelectedFontFamily = SelectedFontFamily;
                        existing.FooterText = FooterText;
                        db.InvoiceTemplates.Update(existing);
                    }
                    db.SaveChanges();
                }
                System.Windows.MessageBox.Show("Invoice design template updated successfully!", "Design Saved", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Failed to save design: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        public override void Refresh()
        {
            LoadTemplate();
        }
    }
}
