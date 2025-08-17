using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CarAnalysisDashboard.Models;
using CarAnalysisDashboard.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System.IO;

namespace CarAnalysisDashboard.ViewModels
{
    public partial class MainViewModel : ViewModelBase
    {
        private readonly ICsvDataService _csvDataService;
        private readonly IFileService _fileService;

        [ObservableProperty]
        private ObservableCollection<CarData> _carDataList = new();

        [ObservableProperty]
        private ObservableCollection<CarData> _filteredDataList = new();

        [ObservableProperty]
        private ObservableCollection<string> _carModels = new();

        [ObservableProperty]
        private string? _selectedCarModel;


        [ObservableProperty]
        private FilterViewModel _filterViewModel;

        [ObservableProperty]
        private DashboardViewModel _dashboardViewModel;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private string _statusMessage = "準備完了";

        [ObservableProperty]
        private string _dataPath = @"data";

        public MainViewModel(ICsvDataService csvDataService, IFileService fileService)
        {
            _csvDataService = csvDataService;
            _fileService = fileService;
            _filterViewModel = new FilterViewModel();
            _dashboardViewModel = new DashboardViewModel();

            _filterViewModel.FilterChanged += OnFilterChanged;
            
            LoadCarModelsCommand = new AsyncRelayCommand(LoadCarModelsAsync);
            ExportDataCommand = new AsyncRelayCommand(ExportDataAsync);
            BrowseDataPathCommand = new RelayCommand(BrowseDataPath);
        }

        public IAsyncRelayCommand LoadCarModelsCommand { get; }
        public IAsyncRelayCommand ExportDataCommand { get; }
        public ICommand BrowseDataPathCommand { get; }

        partial void OnSelectedCarModelChanged(string? value)
        {
            if (value != null)
            {
                _ = LoadAllDataForCarModelAsync(value);
            }
        }

        private async Task LoadAllDataForCarModelAsync(string carModel)
        {
            try
            {
                IsLoading = true;
                StatusMessage = $"{carModel}のデータを読み込み中...";

                var modelPath = Path.Combine(DataPath, carModel);
                if (!Directory.Exists(modelPath))
                {
                    StatusMessage = $"{carModel}のディレクトリが見つかりません: {modelPath}";
                    System.Diagnostics.Debug.WriteLine($"ディレクトリが存在しません: {modelPath}");
                    return;
                }

                var allCsvFiles = new List<string>();
                
                // 車種フォルダ内の日付フォルダを探索
                var dateFolders = Directory.GetDirectories(modelPath);
                foreach (var dateFolder in dateFolders)
                {
                    var csvFiles = Directory.GetFiles(dateFolder, "*.csv");
                    allCsvFiles.AddRange(csvFiles);
                }
                
                // 車種フォルダ直下のCSVファイルも取得
                var directFiles = Directory.GetFiles(modelPath, "*.csv");
                allCsvFiles.AddRange(directFiles);

                if (!allCsvFiles.Any())
                {
                    StatusMessage = $"{carModel}のCSVファイルが見つかりません。パス: {modelPath}";
                    System.Diagnostics.Debug.WriteLine($"CSVファイルが見つかりません。パス: {modelPath}");
                    return;
                }

                // 全CSVファイルを読み込み
                var data = await _csvDataService.LoadCsvFilesAsync(allCsvFiles);

                CarDataList.Clear();
                foreach (var item in data)
                {
                    CarDataList.Add(item);
                }

                FilterViewModel.UpdateFromData(CarDataList.ToList());
                ApplyFilters();

                StatusMessage = $"{carModel}の{allCsvFiles.Count}ファイルから{CarDataList.Count}件のデータを読み込みました";
            }
            catch (Exception ex)
            {
                StatusMessage = $"データ読み込みエラー: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadCarModelsAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "車種を読み込み中...";

                var models = await Task.Run(() => _fileService.GetCarModels(DataPath));
                
                CarModels.Clear();
                foreach (var model in models)
                {
                    CarModels.Add(model);
                }

                if (CarModels.Any())
                {
                    SelectedCarModel = CarModels.First();
                }

                StatusMessage = $"{CarModels.Count}個の車種を読み込みました";
            }
            catch (Exception ex)
            {
                StatusMessage = $"車種読み込みエラー: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void OnFilterChanged(object? sender, EventArgs e)
        {
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            if (!CarDataList.Any())
            {
                FilteredDataList.Clear();
                return;
            }

            var filtered = _csvDataService.FilterData(
                CarDataList.ToList(),
                FilterViewModel.GetFilterSettings());

            FilteredDataList.Clear();
            foreach (var item in filtered)
            {
                FilteredDataList.Add(item);
            }

            DashboardViewModel.UpdateData(FilteredDataList.ToList());
            StatusMessage = $"{CarDataList.Count}件中{FilteredDataList.Count}件を表示";
        }

        private async Task ExportDataAsync()
        {
            if (!FilteredDataList.Any())
            {
                StatusMessage = "エクスポートするデータがありません";
                return;
            }

            try
            {
                var dialog = new SaveFileDialog
                {
                    Filter = "CSVファイル (*.csv)|*.csv|すべてのファイル (*.*)|*.*",
                    FileName = $"CarData_Export_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
                };

                if (dialog.ShowDialog() == true)
                {
                    IsLoading = true;
                    StatusMessage = "データをエクスポート中...";

                    await _csvDataService.ExportToCsvAsync(FilteredDataList.ToList(), dialog.FileName);

                    StatusMessage = $"{FilteredDataList.Count}件のデータを{Path.GetFileName(dialog.FileName)}にエクスポートしました";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"エクスポートエラー: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void BrowseDataPath()
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "データフォルダを選択",
                ShowNewFolderButton = false
            };

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                DataPath = dialog.SelectedPath;
                _ = LoadCarModelsAsync();
            }
        }
    }
}