using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using CarAnalysisDashboard.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.Defaults;
using SkiaSharp;

namespace CarAnalysisDashboard.ViewModels
{
    public partial class DashboardViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string _viewMode = "概要";

        [ObservableProperty]
        private ObservableCollection<string> _viewModes = new()
        {
            "概要", "価格分布", "価格推移", "グレード分析", "走行距離vs価格"
        };

        [ObservableProperty]
        private decimal _averagePrice;

        [ObservableProperty]
        private decimal _medianPrice;

        [ObservableProperty]
        private decimal _minPrice;

        [ObservableProperty]
        private int _maxPrice;

        [ObservableProperty]
        private int _totalCount;

        [ObservableProperty]
        private int _uniqueGradeCount;

        [ObservableProperty]
        private double _repairHistoryPercentage;

        [ObservableProperty]
        private ISeries[] _yearDistributionSeries = Array.Empty<ISeries>();

        [ObservableProperty]
        private ISeries[] _priceDistributionSeries = Array.Empty<ISeries>();

        [ObservableProperty]
        private ISeries[] _priceTrendSeries = Array.Empty<ISeries>();

        [ObservableProperty]
        private ISeries[] _gradeAnalysisSeries = Array.Empty<ISeries>();

        [ObservableProperty]
        private ISeries[] _mileagePriceSeries = Array.Empty<ISeries>();

        [ObservableProperty]
        private ISeries[] _currentSeries = Array.Empty<ISeries>();

        [ObservableProperty]
        private Axis[] _xAxes = Array.Empty<Axis>();

        [ObservableProperty]
        private Axis[] _yAxes = Array.Empty<Axis>();

        [ObservableProperty]
        private SolidColorPaint _tooltipTextPaint = new SolidColorPaint(SKColors.Black)
        {
            SKTypeface = SKTypeface.FromFamilyName("Yu Gothic UI", SKFontStyle.Normal)
        };

        // Price trend visibility settings
        [ObservableProperty]
        private bool _showAveragePrice = true;

        [ObservableProperty]
        private bool _showMedianPrice = true;

        [ObservableProperty]
        private bool _showMinPrice = true;

        [ObservableProperty]
        private bool _showMaxPrice = true;

        private List<CarData> _currentData = new();

        public DashboardViewModel()
        {
            // 初期化時に空のチャートを作成
            InitializeEmptyCharts();
        }

        private void InitializeEmptyCharts()
        {
            // 空のチャートを初期化（データがない場合でもチャートエリアを表示）
            YearDistributionSeries = new ISeries[]
            {
                new ColumnSeries<int>
                {
                    Values = new int[] { },
                    Fill = new SolidColorPaint(SKColors.CornflowerBlue),
                    Name = "台数",
                    YToolTipLabelFormatter = (chartPoint) => $"{chartPoint.Context.Series.Name}: {chartPoint.Coordinate.PrimaryValue}台"
                }
            };

            CurrentSeries = YearDistributionSeries;

            XAxes = new[]
            {
                new Axis
                {
                    Labels = new string[] { },
                    LabelsPaint = new SolidColorPaint(SKColors.Black) 
                    {
                        SKTypeface = SKTypeface.FromFamilyName("Yu Gothic UI", SKFontStyle.Normal)
                    },
                    TextSize = 12,
                    LabelsRotation = 45
                }
            };

            YAxes = new[]
            {
                new Axis
                {
                    Name = "台数",
                    NamePaint = new SolidColorPaint(SKColors.Black)
                    {
                        SKTypeface = SKTypeface.FromFamilyName("Yu Gothic UI", SKFontStyle.Normal)
                    },
                    LabelsPaint = new SolidColorPaint(SKColors.Black)
                    {
                        SKTypeface = SKTypeface.FromFamilyName("Yu Gothic UI", SKFontStyle.Normal)
                    },
                    TextSize = 12
                }
            };
        }

        public void UpdateData(List<CarData> data)
        {
            _currentData = data;
            UpdateStatistics();
            UpdateCharts();
        }

