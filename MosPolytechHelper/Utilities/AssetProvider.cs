namespace MosPolyHelper.Utilities
{
    using Android.Content.Res;
    using MosPolyHelper.Utilities.Interfaces;
    using System;
    using System.IO;

    static class AssetProvider
    {
        static ILogger logger;

        public static void SetUpLogger(ILoggerFactory loggerFactory)
        {
            if (logger == null)
            {
                logger = loggerFactory.Create(typeof(AssetProvider).FullName);
            }
        }

        public static AssetManager AssetManager { get; set; }

        public static Stream GetAsset(string assetName)
        {
            if (AssetManager == null)
            {
                return null;
            }
            try
            {
                return AssetManager.Open(assetName);
            }
            catch (Exception ex)
            {
                logger?.Error(ex, "AssetProviderFail {assetName}", assetName);
                return null;
            }
        }
    }
}