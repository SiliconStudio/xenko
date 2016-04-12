using System.IO;
using NuGet;
using SiliconStudio.Assets;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Shaders.Ast;

namespace SiliconStudio.Xenko.Assets
{
    public static class GameSettingsAssetExtensions
    {
        /// <summary>
        /// Gets the game settings asset from a package.
        /// </summary>
        /// <param name="package">The package.</param>
        /// <returns></returns>
        public static GameSettingsAsset GetGameSettingsAsset(this Package package)
        {
            var gameSettingsAsset = package.Assets.Find(GameSettingsAsset.GameSettingsLocation);
            if (gameSettingsAsset == null && package.TemporaryAssets.Count > 0)
            {
                gameSettingsAsset = package.TemporaryAssets.Find(x => x.Location == GameSettingsAsset.GameSettingsLocation);
            }
            return gameSettingsAsset?.Asset as GameSettingsAsset;
        }

        public static GameSettingsAsset GetGameSettingsAssetOrDefault(this AssetItem assetItem)
        {
            return assetItem.Package.GetGameSettingsAssetOrDefault();
        }

        public static GameSettingsAsset GetGameSettingsAssetOrDefault(this Package package)
        {
            var gameSettings = package.GetGameSettingsAsset();
            if (gameSettings == null)
            {
                var session = package.Session;
                var currentPackage = session.CurrentPackage;
                if (currentPackage != null)
                {
                    gameSettings = currentPackage.GetGameSettingsAsset();
                }
            }
            return gameSettings ?? GameSettingsFactory.Create();
        }

        public static GameSettingsAsset CloneGameSettingsAsset(this Package package)
        {
            lock (package)
            {
                var gameSettings = package.GetGameSettingsAssetOrDefault();
                return (GameSettingsAsset)AssetCloner.Clone(gameSettings);
            }
        }
    }
}
