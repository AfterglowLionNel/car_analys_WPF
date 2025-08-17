using System.Collections.Generic;
using System.Threading.Tasks;
using CarAnalysisDashboard.Models;

namespace CarAnalysisDashboard.Services
{
    public interface ICsvDataService
    {
        Task<List<CarData>> LoadCsvFilesAsync(List<string> filePaths);
        Task<List<CarData>> LoadCsvFileAsync(string filePath);
        List<CarData> FilterData(List<CarData> data, FilterSettings filter);
        Task ExportToCsvAsync(List<CarData> data, string filePath);
    }
}