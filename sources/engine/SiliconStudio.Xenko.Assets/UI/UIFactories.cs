using SiliconStudio.Assets;
using SiliconStudio.Xenko.Assets.Entities;
using SiliconStudio.Xenko.UI.Panels;

namespace SiliconStudio.Xenko.Assets.UI
{
    internal class UIPageFactory : AssetFactory<UIPageAsset>
    {
        public static UIPageAsset Create()
        {
            var grid = new Grid();

            return new UIPageAsset
            {
                Hierarchy = { RootPartIds = { grid.Id }, Parts = { new UIElementDesign(grid) } }
            };
        }

        public override UIPageAsset New() => Create();
    }
}
