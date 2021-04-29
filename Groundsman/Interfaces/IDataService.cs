using System.Collections.Generic;
using Xamarin.Essentials;

namespace Groundsman.Interfaces
{
    public interface IDataService<T>
    {
        bool AddItem(T item, bool save = true);
        bool ImportItem(T item, bool save = true);
        bool UpdateItem(T item);
        bool DeleteItem(T item);
        void ResetItems();
        int ImportItems(IEnumerable<T> items);
        int ImportRawContents(string contents);
        ShareFileRequest ExportFeature(T item);
        ShareFileRequest ExportFeatures(IEnumerable<T> items);
    }
}
