using SQLite;

namespace Groundsman.Models
{
    /// <summary>
    /// Database model for storing Features as serialised GeoJSON paired with the feature id.
    /// </summary>
    [Table("features")]
    public class StoredFeature
    {
        [PrimaryKey]
        public string Id { get; set; }

        public string SerialisedFeature { get; set; }

        public StoredFeature() { }

        public StoredFeature(string id, string serialisedFeature)
        {
            Id = id;
            SerialisedFeature = serialisedFeature;
        }
    }
}
