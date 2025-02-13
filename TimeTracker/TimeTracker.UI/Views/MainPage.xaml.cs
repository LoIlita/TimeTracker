using TimeTracker.UI.ViewModels;
using Microsoft.Extensions.Logging;
using System;

namespace TimeTracker.UI.Views;

public partial class MainPage : ContentPage
{
    private readonly ILogger<MainPage> _logger;
    private readonly IServiceProvider _serviceProvider;

    public MainPage(WorkTrackerViewModel viewModel, ILogger<MainPage> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _logger.LogDebug("Inicjalizacja MainPage");
        InitializeComponent();
        BindingContext = viewModel;
        _logger.LogDebug("BindingContext ustawiony na WorkTrackerViewModel");
    }
} 