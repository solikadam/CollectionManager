using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls;
using CollectionManager.Models;
using Microsoft.Maui.Storage;

namespace CollectionManager.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly DataService _dataService;
        private ObservableCollection<Collection> _collections;
        private string _newCollectionName;

        public ObservableCollection<Collection> Collections
        {
            get => _collections;
            set
            {
                _collections = value;
                OnPropertyChanged();
            }
        }

        public string NewCollectionName
        {
            get => _newCollectionName;
            set
            {
                _newCollectionName = value;
                OnPropertyChanged();
            }
        }

        public Command AddNewCollectionCommand { get; }
        public Command<Collection> DeleteCollectionCommand { get; }
        public Command<Collection> NavigateToCollectionCommand { get; }
        public Command<Collection> ExportCollectionCommand { get; } 
        public Command ImportCollectionCommand { get; } 

        public MainViewModel()
        {
            _dataService = new DataService();
            LoadCollections();
            AddNewCollectionCommand = new Command(AddNewCollection);
            DeleteCollectionCommand = new Command<Collection>(DeleteCollection);
            NavigateToCollectionCommand = new Command<Collection>(NavigateToCollection);
            ExportCollectionCommand = new Command<Collection>(async (c) => await ExportCollection(c));
            ImportCollectionCommand = new Command(async () => await ImportCollection());
        }

        private void LoadCollections()
        {
            Collections = _dataService.LoadCollections();
        }

        private void AddNewCollection()
        {
            if (!string.IsNullOrWhiteSpace(NewCollectionName))
            {
                if (Collections.Any(c => c.Name.Equals(NewCollectionName, StringComparison.OrdinalIgnoreCase)))
                {
                    Application.Current.MainPage.DisplayAlert("Błąd", "Kolekcja o tej nazwie już istnieje.", "OK");
                    return;
                }
                var newCollection = new Collection { Name = NewCollectionName };
                Collections.Add(newCollection);
                _dataService.SaveCollection(newCollection);
                NewCollectionName = string.Empty;
            }
        }

        private void DeleteCollection(Collection collection)
        {
            if (Collections.Contains(collection))
            {
                Collections.Remove(collection);
                _dataService.DeleteCollection(collection);
            }
        }

        private async void NavigateToCollection(Collection collection)
        {
            await Shell.Current.GoToAsync($"CollectionPage?Name={collection.Name}");
        }

        private async Task ExportCollection(Collection collection)
        {
            if (collection == null)
            {
                await Application.Current.MainPage.DisplayAlert("Błąd", "Wybierz kolekcję do eksportu.", "OK");
                return;
            }

            try
            {
                var fileName = $"{collection.Name}_export.txt";
                var filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), fileName);
                var success = await _dataService.ExportCollectionAsync(collection, filePath);
                if (success)
                {
                    await Application.Current.MainPage.DisplayAlert("Sukces", $"Kolekcja {collection.Name} została wyeksportowana do {filePath}.", "OK");
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Błąd", "Eksport kolekcji nie powiódł się.", "OK");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Błąd podczas eksportu: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Błąd", "Wystąpił błąd podczas eksportu kolekcji.", "OK");
            }
        }

        private async Task ImportCollection()
        {
            try
            {
                var customFileType = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.iOS, new[] { "public.text" } },
                    { DevicePlatform.Android, new[] { "text/plain" } },
                    { DevicePlatform.WinUI, new[] { ".txt" } },
                    { DevicePlatform.MacCatalyst, new[] { "public.text" } },
                });

                var result = await FilePicker.PickAsync(new PickOptions
                {
                    PickerTitle = "Wybierz plik kolekcji (.txt)",
                    FileTypes = customFileType
                });

                if (result != null)
                {
                    var collectionName = await Application.Current.MainPage.DisplayPromptAsync(
                        "Import kolekcji",
                        "Podaj nazwę dla importowanej kolekcji:",
                        "OK",
                        "Anuluj",
                        "Nowa kolekcja");

                    if (string.IsNullOrWhiteSpace(collectionName))
                    {
                        await Application.Current.MainPage.DisplayAlert("Błąd", "Nazwa kolekcji nie może być pusta.", "OK");
                        return;
                    }

                    if (Collections.Any(c => c.Name.Equals(collectionName, StringComparison.OrdinalIgnoreCase)))
                    {
                        var overwrite = await Application.Current.MainPage.DisplayAlert(
                            "Duplikat",
                            $"Kolekcja o nazwie '{collectionName}' już istnieje. Czy chcesz ją nadpisać?",
                            "Tak",
                            "Nie");
                        if (!overwrite)
                        {
                            return;
                        }
                        var existingCollection = Collections.First(c => c.Name.Equals(collectionName, StringComparison.OrdinalIgnoreCase));
                        DeleteCollection(existingCollection);
                    }

                    var collection = await _dataService.ImportCollectionAsync(result.FullPath, collectionName);
                    if (collection != null)
                    {
                        Collections.Add(collection);
                        _dataService.SaveCollection(collection);
                        await Application.Current.MainPage.DisplayAlert("Sukces", $"Kolekcja {collection.Name} została zaimportowana.", "OK");
                    }
                    else
                    {
                        await Application.Current.MainPage.DisplayAlert("Błąd", "Nie udało się zaimportować kolekcji. Sprawdź format pliku.", "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Błąd podczas importu: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Błąd", "Wystąpił błąd podczas importu kolekcji.", "OK");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}