using System.Windows;
using CarAnalysisDashboard.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace CarAnalysisDashboard.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
            var app = (App)Application.Current;
            var serviceProvider = app.GetType().GetField("_serviceProvider", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.GetValue(app) as ServiceProvider;
            
            if (serviceProvider != null)
            {
                var viewModel = serviceProvider.GetRequiredService<MainViewModel>();
                DataContext = viewModel;
                viewModel.LoadCarModelsCommand.Execute(null);
            }
        }
    }
}