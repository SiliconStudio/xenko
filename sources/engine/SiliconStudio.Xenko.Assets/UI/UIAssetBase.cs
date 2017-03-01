// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Serializers;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Yaml;
using SiliconStudio.Core.Yaml.Serialization;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.UI;

namespace SiliconStudio.Xenko.Assets.UI
{
    /// <summary>
    /// Base class for assets containing a hierarchy of <see cref="UIElement"/>.
    /// </summary>
    public abstract class UIAssetBase : AssetCompositeHierarchy<UIElementDesign, UIElement>
    {
        [DataContract("UIDesign")]
        public sealed class UIDesign
        {
            [DataMember]
            [Display(category: "Design")]
            public Vector3 Resolution { get; set; } = new Vector3(UIComponent.DefaultWidth, UIComponent.DefaultHeight, UIComponent.DefaultDepth);
        }

        [DataMember(10)]
        [NotNull]
        public UIDesign Design { get; set; } = new UIDesign();

        /// <inheritdoc/>
        public override UIElement GetParent(UIElement part)
        {
            if (part == null) throw new ArgumentNullException(nameof(part));
            return part.VisualParent;
        }

        /// <inheritdoc/>
        public override int IndexOf(UIElement part)
        {
            if (part == null) throw new ArgumentNullException(nameof(part));
            var parent = GetParent(part);
            return parent?.VisualChildren.IndexOf(x => x == part) ?? Hierarchy.RootPartIds.IndexOf(part.Id);
        }

        /// <inheritdoc/>
        public override UIElement GetChild(UIElement part, int index)
        {
            if (part == null) throw new ArgumentNullException(nameof(part));
            return part.VisualChildren[index];
        }

        /// <inheritdoc/>
        public override int GetChildCount(UIElement part)
        {
            if (part == null) throw new ArgumentNullException(nameof(part));
            return part.VisualChildren.Count;
        }

        /// <inheritdoc/>
        public override IEnumerable<UIElement> EnumerateChildParts(UIElement part, bool isRecursive)
        {
            var elementChildren = (IUIElementChildren)part;
            var enumerator = isRecursive ? elementChildren.Children.DepthFirst(t => t.Children) : elementChildren.Children;
            return enumerator.NotNull().Cast<UIElement>();
        }

        protected class BasePartsRemovalComponentUpgrader : AssetUpgraderBase
        {
            protected override void UpgradeAsset(AssetMigrationContext context, PackageVersion currentVersion, PackageVersion targetVersion, dynamic asset, PackageLoadingAssetFile assetFile,
                OverrideUpgraderHint overrideHint)
            {
                var basePartMapping = new Dictionary<string, string>();
                if (asset["~BaseParts"] != null)
                {
                    foreach (dynamic basePart in asset["~BaseParts"])
                    {
                        try
                        {
                            var location = ((YamlScalarNode)basePart.Location.Node).Value;
                            var id = ((YamlScalarNode)basePart.Asset.Id.Node).Value;
                            var assetUrl = $"{id}:{location}";

                            foreach (dynamic part in basePart.Asset.Hierarchy.Parts)
                            {
                                try
                                {
                                    var partId = ((YamlScalarNode)part.UIElement.Id.Node).Value;
                                    basePartMapping[partId] = assetUrl;
                                }
                                catch (Exception e)
                                {
                                    e.Ignore();
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            e.Ignore();
                        }
                    }
                    asset["~BaseParts"] = DynamicYamlEmpty.Default;
                }
                var uiElements = (DynamicYamlArray)asset.Hierarchy.Parts;
                foreach (dynamic uiDesign in uiElements)
                {
                    if (uiDesign.BaseId != null)
                    {
                        try
                        {
                            var baseId = ((YamlScalarNode)uiDesign.BaseId.Node).Value;
                            var baseInstanceId = ((YamlScalarNode)uiDesign.BasePartInstanceId.Node).Value;
                            string assetUrl;
                            if (basePartMapping.TryGetValue(baseId, out assetUrl))
                            {
                                var baseNode = (dynamic)(new DynamicYamlMapping(new YamlMappingNode()));
                                baseNode.BasePartAsset = assetUrl;
                                baseNode.BasePartId = baseId;
                                baseNode.InstanceId = baseInstanceId;
                                uiDesign.Base = baseNode;
                            }
                            uiDesign.BaseId = DynamicYamlEmpty.Default;
                            uiDesign.BasePartInstanceId = DynamicYamlEmpty.Default;
                        }
                        catch (Exception e)
                        {
                            e.Ignore();
                        }
                    }
                }
            }
        }
    }
}
