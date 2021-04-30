using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Groundsman.Interfaces
{
    public interface IDataService<T>
    {
        Task<bool> AddItem(T item, bool save = true);
        Task<bool> ImportItem(T item, bool save = true);
        Task<bool> UpdateItem(T item);
        Task<bool> DeleteItem(T item);
        Task ResetItems();
        Task<int> ImportItems(IEnumerable<T> items);
        Task<int> ImportRawContents(string contents);
        Task<string> ExportFeature(T item);
        Task<string> ExportFeatures(IEnumerable<T> items);
        ObservableCollection<T> GetItems();
    }
}
