using Groundsman.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Groundsman.Interfaces
{
    public interface IDataService<T>
    {
        ObservableCollection<Feature> FeatureList { get; }

        Task<int> SaveItem(T item);
        Task<int> SaveItems(IEnumerable<T> item);
        Task<int> DeleteItem(T item);
        Task<int> ClearItems();
        Task<int> ImportRawContents(string contents);

        Task<IEnumerable<T>> GetItemsAsync();

    }
}
