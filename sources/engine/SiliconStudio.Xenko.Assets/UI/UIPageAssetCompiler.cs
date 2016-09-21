// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.BuildEngine;
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Assets.UI
{
    public sealed class UIPageAssetCompiler : UIAssetCompilerBase<UIPageAsset>
    {
        protected override UIConvertCommand Create(string url, UIPageAsset parameters)
        {
            return new UIPageCommand(url, parameters);
        }
        
        private sealed class UIPageCommand : UIConvertCommand
        {
            public UIPageCommand(string url, UIPageAsset parameters)
                : base(url, parameters)
            {
            }

            protected override ComponentBase Create(ICommandContext commandContext)
            {
                return new Engine.UIPage
                {
                    RootElement = AssetParameters.Hierarchy.RootPartIds.Count == 1
                        ? AssetParameters.Hierarchy.Parts[AssetParameters.Hierarchy.RootPartIds[0]].UIElement
                        : null,
                };
            }
        }
    }
}
