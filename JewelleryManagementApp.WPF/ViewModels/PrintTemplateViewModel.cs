using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Documents;
using System.Windows.Markup;
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
        private InvoiceTemplate _template;

        public InvoiceTemplate Template
        {
            get => _template;
            set { _template = value; OnPropertyChanged(); }
        }

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
            _template = _dbContext.InvoiceTemplates.FirstOrDefault() ?? CreateDefaultTemplate();
            SaveTemplateCommand = new RelayCommand(_ => SaveTemplate());
            ResetTemplateCommand = new RelayCommand(_ => ResetTemplate());
            UpdatePreview();
        }

        public void UpdatePreview()
        {
            try
            {
                string xaml = Template.TemplateXaml;
                xaml = xaml.Replace("{{ShopName}}", "Preview Shop")
                           .Replace("{{GSTNumber}}", "123456789")
                           .Replace("{{Address}}", "123 Preview St");

                using (var stringReader = new StringReader(xaml))
                {
                    using (var xmlReader = XmlReader.Create(stringReader))
                    {
                        PreviewDocument = (FlowDocument)XamlReader.Load(xmlReader);
                    }
                }
            }
            catch { }
        }

        public void OnXamlChanged() => UpdatePreview();

        private InvoiceTemplate CreateDefaultTemplate()
        {
            return new InvoiceTemplate
            {
                ShopName = "My Jewelry Shop",
                TemplateXaml = "<FlowDocument xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"><Paragraph FontSize=\"24\" FontWeight=\"Bold\">{{ShopName}}</Paragraph><Paragraph>GST: {{GSTNumber}}</Paragraph></FlowDocument>"
            };
        }

        private void ResetTemplate()
        {
            Template = CreateDefaultTemplate();
        }

        private void SaveTemplate()
        {
            if (_template.Id == 0) _dbContext.InvoiceTemplates.Add(_template);
            else _dbContext.InvoiceTemplates.Update(_template);
            _dbContext.SaveChanges();
        }

        public override void Refresh()
        {
            Template = _dbContext.InvoiceTemplates.FirstOrDefault() ?? new InvoiceTemplate();
        }
    }
}
