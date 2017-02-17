using SiliconStudio.Core;

namespace SiliconStudio.Assets.Quantum
{
    /// <summary>
    /// A base visitor class that allows to visit a single part of an <see cref="AssetComposite"/> at a time.
    /// </summary>
    /// <typeparam name="TAssetPartDesign">The type of the design-time object containing the part.</typeparam>
    /// <typeparam name="TAssetPart">The type of the part.</typeparam>
    public class AssetCompositeHierarchyPartVisitor<TAssetPartDesign, TAssetPart> : AssetGraphVisitorBase
        where TAssetPartDesign : class, IAssetPartDesign<TAssetPart>
        where TAssetPart : class, IIdentifiable
    {
        public AssetCompositeHierarchyPartVisitor(AssetCompositeHierarchyPropertyGraph<TAssetPartDesign, TAssetPart> propertyGraph) : base(propertyGraph)
        {
        }
    }
}