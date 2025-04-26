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
        private string _newItemComment;
        private string _collectionName;
        private Collection _currentCollection;
        public List<string> StatusOptions { get; } = new List<string> { "nowy", "używany" };

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

        public int NewItemSatisfaction
        {
            get => _newItemSatisfaction;
            set
            {
                _newItemSatisfaction = value;
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

        public CollectionViewModel()
        {
            _dataService = new DataService();
            AddItemCommand = new Command(AddItem);
            DeleteItemCommand = new Command<CollectionItem>(DeleteItem);
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
                    Items = new ObservableCollection<CollectionItem>(_currentCollection.Items);
                }
                else
                {
                    Items.Clear();
                }
            }
        }

        private void AddItem()
        {
            if (!string.IsNullOrWhiteSpace(NewItemName) && _currentCollection != null)
            {
                var newItem = new CollectionItem
                {
                    Name = NewItemName,
                    Price = NewItemPrice,
                    Status = NewItemStatus,
                    Satisfaction = NewItemSatisfaction,
                    Comment = NewItemComment
                };
                _currentCollection.Items.Add(newItem);
                Items.Add(newItem);
                _dataService.SaveCollection(_currentCollection);
                NewItemName = string.Empty;
                NewItemPrice = 0;
                NewItemStatus = "nowy"; // Reset do domyślnej wartości
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