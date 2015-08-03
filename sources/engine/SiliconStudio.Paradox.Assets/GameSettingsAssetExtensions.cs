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
            if (gameSettingsAsset == null)
                return null;

            return gameSettingsAsset.Asset as GameSettingsAsset;
        }
    }
}