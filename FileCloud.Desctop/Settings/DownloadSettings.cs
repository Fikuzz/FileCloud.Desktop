using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileCloud.Desktop.Settings
{
    public static class DownloadSettings
    {
        public static string DownloadPath { get; } = "";

        static DownloadSettings()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            var pathFromConfig = config["DownloadSettings:DefaultDownloadPath"];

            if (string.IsNullOrWhiteSpace(pathFromConfig))
            {
                pathFromConfig = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "FileCloudDownloads");
            }

            // Ensure directory exists
            if (!Directory.Exists(pathFromConfig))
            {
                Directory.CreateDirectory(pathFromConfig);
            }

            DownloadPath = pathFromConfig;
        }
    }
}