        private string CleanGradeName(string grade)
        {
            if (string.IsNullOrWhiteSpace(grade)) return grade;
            
            // 価格情報を除去 (例: "1489.5万円")
            grade = Regex.Replace(grade, @"\d+\.?\d*万?円", "").Trim();
            
            // 販売店情報を除去 (例: "売#200台")
            grade = Regex.Replace(grade, @"売#?\d+台", "").Trim();
            
            // ナンバープレート情報を除去 (例: "8-SPEED")
            grade = Regex.Replace(grade, @"\d+-?SPEED", "").Trim();
            
            // エンジン情報を簡略化 (V8, NAなどは残す)
            grade = Regex.Replace(grade, @"自然吸気|エンジン最終搭載", "").Trim();
            
            // 連続するスペースを一つに
            grade = Regex.Replace(grade, @"\s+", " ").Trim();
            
            // 最大長を制限（50文字）
            if (grade.Length > 50)
            {
                grade = grade.Substring(0, 47) + "...";
            }
            
            return grade;
        }
        
        private void UpdateStatistics()
        {
            if (!_currentData.Any())
            {
                AveragePrice = 0;
                MedianPrice = 0;
                MinPrice = 0;
                MaxPrice = 0;
                TotalCount = 0;
                UniqueGradeCount = 0;
                RepairHistoryPercentage = 0;
                return;
            }

            var prices = _currentData.Where(d => d.Price > 0).Select(d => d.Price).ToList();
            
            if (prices.Any())
            {
                AveragePrice = (decimal)System.Math.Round(prices.Average(x => (double)x) / 10000.0, 1);
                MinPrice = (decimal)System.Math.Round((double)prices.Min() / 10000.0, 1);
                MaxPrice = (int)(prices.Max() / 10000);
                
                var sortedPrices = prices.OrderBy(p => p).ToList();
                var middle = sortedPrices.Count / 2;
                MedianPrice = (decimal)System.Math.Round(
                    sortedPrices.Count % 2 == 0
                        ? ((double)sortedPrices[middle - 1] + (double)sortedPrices[middle]) / 2.0 / 10000.0
                        : (double)sortedPrices[middle] / 10000.0, 1);
            }

            TotalCount = _currentData.Count;
            UniqueGradeCount = _currentData.Select(d => CleanGradeName(d.Grade)).Distinct().Count();
            RepairHistoryPercentage = _currentData.Any() 
                ? System.Math.Round(_currentData.Count(d => d.HasRepairHistory) * 100.0 / _currentData.Count, 1)
                : 0;
        }

        private void UpdateCharts()
        {
            System.Diagnostics.Debug.WriteLine($"チャート更新: ViewMode={ViewMode}, データ件数={_currentData.Count}");
            
            switch (ViewMode)
            {
                case "概要":
                    UpdateYearDistributionChart();
                    break;
                case "価格分布":
                    UpdatePriceDistributionChart();
                    break;
                case "価格推移":
                    UpdatePriceTrendChart();
                    break;
                case "グレード分析":
                    UpdateGradeAnalysisChart();
                    break;
                case "走行距離vs価格":
                    UpdateMileagePriceChart();
                    break;
                default:
                    UpdateYearDistributionChart();
                    break;
            }
        }

