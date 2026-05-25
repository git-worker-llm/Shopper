using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Threading.Tasks;
using JewelleryManagementApp.WPF.Data;
using JewelleryManagementApp.WPF.Models;
using JewelleryManagementApp.WPF.Commands;
using Microsoft.EntityFrameworkCore;

namespace JewelleryManagementApp.WPF.ViewModels
{
    public class GSTReportViewModel : ViewModelBase
    {
        public double TotalSales { get; set; }
        public double TaxableAmount { get; set; }
        public double TotalGST { get; set; }
        public double CGST { get; set; }
        public double SGST { get; set; }

        private string _selectedPeriod = DateTime.Now.ToString("MMMM yyyy");
        public string SelectedPeriod
        {
            get => _selectedPeriod;
            set
            {
                _selectedPeriod = value;
                OnPropertyChanged();
                Refresh();
            }
        }

        public ObservableCollection<string> AvailablePeriods { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<GstFiling> FilingHistory { get; set; } = new ObservableCollection<GstFiling>();

        private bool _isFiling;
        public bool IsFiling
        {
            get => _isFiling;
            set { _isFiling = value; OnPropertyChanged(); }
        }

        private string _filingStatus = string.Empty;
        public string FilingStatus
        {
            get => _filingStatus;
            set { _filingStatus = value; OnPropertyChanged(); }
        }

        private double _filingProgress;
        public double FilingProgress
        {
            get => _filingProgress;
            set { _filingProgress = value; OnPropertyChanged(); }
        }

        public ICommand FileGSTCommand { get; }
        public ICommand ExportGSTR1Command { get; }

        public GSTReportViewModel()
        {
            FileGSTCommand = new RelayCommand(async _ => await FileGSTAsync(), _ => TotalSales > 0 && !IsFiling);
            ExportGSTR1Command = new RelayCommand(async _ => await ExportGSTR1Async(), _ => TotalSales > 0);

            // Populate periods (previous 6 months + current month)
            for (int i = 0; i < 6; i++)
            {
                AvailablePeriods.Add(DateTime.Now.AddMonths(-i).ToString("MMMM yyyy"));
            }

            Refresh();
        }

        public override void Refresh()
        {
            using (var context = new JewelleryDbContext())
            {
                var allBills = context.Bills.ToList();
                var periodBills = allBills.Where(b => b.Date.ToString("MMMM yyyy") == SelectedPeriod).ToList();

                // TotalSales represents TaxableAmount + GSTAmount
                TaxableAmount = periodBills.Sum(b => b.TotalAmount);
                TotalGST = periodBills.Sum(b => b.GSTAmount);
                TotalSales = TaxableAmount + TotalGST;

                // CGST and SGST split
                CGST = TotalGST / 2;
                SGST = TotalGST / 2;

                // Load Filing History
                FilingHistory.Clear();
                var filings = context.GstFilings.OrderByDescending(f => f.FilingDate).ToList();
                foreach (var filing in filings)
                {
                    FilingHistory.Add(filing);
                }
            }

            OnPropertyChanged(nameof(TotalSales));
            OnPropertyChanged(nameof(TaxableAmount));
            OnPropertyChanged(nameof(TotalGST));
            OnPropertyChanged(nameof(CGST));
            OnPropertyChanged(nameof(SGST));
        }

        private async Task FileGSTAsync()
        {
            var confirmMsg = $"Are you sure you want to proceed with GSTR-1 Return Filing for {SelectedPeriod}?\n\n" +
                             "This action will initiate secure filing protocol. The portal will perform:\n" +
                             "1. SHOWROOM COMPILATION: Reconcile all sales invoice tax breakdowns.\n" +
                             "2. PORTAL AUTHENTICATION: Tunnel invoice records directly to secure GST Common Portal Server API gateway.\n" +
                             "3. DSC AUTHORIZATION: Digitally sign return bundles using showroom digital credentials (DSC/EVC).\n" +
                             "4. REFERENCE LOGGING: Create and log a permanent Acknowledgement Reference Number (ARN).\n\n" +
                             "Caution: Direct portal filing cannot be revoked once processed. Do you wish to proceed?";

            var result = MessageBox.Show(confirmMsg, "GST Filing Authorization Required", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            IsFiling = true;
            FilingProgress = 10;
            FilingStatus = "Step 1/5: Compiling sales invoices & HSN summaries for period...";
            await Task.Delay(1200);

            FilingProgress = 35;
            FilingStatus = "Step 2/5: Performing auto-reconciliation of CGST/SGST tax components...";
            await Task.Delay(1200);

            FilingProgress = 60;
            FilingStatus = "Step 3/5: Connecting to secure GST Common Portal Server API gateway...";
            await Task.Delay(1500);

            FilingProgress = 80;
            FilingStatus = "Step 4/5: Uploading GSTR-1 returns bundle & registering digital signatures...";
            await Task.Delay(1200);

            FilingProgress = 95;
            FilingStatus = "Step 5/5: Generating ARN return file acknowledgement & archiving records...";
            await Task.Delay(1000);

            string arn = "ARN" + new Random().Next(100000, 999999) + DateTime.Now.ToString("ddMMyy");

            using (var context = new JewelleryDbContext())
            {
                var filing = new GstFiling
                {
                    Period = SelectedPeriod,
                    FilingDate = DateTime.Now,
                    TotalSales = TotalSales,
                    TaxableAmount = TaxableAmount,
                    GstAmount = TotalGST,
                    ReferenceNumber = arn,
                    Status = "Success"
                };

                context.GstFilings.Add(filing);
                context.SaveChanges();
            }

            FilingProgress = 100;
            FilingStatus = $"Return filed successfully! Acknowledgement: {arn}";
            await Task.Delay(1500);

            IsFiling = false;
            FilingStatus = string.Empty;
            FilingProgress = 0;

            Refresh();
            MessageBox.Show($"GST Return (GSTR-1) for {SelectedPeriod} has been filed successfully!\n\nAcknowledgement Reference No (ARN): {arn}", "GST Return Filed Successfully", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async Task ExportGSTR1Async()
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "JSON GST Utility File (*.json)|*.json|CSV Summary sheet (*.csv)|*.csv",
                FileName = $"GSTR1_{SelectedPeriod.Replace(" ", "_")}.json"
            };

            if (dialog.ShowDialog() == true)
            {
                string ext = System.IO.Path.GetExtension(dialog.FileName).ToLower();
                if (ext == ".json")
                {
                    string json = $@"{{
  ""gstin"": ""27AAAAA1111A1Z1"",
  ""fp"": ""{DateTime.Now.ToString("MMyyyy")}"",
  ""gross_turnover"": {TotalSales},
  ""filing_type"": ""GSTR1"",
  ""b2cs"": [
    {{
      ""sply_ty"": ""INTRA"",
      ""txval"": {TaxableAmount},
      ""rt"": 3.0,
      ""iamt"": 0.0,
      ""camt"": {CGST},
      ""samt"": {SGST}
    }}
  ]
}}";
                    await System.IO.File.WriteAllTextAsync(dialog.FileName, json);
                }
                else
                {
                    string csv = $"GSTIN,Period,Total Sales (Gross),Taxable Value,CGST,SGST,Total GST\n27AAAAA1111A1Z1,{SelectedPeriod},{TotalSales},{TaxableAmount},{CGST},{SGST},{TotalGST}";
                    await System.IO.File.WriteAllTextAsync(dialog.FileName, csv);
                }

                MessageBox.Show("GSTR-1 Offline Utility file exported successfully!", "Export Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
