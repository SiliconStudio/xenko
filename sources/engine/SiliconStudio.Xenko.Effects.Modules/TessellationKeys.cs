namespace Paradox.Effects.Modules
{
    public class TessellationKeys
    {
        /// <summary>
        /// Desired maximum triangle size in screen space during tessellation.
        /// </summary>
        public static readonly ParameterValueKey<float> DesiredTriangleSize = ParameterKeys.Value(12.0f);
    }
}