        private void UpdateYearDistributionChart()
        {
            if (!_currentData.Any())
            {
                // データがない場合でも空のチャートを表示
                InitializeEmptyCharts();
                return;
            }

            var yearGroups = _currentData
                .Where(d => d.Year > 0)
                .GroupBy(d => d.Year)
                .OrderBy(g => g.Key)
                .Select(g => new { Year = g.Key, Count = g.Count() })
                .ToList();

            var values = yearGroups.Select(g => g.Count).ToArray();
            System.Diagnostics.Debug.WriteLine($"年式分布チャート: {values.Length}個のデータポイント");
            
            YearDistributionSeries = new ISeries[]
            {
                new ColumnSeries<int>
                {
                    Values = values,
                    Fill = new SolidColorPaint(SKColors.CornflowerBlue),
                    Name = "台数",
                    YToolTipLabelFormatter = (chartPoint) => $"{chartPoint.Context.Series.Name}: {chartPoint.Coordinate.PrimaryValue}台"
                }
            };

            XAxes = new[]
            {
                new Axis
                {
                    Labels = yearGroups.Select(g => g.Year.ToString()).ToArray(),
                    LabelsRotation = 45,
                    LabelsPaint = new SolidColorPaint(SKColors.Black)
                    {
                        SKTypeface = SKTypeface.FromFamilyName("Yu Gothic UI", SKFontStyle.Normal)
                    },
                    TextSize = 12
                }
            };

            YAxes = new[]
            {
                new Axis
                {
                    Name = "台数",
                    NamePaint = new SolidColorPaint(SKColors.Black)
                    {
                        SKTypeface = SKTypeface.FromFamilyName("Yu Gothic UI", SKFontStyle.Normal)
                    },
                    LabelsPaint = new SolidColorPaint(SKColors.Black)
                    {
                        SKTypeface = SKTypeface.FromFamilyName("Yu Gothic UI", SKFontStyle.Normal)
                    },
                    TextSize = 12
                }
            };

            CurrentSeries = YearDistributionSeries;
        }

        private void UpdatePriceDistributionChart()
        {
            if (!_currentData.Any())
            {
                PriceDistributionSeries = Array.Empty<ISeries>();
                CurrentSeries = PriceDistributionSeries;
                return;
            }

            // 価格データを万円単位で取得し、範囲を設定
            List<double> prices = _currentData.Where(d => d.Price > 0).Select(d => (double)d.Price / 10000.0).ToList();
            if (!prices.Any())
            {
                PriceDistributionSeries = Array.Empty<ISeries>();
                CurrentSeries = PriceDistributionSeries;
                return;
            }

            double minPrice = System.Math.Floor(prices.Min());
            double maxPrice = System.Math.Ceiling(prices.Max());
            
            // 価格帯を25万円刻みで分割（CarSensorのような細かな分布表示）
            double binSize = 25.0; // 25万円刻み
            int binCount = (int)System.Math.Ceiling((maxPrice - minPrice) / binSize);
            
            var priceRanges = new List<(double Start, double End, int Count)>();
            
            for (int i = 0; i < binCount; i++)
            {
                double start = minPrice + (i * binSize);
                double end = start + binSize;
                int count = prices.Count(p => p >= start && (i == binCount - 1 ? p <= end : p < end));
                priceRanges.Add((start, end, count));
            }

            var values = priceRanges.Select(r => r.Count).ToArray();
            var labels = priceRanges.Select(r => $"{r.Start:N0}-{r.End:N0}").ToArray();

            System.Diagnostics.Debug.WriteLine($"【価格分布計算】価格帯数: {values.Length}個, 最大件数: {values.Max()}台");
            System.Diagnostics.Debug.WriteLine($"【価格分布範囲】{minPrice:N0}万円～{maxPrice:N0}万円 (総データ数:{prices.Count}台)");

            PriceDistributionSeries = new ISeries[]
            {
                new ColumnSeries<int>
                {
                    Values = values,
                    Fill = new SolidColorPaint(SKColors.MediumSeaGreen),
                    Name = "台数",
                    YToolTipLabelFormatter = (chartPoint) => $"価格帯: {labels[chartPoint.Index]}万円\n車両数: {chartPoint.Coordinate.PrimaryValue}台"
                }
            };

            XAxes = new[]
            {
                new Axis
                {
                    Labels = labels,
                    LabelsRotation = 45,
                    LabelsPaint = new SolidColorPaint(SKColors.Black)
                    {
                        SKTypeface = SKTypeface.FromFamilyName("Yu Gothic UI", SKFontStyle.Normal)
                    },
                    TextSize = 10
                }
            };

            YAxes = new[]
            {
                new Axis
                {
                    Name = "台数",
                    NamePaint = new SolidColorPaint(SKColors.Black)
                    {
                        SKTypeface = SKTypeface.FromFamilyName("Yu Gothic UI", SKFontStyle.Normal)
                    },
                    LabelsPaint = new SolidColorPaint(SKColors.Black)
                    {
                        SKTypeface = SKTypeface.FromFamilyName("Yu Gothic UI", SKFontStyle.Normal)
                    },
                    TextSize = 12
                }
            };

            CurrentSeries = PriceDistributionSeries;
        }

