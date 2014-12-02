namespace SiliconStudio.Assets.Diff
{
    /// <summary>
    /// Provides a hook in <see cref="AssetDiff.Compute(bool)"/>.
    /// </summary>
    public interface IDiffResolver
    {
        void BeforeDiff(Asset baseAsset, Asset asset1, Asset asset2);
    }
}