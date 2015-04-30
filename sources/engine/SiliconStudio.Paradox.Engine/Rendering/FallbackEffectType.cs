namespace SiliconStudio.Paradox.Rendering
{
    public enum FallbackEffectType
    {
        /// <summary>
        /// The effect is being asynchrounously compiled.
        /// </summary>
        Compiling = 1,
        
        /// <summary>
        /// There was an error while compiling the effect.
        /// </summary>
        Error = 2,
    }
}