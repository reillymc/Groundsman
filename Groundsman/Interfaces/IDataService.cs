﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace Groundsman.Interfaces
{
    public interface IDataService<T>
    {
        Task<bool> AddItemAsync(T item);
        Task<bool> UpdateItemAsync(T item);
        Task<bool> DeleteItemAsync(T item);
        Task<bool> DeleteItemsAsync();
        Task<int> ImportFeaturesAsync(string contents);
        ShareFileRequest ExportFeatures(IList<T> items);
    }
}
