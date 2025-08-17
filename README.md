# Car Analysis Dashboard

A WPF application for analyzing used car data from CSV files with advanced filtering and visualization capabilities.

## Features

- **Multi-file CSV Import**: Load multiple CSV files organized by car model directories
- **Advanced Filtering**: Filter by grade, year, price, mileage, transmission type, and repair history
- **Multiple Analysis Views**:
  - Overview: Basic statistics and year distribution
  - Price Trend: Time-series price analysis
  - Grade Analysis: Comparison across different grades
  - Mileage vs Price: Scatter plot visualization
- **Data Export**: Export filtered data to CSV format
- **Real-time Updates**: Instant chart updates based on filter changes

## Prerequisites

- .NET 8.0 SDK or later
- Windows OS (WPF application)

## Project Structure

```
CarAnalysisDashboard/
├── Models/              # Data models and CSV mappings
├── Services/            # Business logic and data services
├── ViewModels/          # MVVM ViewModels
├── Views/              # WPF Views (XAML)
├── Converters/         # Value converters for data binding
├── Styles/             # Application styles and themes
└── data/               # CSV data files (organized by car model)
    ├── RCF/
    │   ├── 2025_07_11_F.No1.csv
    │   └── ...
    └── スープラ/
        ├── 2025_07_11_スープラ.No1.csv
        └── ...
```

## CSV File Format

The application expects CSV files with the following columns (in Japanese):
- 車名 (Car Name)
- グレード (Grade)
- 価格 (Price)
- 年式 (Year)
- 走行距離 (Mileage)
- ミッション (Transmission)
- 修復歴 (Repair History)
- 色 (Color)
- 地域 (Location)
- 詳細URL (Detail URL)
- And more...

## Building and Running

1. Clone or download the project
2. Open a terminal in the project directory
3. Build the project:
   ```bash
   dotnet build
   ```
4. Run the application:
   ```bash
   dotnet run
   ```

## Usage

1. **Select Data Path**: Click the "..." button to browse and select your data folder
2. **Choose Car Model**: Select from the dropdown list of available car models
3. **Select CSV Files**: Choose one or more CSV files to analyze
4. **Load Data**: Click "Load Data" to import the selected files
5. **Apply Filters**: Use the left sidebar to filter data by various criteria
6. **Switch Views**: Use the radio buttons to switch between different analysis views
7. **Export Results**: Click "Export CSV" to save filtered data

## Technologies Used

- **WPF** (Windows Presentation Foundation)
- **MVVM Pattern** with CommunityToolkit.Mvvm
- **LiveCharts2** for data visualization
- **CsvHelper** for CSV parsing
- **Dependency Injection** with Microsoft.Extensions.DependencyInjection

## License

This project is for educational and analysis purposes.