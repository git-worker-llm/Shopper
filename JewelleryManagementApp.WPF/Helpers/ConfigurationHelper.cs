using System.IO;
using Microsoft.Extensions.Configuration;

namespace JewelleryManagementApp.WPF.Helpers
{
    public static class ConfigurationHelper
    {
        public static IConfiguration Configuration { get; private set; } = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();
    }
}
