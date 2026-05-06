namespace DevPilotAgent.App.Views;

using System.Windows;
using DevPilotAgent.App.ViewModels;
using MaterialDesignThemes.Wpf;

/// <summary>
/// WPF 메인 윈도우. BlazorWebView를 호스팅하고 타이틀 바 버튼을 처리한다.
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        BlazorWebView.Services = App.Services;
        StateChanged += OnStateChanged;
    }

    private void MinimizeButton_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void MaximizeButton_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState == WindowState.Maximized
            ? WindowState.Normal
            : WindowState.Maximized;
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void OnStateChanged(object? sender, EventArgs e)
    {
        MaximizeIcon.Kind = WindowState == WindowState.Maximized
            ? PackIconKind.WindowRestore
            : PackIconKind.WindowMaximize;
    }
}
