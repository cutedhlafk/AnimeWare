# Velocity Studio — pierwsza wersja nowego projektu

Samodzielna aplikacja Windows z ciemnym motywem i łososiowo-różowym akcentem. Nazwa „Velocity Studio” jest robocza. Kod tej aplikacji został napisany od zera w C# / WPF; nie bazuje na zdekompilowanym C++.

## Uruchomienie

Otwórz `App/VelocityStudio.exe`. Zachowaj pozostałe pliki w folderze App obok programu. Aplikacja korzysta z .NET Desktop Runtime 8 dla Windows; na komputerze, na którym ją przygotowano, została uruchomiona i przetestowana.

## Co działa

- Trzy zakładki: Wygląd, Profile i Zasoby.
- Łososiowy akcent oraz dwa warianty: różowy i brzoskwiniowy.
- Podgląd skali, krycia, odstępów i podpisu zmieniający się na żywo.
- Zapisywanie i wczytywanie nazwanych profili.
- Import i eksport profilu jako JSON.
- Wczytanie własnej grafiki PNG/JPG do galerii Zasoby i jej zachowanie między uruchomieniami.
- Osadzone czcionki Inter oraz 19 tymczasowych ikon z dostarczonego DLL.
- Walidacja importowanych ustawień i ochrona uszkodzonego pliku profili przed nadpisaniem.

Przycisk „Zapisz profil” zachowuje aktualny wygląd. Import pokazuje profil do sprawdzenia, a jego trwały zapis wymaga użycia tego przycisku. Zmiany suwaków dotyczą panelu podglądu, nie wielkości całego okna. Własna grafika pojawia się w zakładce Zasoby; jej docelowe użycie w układzie możemy ustalić po otrzymaniu projektu graficznego.

Dane trafiają do `App/UserData`: `profiles.json` i opcjonalnie `brand.png`. Program jest lokalny, nie wymaga konta ani połączenia sieciowego.

## Zakres tej wersji

Jest to działający fundament interfejsu i konfiguracji. Integracja z CS2 oraz moduły rozgrywki nie zostały zaimplementowane. W projekcie nie użyto odzyskanego logo NW jako nowej identyfikacji. Docelowe grafiki dostarczy użytkownik.

## Kod i dalszy rozwój

- `MainWindow.xaml` — układ widoków.
- `App.xaml` — wspólne style i kontrolki.
- `MainWindow.xaml.cs` — interakcje, podgląd i grafika.
- `ProfileStore.cs` — model ustawień, walidacja i zapis.
- `Assets/` — pliki graficzne, ikony i czcionki.
- `SelfTests.cs` — test zapisu, importu, błędnych danych oraz działania interfejsu.

Do budowania potrzebny jest .NET SDK 8 z obsługą aplikacji Windows. Uruchom `build.ps1` w PowerShell albo wykonaj:

```powershell
dotnet build VelocityStudio.csproj -c Release -o App --configfile NuGet.Config
```

Projekt nie wymaga dodatkowych pakietów NuGet. Testy można uruchomić przez `App/VelocityStudio.exe --self-test`; wynik jest zapisywany do `App/self-test-result.txt`. Nie korzystają z rzeczywistych profili użytkownika.

## Sprawdzenie wersji

Kompilacja Release: bez błędów i ostrzeżeń. Testy: zapis/odczyt, import/eksport, polskie znaki, walidacja zakresów, zachowanie uszkodzonych danych, reakcja kontrolek, nawigacja, zapis/wczytanie z interfejsu i ładowanie 19 ikon. Widok aplikacji został wyrenderowany i obejrzany. Nie wykonano testu na innych komputerach ani pełnego testu dostępności.

Zachowaj kopię ZIP i katalogu źródłowego jako punkt startowy dalszych zmian.
