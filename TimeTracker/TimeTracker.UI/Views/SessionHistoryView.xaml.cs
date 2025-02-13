using TimeTracker.UI.ViewModels;

namespace TimeTracker.UI.Views;

public partial class SessionHistoryView : ContentPage
{
    private readonly SessionHistoryViewModel _viewModel;

    public SessionHistoryView(SessionHistoryViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }
} 