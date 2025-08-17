using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CarAnalysisDashboard.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CarAnalysisDashboard.ViewModels
{
    public partial class FilterViewModel : ViewModelBase
    {
        public event EventHandler? FilterChanged;

        [ObservableProperty]
        private ObservableCollection<GradeFilterItem> _grades = new();

        [ObservableProperty]
        private int _minYear = 2000;

        [ObservableProperty]
        private int _maxYear = DateTime.Now.Year;

        [ObservableProperty]
        private decimal _minPrice = 0;

        [ObservableProperty]
        private decimal _maxPrice = 1000;

        [ObservableProperty]
        private int _minMileage = 0;

        [ObservableProperty]
        private int _maxMileage = 200000;

        [ObservableProperty]
        private ObservableCollection<string> _transmissionTypes = new()
        {
            "すべて", "AT", "CVT", "MT", "その他"
        };

        [ObservableProperty]
        private string _selectedTransmission = "すべて";

        [ObservableProperty]
        private ObservableCollection<string> _repairHistoryOptions = new()
        {
            "すべて", "なし", "あり"
        };

        [ObservableProperty]
        private ObservableCollection<string> _yearMinOptions = new();
        
        [ObservableProperty]
        private ObservableCollection<string> _yearMaxOptions = new();

        [ObservableProperty]
        private string _selectedMinYear = "下限なし";

        [ObservableProperty]
        private string _selectedMaxYear = "上限なし";

        [ObservableProperty]
        private ObservableCollection<string> _mileageMinOptions = new();
        
        [ObservableProperty]
        private ObservableCollection<string> _mileageMaxOptions = new();

        [ObservableProperty]
        private string _selectedMinMileage = "下限なし";

        [ObservableProperty]
        private string _selectedMaxMileage = "上限なし";

        [ObservableProperty]
        private string _selectedRepairHistory = "すべて";

        [ObservableProperty]
        private int _yearRangeMin = 1990;

        [ObservableProperty]
        private int _yearRangeMax = DateTime.Now.Year;

        [ObservableProperty]
        private decimal _priceRangeMin = 0;

        [ObservableProperty]
        private decimal _priceRangeMax = 5000;

        [ObservableProperty]
        private int _mileageRangeMin = 0;

        [ObservableProperty]
        private int _mileageRangeMax = 500000;

        public FilterViewModel()
        {
            SelectAllGradesCommand = new RelayCommand(SelectAllGrades);
            ClearAllGradesCommand = new RelayCommand(ClearAllGrades);
            ResetFiltersCommand = new RelayCommand(ResetFilters);
        }

        public IRelayCommand SelectAllGradesCommand { get; }
        public IRelayCommand ClearAllGradesCommand { get; }
        public IRelayCommand ResetFiltersCommand { get; }

        partial void OnMinYearChanged(int value) => RaiseFilterChanged();
        partial void OnMaxYearChanged(int value) => RaiseFilterChanged();
        partial void OnMinPriceChanged(decimal value) => RaiseFilterChanged();
        partial void OnMaxPriceChanged(decimal value) => RaiseFilterChanged();
        partial void OnMinMileageChanged(int value) => RaiseFilterChanged();
        partial void OnMaxMileageChanged(int value) => RaiseFilterChanged();
        partial void OnSelectedTransmissionChanged(string value) => RaiseFilterChanged();
        partial void OnSelectedRepairHistoryChanged(string value) => RaiseFilterChanged();
        
        partial void OnSelectedMinYearChanged(string value)
        {
            if (value == "下限なし")
            {
                MinYear = YearRangeMin;
            }
            else if (int.TryParse(value, out int year))
            {
                MinYear = year;
            }
        }
        
        partial void OnSelectedMaxYearChanged(string value)
        {
            if (value == "上限なし")
            {
                MaxYear = YearRangeMax;
            }
            else if (int.TryParse(value, out int year))
            {
                MaxYear = year;
            }
        }
        
        partial void OnSelectedMinMileageChanged(string value)
        {
            if (value == "下限なし")
            {
                MinMileage = MileageRangeMin;
            }
            else
            {
                // "10,000km" 形式から数値を抽出
                var cleanValue = value.Replace(",", "").Replace("km", "");
                if (int.TryParse(cleanValue, out int mileage))
                {
                    MinMileage = mileage;
                }
            }
        }
        
        partial void OnSelectedMaxMileageChanged(string value)
        {
            if (value == "上限なし")
            {
                MaxMileage = MileageRangeMax;
            }
            else
            {
                // "10,000km" 形式から数値を抽出
                var cleanValue = value.Replace(",", "").Replace("km", "");
                if (int.TryParse(cleanValue, out int mileage))
                {
                    MaxMileage = mileage;
                }
            }
        }

        public void UpdateFromData(List<CarData> data)
        {
            if (!data.Any()) return;

            var uniqueGrades = data.Select(d => d.Grade)
                                  .Where(g => !string.IsNullOrWhiteSpace(g))
                                  .Distinct()
                                  .OrderBy(g => g)
                                  .ToList();

            Grades.Clear();
            foreach (var grade in uniqueGrades)
            {
                var item = new GradeFilterItem { Grade = grade, IsSelected = true };
                item.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(GradeFilterItem.IsSelected))
                    {
                        RaiseFilterChanged();
                    }
                };
                Grades.Add(item);
            }

            var years = data.Where(d => d.Year > 0).Select(d => d.Year).Distinct().OrderBy(y => y).ToList();
            if (years.Any())
            {
                YearRangeMin = years.Min();
                YearRangeMax = years.Max();
                MinYear = YearRangeMin;
                MaxYear = YearRangeMax;

                // 年式オプションを生成
                YearMinOptions.Clear();
                YearMinOptions.Add("下限なし");
                foreach (var year in years)
                {
                    YearMinOptions.Add(year.ToString());
                }

                YearMaxOptions.Clear();
                YearMaxOptions.Add("上限なし");
                foreach (var year in years)
                {
                    YearMaxOptions.Add(year.ToString());
                }
                
                System.Diagnostics.Debug.WriteLine($"年式オプション生成: Min={YearMinOptions.Count}個, Max={YearMaxOptions.Count}個");
            }

            var prices = data.Where(d => d.Price > 0).Select(d => d.Price).ToList();
            if (prices.Any())
            {
                // 実際のデータから万円単位で範囲を設定
                PriceRangeMin = Math.Floor((decimal)prices.Min() / 10000);
                PriceRangeMax = Math.Ceiling((decimal)prices.Max() / 10000);
                MinPrice = PriceRangeMin;
                MaxPrice = PriceRangeMax;
                
                System.Diagnostics.Debug.WriteLine($"価格範囲設定: {PriceRangeMin}万円 - {PriceRangeMax}万円");
            }

            var mileages = data.Where(d => d.Mileage > 0).Select(d => d.Mileage).ToList();
            if (mileages.Any())
            {
                // 実際のデータから走行距離範囲を設定
                MileageRangeMin = mileages.Min();
                MileageRangeMax = mileages.Max();
                MinMileage = MileageRangeMin;
                MaxMileage = MileageRangeMax;

                // 走行距離オプションを生成（1万km刻み）
                MileageMinOptions.Clear();
                MileageMinOptions.Add("下限なし");
                for (int km = 0; km <= MileageRangeMax + 50000; km += 10000)
                {
                    if (km <= MileageRangeMax)
                    {
                        MileageMinOptions.Add($"{km:N0}km");
                    }
                }

                MileageMaxOptions.Clear();
                MileageMaxOptions.Add("上限なし");
                for (int km = 0; km <= MileageRangeMax + 50000; km += 10000)
                {
                    if (km <= MileageRangeMax)
                    {
                        MileageMaxOptions.Add($"{km:N0}km");
                    }
                }
                
                System.Diagnostics.Debug.WriteLine($"走行距離範囲設定: {MileageRangeMin}km - {MileageRangeMax}km");
                System.Diagnostics.Debug.WriteLine($"走行距離オプション生成: Min={MileageMinOptions.Count}個, Max={MileageMaxOptions.Count}個");
            }
        }

        public FilterSettings GetFilterSettings()
        {
            return new FilterSettings
            {
                SelectedGrades = Grades.Where(g => g.IsSelected).Select(g => g.Grade).ToList(),
                MinYear = MinYear,
                MaxYear = MaxYear,
                MinPrice = MinPrice * 10000,
                MaxPrice = MaxPrice * 10000,
                MinMileage = MinMileage,
                MaxMileage = MaxMileage,
                SelectedTransmission = SelectedTransmission,
                RepairHistoryFilter = SelectedRepairHistory
            };
        }

        private void SelectAllGrades()
        {
            foreach (var grade in Grades)
            {
                grade.IsSelected = true;
            }
            RaiseFilterChanged();
        }

        private void ClearAllGrades()
        {
            foreach (var grade in Grades)
            {
                grade.IsSelected = false;
            }
            RaiseFilterChanged();
        }

        private void ResetFilters()
        {
            MinYear = YearRangeMin;
            MaxYear = YearRangeMax;
            MinPrice = PriceRangeMin;
            MaxPrice = PriceRangeMax;
            MinMileage = MileageRangeMin;
            MaxMileage = MileageRangeMax;
            SelectedMinYear = "下限なし";
            SelectedMaxYear = "上限なし";
            SelectedMinMileage = "下限なし";
            SelectedMaxMileage = "上限なし";
            SelectedTransmission = "すべて";
            SelectedRepairHistory = "すべて";
            SelectAllGrades();
        }

        private void RaiseFilterChanged()
        {
            FilterChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public partial class GradeFilterItem : ObservableObject
    {
        [ObservableProperty]
        private string _grade = string.Empty;

        [ObservableProperty]
        private bool _isSelected = true;
    }
}