using System.Collections.Generic;

namespace CarAnalysisDashboard.Models
{
    public class FilterSettings
    {
        public List<string> SelectedGrades { get; set; } = new();
        public int MinYear { get; set; }
        public int MaxYear { get; set; }
        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }
        public int MinMileage { get; set; } = 0;
        public int MaxMileage { get; set; } = int.MaxValue;
        public string SelectedTransmission { get; set; } = "すべて";
        public string RepairHistoryFilter { get; set; } = "すべて";
        public List<string> ExcludeKeywords { get; set; } = new();
    }
}