using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CollectionManager.Models;

public class DataService
{
    private readonly string _dataPath;
    private const string ItemsFileName = "items.txt";
    private const string ItemDataSeparator = "||";

    public DataService()
    {
        _dataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CollectionsData");
        Directory.CreateDirectory(_dataPath);
#if DEBUG
        System.Diagnostics.Debug.WriteLine($"Ścieżka do danych: {_dataPath}");
#endif
    }

    public ObservableCollection<Collection> LoadCollections()
    {
        var collections = new ObservableCollection<Collection>();
        if (!Directory.Exists(_dataPath))
        {
            System.Diagnostics.Debug.WriteLine($"Folder danych nie istnieje: {_dataPath}");
            return collections;
        }

        foreach (var directory in Directory.GetDirectories(_dataPath))
        {
            var collectionName = Path.GetFileName(directory);
            if (string.IsNullOrWhiteSpace(collectionName))
            {
                System.Diagnostics.Debug.WriteLine($"Znaleziono folder bez poprawnej nazwy: {directory}");
                continue;
            }
            var collection = new Collection { Name = collectionName };
            var filePath = Path.Combine(directory, ItemsFileName);
            if (File.Exists(filePath))
            {
                try
                {
                    foreach (var line in File.ReadAllLines(filePath))
                    {
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            var parts = line.Split(new[] { ItemDataSeparator }, StringSplitOptions.None);
                            if (parts.Length >= 6)
                            {
                                if (decimal.TryParse(parts[1], out var price) &&
                                    int.TryParse(parts[3], out var satisfaction) &&
                                    bool.TryParse(parts[5], out var isSold))
                                {
                                    collection.Items.Add(new CollectionItem
                                    {
                                        Name = parts[0].Trim(),
                                        Price = price,
                                        Status = parts[2]?.Trim(),
                                        Satisfaction = satisfaction,
                                        Comment = parts[4]?.Trim(),
                                        IsSold = isSold
                                    });
                                }
                                else
                                {
                                    System.Diagnostics.Debug.WriteLine($"Błąd parsowania ceny, satysfakcji lub statusu sprzedaży w linii: {line}");
                                }
                            }
                            else if (parts.Length >= 5)
                            {
                                if (decimal.TryParse(parts[1], out var price) && int.TryParse(parts[3], out var satisfaction))
                                {
                                    collection.Items.Add(new CollectionItem
                                    {
                                        Name = parts[0].Trim(),
                                        Price = price,
                                        Status = parts[2]?.Trim(),
                                        Satisfaction = satisfaction,
                                        Comment = parts[4]?.Trim(),
                                        IsSold = false
                                    });
                                }
                                else
                                {
                                    System.Diagnostics.Debug.WriteLine($"Błąd parsowania ceny lub satysfakcji w linii (stary format): {line}");
                                }
                            }
                            else if (parts.Length == 1)
                            {
                                collection.Items.Add(new CollectionItem { Name = parts[0].Trim(), IsSold = false });
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine($"Nieprawidłowa liczba danych w linii: {line}");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Błąd podczas odczytu pliku {filePath}: {ex.Message}");
                }
            }
            collections.Add(collection);
        }
        return collections;
    }

    public void SaveCollection(Collection collection)
    {
        if (collection == null)
        {
            System.Diagnostics.Debug.WriteLine("Próba zapisania null jako kolekcji.");
            return;
        }

        if (string.IsNullOrWhiteSpace(collection.Name))
        {
            System.Diagnostics.Debug.WriteLine("Nazwa kolekcji nie może być pusta podczas zapisu.");
            return;
        }

        var sanitizedName = SanitizeFileName(collection.Name);
        if (string.IsNullOrWhiteSpace(sanitizedName))
        {
            System.Diagnostics.Debug.WriteLine($"Nieprawidłowa nazwa kolekcji po sanityzacji: {collection.Name}");
            return;
        }

        var collectionDirectory = Path.Combine(_dataPath, sanitizedName);
        Directory.CreateDirectory(collectionDirectory);
        var filePath = Path.Combine(collectionDirectory, ItemsFileName);
        try
        {
            var linesToSave = collection.Items
                .Where(item => !string.IsNullOrWhiteSpace(item?.Name))
                .Select(item => $"{item.Name.Trim()}{ItemDataSeparator}{item.Price}{ItemDataSeparator}{item.Status?.Trim() ?? ""}{ItemDataSeparator}{item.Satisfaction}{ItemDataSeparator}{item.Comment?.Trim() ?? ""}{ItemDataSeparator}{item.IsSold}")
                .ToList();
            File.WriteAllLines(filePath, linesToSave);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Błąd podczas zapisu pliku {filePath}: {ex.Message}");
        }
    }

    public void DeleteCollection(Collection collection)
    {
        if (collection == null || string.IsNullOrWhiteSpace(collection.Name))
        {
            System.Diagnostics.Debug.WriteLine("Próba usunięcia null lub kolekcji bez nazwy.");
            return;
        }

        var collectionDirectory = Path.Combine(_dataPath, SanitizeFileName(collection.Name));
        if (Directory.Exists(collectionDirectory))
        {
            try
            {
                Directory.Delete(collectionDirectory, true);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($" mężczyznBłąd podczas usuwania folderu {collectionDirectory}: {ex.Message}");
            }
        }
    }

    private string SanitizeFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return string.Empty;
        }
        return Regex.Replace(fileName, "[^a-zA-Z0-9_]+", "_");
    }

