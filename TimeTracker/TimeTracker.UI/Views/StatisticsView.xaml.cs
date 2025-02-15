using TimeTracker.UI.ViewModels;

namespace TimeTracker.UI.Views;

public partial class StatisticsView : ContentPage
{
    private readonly StatisticsViewModel _viewModel;

    public StatisticsView(StatisticsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }
} 