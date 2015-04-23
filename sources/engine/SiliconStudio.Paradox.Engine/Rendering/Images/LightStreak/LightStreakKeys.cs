// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Linq;

namespace SiliconStudio.Paradox.Rendering.Images
{
    /// <summary>
    /// Keys used by <see cref="LightStreak"/> and LightStreakEffect pdxfx.
    /// </summary>
    internal static class LightStreakKeys
    {
        public static readonly ParameterKey<int> Count = ParameterKeys.New<int>();

        public static readonly ParameterKey<int> AnamorphicCount = ParameterKeys.New<int>();
    }
}
