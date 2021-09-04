using Groundsman.Models;
using SQLite;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Groundsman.Services
{
    public class Database
    {
        private readonly SQLiteAsyncConnection _db;


        public Database(string dbPath)
        {
            _db = new SQLiteAsyncConnection(dbPath);
            _db.CreateTableAsync<Feature>();
        }

        public Task<List<Feature>> GetFeaturesAsync()
        {
            var results = _db.Table<Feature>().ToListAsync();
            return results;
        }

        public Task<int> AddFeatures(Feature feature)
        {
            var results = _db.InsertOrReplaceAsync(feature);
            return results;
        }

        public Task<int> AddFeatures(IEnumerable<Feature> features)
        {
            var results = _db.InsertAllAsync(features);
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
