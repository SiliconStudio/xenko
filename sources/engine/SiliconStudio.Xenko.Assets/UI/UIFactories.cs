// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
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
                Hierarchy = { RootParts = { grid }, Parts = { new UIElementDesign(grid) } }
            };
        }

        public override UIPageAsset New() => Create();
    }
}
