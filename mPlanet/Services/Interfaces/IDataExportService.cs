using System.Collections.Generic;
using System.Threading.Tasks;
using mPlanet.Models;

namespace mPlanet.Services.Interfaces
{
    public interface IDataExportService
    {
        Task<bool> ExportAsync(IEnumerable<TagInfo> tags, string filePath, string format = "json");
    }
}
