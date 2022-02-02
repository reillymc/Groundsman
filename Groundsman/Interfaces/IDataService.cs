namespace Groundsman.Interfaces;

public interface IDataService<T>
{
    ICollection<T> FeatureList { get; }

    Task<int> SaveItem(T item);
    Task<int> SaveItems(IEnumerable<T> item);
    Task<int> DeleteItem(string id);
    Task<int> ClearItems();
    Task<int> ImportRawContents(string contents);

    Task<IEnumerable<T>> GetItemsAsync();
}
