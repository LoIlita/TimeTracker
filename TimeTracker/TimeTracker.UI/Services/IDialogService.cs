using System.Threading.Tasks;

namespace TimeTracker.UI.Services;

public interface IDialogService
{
    Task ShowErrorAsync(string title, string message);
    Task ShowInfoAsync(string title, string message);
    Task<bool> ShowConfirmationAsync(string title, string message);
    Task<string?> ShowInputAsync(string title, string message, string defaultValue = "");
} 