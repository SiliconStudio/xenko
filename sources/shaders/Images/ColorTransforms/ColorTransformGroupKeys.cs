using System.Collections.Generic;

namespace SiliconStudio.Paradox.Effects.Images
{
    /// <summary>
    /// Keys used by <see cref="ToneMap"/> and ToneMapEffect pdxfx
    /// </summary>
    internal static class ColorTransformGroupKeys
    {
        public static readonly ParameterKey<List<ColorTransform>> Transforms = ParameterKeys.New<List<ColorTransform>>();
    }
}