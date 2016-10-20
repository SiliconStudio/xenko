namespace SiliconStudio.Xenko.Assets.Scripts
{
    /// <summary>
    /// Describes accessibility of a <see cref="VisualScriptAsset"/>, <see cref="Function"/> or <see cref="Variable"/>.
    /// </summary>
    public enum Accessibility
    {
        Public = 0,
        Private = 1,
        Protected = 2,
        Internal = 3,
        ProtectedOrInternal = 4,
    }
}