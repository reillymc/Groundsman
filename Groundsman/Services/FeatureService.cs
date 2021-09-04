using Groundsman.Interfaces;
using Groundsman.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Groundsman.Services
{
    public class FeatureService : IDataService<Feature>
    {
        public ObservableCollection<Feature> FeatureList { get; set; }

        public FeatureService()
        {
            FeatureList = new ObservableCollection<Feature>();
            _ = GetItemsAsync();
        }

        /// <summary>
        /// Add or update a feature
        /// </summary>
        /// <param name="feature">Feature to add</param>
        /// <returns>Number of items saved</returns>
        public Task<int> SaveItem(Feature feature)
        {
            feature.GeometryBlobbed = JsonConvert.SerializeObject(feature.Geometry);
            feature.PropertiesBlobbed = JsonConvert.SerializeObject(feature.Properties);
            return App.Database.AddFeatures(feature);
        }

        public Task<int> SaveItems(IEnumerable<Feature> features)
        {
            foreach (var feature in features)
            {
                feature.GeometryBlobbed = JsonConvert.SerializeObject(feature.Geometry);
                feature.PropertiesBlobbed = JsonConvert.SerializeObject(feature.Properties);
            }
            return App.Database.AddFeatures(features);
        }

        /// <summary>
        /// Delete a feature
        /// </summary>
        /// <param name="feature">Feature to delete</param>
        /// <returns>Number of items deleted</returns>
        public Task<int> DeleteItem(Feature feature)
        {
            return App.Database.DeleteFeature(feature.Id);
        }

        /// <summary>
        /// Fetches all features from database then updates the feature list and also returns the list
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<Feature>> GetItemsAsync()
        {
            List<Feature> newFeatures = await App.Database.GetFeaturesAsync();

            if (newFeatures == null || newFeatures.Count < 1)
            {
                FeatureList.Clear();
            }
            else
            {

                var orderedFeatures = newFeatures.OrderBy(feature => feature.Name).ToList();
                FeatureList.Clear();
                foreach (var newFeature in orderedFeatures)
                {
                    try {
                        newFeature.Geometry = JsonConvert.DeserializeObject<Geometry>(newFeature.GeometryBlobbed);
                        newFeature.Properties = JsonConvert.DeserializeObject<Dictionary<string, object>>(newFeature.PropertiesBlobbed);
                        FeatureList.Add(newFeature);
                    }
                    catch (Exception ex)
                    {
                        //handle error - warning / prompt for export
                    }
                    
                }
            }
            return FeatureList;


            //for (int i = 0; i < newFeatures.Count; i++)
            //{
            //    newFeatures[i].Geometry = JsonConvert.DeserializeObject<Geometry>(newFeatures[i].GeometryBlobbed);
            //    newFeatures[i].Properties = JsonConvert.DeserializeObject<Dictionary<string, object>>(newFeatures[i].PropertiesBlobbed);
            //};

            //if (newFeatures.Count() > 0)
            //{
            //    List<Feature> itemsToRemove = FeatureList.Where(e => !newFeatures.Any(u => e.Id == u.Id)).ToList();

            //    foreach (var item in itemsToRemove)
            //    {
            //        _ = FeatureList.Remove(item);
            //    };
            //}

            //for (int i = 0; i < FeatureList.Count; i++)
            //{
            //    Feature updatedItem = newFeatures.First(u => FeatureList[i].Id == u.Id);
            //    FeatureList[i] = updatedItem;
            //};


            //List<Feature> itemsToAdd = newFeatures.Where(u => !FeatureList.Any(e => e.Id == u.Id)).ToList();

            //foreach (var item in itemsToAdd)
            //{
            //    FeatureList.Add(item);
            //};
            //return newFeatures;
        }

        /// <summary>
        /// Imports GeoJSON and adds the new features to the feature list
        /// </summary>
        /// <param name="contents">A serialised GeoJSON object</param>
        /// <returns>Number of features successfully imported</returns>
        public Task<int> ImportRawContents(string contents)
        {
            GeoJSONObject importedGeoJSON = GeoJSONObject.ImportGeoJSON(contents);
            if (importedGeoJSON == null)
            {
                throw new ArgumentException("Import data does not contain valid GeoJSON");
            }

            return importedGeoJSON.Type switch
            {
                GeoJSONType.Point => SaveItem(new Feature((Point)importedGeoJSON)),
                GeoJSONType.LineString => SaveItem(new Feature((LineString)importedGeoJSON, null)),
                GeoJSONType.Polygon => SaveItem(new Feature((Polygon)importedGeoJSON)),
                GeoJSONType.Feature => SaveItem((Feature)importedGeoJSON),
                GeoJSONType.FeatureCollection => SaveItems(((FeatureCollection)importedGeoJSON).Features),
                _ => throw new ArgumentException("Import data does not contain a supported GeoJSON type."),
            };
        }

        /// <summary>
        /// Clears the feature list
        /// </summary>
        public Task<int> ClearItems()
        {
            return App.Database.DeleteAllFeatures();
        }
    }
}
