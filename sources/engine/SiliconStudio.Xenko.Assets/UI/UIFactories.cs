using SiliconStudio.Assets;
using SiliconStudio.Xenko.UI.Panels;

namespace SiliconStudio.Xenko.Assets.UI
{
    internal class UICanvasFactory : AssetFactory<UIPageAsset>
    {
        public static UIPageAsset Create()
        {
            return new UIPageAsset
            {
                RootElement = new Canvas(),
            };
        }

        public override UIPageAsset New()
        {
            return Create();
        }
    }

    internal class UIGridFactory : AssetFactory<UIPageAsset>
    {
        public static UIPageAsset Create()
        {
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new Xenko.UI.StripDefinition());
            grid.LayerDefinitions.Add(new Xenko.UI.StripDefinition());
            grid.RowDefinitions.Add(new Xenko.UI.StripDefinition());

            return new UIPageAsset
            {
                RootElement = grid,
            };
        }

        public override UIPageAsset New()
        {
            return Create();
        }
    }

    internal class UIStackPanelFactory : AssetFactory<UIPageAsset>
    {
        public static UIPageAsset Create()
        {
            return new UIPageAsset
            {
                RootElement = new StackPanel(),
            };
        }

        public override UIPageAsset New()
        {
            return Create();
        }
    }

    internal class UIUniformGridFactory : AssetFactory<UIPageAsset>
    {
        public static UIPageAsset Create()
        {
            return new UIPageAsset
            {
                RootElement = new UniformGrid(),
            };
        }

        public override UIPageAsset New()
        {
            return Create();
        }
    }
}
