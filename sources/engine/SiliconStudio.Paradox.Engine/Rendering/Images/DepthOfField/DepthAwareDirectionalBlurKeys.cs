// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Linq;

namespace SiliconStudio.Paradox.Rendering.Images
{
    /// <summary>
    /// Keys used by the DepthAwareDirectionalBlurEffect
    /// </summary>
    public static class DepthAwareDirectionalBlurKeys
    {
        public static readonly ParameterKey<int> Count = ParameterKeys.New<int>();

        public static readonly ParameterKey<int> TotalTap = ParameterKeys.New<int>();
    }
}