// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Assets.Analysis;
using SiliconStudio.Core;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Core.Yaml.Serialization;

namespace SiliconStudio.Assets
{
    /// <summary>
    /// Base class for an asset that supports inheritance by composition.
    /// </summary>
    public abstract class AssetComposite : Asset, IAssetComposite
    {
        public abstract IEnumerable<AssetPart> CollectParts();

        public abstract IIdentifiable FindPart(Guid partId);

        public abstract bool ContainsPart(Guid id);

        public override void FixupPartReferences(bool clearMissingReferences = true)
        {
            base.FixupPartReferences(clearMissingReferences);

            var visitor = new AssetCompositePartReferenceCollector();
            visitor.VisitAsset(this);
            var references = visitor.Result;

            // Reverse the list, so that we can still properly update everything
            // (i.e. if we have a[0], a[1], a[1].Test, we have to do it from back to front to be valid at each step)
            // TODO: I don't think this is needed. Find a proper example or remove it.
            references.Reverse();

            foreach (var reference in references)
            {
                var realPart = ResolvePartReference(reference.AssetPart);
                if (realPart != reference.AssetPart)
                {
                    if (realPart != null || clearMissingReferences)
                    {
                        reference.Path.Apply(this, MemberPathAction.ValueSet, realPart);
                    }
                }
            }
        }

        /// <summary>
        /// Resolves the actual target of a reference to a part of this asset that is currently targeting the given <paramref name="referencedObject"/>.
        /// </summary>
        /// <param name="referencedObject">The object currently referenced by the part.</param>
        /// <returns></returns>
        /// <seealso cref="FixupPartReferences"/>
        /// <remarks>
        /// The <paramref name="referencedObject"/> can already be the actual target of the reference, but it can also be a proxy object,
        /// a temporary object, or an old version of the actual object. Implementations of this methods are supposed to identify this given object
        /// and retrieve the actual one from the asset itself to return it.
        /// </remarks>
        // TODO: turn protected or remove
        public abstract object ResolvePartReference(object referencedObject);

        protected class FixPartReferenceUpgrader : AssetUpgraderBase
        {
            protected override void UpgradeAsset(AssetMigrationContext context, PackageVersion currentVersion, PackageVersion targetVersion, dynamic asset, PackageLoadingAssetFile assetFile, OverrideUpgraderHint overrideHint)
            {
                var rootNode = (YamlNode)asset.Node;

                var allScalarNodes = rootNode.AllNodes.OfType<YamlScalarNode>().ToList();

                var nextIsId = false;
                var inPublicUIElements = false;
                foreach (var node in allScalarNodes)
                {
                    var indexFirstSlash = node.Value.IndexOf('/');
                    Guid targetGuid = Guid.Empty;
                    if (indexFirstSlash == -1)
                    {
                        Guid.TryParseExact(node.Value, "D", out targetGuid);
                    }
                    else
                    {
                        Guid entityGuid;
                        if (Guid.TryParseExact(node.Value.Substring(0, indexFirstSlash), "D", out entityGuid))
                        {
                            Guid.TryParseExact(node.Value.Substring(indexFirstSlash + 1), "D", out targetGuid);
                        }
                    }

                    if (targetGuid != Guid.Empty && !nextIsId && !inPublicUIElements)
                    {
                        node.Value = "ref!! " + targetGuid;
                    }
                    else
                    {
                        if (nextIsId && targetGuid == Guid.Empty)
                            nextIsId = false;

                        if (inPublicUIElements && node.Value == "Hierarchy")
                            inPublicUIElements = false;

                        if (node.Value.Contains("Id"))
                            nextIsId = true;

                        if (node.Value == "PublicUIElements")
                            inPublicUIElements = true;
                    }
                }
            }
        }
    }
}
