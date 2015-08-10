namespace SiliconStudio.Paradox.Assets.Textures.Packing
{
    /// <summary>
    /// The Heuristic methods used to place sprites in atlas.
    /// </summary>
    public enum TexturePackingMethod
    {
        Best,
        BestShortSideFit,
        BestLongSideFit,
        BestAreaFit,
        BottomLeftRule,
        ContactPointRule
    }
}