# TimeTracker

TimeTracker to aplikacja do śledzenia czasu pracy i aktywności, stworzona w technologii .NET MAUI.

## Wymagania systemowe

- Windows 10 (wersja 10.0.17763.0 lub nowsza)
- .NET 8.0 SDK (lub nowszy)
- Visual Studio 2022 (opcjonalnie, do rozwoju aplikacji)

## Instalacja

### Metoda 1: Instalacja z plików źródłowych

1. Sklonuj repozytorium:

```bash
git clone https://github.com/LoIlita/WorkTracker.git
cd WorkTracker
```

2. Skompiluj i uruchom aplikację:

```bash
dotnet build
dotnet run --project TimeTracker/TimeTracker.UI/TimeTracker.UI.csproj
```

### Metoda 2: Instalacja z pliku wykonywalnego

1. Pobierz najnowszą wersję aplikacji z zakładki [Releases](https://github.com/LoIlita/WorkTracker/releases)
2. Rozpakuj pobrany plik ZIP
3. Uruchom plik `TimeTracker.exe`

## Funkcje

- Śledzenie czasu pracy
- Dodawanie opisów i hashtagów do sesji
- Historia sesji z filtrowaniem
- Statystyki pracy
- Tworzenie kopii zapasowych

## Rozwój aplikacji

Aby rozpocząć rozwój aplikacji:

1. Zainstaluj wymagane narzędzia:

   - Visual Studio 2022 z obsługą .NET MAUI
   - .NET 8.0 SDK

2. Otwórz solucję `TimeTracker.sln` w Visual Studio

3. Skompiluj i uruchom projekt `TimeTracker.UI`

## Licencja

MIT License

## Autor

LoIlita