    public async Task<bool> ExportCollectionAsync(Collection collection, string filePath)
    {
        if (collection == null || string.IsNullOrWhiteSpace(collection.Name))
        {
            System.Diagnostics.Debug.WriteLine("Próba eksportu null lub kolekcji bez nazwy.");
            return false;
        }

        try
        {
            var linesToSave = collection.Items
                .Where(item => !string.IsNullOrWhiteSpace(item?.Name))
                .Select(item => $"{item.Name.Trim()}{ItemDataSeparator}{item.Price}{ItemDataSeparator}{item.Status?.Trim() ?? ""}{ItemDataSeparator}{item.Satisfaction}{ItemDataSeparator}{item.Comment?.Trim() ?? ""}{ItemDataSeparator}{item.IsSold}")
                .ToList();
            await File.WriteAllLinesAsync(filePath, linesToSave);
            System.Diagnostics.Debug.WriteLine($"Kolekcja {collection.Name} wyeksportowana do: {filePath}");
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Błąd podczas eksportu kolekcji {collection.Name}: {ex.Message}");
            return false;
        }
    }

    public async Task<Collection> ImportCollectionAsync(string filePath, string collectionName)
    {
        if (string.IsNullOrWhiteSpace(collectionName))
        {
            System.Diagnostics.Debug.WriteLine("Nazwa kolekcji nie może być pusta podczas importu.");
            return null;
        }

        var collection = new Collection { Name = collectionName, Items = new List<CollectionItem>() };
        try
        {
            if (!File.Exists(filePath))
            {
                System.Diagnostics.Debug.WriteLine($"Plik {filePath} nie istnieje.");
                return null;
            }

            foreach (var line in await File.ReadAllLinesAsync(filePath))
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    var parts = line.Split(new[] { ItemDataSeparator }, StringSplitOptions.None);
                    if (parts.Length >= 6)
                    {
                        if (decimal.TryParse(parts[1], out var price) &&
                            int.TryParse(parts[3], out var satisfaction) &&
                            bool.TryParse(parts[5], out var isSold))
                        {
                            collection.Items.Add(new CollectionItem
                            {
                                Name = parts[0].Trim(),
                                Price = price,
                                Status = parts[2]?.Trim(),
                                Satisfaction = satisfaction,
                                Comment = parts[4]?.Trim(),
                                IsSold = isSold
                            });
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"Błąd parsowania ceny, satysfakcji lub statusu sprzedaży w linii: {line}");
                        }
                    }
                    else if (parts.Length >= 5)
                    {
                        if (decimal.TryParse(parts[1], out var price) && int.TryParse(parts[3], out var satisfaction))
                        {
                            collection.Items.Add(new CollectionItem
                            {
                                Name = parts[0].Trim(),
                                Price = price,
                                Status = parts[2]?.Trim(),
                                Satisfaction = satisfaction,
                                Comment = parts[4]?.Trim(),
                                IsSold = false
                            });
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"Błąd parsowania ceny lub satysfakcji w linii (stary format): {line}");
                        }
                    }
                    else if (parts.Length == 1)
                    {
                        collection.Items.Add(new CollectionItem { Name = parts[0].Trim(), IsSold = false });
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"Nieprawidłowa liczba danych w linii: {line}");
                    }
                }
            }

            if (collection.Items.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine($"Brak poprawnych danych w pliku {filePath}.");
                return null;
            }

            collection.Name = SanitizeFileName(collectionName);
            if (string.IsNullOrWhiteSpace(collection.Name))
            {
                System.Diagnostics.Debug.WriteLine("Nieprawidłowa nazwa kolekcji po sanityzacji.");
                return null;
            }

            System.Diagnostics.Debug.WriteLine($"Zaimportowano kolekcję: {collection.Name}");
            return collection;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Błąd podczas importu kolekcji z pliku {filePath}: {ex.Message}");
            return null;
        }
    }
}