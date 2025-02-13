using Microsoft.UI.Xaml;
using System.Diagnostics;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TimeTracker.UI.WinUI;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : MauiWinUIApplication
{
	/// <summary>
	/// Initializes the singleton application object.  This is the first line of authored code
	/// executed, and as such is the logical equivalent of main() or WinMain().
	/// </summary>
	public App()
	{
		this.InitializeComponent();

		UnhandledException += App_UnhandledException;
	}

	private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
	{
		Debug.WriteLine($"=== NIEOBSŁUŻONY WYJĄTEK ===");
		Debug.WriteLine($"Message: {e.Message}");
		Debug.WriteLine($"Exception: {e.Exception}");
		if (e.Exception.InnerException != null)
		{
			Debug.WriteLine($"Inner Exception: {e.Exception.InnerException.Message}");
		}
	}

	protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp().GetAwaiter().GetResult();
}

