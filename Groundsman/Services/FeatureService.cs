using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Groundsman.Interfaces;
using Groundsman.Models;
using Newtonsoft.Json;

namespace Groundsman.Services
{
    public class FeatureService : IDataService<Feature>
    {
        private readonly ObservableCollection<Feature> FeatureList = new ObservableCollection<Feature>();
        public ObservableCollection<Feature> GetItems() => FeatureList;

        /// <summary>
        /// Add a feature to the feature list
        /// </summary>
        /// <param name="item">Feature to add</param>
        /// <param name="save">Whether to save features to file</param>
        /// <returns>Success or fail</returns>
        public async Task<bool> AddItem(Feature item, bool save = true)
        {
            try
            {
                item.Properties[Constants.IdentifierProperty] = Guid.NewGuid().ToString();
                FeatureList.Add(item);
            }
            catch
            {
                return false;
            }
            if (save)
            {
                await SaveFeaturesToFile(FeatureList, Constants.FEATURES_FILE);
            }
            return true;
        }

        /// <summary>
        /// Imports a feature and adds it to the feature list
        /// </summary>
        /// <param name="item">Feature to import</param>
        /// <param name="save">Whether to save features to file</param>
        /// <returns>Success or fail</returns>
        public async Task<bool> ImportItem(Feature item, bool save = true)
        {
            if (item.Properties == null)
            {
                item.Properties = new Dictionary<string, object> { };
            }
            else
            {
                // If author ID hasn't been set on the feature, default it to the user's ID.
                string author = "Unknown";
                if (item.Properties.ContainsKey(Constants.AuthorProperty))
                {
                    author = (string)item.Properties[Constants.AuthorProperty];
                    if (author.Length > 30)
                    {
                        author = author.Substring(0, 30);
                    }
                }
                item.Properties[Constants.AuthorProperty] = author;

                // Add default name if empty
                string name = item.Geometry.Type.ToString();
                if (item.Properties.ContainsKey(Constants.NameProperty))
                {
                    name = (string)item.Properties[Constants.NameProperty];
                    if (name.Length > 30)
                    {
                        name = name.Substring(0, 30);
                        foreach (char c in Path.GetInvalidFileNameChars())
                        {
                            name = name.Replace(c, '-');
                        }
                    }
                }
                item.Properties[Constants.NameProperty] = name;

                // If the date field is missing or invalid, convert it into DateTime.Now.
                DateTime date = DateTime.Now;
                if (item.Properties.ContainsKey(Constants.DateProperty))
                {
                    DateTime.TryParse((string)item.Properties[Constants.DateProperty], out date);
                }
                item.Properties[Constants.DateProperty] = date.ToShortDateString();
            }
            return await AddItem(item, save);
        }

