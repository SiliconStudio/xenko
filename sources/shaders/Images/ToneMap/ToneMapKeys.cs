// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.Paradox.Effects.Images
{
    /// <summary>
    /// Keys used by <see cref="ToneMap"/> and ToneMapEffect pdxfx
    /// </summary>
    internal static class ToneMapKeys
    {
        public static readonly ParameterKey<string> Operator = ParameterKeys.New<string>();
        public static readonly ParameterKey<LuminanceResult> LuminanceResult = ParameterKeys.New<LuminanceResult>();
    }
}