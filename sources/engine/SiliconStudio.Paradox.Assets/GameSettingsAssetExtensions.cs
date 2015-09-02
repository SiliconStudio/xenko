using SiliconStudio.Assets;

namespace SiliconStudio.Paradox.Assets
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
            return gameSettingsAsset?.Asset as GameSettingsAsset;
        }

        public static GameSettingsAsset GetGameSettingsAsset(this AssetItem assetItem)
        {
            var currentPackage = assetItem.Package.Session.CurrentPackage;
            var assetGameSettings = currentPackage?.GetGameSettingsAsset();
            return assetGameSettings;
        }
    }
}