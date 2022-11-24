using Groundsman.Models;

namespace Groundsman.Services;

public class FeatureService
{
    private readonly DatabaseService databaseService;

    public FeatureService(DatabaseService databaseService)
    {
        this.databaseService = databaseService;
    }

    /// <summary>
    /// Add or update a feature
    /// </summary>
    /// <param name="feature">Feature to add</param>
    /// <returns>Number of features saved</returns>
    public Task<int> Save(Feature feature) => databaseService.AddFeatures(feature);

    /// <summary>
    /// Add or update features
    /// </summary>
    /// <param name="features">Features to add</param>
    /// <returns>Number of features saved</returns>
    public Task<int> Save(IEnumerable<Feature> features) => databaseService.AddFeatures(features);


    /// <summary>
    /// Fetches all features from database then updates the feature list and also returns the list
    /// </summary>
    /// <returns></returns>
    public Task<IEnumerable<Feature>> GetFeatures() => databaseService.GetFeatures();

    /// <summary>
    /// Clears the feature list
    /// </summary>
    public Task<int> ClearItems() => databaseService.DeleteAllFeatures();
}

