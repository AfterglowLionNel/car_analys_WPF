using System;

namespace CarAnalysisDashboard.Models
{
    public class CarData
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;  // 車種名
        public string Model { get; set; } = string.Empty;  // モデル
        public string Grade { get; set; } = string.Empty;  // グレード
        public long Price { get; set; }  // 支払総額（円単位の整数）
        public int Year { get; set; }  // 年式
        public int Mileage { get; set; }  // 走行距離
        public string Transmission { get; set; } = string.Empty;  // ミッション
        public bool HasRepairHistory { get; set; }  // 修復歴
        public string EngineCapacity { get; set; } = string.Empty;  // 排気量
        public string AcquisitionDateTime { get; set; } = string.Empty;  // 取得日時
        public string AcquisitionDate { get; set; } = string.Empty;  // 取得日
        public string AcquisitionTime { get; set; } = string.Empty;  // 取得時刻
        public string SourceUrl { get; set; } = string.Empty;  // ソースURL
        public string DetailUrl { get; set; } = string.Empty;  // 車両URL
        
        // オプショナルフィールド（現在のCSVには存在しない）
        public string Color { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public DateTime DataDate { get; set; }
        public string CarModel { get; set; } = string.Empty;
        public string FuelType { get; set; } = string.Empty;
        public string DriveType { get; set; } = string.Empty;
        public string BodyType { get; set; } = string.Empty;
        public int? Doors { get; set; }
        public int? Seats { get; set; }
        public string Inspection { get; set; } = string.Empty;
        public string Warranty { get; set; } = string.Empty;
        public string RecyclingFee { get; set; } = string.Empty;
        public string LegalMaintenance { get; set; } = string.Empty;
        public string Dealer { get; set; } = string.Empty;
        public string Comments { get; set; } = string.Empty;
    }
}