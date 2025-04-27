using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls;
using CollectionManager.Models;

namespace CollectionManager.ViewModels
{
    public class CollectionViewModel : INotifyPropertyChanged, IQueryAttributable
    {
        private readonly DataService _dataService;
        private ObservableCollection<CollectionItem> _items;
        private string _newItemName;
        private decimal _newItemPrice;
        private string _newItemStatus; // Zmieniono typ na string
        private int _newItemSatisfaction;
        public int NewItemSatisfaction
        {
            get => _newItemSatisfaction;
            set
            {
                if (value >= 0 && value <= 10)
                {
                    _newItemSatisfaction = value;
                    OnPropertyChanged();
                }
                else
                {
                    Application.Current.MainPage.DisplayAlert("Błąd", "Zadowolenie musi być w zakresie od 0 do 10.", "OK");
                    OnPropertyChanged(nameof(NewItemSatisfaction));
                }
            }
        }
        private string _newItemComment;
        private string _collectionName;
        private Collection _currentCollection;
        public List<string> StatusOptions { get; } = new List<string> { "nowy", "używany", "na sprzedaż" }; // Dodano status "na sprzedaż"

        public ObservableCollection<CollectionItem> Items
        {
            get => _items;
            set
            {
                _items = value;
                OnPropertyChanged();
            }
        }

        public string NewItemName
        {
            get => _newItemName;
            set
            {
                _newItemName = value;
                OnPropertyChanged();
            }
        }

        public decimal NewItemPrice
        {
            get => _newItemPrice;
            set
            {
                _newItemPrice = value;
                OnPropertyChanged();
            }
        }

        public string NewItemStatus
        {
            get => _newItemStatus;
            set
            {
                _newItemStatus = value;
                OnPropertyChanged();
            }
        }

        public string NewItemComment
        {
            get => _newItemComment;
            set
            {
                _newItemComment = value;
                OnPropertyChanged();
            }
        }

        public string CollectionName
        {
            get => _collectionName;
            set
            {
                _collectionName = value;
                OnPropertyChanged();
                LoadCollectionItems();
            }
        }

        public Command AddItemCommand { get; }
        public Command<CollectionItem> DeleteItemCommand { get; }
        public Command<CollectionItem> MarkAsSoldCommand { get; } // Nowa komenda
        public Command ShowSummaryCommand { get; } // Nowa komenda

        public CollectionViewModel()
        {
            _dataService = new DataService();
            AddItemCommand = new Command(AddItem);
            DeleteItemCommand = new Command<CollectionItem>(DeleteItem);
            MarkAsSoldCommand = new Command<CollectionItem>(MarkAsSold); // Inicjalizacja nowej komendy
            ShowSummaryCommand = new Command(ShowSummary); // Inicjalizacja nowej komendy
            Items = new ObservableCollection<CollectionItem>();
            _newItemPrice = 0;
            _newItemSatisfaction = 5;
            _newItemStatus = StatusOptions.First();
        }

        private void LoadCollectionItems()
        {
            if (!string.IsNullOrWhiteSpace(CollectionName))
            {
                var allCollections = _dataService.LoadCollections();
                _currentCollection = allCollections.FirstOrDefault(c => c.Name == CollectionName);
                if (_currentCollection != null)
                {
                    // Sortowanie: najpierw niesprzedane, potem sprzedane
                    Items = new ObservableCollection<CollectionItem>(_currentCollection.Items.OrderBy(item => item.IsSold));
                }
                else
                {
                    Items.Clear();
                }
            }
        }

        private async void AddItem()
        {
            if (!string.IsNullOrWhiteSpace(NewItemName) && _currentCollection != null)
            {
                if (_currentCollection.Items.Any(item => item.Name.ToLower() == NewItemName.ToLower()))
                {
                    bool result = await Application.Current.MainPage.DisplayAlert(
                        "Duplikat elementu",
                        $"Element o nazwie '{NewItemName}' już istnieje w kolekcji. Czy chcesz go dodać ponownie?",
                        "Tak",
                        "Nie");

                    if (!result)
                    {
                        return;
                    }
                }

                var newItem = new CollectionItem
                {
                    Name = NewItemName,
                    Price = NewItemPrice,
                    Status = NewItemStatus,
                    Satisfaction = NewItemSatisfaction,
                    Comment = NewItemComment,
                    IsSold = false // Domyślnie nowy element nie jest sprzedany
                };
                _currentCollection.Items.Add(newItem);
                Items.Add(newItem);
                _dataService.SaveCollection(_currentCollection);
                NewItemName = string.Empty;
                NewItemPrice = 0;
                NewItemStatus = StatusOptions.First();
                NewItemSatisfaction = 5;
                NewItemComment = string.Empty;
            }
        }


        private void DeleteItem(CollectionItem item)
        {
            if (_currentCollection != null && _currentCollection.Items.Contains(item))
            {
                _currentCollection.Items.Remove(item);
                Items.Remove(item);
                _dataService.SaveCollection(_currentCollection);
            }
        }

        private void MarkAsSold(CollectionItem item)
        {
            if (_currentCollection != null && _currentCollection.Items.Contains(item))
            {
                item.IsSold = true;
                _dataService.SaveCollection(_currentCollection);
                // Przenieś element na koniec listy
                Items.Remove(item);
                Items.Add(item);
            }
        }

        private async void ShowSummary()
        {
            if (_currentCollection != null)
            {
                int totalItems = _currentCollection.Items.Count;
                int soldItems = _currentCollection.Items.Count(item => item.IsSold);
                int forSaleItems = _currentCollection.Items.Count(item => item.Status?.ToLower() == "na sprzedaż" && !item.IsSold);

                await Application.Current.MainPage.DisplayAlert(
                    "Podsumowanie kolekcji",
                    $"Ilość przedmiotów: {totalItems}\n" +
                    $"Przedmioty sprzedane: {soldItems}\n" +
                    $"Przedmioty na sprzedaż: {forSaleItems}",
                    "OK");
            }
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.ContainsKey("Name"))
            {
                CollectionName = query["Name"] as string;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}