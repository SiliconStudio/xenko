using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Analysis;
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
    public abstract class UIAssetBase : AssetCompositeHierarchy<UIElementDesign, UIElement>
    {
        [DataMember(10)]
        [NotNull]
        [Display("Design")]
        public UIDesign Design { get; set; } = new UIDesign();

        /// <summary>
        /// Clones a sub-hierarchy of this asset.
        /// </summary>
        /// <param name="sourceRootId">The id of the root of the sub-hierarchy to clone</param>
        /// <param name="cleanReference">If true, any reference to a part external to the cloned hierarchy will be set to null.</param>
        /// <returns>A <see cref="AssetCompositeHierarchyData{UIElementDesign, UIElement}"/> corresponding to the cloned parts.</returns>
        public AssetCompositeHierarchyData<UIElementDesign, UIElement> CloneSubHierarchy(Guid sourceRootId, bool cleanReference)
        {
            Dictionary<Guid, Guid> idRemapping;
            return CloneSubHierarchy(sourceRootId, cleanReference, out idRemapping);
        }

        /// <summary>
        /// Clones a sub-hierarchy of this asset.
        /// </summary>
        /// <param name="sourceRootId">The id of the root of the sub-hierarchy to clone</param>
        /// <param name="cleanReference">If true, any reference to an part external to the cloned hierarchy will be set to null.</param>
        /// <param name="idRemapping">A dictionary containing the mapping of ids from the source parts to the new parts.</param>
        /// <returns>A <see cref="AssetCompositeHierarchyData{UIElementDesign, UIElement}"/> corresponding to the cloned parts.</returns>
        public AssetCompositeHierarchyData<UIElementDesign, UIElement> CloneSubHierarchy(Guid sourceRootId, bool cleanReference, out Dictionary<Guid, Guid> idRemapping)
        {
            if (!Hierarchy.Parts.ContainsKey(sourceRootId))
                throw new ArgumentException(@"The source root part must be an part of this asset.", nameof(sourceRootId));

            // Note: Instead of copying the whole asset (with its potentially big hierarchy),
            // we first copy the asset only (without the hierarchy), then the sub-hierarchy to extract.
            var subTreeRoot = Hierarchy.Parts[sourceRootId];
            var subTreeHierarchy = new AssetCompositeHierarchyData<UIElementDesign, UIElement> { Parts = { subTreeRoot }, RootPartIds = { sourceRootId } };
            foreach (var subTreeDesign in EnumerateChildParts(subTreeRoot, true))
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

        /// <inheritdoc/>
        public override UIElement GetParent(UIElement part)
        {
            return part.Parent;
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
            public float Depth { get; set; } = UIComponent.DefaultDepth;

            [DataMember]
            public float Height { get; set; } = UIComponent.DefaultHeight;

            [DataMember]
            public float Width { get; set; } = UIComponent.DefaultWidth;

            [DataMember]
            public Color AreaBackgroundColor { get; set; } = Color.WhiteSmoke * 0.5f;

            [DataMember]
            public Color AreaBorderColor { get; set; } = Color.WhiteSmoke;

            [DataMember]
            public float AreaBorderThickness { get; set; } = 2.0f;

            [DataMember]
            public Color AdornerBackgroundColor { get; set; } = Color.LimeGreen * 0.2f;

            [DataMember]
            public Color AdornerBorderColor { get; set; } = Color.LimeGreen;

            [DataMember]
            public float AdornerBorderThickness { get; set; } = 2.0f;
        }
    }
}
