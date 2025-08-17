using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CarAnalysisDashboard.Models;
using CsvHelper;
using CsvHelper.Configuration;

namespace CarAnalysisDashboard.Services
{
    public class CsvDataService : ICsvDataService
    {
        public async Task<List<CarData>> LoadCsvFilesAsync(List<string> filePaths)
        {
            var allData = new List<CarData>();
            
            foreach (var filePath in filePaths)
            {
                var data = await LoadCsvFileAsync(filePath);
                allData.AddRange(data);
            }
            
            return RemoveDuplicates(allData);
        }

        public async Task<List<CarData>> LoadCsvFileAsync(string filePath)
        {
            return await Task.Run(() =>
            {
                var data = new List<CarData>();
                
                if (!File.Exists(filePath))
                    return data;
                
                // Shift-JISエンコーディングを設定
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                var encoding = Encoding.GetEncoding("Shift_JIS");
                
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    Encoding = encoding,
                    HasHeaderRecord = true,
                    MissingFieldFound = null,
                    BadDataFound = null,
                    HeaderValidated = null  // ヘッダー検証を無効化
                };
                
                using var reader = new StreamReader(filePath, encoding);
                using var csv = new CsvReader(reader, config);
                
                csv.Context.RegisterClassMap<CarDataCsvMap>();
                
                var records = new List<CarData>();
                try
                {
                    records = csv.GetRecords<CarData>().ToList();
                    System.Diagnostics.Debug.WriteLine($"CSV読み込み成功: {filePath} - {records.Count}件");
                    
                    // 価格が0の車両数のみチェック（高額車両表示は削除）
                    var zeroPriceCars = records.Count(r => r.Price == 0);
                    if (zeroPriceCars > 0)
                    {
                        System.Diagnostics.Debug.WriteLine($"価格が0円の車両: {zeroPriceCars}件");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"CSV読み込みエラー: {filePath} - {ex.Message}");
                    throw;
                }
                
                var fileName = Path.GetFileNameWithoutExtension(filePath);
                var dateStr = fileName.Split('_').Take(3).ToArray();
                if (dateStr.Length >= 3 && 
                    int.TryParse(dateStr[0], out var year) &&
                    int.TryParse(dateStr[1], out var month) &&
                    int.TryParse(dateStr[2], out var day))
                {
                    var dataDate = new DateTime(year, month, day);
                    foreach (var record in records)
                    {
                        record.DataDate = dataDate;
                        record.CarModel = Path.GetFileName(Path.GetDirectoryName(filePath)) ?? "";
                        record.Id = Guid.NewGuid().ToString();
                    }
                }
                
                return records;
            });
        }

        public List<CarData> FilterData(List<CarData> data, FilterSettings filter)
        {
            var filtered = data.AsEnumerable();
            
            System.Diagnostics.Debug.WriteLine($"フィルター開始: 元データ{data.Count}件");
            System.Diagnostics.Debug.WriteLine($"価格フィルター: {filter.MinPrice:N0}円 - {filter.MaxPrice:N0}円");
            System.Diagnostics.Debug.WriteLine($"年式フィルター: {filter.MinYear} - {filter.MaxYear}");
            
            if (filter.SelectedGrades?.Any() == true)
            {
                var beforeGrade = filtered.Count();
                filtered = filtered.Where(d => filter.SelectedGrades.Contains(d.Grade));
                System.Diagnostics.Debug.WriteLine($"グレードフィルター後: {beforeGrade}件 → {filtered.Count()}件");
            }
            
            var beforeYear = filtered.Count();
            filtered = filtered.Where(d => d.Year >= filter.MinYear && d.Year <= filter.MaxYear);
            System.Diagnostics.Debug.WriteLine($"年式フィルター後: {beforeYear}件 → {filtered.Count()}件");
            
            var beforePrice = filtered.Count();
            var dataBeforeFilter = filtered.ToList(); // フィルター前のデータを保存
            filtered = filtered.Where(d => d.Price >= filter.MinPrice && d.Price <= filter.MaxPrice);
            
            // 価格フィルターで多数の車両が除外された場合のみ警告
            var excludedCount = dataBeforeFilter.Count(d => d.Price > filter.MaxPrice);
            if (excludedCount > filtered.Count() * 0.1) // 10%以上が除外された場合
            {
                System.Diagnostics.Debug.WriteLine($"警告: 価格フィルターで{excludedCount}件が除外されました (上限:{filter.MaxPrice/10000:N0}万円)");
            }
            
            System.Diagnostics.Debug.WriteLine($"価格フィルター後: {beforePrice}件 → {filtered.Count()}件");
            
            var beforeMileage = filtered.Count();
            filtered = filtered.Where(d => d.Mileage >= filter.MinMileage && d.Mileage <= filter.MaxMileage);
            System.Diagnostics.Debug.WriteLine($"走行距離フィルター({filter.MinMileage}km-{filter.MaxMileage}km)後: {beforeMileage}件 → {filtered.Count()}件");
            
            if (filter.SelectedTransmission != "すべて")
            {
                filtered = filtered.Where(d => d.Transmission == filter.SelectedTransmission);
            }
            
            if (filter.RepairHistoryFilter == "なし")
            {
                filtered = filtered.Where(d => !d.HasRepairHistory);
            }
            else if (filter.RepairHistoryFilter == "あり")
            {
                filtered = filtered.Where(d => d.HasRepairHistory);
            }
            
            if (filter.ExcludeKeywords?.Any() == true)
            {
                foreach (var keyword in filter.ExcludeKeywords)
                {
                    filtered = filtered.Where(d => 
                        !d.Name.Contains(keyword) && 
                        !d.Grade.Contains(keyword) &&
                        !d.Comments.Contains(keyword));
                }
            }
            
            var result = filtered.ToList();
            System.Diagnostics.Debug.WriteLine($"フィルター完了: 最終結果{result.Count}件");
            
            return result;
        }

        public async Task ExportToCsvAsync(List<CarData> data, string filePath)
        {
            await Task.Run(() =>
            {
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    Encoding = Encoding.UTF8,
                    HasHeaderRecord = true
                };
                
                using var writer = new StreamWriter(filePath, false, new UTF8Encoding(true));
                using var csv = new CsvWriter(writer, config);
                
                csv.WriteRecords(data);
            });
        }

        private List<CarData> RemoveDuplicates(List<CarData> data)
        {
            return data.GroupBy(d => new { d.Name, d.Grade, d.Price, d.Year, d.Mileage })
                      .Select(g => g.First())
                      .ToList();
        }
    }
}