// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Rendering
{
    public partial class ParticleBaseKeys
    {
        static ParticleBaseKeys()
        {
        //    MatrixTransform = ParameterKeys.New(Matrix.Identity);
            ViewMatrix = ParameterKeys.New(Matrix.Identity);
            ProjectionMatrix = ParameterKeys.New(Matrix.Identity);
        }

        public static readonly ParameterKey<bool> ColorIsSRgb = ParameterKeys.New(false);
    }
}