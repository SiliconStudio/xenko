// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Assets;
using SiliconStudio.BuildEngine;
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Assets.UI
{
    public sealed class UIPageAssetCompiler : UIAssetCompilerBase<UIPageAsset>
    {
        protected override UIConvertCommand Create(string url, UIPageAsset parameters, Package package)
        {
            return new UIPageCommand(url, parameters, package);
        }
        
        private sealed class UIPageCommand : UIConvertCommand
        {
            public UIPageCommand(string url, UIPageAsset parameters, Package package)
                : base(url, parameters, package)
            {
            }

            protected override ComponentBase Create(ICommandContext commandContext)
            {
                return new Engine.UIPage
                {
                    RootElement = Parameters.Hierarchy.RootPartIds.Count == 1
                        ? Parameters.Hierarchy.Parts[Parameters.Hierarchy.RootPartIds[0]].UIElement
                        : null,
                };
            }
        }
    }
}
