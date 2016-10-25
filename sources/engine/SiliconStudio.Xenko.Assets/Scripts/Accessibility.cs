namespace SiliconStudio.Xenko.Assets.Scripts
{
    /// <summary>
    /// Describes accessibility of a <see cref="VisualScriptAsset"/>, <see cref="Method"/> or <see cref="Symbol"/>.
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