using System.Windows;
using LAB_PI_3.Services;
using LAB_PI_3.ViewModels;

namespace LAB_PI_3.Views;

public partial class MainWindow : Window
{
    private readonly MainViewModel viewModel;

    public MainWindow()
    {
        InitializeComponent();

        viewModel = new MainViewModel(new JsonTodoRepository(), new MessageBoxConfirmationService());
        DataContext = viewModel;
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        await viewModel.InitializeAsync();
    }
}
