using FeatureItem = Groundsman.Models.Feature;
using SQLite;
using System.Collections.Generic;
using System.Threading.Tasks;
using Groundsman.Models;

namespace Groundsman.Services
{
    public class Database
    {
        private class Feature
        {
            [PrimaryKey]
            public string Id { get; set; }

            public string SerialisedFeature { get; set; }

            public Feature() { }

            public Feature(string id, string serialisedFeature)
            {
                Id = id;
                SerialisedFeature = serialisedFeature;
            }
        }

        private readonly SQLiteAsyncConnection _db;


        public Database(string dbPath)
        {
            _db = new SQLiteAsyncConnection(dbPath);
            _db.CreateTableAsync<Feature>();
        }

        public async Task<List<FeatureItem>> GetFeaturesAsync()
        {
            var results = await _db.Table<Feature>().ToListAsync();
            List<FeatureItem> features = new List<FeatureItem>();
            foreach (var result in results)
            {
                if (result == null || result.SerialisedFeature == null) continue;
                FeatureItem feature = (FeatureItem)GeoJSONObject.ImportGeoJSON(result.SerialisedFeature);
                feature.Properties[Constants.IdentifierProperty] = result.Id;
                features.Add(feature);
            }
            return features;
        }

        public Task<int> AddFeatures(FeatureItem feature)
        {
            Feature serialisedFeature = new Feature(feature.Id, GeoJSONObject.ExportGeoJSON(feature));
            var results = _db.InsertOrReplaceAsync(serialisedFeature);
            return results;
        }

        public Task<int> AddFeatures(IEnumerable<FeatureItem> features)
        {
            List<Feature> serialisedFeatures = new List<Feature>();
            foreach (var feature in features)
            {
                serialisedFeatures.Add(new Feature(feature.Id, GeoJSONObject.ExportGeoJSON(feature)));
            }
            var results = _db.InsertAllAsync(serialisedFeatures);
            return results;
        }

        public Task<int> DeleteFeature(string id)
        {
            var results = _db.DeleteAsync<Feature>(id);
            return results;
        }

        public Task<int> DeleteAllFeatures()
        {
            var results = _db.DeleteAllAsync<Feature>();
            return results;
        }
    }
}
