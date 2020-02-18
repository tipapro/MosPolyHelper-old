namespace MosPolyHelper.Features.Buildings
{
    using MosPolyHelper.Domains.BuildingsDomain;
    using MosPolyHelper.Utilities;
    using MosPolyHelper.Utilities.Interfaces;
    using System;
    using System.IO;
    using System.Net;
    using System.Threading.Tasks;

    class BuildingsModel
    {
        const string BuildingsFile = "cached_buildings";
        const string BuildingsUrl = "https://raw.githubusercontent.com/tipapro/MosPolyHelper-UpdatedData/master/buildings.json";

        IDeserializer deserializer;
        ISerializer serializer;

        Task<Buildings> ReadBuildingsAsync()
        {
            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), BuildingsFile);
            if (!File.Exists(filePath))
            {
                return Task.FromResult<Buildings>(null);
            }
            else
            {
                var serBuildings = File.OpenRead(filePath);
                return deserializer.DeserializeAsync<Buildings>(serBuildings);
            }
        }

        async Task<Buildings> DownloadBuildingsAsync()
        {
            try
            {
                var client = new WebClient();
                var serBuildings = await client.DownloadStringTaskAsync(BuildingsUrl);
                return await deserializer.DeserializeAsync<Buildings>(serBuildings);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        Task SaveBuildingsAsync(Buildings buildings)
        {
            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), BuildingsFile);
            File.Delete(filePath);
            return serializer.SerializeAndSaveAsync(filePath, buildings);
        }

        Task<Buildings> GetBuildingsFromAssets()
        {
            return deserializer.DeserializeAsync<Buildings>(AssetProvider.GetAsset("buildings.json"));
        }

        public BuildingsModel()
        {
            this.serializer = DependencyInjector.GetJsonISerializer();
            this.deserializer = DependencyInjector.GetJsonIDeserializer();
        }

        public async Task<Buildings> GetBuildingsAsync(bool downloadNew)
        {
            Buildings buildings = null;
            if (!downloadNew)
            {
                try
                {
                    buildings = await ReadBuildingsAsync();
                }
                catch (Exception ex)
                {

                }
            }
            if (buildings == null)
            {
                buildings = await DownloadBuildingsAsync();
                if (buildings == null)
                {
                    if (downloadNew)
                    {
                        try
                        {
                            buildings = await ReadBuildingsAsync();
                            if (buildings != null)
                            {
                                return buildings;
                            }
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                    buildings = await GetBuildingsFromAssets();
                }
                else
                {
                    await SaveBuildingsAsync(buildings);
                }
            }
            return buildings;
        }
    }
}