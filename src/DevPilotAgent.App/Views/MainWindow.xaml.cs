namespace DevPilotAgent.App.Views;

using System.Windows;
using DevPilotAgent.App.ViewModels;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        BlazorWebView.Services = App.Services;
    }
}
