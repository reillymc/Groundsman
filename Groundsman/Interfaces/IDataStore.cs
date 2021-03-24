using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Groundsman.Interfaces
{
    public interface IDataStore<T>
    {
        Task<bool> AddItemAsync(T item);
        Task<bool> UpdateItemAsync(T item);
        Task<bool> DeleteItemAsync(T item);
        Task<bool> DeleteItemsAsync();
        Task<T> GetItemAsync(string id);
        Task<ObservableCollection<T>> GetItemsAsync(bool forceRefresh = false);
        Task<int> ImportFeaturesAsync(string contents, bool notify);
        Task<bool> ExportFeatures(IList<T> features);
    }
}
