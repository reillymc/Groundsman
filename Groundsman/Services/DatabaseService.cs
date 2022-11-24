using SQLite;
using Groundsman.Models;

namespace Groundsman.Services;

public class DatabaseService
{

    private readonly SQLiteAsyncConnection db;


    public DatabaseService(string dbPath)
    {
        db = new SQLiteAsyncConnection(dbPath);
        _ = db.CreateTableAsync<StoredFeature>();
    }

    public async Task<IEnumerable<Feature>> GetFeatures()
    {
        var results = await db.Table<StoredFeature>().ToListAsync();
        List<Feature> features = new();
        foreach (var result in results)
        {
            if (result == null || result.SerialisedFeature == null) continue;
            Feature feature = (Feature)GeoJSONObject.ImportGeoJSON(result.SerialisedFeature);
            feature.Properties[DefaultProperties.Id] = result.Id;
            features.Add(feature);
        }
        return features;
    }

    public Task<int> AddFeatures(Feature feature)
    {
        StoredFeature serialisedFeature = new StoredFeature(feature.Id, GeoJSONObject.ExportGeoJSON(feature));
        var results = db.InsertOrReplaceAsync(serialisedFeature);
        return results;
    }

    public Task<int> AddFeatures(IEnumerable<Feature> features)
    {
        List<StoredFeature> serialisedFeatures = new List<StoredFeature>();
        foreach (var feature in features)
        {
            serialisedFeatures.Add(new StoredFeature(feature.Id, GeoJSONObject.ExportGeoJSON(feature)));
        }
        var results = db.InsertAllAsync(serialisedFeatures);
        return results;
    }

    public Task<int> DeleteFeature(string id)
    {
        var results = db.DeleteAsync<StoredFeature>(id);
        return results;
    }

    public Task<int> DeleteAllFeatures()
    {
        var results = db.DeleteAllAsync<StoredFeature>();
        return results;
    }
}

