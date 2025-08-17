using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CarAnalysisDashboard.Services
{
    public class FileService : IFileService
    {
        public async Task<Dictionary<string, List<string>>> GetCarDataFilesAsync(string rootPath)
        {
            return await Task.Run(() =>
            {
                var result = new Dictionary<string, List<string>>();
                
                if (!Directory.Exists(rootPath))
                    return result;
                
                var directories = Directory.GetDirectories(rootPath);
                
                foreach (var dir in directories)
                {
                    var carModel = Path.GetFileName(dir);
                    var csvFiles = new List<string>();
                    
                    // 車種フォルダ内の日付フォルダを探索
                    var dateFolders = Directory.GetDirectories(dir);
                    foreach (var dateFolder in dateFolders)
                    {
                        // 各日付フォルダ内のCSVファイルを取得
                        var files = Directory.GetFiles(dateFolder, "*.csv");
                        csvFiles.AddRange(files);
                    }
                    
                    // 車種フォルダ直下のCSVファイルも取得（互換性のため）
                    var directFiles = Directory.GetFiles(dir, "*.csv");
                    csvFiles.AddRange(directFiles);
                    
                    if (csvFiles.Any())
                    {
                        result[carModel] = csvFiles.OrderBy(f => f).ToList();
                    }
                }
                
                return result;
            });
        }

        public async Task<List<string>> GetExcludeKeywordsAsync(string filePath)
        {
            return await Task.Run(() =>
            {
                if (!File.Exists(filePath))
                    return new List<string>();
                
                return File.ReadAllLines(filePath)
                          .Where(line => !string.IsNullOrWhiteSpace(line))
                          .Select(line => line.Trim())
                          .ToList();
            });
        }

        public List<string> GetCarModels(string rootPath)
        {
            if (!Directory.Exists(rootPath))
                return new List<string>();
            
            return Directory.GetDirectories(rootPath)
                          .Select(Path.GetFileName)
                          .Where(name => !string.IsNullOrEmpty(name))
                          .OrderBy(name => name)
                          .ToList()!;
        }
    }
}