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
        public static readonly ParameterKey<int> Count = ParameterKeys.New<int>();

        public static readonly ParameterKey<bool> VerticalBlur = ParameterKeys.New<bool>();
    }
}