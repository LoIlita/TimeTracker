using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Runtime.InteropServices;

namespace TimeTracker.UI.Services;

public class GitHubUpdateService : IUpdateService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<GitHubUpdateService> _logger;
    private readonly string _currentVersion;
    private const string GITHUB_API_URL = "https://api.github.com/repos/annapawlik/TimeTracker/releases/latest";

    public GitHubUpdateService(ILogger<GitHubUpdateService> logger)
    {
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "TimeTracker-App");
        _logger = logger;
        _currentVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0";
        
        _logger.LogInformation("Serwis aktualizacji zainicjalizowany. Aktualna wersja: {Version}", _currentVersion);
    }

    public async Task<UpdateInfo?> CheckForUpdatesAsync()
    {
        try
        {
            _logger.LogInformation("Sprawdzanie dostępności aktualizacji...");
            
            var response = await _httpClient.GetFromJsonAsync<GitHubRelease>(GITHUB_API_URL);
            if (response == null)
            {
                _logger.LogWarning("Nie udało się pobrać informacji o najnowszej wersji");
                return null;
            }

            var latestVersion = response.tag_name.TrimStart('v');
            _logger.LogInformation("Znaleziono wersję: {Version}", latestVersion);

            if (IsNewerVersion(latestVersion, _currentVersion))
            {
                _logger.LogInformation("Dostępna jest nowsza wersja: {Version}", latestVersion);
                
                var asset = response.assets.FirstOrDefault(a => 
                    a.name.Contains("TimeTracker") && 
                    a.name.EndsWith(".zip") && 
                    a.name.Contains(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "win" : "osx")
                );

                if (asset == null)
                {
                    _logger.LogWarning("Nie znaleziono odpowiedniego pliku do pobrania");
                    return null;
                }

                return new UpdateInfo
                {
                    Version = latestVersion,
                    DownloadUrl = asset.browser_download_url,
                    ReleaseNotes = response.body,
                    IsRequired = response.body.Contains("[REQUIRED]")
                };
            }
            
            _logger.LogInformation("Używana jest najnowsza wersja aplikacji");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas sprawdzania aktualizacji");
            return null;
        }
    }

    public async Task<bool> DownloadAndInstallUpdateAsync(UpdateInfo updateInfo)
    {
        try
        {
            _logger.LogInformation("Rozpoczynanie pobierania aktualizacji {Version}", updateInfo.Version);
            
            var downloadPath = Path.Combine(
                Path.GetTempPath(),
                $"TimeTracker_Update_{updateInfo.Version}.zip"
            );

            // Pobierz plik aktualizacji
            _logger.LogInformation("Pobieranie pliku do: {Path}", downloadPath);
            using (var response = await _httpClient.GetAsync(updateInfo.DownloadUrl))
            using (var fs = new FileStream(downloadPath, FileMode.Create))
            {
                await response.Content.CopyToAsync(fs);
            }

            // Przygotuj ścieżkę do rozpakowania
            var extractPath = Path.Combine(
                Path.GetTempPath(),
                $"TimeTracker_Update_{updateInfo.Version}"
            );
            _logger.LogInformation("Rozpakowywanie do: {Path}", extractPath);

            // Rozpakuj archiwum
            System.IO.Compression.ZipFile.ExtractToDirectory(downloadPath, extractPath, true);

            // Przygotuj skrypt aktualizacji
            var currentDir = AppDomain.CurrentDomain.BaseDirectory;
            var updateScript = Path.Combine(Path.GetTempPath(), "update.bat");
            var scriptContent = $@"
@echo off
timeout /t 2 /nobreak
xcopy /s /y ""{extractPath}\*.*"" ""{currentDir}""
start """" ""{currentDir}\TimeTracker.UI.exe""
del ""{downloadPath}""
rmdir /s /q ""{extractPath}""
del ""%~f0""
";

            await File.WriteAllTextAsync(updateScript, scriptContent);
            _logger.LogInformation("Utworzono skrypt aktualizacji: {Path}", updateScript);

            // Uruchom skrypt aktualizacji
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = updateScript,
                UseShellExecute = true,
                CreateNoWindow = true
            });

            _logger.LogInformation("Uruchomiono skrypt aktualizacji");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas pobierania i instalowania aktualizacji");
            return false;
        }
    }

    private bool IsNewerVersion(string latestVersion, string currentVersion)
    {
        var latest = Version.Parse(latestVersion);
        var current = Version.Parse(currentVersion);
        return latest > current;
    }

    private class GitHubRelease
    {
        public string tag_name { get; set; } = string.Empty;
        public string body { get; set; } = string.Empty;
        public GitHubAsset[] assets { get; set; } = Array.Empty<GitHubAsset>();
    }

    private class GitHubAsset
    {
        public string name { get; set; } = string.Empty;
        public string browser_download_url { get; set; } = string.Empty;
    }
} 