        private void UpdatePriceTrendChart()
        {
            if (!_currentData.Any())
            {
                PriceTrendSeries = Array.Empty<ISeries>();
                CurrentSeries = PriceTrendSeries;
                return;
            }

            // DataDateが設定されていない場合はAcquisitionDateを使用
            var dateGroups = _currentData
                .Select(d => 
                {
                    // AcquisitionDateをDateTimeに変換
                    DateTime date;
                    if (d.DataDate != default(DateTime))
                    {
                        date = d.DataDate;
                    }
                    else if (!string.IsNullOrEmpty(d.AcquisitionDate))
                    {
                        // "2025-08-06" 形式の文字列をパース
                        if (DateTime.TryParse(d.AcquisitionDate, out var parsedDate))
                        {
                            date = parsedDate;
                        }
                        else
                        {
                            date = DateTime.Now;
                        }
                    }
                    else
                    {
                        date = DateTime.Now;
                    }
                    return new { Data = d, Date = date };
                })
                .GroupBy(x => x.Date.Date)
                .OrderBy(g => g.Key)
                .Select(g => new
                {
                    Date = g.Key,
                    Average = g.Where(x => x.Data.Price > 0).Average(x => (double)x.Data.Price) / 10000.0,
                    Median = GetMedian(g.Where(x => x.Data.Price > 0).Select(x => x.Data.Price).ToList()) / 10000.0,
                    Min = g.Where(x => x.Data.Price > 0).Min(x => x.Data.Price) / 10000.0,
                    Max = (int)(g.Where(x => x.Data.Price > 0).Max(x => x.Data.Price) / 10000),
                    PriceList = g.Where(x => x.Data.Price > 0).Select(x => x.Data.Price).ToList() // デバッグ用
                })
                .ToList();

            // 価格推移データの検証（異常値検出のみ）
            var maxPriceInData = dateGroups.SelectMany(g => g.PriceList).Max();
            if (maxPriceInData > 20000000) // 2000万円超の場合のみ警告
            {
                System.Diagnostics.Debug.WriteLine($"【警告】価格推移: 異常に高額な車両を検出しました ({maxPriceInData/10000:N0}万円)");
            }
            
            // 価格比較の詳細ログを追加
            System.Diagnostics.Debug.WriteLine($"【価格推移計算】データ日数: {dateGroups.Count}日, 最高価格: {maxPriceInData/10000:N0}万円");
            foreach (var group in dateGroups.Where(g => g.PriceList.Any()).Take(3))
            {
                var maxInGroup = group.PriceList.Max();
                System.Diagnostics.Debug.WriteLine($"  {group.Date:MM/dd}: 最高{maxInGroup/10000:N0}万円 (データ数:{group.PriceList.Count}台)");
            }

            var seriesList = new List<ISeries>();

            if (ShowAveragePrice)
            {
                seriesList.Add(new LineSeries<double>
                {
                    Values = dateGroups.Select(g => (double)g.Average).ToArray(),
                    Name = "平均価格",
                    Fill = null,
                    GeometrySize = 6,
                    Stroke = new SolidColorPaint(SKColors.Blue) { StrokeThickness = 2 },
                    YToolTipLabelFormatter = (chartPoint) => $"{chartPoint.Context.Series.Name}: {chartPoint.Coordinate.PrimaryValue:N1}万円"
                });
            }

            if (ShowMedianPrice)
            {
                seriesList.Add(new LineSeries<double>
                {
                    Values = dateGroups.Select(g => (double)g.Median).ToArray(),
                    Name = "中央価格",
                    Fill = null,
                    GeometrySize = 6,
                    Stroke = new SolidColorPaint(SKColors.Green) { StrokeThickness = 2 },
                    YToolTipLabelFormatter = (chartPoint) => $"{chartPoint.Context.Series.Name}: {chartPoint.Coordinate.PrimaryValue:N1}万円"
                });
            }

            if (ShowMinPrice)
            {
                seriesList.Add(new LineSeries<double>
                {
                    Values = dateGroups.Select(g => (double)g.Min).ToArray(),
                    Name = "最低価格",
                    Fill = null,
                    GeometrySize = 4,
                    Stroke = new SolidColorPaint(SKColors.Red) { StrokeThickness = 1 },
                    YToolTipLabelFormatter = (chartPoint) => $"{chartPoint.Context.Series.Name}: {chartPoint.Coordinate.PrimaryValue:N1}万円"
                });
            }

            if (ShowMaxPrice)
            {
                seriesList.Add(new LineSeries<double>
                {
                    Values = dateGroups.Select(g => (double)g.Max).ToArray(),
                    Name = "最高価格",
                    Fill = null,
                    GeometrySize = 4,
                    Stroke = new SolidColorPaint(SKColors.Purple) { StrokeThickness = 1 },
                    YToolTipLabelFormatter = (chartPoint) => $"{chartPoint.Context.Series.Name}: {chartPoint.Coordinate.PrimaryValue:N1}万円"
                });
            }

            PriceTrendSeries = seriesList.ToArray();

            XAxes = new[]
            {
                new Axis
                {
                    Labels = dateGroups.Select(g => g.Date.ToString("MM/dd")).ToArray(),
                    LabelsRotation = 45,
                    LabelsPaint = new SolidColorPaint(SKColors.Black)
                    {
                        SKTypeface = SKTypeface.FromFamilyName("Yu Gothic UI", SKFontStyle.Normal)
                    },
                    TextSize = 12
                }
            };

            YAxes = new[]
            {
                new Axis
                {
                    Name = "価格（万円）",
                    NamePaint = new SolidColorPaint(SKColors.Black)
                    {
                        SKTypeface = SKTypeface.FromFamilyName("Yu Gothic UI", SKFontStyle.Normal)
                    },
                    LabelsPaint = new SolidColorPaint(SKColors.Black)
                    {
                        SKTypeface = SKTypeface.FromFamilyName("Yu Gothic UI", SKFontStyle.Normal)
                    },
                    TextSize = 12
                }
            };

            CurrentSeries = PriceTrendSeries;
        }

