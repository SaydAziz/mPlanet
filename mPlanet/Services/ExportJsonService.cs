using mPlanet.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using mPlanet.Models;
using System.IO;
using mPlanet.Configuration;
using System.Linq;
using Newtonsoft.Json;

namespace mPlanet.Services
{
    public class ExportJsonService : IDataExportService
    {

        public async Task<bool> ExportAsync(IEnumerable<TagInfo> tags, string filePath, string format = "json")
        {
            try
            {
                if (!format.Equals("json", StringComparison.OrdinalIgnoreCase))
                {
                    throw new NotSupportedException($"Export format '{format}' is not supported. Only 'json' format is available.");
                }

                if (tags == null)
                {
                    throw new ArgumentNullException(nameof(tags));
                }

                if (string.IsNullOrWhiteSpace(filePath))
                {
                    filePath = $"RFID_Scan_{DateTime.Now:ddMMyyyHHmmss}.json";
                }

                if (!filePath.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                {
                    filePath += ".json";
                }

                string fullPath;
                if (Path.IsPathRooted(filePath))
                {
                    fullPath = filePath;
                }
                else
                {
                    var exportPath = AppSettings.DefaultExportPath;
                    if (!Directory.Exists(exportPath))
                    {
                        Directory.CreateDirectory(exportPath);
                    }
                    fullPath = Path.Combine(exportPath, filePath);
                }

                var directory = Path.GetDirectoryName(fullPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var exportData = new
                {
                    ScanDate = DateTime.Now.ToString("dd.MM.yyyy"),
                    ExportTime = DateTime.Now.ToString("HH:mm:ss"),
                    TagCount = tags.Count(),
                    Tags = tags
                    
                };

                var jsonData = JsonConvert.SerializeObject(exportData, Formatting.Indented);

                await Task.Run(() => File.WriteAllText(fullPath, jsonData));

                Console.WriteLine($"Successfully exported {tags.Count()} tags to: {fullPath}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Export failed: {ex.Message}");
                return false;
            }
        }

        public string GetExportPath(string fileName = null)
        {
            fileName = fileName ?? $"RFID_Scan_{DateTime.Now:yyyyMMddHHmmss}.json";

            if (!fileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
                fileName += ".json";
            }

            return Path.Combine(AppSettings.DefaultExportPath, fileName);
        }
    }
}