        /// <summary>
        /// Update a feature in the feature list based on feature Id
        /// </summary>
        /// <param name="item">New feature to replace old feature with same Id</param>
        /// <returns>Success if the feature was found and updated or fail if not</returns>
        public async Task<bool> UpdateItem(Feature item)
        {
            for (int i = 0; i < FeatureList.Count; i++)
            {
                if (FeatureList[i].Properties[Constants.IdentifierProperty] == item.Properties[Constants.IdentifierProperty])
                {
                    FeatureList[i] = item;
                    await SaveFeaturesToFile(FeatureList, Constants.FEATURES_FILE);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Delete feature from the feature list
        /// </summary>
        /// <param name="item">Feature to delete</param>
        /// <returns>Success or fail</returns>
        public async Task<bool> DeleteItem(Feature item)
        {
            bool deleteSuccessful = FeatureList.Remove(item);
            await SaveFeatureToFile(item, Constants.DELETED_FEATURE_FILE); // Save deleted feature
            await SaveFeaturesToFile(FeatureList, Constants.FEATURES_FILE);
            return deleteSuccessful;
        }

        /// <summary>
        /// Clears and resets feature list to default template items
        /// </summary>
        public async Task ResetItems()
        {
            FeatureList.Clear();
            await ImportRawContents(Constants.GetTemplateFile());
            await SaveFeaturesToFile(FeatureList, Constants.FEATURES_FILE);
        }

        /// <summary>
        /// Imports features and adds them to the feature list
        /// </summary>
        /// <param name="items">IEnumerable of features</param>
        /// <returns>Number of features successfully imported</returns>
        public async Task<int> ImportItems(IEnumerable<Feature> items)
        {
            int successfulImport = 0;
            foreach (Feature item in items)
            {
                if (await ImportItem(item, false))
                {
                    successfulImport++;
                }
            }
            await SaveFeaturesToFile(FeatureList, Constants.FEATURES_FILE);
            return successfulImport;
        }

        /// <summary>
        /// Imports GeoJSON and adds the new features to the feature list
        /// </summary>
        /// <param name="contents">A serialised GeoJSON object</param>
        /// <returns>Number of features successfully imported</returns>
        public async Task<int> ImportRawContents(string contents)
        {
            GeoJSONObject importedGeoJSON = GeoJSONObject.ImportGeoJSON(contents);
            if (importedGeoJSON == null)
            {
                throw new ArgumentException("Import data does not contain valid GeoJSON");
            }

            return importedGeoJSON.Type switch
            {
                GeoJSONType.Point => await ImportItem(new Feature((Models.Point)importedGeoJSON)) ? 1 : 0,
                GeoJSONType.LineString => await ImportItem(new Feature((LineString)importedGeoJSON)) ? 1 : 0,
                GeoJSONType.Polygon => await ImportItem(new Feature((Polygon)importedGeoJSON)) ? 1 : 0,
                GeoJSONType.Feature => await ImportItem((Feature)importedGeoJSON) ? 1 : 0,
                GeoJSONType.FeatureCollection => await ImportItems(((FeatureCollection)importedGeoJSON).Features),
                _ => throw new ArgumentException("Import data does not contain a supported GeoJSON type."),
            };
        }

        /// <summary>
        /// Exports features to a sharefile request
        /// </summary>
        /// <param name="items">IEnumerable of features to export</param>
        /// <returns>Export file path</returns>
        public async Task<string> ExportFeatures(IEnumerable<Feature> items)
        {
            string fileName = Constants.GetExportFile("Groundsman Feature Collection", ExportType.GeoJSON);
            await SaveFeaturesToFile(items, fileName);
            return fileName;
        }

        /// <summary>
        /// Exports a feature to a sharefile request
        /// </summary>
        /// <param name="item">Feature to export</param>
        /// <returns>Export file path</returns>
        public async Task<string> ExportFeature(Feature item)
        {
            string fileName = Constants.GetExportFile((string)item.Properties[Constants.NameProperty], ExportType.GeoJSON);
            await SaveFeatureToFile(item, fileName);
            return fileName;
        }

        /// <summary>
        /// Saves a feature to file
        /// </summary>
        /// <param name="item">Feature to save</param>
        /// <param name="fileName">File name to save to</param>
        private async Task SaveFeatureToFile(Feature item, string fileName)
        {
            using StreamWriter writer = new StreamWriter(File.Create(fileName));
            await writer.WriteAsync(JsonConvert.SerializeObject(item));
        }

        /// <summary>
        /// Saves a feature list to file
        /// </summary>
        /// <param name="item">IEnumearble of seatures to save</param>
        /// <param name="FileName">File name to save to</param>
        private async Task SaveFeaturesToFile(IEnumerable<Feature> items, string FileName)
        {
            FeatureCollection geoJSONObject = new FeatureCollection(items);
            using StreamWriter writer = new StreamWriter(File.Create(FileName));
            await writer.WriteAsync(JsonConvert.SerializeObject(geoJSONObject));
        }
    }
}
