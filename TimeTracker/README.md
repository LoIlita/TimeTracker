# 🕒 TimeTracker - Aplikacja do Monitorowania Czasu Aktywności

## 📝 Opis Projektu

TimeTracker to aplikacja napisana w C# (.NET 8) z interfejsem w MAUI, zgodna z zasadami DDD. Służy do monitorowania i analizy czasu spędzonego na różnych aktywnościach poprzez organizację ich w projekty i sesje.

## ✨ Funkcjonalności

### Zaimplementowane:

- ✅ Podstawowe śledzenie czasu (start, pauza, koniec)
- ✅ Historia sesji
- ✅ Precyzyjny timer z możliwością wstrzymania
- ✅ Opisy sesji i notatki
- ✅ Automatyczne zapisywanie
- ✅ Walidacja danych wejściowych
- ✅ Obsługa błędów i logowanie
- ✅ Organizacja aktywności w projekty
- ✅ Serwis dialogowy do komunikacji z użytkownikiem
- ✅ Lista projektów z wyszukiwaniem
- ✅ Filtrowanie projektów według typu
- ✅ Podstawowa nawigacja między widokami
- ✅ Konwerter typów projektów
- ✅ Formularz tworzenia/edycji projektu
- ✅ Widok szczegółów projektu (podstawowy)
- ✅ Integracja projektów z sesjami (model danych)
- ✅ Serwis wyboru projektu (ProjectSelector)
- ✅ Rejestracja ProjectSelector w DI
- ✅ Integracja ProjectSelector z WorkTrackerViewModel
- ✅ Wyświetlanie nazwy projektu w sesji
- ✅ Stylizacja pola projektu w sesji
- ✅ Obsługa przypadku braku projektu
- ✅ Filtrowanie sesji po projekcie

## 🎯 Plan rozwoju

### Etap 1 - Integracja UI z Projektami ⏳

#### Krok 1 - Wyświetlanie informacji o projekcie w sesji:

1. Aktualizacja szablonu sesji:

   - [ ] Dodanie pola z nazwą projektu w DataTemplate dla WorkSessionDto
   - [ ] Stylizacja nowego pola
   - [ ] Obsługa przypadku braku projektu

2. Aktualizacja filtrowania:

   - [ ] Dodanie możliwości filtrowania sesji po projekcie
   - [ ] Rozszerzenie interfejsu o filtr projektów
   - [ ] Implementacja logiki filtrowania

3. Testy:
   - [ ] Testy jednostkowe dla filtrowania
   - [ ] Testy integracyjne dla wyświetlania
   - [ ] Testy UI dla nowych funkcjonalności

#### Krok 2 - Usprawnienia interfejsu:

1. Interakcje z projektem:

   - [ ] Przejście do szczegółów projektu po kliknięciu nazwy
   - [ ] Szybka zmiana projektu dla sesji
   - [ ] Podgląd podstawowych statystyk projektu

2. Rozszerzenie filtrowania:

   - [ ] Filtrowanie po dacie
   - [ ] Filtrowanie po statusie (aktywne/zakończone)
   - [ ] Sortowanie wyników

3. Testy:
   - [ ] Testy jednostkowe dla filtrowania
   - [ ] Testy integracyjne dla wyświetlania
   - [ ] Testy UI dla nowych funkcjonalności

### Etap 2 - Ulepszenia UX 🎨

1. Usprawnienia interfejsu:

   - [ ] Dodanie tooltipów dla kontrolek
   - [ ] Wizualne oznaczenie aktywnego projektu
   - [ ] Animacje przejść między stanami
   - [ ] Responsywność layoutu

2. Obsługa błędów:
   - [ ] Rozbudowa komunikatów błędów
   - [ ] Dodanie obsługi przypadków brzegowych
   - [ ] Ulepszenie walidacji

### Etap 3 - Analityka i Raporty 📊

1. Podstawowe statystyki:

   - [ ] Implementacja agregacji danych
   - [ ] Generowanie raportów dziennych/tygodniowych
   - [ ] Eksport danych do CSV

2. Wizualizacje:
   - [ ] Wykresy czasu pracy
   - [ ] Statystyki projektów
   - [ ] Trendy produktywności

## 📋 Następne kroki (priorytetowe):

1. Aktualizacja szablonu sesji:

   ```xaml
   <!-- W MainPage.xaml -->
   <DataTemplate x:DataType="models:WorkSessionDto">
       <Frame>
           <!-- Dodanie wyświetlania projektu -->
           <Label Text="{Binding ProjectName}" />
       </Frame>
   </DataTemplate>
   ```

2. Implementacja filtrowania:

   ```csharp
   // W WorkTrackerViewModel
   private void FilterSessions()
   {
       // Dodanie filtrowania po projekcie
   }
   ```

3. Testy:
   ```csharp
   // Nowe testy dla filtrowania i wyświetlania
   public class WorkTrackerViewModelTests
   {
       [Fact]
       public void FilterSessions_WithProject_ShowsOnlyProjectSessions()
   }
   ```

## 🧪 Testy

### Zaimplementowane:

- ✅ Testy jednostkowe WorkSession
- ✅ Testy WorkSessionValidator
- ✅ Testy WorkSessionService
- ✅ Testy TimerService
- ✅ Testy ProjectService
- ✅ Testy ProjectRepository
- ✅ Testy ActivitySession
- ✅ Testy ProjectFormViewModel
- ✅ Testy ProjectSelector

### Do implementacji:

- [ ] Testy filtrowania sesji po projekcie
- [ ] Testy wyświetlania informacji o projekcie
- [ ] Testy UI dla nowych elementów interfejsu

## 🛠️ Zasady Rozwoju

1. SOLID:

   - Single Responsibility: każda klasa ma jedno zadanie
   - Open/Closed: rozszerzamy przez dziedziczenie
   - Liskov Substitution: poprawna hierarchia klas
   - Interface Segregation: małe, specyficzne interfejsy
   - Dependency Inversion: zależności od abstrakcji

2. Clean Code:

   - Czytelne nazewnictwo
   - Małe, skupione metody
   - DRY (Don't Repeat Yourself)
   - KISS (Keep It Simple, Stupid)

3. Best Practices:
   - Pełne logowanie
   - Obsługa błędów
   - Dokumentacja kodu
   - Code review

## 🔧 Wymagania

- .NET 8
- Visual Studio 2022
- SQLite

## 📋 Instrukcja uruchomienia

1. Sklonuj repozytorium
2. Otwórz TimeTracker.sln w Visual Studio
3. Uruchom aplikację (F5)

## ⚠️ Znane problemy

- Komenda && nie działa w PowerShell - należy wykonywać polecenia pojedynczo
- Problemy z wyświetlaniem dialogów - w trakcie naprawy

## 👥 Autorzy
