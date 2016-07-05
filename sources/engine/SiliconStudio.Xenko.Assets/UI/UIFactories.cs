using SiliconStudio.Assets;
using SiliconStudio.Xenko.Assets.Entities;
using SiliconStudio.Xenko.UI;
using SiliconStudio.Xenko.UI.Panels;

namespace SiliconStudio.Xenko.Assets.UI
{
    internal abstract class UIPageFactory : AssetFactory<UIPageAsset>
    {
        protected static UIPageAsset Create(UIElement rootElement)
        {
            return new UIPageAsset
            {
                Hierarchy = { RootPartIds = { rootElement.Id }, Parts = { new UIElementDesign(rootElement) } }
            };
        }
    }
    internal class UICanvasFactory : UIPageFactory
    {
        public static UIPageAsset Create() => Create(new Canvas());

        public override UIPageAsset New() => Create();
    }

    internal class UIGridFactory : UIPageFactory
    {
        public static UIPageAsset Create()
        {
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new StripDefinition());
            grid.LayerDefinitions.Add(new StripDefinition());
            grid.RowDefinitions.Add(new StripDefinition());

            return Create(grid);
        }

        public override UIPageAsset New() => Create();
    }

    internal class UIStackPanelFactory : UIPageFactory
    {
        public static UIPageAsset Create() => Create(new StackPanel());

        public override UIPageAsset New() => Create();
    }

    internal class UIUniformGridFactory : UIPageFactory
    {
        public static UIPageAsset Create() => Create(new UniformGrid());

        public override UIPageAsset New() => Create();
    }
}
