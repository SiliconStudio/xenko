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

        public static GameSettingsAsset GetGameSettingsAssetOrDefault(this AssetItem assetItem)
        {
            var gameSettings = assetItem.Package.GetGameSettingsAsset();
            if (gameSettings == null)
            {
                var session = assetItem.Package.Session;
                var currentPackage = session.CurrentPackage;
                if (currentPackage != null)
                {
                    gameSettings = currentPackage.GetGameSettingsAsset();
                }
            }
            return gameSettings ?? new GameSettingsAsset();
        }
    }
}