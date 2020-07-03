using Groundsman.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;

namespace Groundsman.Services
{
    public class FeatureStore : IDataStore<Feature>
    {

        public ObservableCollection<Feature> features;

        public FeatureStore()
        {
            features = new ObservableCollection<Feature>();
        }

        public async Task<bool> AddItemAsync(Feature item)
        {
            if (item.properties.id == AppConstants.NEW_ENTRY_ID)
            {
                item.properties.id = Guid.NewGuid().ToString();
                features.Add(item);
            }
            else
            {
                // Otherwise we are saving over an existing feature, so override its contents without changing ID.
                int indexToEdit = -1;
                for (int i = 0; i < features.Count; i++)
                {
                    if (features[i].properties.id == item.properties.id)
                    {
                        features[indexToEdit] = item;
                        break;
                    }
                }
            }
            var save = FeatureService.SaveCurrentFeaturesToEmbeddedFile(features);
            return save;
        }

        public async Task<bool> DeleteItemAsync(Feature item)
        {
            bool deleteSuccessful = features.Remove(item);
            var save = FeatureService.SaveCurrentFeaturesToEmbeddedFile(features);
            return save;
        }

        public Task<Feature> GetItemAsync(string id)
        {
            throw new NotImplementedException();
        }

        public async Task<ObservableCollection<Feature>> GetItemsAsync(bool forceRefresh = false)
        {
            ObservableCollection<Feature> features = await FeatureService.FetchFeaturesFromFile();
            return features;
        }

        public Task<bool> UpdateItemAsync(Feature item)
        {
            throw new NotImplementedException();
        }
    }
}