        private void UpdateGradeAnalysisChart()
        {
            if (!_currentData.Any())
            {
                GradeAnalysisSeries = Array.Empty<ISeries>();
                CurrentSeries = GradeAnalysisSeries;
                return;
            }

            var gradeGroups = _currentData
                .Where(d => !string.IsNullOrWhiteSpace(d.Grade))
                .GroupBy(d => CleanGradeName(d.Grade))
                .Select(g => new
                {
                    Grade = g.Key,
                    Average = g.Where(d => d.Price > 0).Any() 
                        ? g.Where(d => d.Price > 0).Average(d => (double)d.Price) / 10000.0 
                        : 0,
                    Count = g.Count()
                })
                .OrderByDescending(g => g.Average)
                .Take(15)
                .ToList();

            GradeAnalysisSeries = new ISeries[]
            {
                new ColumnSeries<double>
                {
                    Values = gradeGroups.Select(g => (double)g.Average).ToArray(),
                    Fill = new SolidColorPaint(SKColors.MediumSeaGreen),
                    Name = "平均価格",
                    YToolTipLabelFormatter = (chartPoint) => $"{chartPoint.Context.Series.Name}: {chartPoint.Coordinate.PrimaryValue:N1}万円"
                }
            };

            XAxes = new[]
            {
                new Axis
                {
                    Labels = gradeGroups.Select(g => $"{g.Grade}\n({g.Count}台)").ToArray(),
                    LabelsRotation = 45,
                    LabelsPaint = new SolidColorPaint(SKColors.Black)
                    {
                        SKTypeface = SKTypeface.FromFamilyName("Yu Gothic UI", SKFontStyle.Normal)
                    },
                    TextSize = 10
                }
            };

            YAxes = new[]
            {
                new Axis
                {
                    Name = "平均価格（万円）",
                    NamePaint = new SolidColorPaint(SKColors.Black)
                    {
                        SKTypeface = SKTypeface.FromFamilyName("Yu Gothic UI", SKFontStyle.Normal)
                    },
                    LabelsPaint = new SolidColorPaint(SKColors.Black)
                    {
                        SKTypeface = SKTypeface.FromFamilyName("Yu Gothic UI", SKFontStyle.Normal)
                    },
                    TextSize = 12
                }
            };

            CurrentSeries = GradeAnalysisSeries;
        }

