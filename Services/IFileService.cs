using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarAnalysisDashboard.Services
{
    public interface IFileService
    {
        Task<Dictionary<string, List<string>>> GetCarDataFilesAsync(string rootPath);
        Task<List<string>> GetExcludeKeywordsAsync(string filePath);
        List<string> GetCarModels(string rootPath);
    }
}