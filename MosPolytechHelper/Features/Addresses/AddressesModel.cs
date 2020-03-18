namespace MosPolyHelper.Features.Addresses
{
    using MosPolyHelper.Domains.AddressesDomain;
    using MosPolyHelper.Utilities;
    using MosPolyHelper.Utilities.Interfaces;
    using System;
    using System.IO;
    using System.Net;
    using System.Threading.Tasks;

    class AddressesModel
    {
        const string BuildingsFile = "cached_addresses";
        const string BuildingsUrl = "https://raw.githubusercontent.com/tipapro/MosPolyHelper-UpdatedData/master/addresses.json";

        IDeserializer deserializer;
        ISerializer serializer;

        Task<Addresses> ReadAddressesAsync()
        {
            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), BuildingsFile);
            if (!File.Exists(filePath))
            {
                return Task.FromResult<Addresses>(null);
            }
            else
            {
                var serBuildings = File.OpenRead(filePath);
                return deserializer.DeserializeAsync<Addresses>(serBuildings);
            }
        }

        async Task<Addresses> DownloadAddressesAsync()
        {
            try
            {
                var client = new WebClient();
                var serBuildings = await client.DownloadStringTaskAsync(BuildingsUrl);
                return await deserializer.DeserializeAsync<Addresses>(serBuildings);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        Task SaveAddressesAsync(Addresses buildings)
        {
            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), BuildingsFile);
            File.Delete(filePath);
            return serializer.SerializeAndSaveAsync(filePath, buildings);
        }

        Task<Addresses> GetAddressesFromAssets()
        {
            return deserializer.DeserializeAsync<Addresses>(AssetProvider.GetAsset("addresses.json"));
        }

        public AddressesModel()
        {
            this.serializer = DependencyInjector.GetJsonISerializer();
            this.deserializer = DependencyInjector.GetJsonIDeserializer();
        }

        public async Task<Addresses> GetAddressesAsync(bool downloadNew)
        {
            Addresses addresses = null;
            if (!downloadNew)
            {
                try
                {
                    addresses = await ReadAddressesAsync();
                }
                catch (Exception ex)
                {

                }
            }
            if (addresses == null)
            {
                addresses = await DownloadAddressesAsync();
                if (addresses == null)
                {
                    if (downloadNew)
                    {
                        try
                        {
                            addresses = await ReadAddressesAsync();
                            if (addresses != null)
                            {
                                return addresses;
                            }
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                    addresses = await GetAddressesFromAssets();
                }
                else
                {
                    await SaveAddressesAsync(addresses);
                }
            }
            return addresses;
        }
    }
}