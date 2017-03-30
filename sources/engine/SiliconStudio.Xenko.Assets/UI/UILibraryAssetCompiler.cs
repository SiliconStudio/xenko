// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Analysis;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.BuildEngine;
using SiliconStudio.Core;
using SiliconStudio.Xenko.Assets.Sprite;
using SiliconStudio.Xenko.Assets.SpriteFont;

namespace SiliconStudio.Xenko.Assets.UI
{
    [AssetCompiler(typeof(UILibraryAsset), typeof(AssetCompilationContext))]
    public sealed class UILibraryAssetCompiler : UIAssetCompilerBase<UILibraryAsset>
    {
        public override IEnumerable<KeyValuePair<Type, BuildDependencyType>> GetInputTypes(AssetCompilerContext context, AssetItem assetItem)
        {
            yield return new KeyValuePair<Type, BuildDependencyType>(typeof(SpriteFontAsset), BuildDependencyType.Runtime | BuildDependencyType.CompileContent);
            yield return new KeyValuePair<Type, BuildDependencyType>(typeof(SpriteSheetAsset), BuildDependencyType.Runtime | BuildDependencyType.CompileContent);
        }

        protected override UIConvertCommand Create(string url, UILibraryAsset parameters, Package package)
        {
            return new UILibraryCommand(url, parameters, package);
        }

        private sealed class UILibraryCommand : UIConvertCommand
        {
            public UILibraryCommand(string url, UILibraryAsset parameters, Package package)
                : base(url, parameters, package)
            {
            }

            protected override ComponentBase Create(ICommandContext commandContext)
            {
                var uiLibrary = new Engine.UILibrary();
                foreach (var kv in Parameters.PublicUIElements)
                {
                    if (!Parameters.Hierarchy.RootPartIds.Contains(kv.Key))
                    {
                        // We might want to allow that in the future.
                        commandContext.Logger.Warning($"Only root elements can be exposed publicly. Skipping [{kv.Key}].");
                        continue;
                    }

                    // Copy Key/Value pair
                    UIElementDesign element;
                    if (Parameters.Hierarchy.Parts.TryGetValue(kv.Key, out element))
                    {
                        uiLibrary.UIElements.Add(kv.Value, element.UIElement);
                    }
                    else
                    {
                        commandContext.Logger.Error($"Cannot find the element with the id [{kv.Value}] to expose [{kv.Key}].");
                    }
                }
                return uiLibrary;
            }
        }
    }
}
