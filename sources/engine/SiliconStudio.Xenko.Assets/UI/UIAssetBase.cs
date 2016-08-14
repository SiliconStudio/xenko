// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Analysis;
using SiliconStudio.Assets.Serializers;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.UI;

namespace SiliconStudio.Xenko.Assets.UI
{
    /// <summary>
    /// Base class for assets containing a hierarchy of <see cref="UIElement"/>.
    /// </summary>
    [AssetPartReference(typeof(UIElement))]
    public abstract class UIAssetBase : AssetCompositeHierarchy<UIElementDesign, UIElement>
    {
        [DataMember(10)]
        [NotNull]
        public UIDesign Design { get; set; } = new UIDesign();

        /// <summary>
        /// Clones a sub-hierarchy of this asset.
        /// </summary>
        /// <param name="sourceRootId">The id of the root of the sub-hierarchy to clone</param>
        /// <param name="cleanReference">If true, any reference to a part external to the cloned hierarchy will be set to null.</param>
        /// <param name="idRemapping">A dictionary containing the mapping of ids from the source parts to the new parts.</param>
        /// <returns>A <see cref="AssetCompositeHierarchyData{UIElementDesign, UIElement}"/> corresponding to the cloned parts.</returns>
        public override AssetCompositeHierarchyData<UIElementDesign, UIElement> CloneSubHierarchy(Guid sourceRootId, bool cleanReference, out Dictionary<Guid, Guid> idRemapping)
        {
            if (!Hierarchy.Parts.ContainsKey(sourceRootId))
                throw new ArgumentException(@"The source root part must be an part of this asset.", nameof(sourceRootId));

            // Note: Instead of copying the whole asset (with its potentially big hierarchy),
            // we first copy the asset only (without the hierarchy), then the sub-hierarchy to extract.
            var subTreeRoot = Hierarchy.Parts[sourceRootId];
            var subTreeHierarchy = new AssetCompositeHierarchyData<UIElementDesign, UIElement> { Parts = { subTreeRoot }, RootPartIds = { sourceRootId } };
            foreach (var subTreeDesign in EnumerateChildParts(subTreeRoot, Hierarchy, true))
                subTreeHierarchy.Parts.Add(Hierarchy.Parts[subTreeDesign.UIElement.Id]);

            // clone the parts of the sub-tree
            var clonedHierarchy = (AssetCompositeHierarchyData<UIElementDesign, UIElement>)AssetCloner.Clone(subTreeHierarchy);
            //clonedHierarchy.Parts[sourceRootEntity].UIElement.Parent = null;

            //if (cleanReference)
            //{
            //    // set to null reference outside of the sub-tree
            //    var tempAsset = new PrefabAsset { Hierarchy = clonedHierarchy };
            //    tempAsset.FixupPartReferences();
            //}

            // temporary nullify the hierarchy to avoid to clone it
            var sourceHierarchy = Hierarchy;
            Hierarchy = null;

            // revert the source hierarchy
            Hierarchy = sourceHierarchy;

            // Generate part mapping
            idRemapping = new Dictionary<Guid, Guid>();
            foreach (var partDesign in clonedHierarchy.Parts)
            {
                // Generate new Id
                var newPartId = Guid.NewGuid();

                // Update mappings
                idRemapping.Add(partDesign.UIElement.Id, newPartId);

                // Update part with new id
                partDesign.UIElement.Id = newPartId;
            }

            // Rewrite part references
            // Should we nullify invalid references?
            AssetPartsAnalysis.RemapPartsId(clonedHierarchy, idRemapping);

            return clonedHierarchy;
        }

        /// <summary>
        /// Clones a sub-hierarchy of this asset.
        /// </summary>
        /// <param name="sourceRootIds">The ids that are the roots of the sub-hierarchies to clone</param>
        /// <param name="cleanReference">If true, any reference to a part external to the cloned hierarchy will be set to null.</param>
        /// <param name="idRemapping">A dictionary containing the mapping of ids from the source parts to the new parts.</param>
        /// <returns>A <see cref="AssetCompositeHierarchyData{UIElementDesign, UIElement}"/> corresponding to the cloned parts.</returns>
        /// <remarks>The root ids passed to this methods must be independent in the hierarchy.</remarks>
        public AssetCompositeHierarchyData<UIElementDesign, UIElement> CloneSubHierarchies(IEnumerable<Guid> sourceRootIds, bool cleanReference, out Dictionary<Guid, Guid> idRemapping)
        {
            // Note: Instead of copying the whole asset (with its potentially big hierarchy),
            // we first copy the asset only (without the hierarchy), then the sub-hierarchy to extract.
            var subTreeHierarchy = new AssetCompositeHierarchyData<UIElementDesign, UIElement>();
            foreach (var sourceRootEntity in sourceRootIds)
            {
                if (!Hierarchy.Parts.ContainsKey(sourceRootEntity))
                    throw new ArgumentException(@"The source root parts must be parts of this asset.", nameof(sourceRootIds));

                var subTreeRoot = Hierarchy.Parts[sourceRootEntity].UIElement;
                subTreeHierarchy.Parts.Add(new UIElementDesign(subTreeRoot));
                subTreeHierarchy.RootPartIds.Add(sourceRootEntity);
                foreach (var subTreeEntity in EnumerateChildParts(subTreeRoot, true))
                    subTreeHierarchy.Parts.Add(Hierarchy.Parts[subTreeEntity.Id]);
            }

            // clone the entities of the sub-tree
            var clonedHierarchy = (AssetCompositeHierarchyData<UIElementDesign, UIElement>)AssetCloner.Clone(subTreeHierarchy);
            //foreach (var rootEntity in clonedHierarchy.RootPartIds)
            //{
            //    clonedHierarchy.Parts[rootEntity].UIElement.Parent = null;
            //}

            //if (cleanReference)
            //{
            //    // set to null reference outside of the sub-tree
            //    var tempAsset = new PrefabAsset { Hierarchy = clonedHierarchy };
            //    tempAsset.FixupPartReferences();
            //}

            // temporary nullify the hierarchy to avoid to clone it
            var sourceHierarchy = Hierarchy;
            Hierarchy = null;

            // revert the source hierarchy
            Hierarchy = sourceHierarchy;

            // Generate entity mapping
            idRemapping = new Dictionary<Guid, Guid>();
            foreach (var entityDesign in clonedHierarchy.Parts)
            {
                // Generate new Id
                var newEntityId = Guid.NewGuid();

                // Update mappings
                idRemapping.Add(entityDesign.UIElement.Id, newEntityId);

                // Update entity with new id
                entityDesign.UIElement.Id = newEntityId;
            }

            // Rewrite entity references
            // Should we nullify invalid references?
            AssetPartsAnalysis.RemapPartsId(clonedHierarchy, idRemapping);

            return clonedHierarchy;
        }

        /// <inheritdoc/>
        public override UIElement GetParent(UIElement part)
        {
            if (part == null) throw new ArgumentNullException(nameof(part));
            return part.Parent;
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

        [DataContract("UIDesign")]
        [NonIdentifiable]
        public sealed class UIDesign
        {
            [DataMember]
            [Display(category: "Design")]
            public Vector3 Resolution { get; set; } = new Vector3(UIComponent.DefaultWidth, UIComponent.DefaultHeight, UIComponent.DefaultDepth);
        }
    }
}
