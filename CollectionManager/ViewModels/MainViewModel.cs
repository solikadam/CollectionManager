// ViewModels/MainViewModel.cs
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls;
using CollectionManager.Models;

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

        public MainViewModel()
        {
            _dataService = new DataService();
            LoadCollections();
            AddNewCollectionCommand = new Command(AddNewCollection);
            DeleteCollectionCommand = new Command<Collection>(DeleteCollection);
            NavigateToCollectionCommand = new Command<Collection>(NavigateToCollection);
        }

        private void LoadCollections()
        {
            Collections = _dataService.LoadCollections();
        }

        private void AddNewCollection()
        {
            if (!string.IsNullOrWhiteSpace(NewCollectionName))
            {
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

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