        private void UpdateMileagePriceChart()
        {
            if (!_currentData.Any())
            {
                MileagePriceSeries = Array.Empty<ISeries>();
                CurrentSeries = MileagePriceSeries;
                return;
            }

            var points = _currentData
                .Where(d => d.Price > 0 && d.Mileage > 0)
                .Select(d => new ObservablePoint(d.Mileage / 10000.0, (double)d.Price / 10000.0))
                .ToArray();

            MileagePriceSeries = new ISeries[]
            {
                new ScatterSeries<ObservablePoint>
                {
                    Values = points,
                    Fill = new SolidColorPaint(SKColors.CornflowerBlue.WithAlpha(128)),
                    GeometrySize = 5,
                    Name = "車両",
                    YToolTipLabelFormatter = (chartPoint) => $"走行距離: {chartPoint.Coordinate.SecondaryValue:N1}万km\n価格: {chartPoint.Coordinate.PrimaryValue:N1}万円"
                }
            };

            XAxes = new[]
            {
                new Axis
                {
                    Name = "走行距離（万km）",
                    NamePaint = new SolidColorPaint(SKColors.Black)
                    {
                        SKTypeface = SKTypeface.FromFamilyName("Yu Gothic UI", SKFontStyle.Normal)
                    },
                    LabelsPaint = new SolidColorPaint(SKColors.Black)
                    {
                        SKTypeface = SKTypeface.FromFamilyName("Yu Gothic UI", SKFontStyle.Normal)
                    },
                    TextSize = 12
                }
            };

            YAxes = new[]
            {
                new Axis
                {
                    Name = "価格（万円）",
                    NamePaint = new SolidColorPaint(SKColors.Black)
                    {
                        SKTypeface = SKTypeface.FromFamilyName("Yu Gothic UI", SKFontStyle.Normal)
                    },
                    LabelsPaint = new SolidColorPaint(SKColors.Black)
                    {
                        SKTypeface = SKTypeface.FromFamilyName("Yu Gothic UI", SKFontStyle.Normal)
                    },
                    TextSize = 12
                }
            };

            CurrentSeries = MileagePriceSeries;
        }

        private long GetMedian(List<long> values)
        {
            if (!values.Any()) return 0;
            
            var sorted = values.OrderBy(v => v).ToList();
            var middle = sorted.Count / 2;
            
            return sorted.Count % 2 == 0
                ? (sorted[middle - 1] + sorted[middle]) / 2
                : sorted[middle];
        }

        partial void OnViewModeChanged(string value)
        {
            UpdateCharts();
        }

        partial void OnShowAveragePriceChanged(bool value)
        {
            if (ViewMode == "価格推移") UpdatePriceTrendChart();
        }

        partial void OnShowMedianPriceChanged(bool value)
        {
            if (ViewMode == "価格推移") UpdatePriceTrendChart();
        }

        partial void OnShowMinPriceChanged(bool value)
        {
            if (ViewMode == "価格推移") UpdatePriceTrendChart();
        }

        partial void OnShowMaxPriceChanged(bool value)
        {
            if (ViewMode == "価格推移") UpdatePriceTrendChart();
        }
    }
}