// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Rendering
{
    public partial class SpriteBaseKeys
    {
        static SpriteBaseKeys()
        {
            MatrixTransform = ParameterKeys.NewValue(Matrix.Identity);
        }

        public static readonly PermutationParameterKey<bool> ColorIsSRgb = ParameterKeys.NewPermutation(false);
    }
}