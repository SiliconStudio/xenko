using SiliconStudio.Assets;
using SiliconStudio.Xenko.Assets.Textures;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Physics;

namespace SiliconStudio.Xenko.Assets
{
    public class GameSettingsFactory : AssetFactory<GameSettingsAsset>
    {
        public static GameSettingsAsset Create()
        {
            var asset = new GameSettingsAsset();
            //add default filters, todo maybe a config file somewhere is better
            asset.PlatformFilters.Add("PowerVR SGX 54[0-9]");
            asset.PlatformFilters.Add("Adreno \\(TM\\) 2[0-9][0-9]");
            asset.PlatformFilters.Add("Adreno (TM) 320");
            asset.PlatformFilters.Add("Adreno (TM) 330");
            asset.PlatformFilters.Add("Adreno \\(TM\\) 4[0-9][0-9]");
            asset.PlatformFilters.Add("NVIDIA Tegra");
            asset.PlatformFilters.Add("Intel(R) HD Graphics");
            asset.PlatformFilters.Add("^Mali\\-4");
            asset.PlatformFilters.Add("^Mali\\-T6");
            asset.PlatformFilters.Add("^Mali\\-T7");

            asset.Get<RenderingSettings>();
            asset.Get<EditorSettings>();
            asset.Get<TextureSettings>();
            asset.Get<PhysicsSettings>();

            return asset;
        }

        public override GameSettingsAsset New()
        {
            return Create();
        }
    }
}
