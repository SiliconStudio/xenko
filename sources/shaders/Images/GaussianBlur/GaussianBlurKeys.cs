// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Linq;

namespace SiliconStudio.Paradox.Effects.Images
{
    /// <summary>
    /// Keys used by <see cref="GaussianBlur"/> and GaussianBlurEffect pdxfx
    /// </summary>
    internal static class GaussianBlurKeys
    {
        public static readonly ParameterKey<int> Radius;

        public static readonly ParameterKey<bool> VerticalBlur;

        public static readonly ParameterKey<float> SigmaRatio;

        static GaussianBlurKeys()
        {
            // Predefine supported radius from [1,20]
            var defaultRadius = Enumerable.Range(1, 20).Select(value => (object)value).ToArray();
            Radius = ParameterKeys.NewWithMetas<int>(new ParameterKeyPermutationsMetadata(defaultRadius));

            // Predefine vertical and horizontal blur
            VerticalBlur = ParameterKeys.NewWithMetas<bool>(new ParameterKeyPermutationsMetadata(true, false));

            // Predefine SigmaRatio to 2.0f and 3.0f
            SigmaRatio = ParameterKeys.NewWithMetas<float>(new ParameterKeyPermutationsMetadata(2.0f, 3.0f));
        }
    }
}