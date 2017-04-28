// Copyright (c) 2016-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.Collections.Generic;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Analysis;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.BuildEngine;
using SiliconStudio.Core;
using SiliconStudio.Xenko.Assets.SpriteFont;

namespace SiliconStudio.Xenko.Assets.UI
{
    [AssetCompiler(typeof(UIPageAsset), typeof(AssetCompilationContext))]
    public sealed class UIPageAssetCompiler : UIAssetCompilerBase<UIPageAsset>
    {
        public override IEnumerable<KeyValuePair<Type, BuildDependencyType>> GetInputTypes(AssetCompilerContext context, AssetItem assetItem)
        {
            yield return new KeyValuePair<Type, BuildDependencyType>(typeof(SpriteFontAsset), BuildDependencyType.Runtime | BuildDependencyType.CompileContent);
            yield return new KeyValuePair<Type, BuildDependencyType>(typeof(PrecompiledSpriteFontAsset), BuildDependencyType.Runtime | BuildDependencyType.CompileContent);
        }

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
                    RootElement = Parameters.Hierarchy.RootParts.Count == 1 ? Parameters.Hierarchy.RootParts[0] : null
                };
            }
        }
    }
}
