using System.Collections.Generic;
using Xamarin.Essentials;

namespace Groundsman.Interfaces
{
    public interface IDataService<T>
    {
        bool AddItem(T item);
        int AddItems(IEnumerable<T> item);
        bool UpdateItem(T item);
        bool DeleteItem(T item);
        void ResetItems();
        int ImportItems(string contents);
        ShareFileRequest ExportFeatures(IList<T> items);
    }
}
