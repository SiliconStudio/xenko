// Copyright (c) 2014-2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Physics
{
    public partial class PhysicsDebugEffect : Effect
    {
        private static EffectBytecode bytecode;

        private readonly NextGenParameterCollection parameters;

        public PhysicsDebugEffect(GraphicsDevice graphicsDevice)
            : base(graphicsDevice, bytecode ?? (bytecode = EffectBytecode.FromBytesSafe(binaryBytecode)))
        {
            parameters = new NextGenParameterCollection();
            Color = new Color4(1.0f);
            WorldViewProj = Matrix.Identity;
            UseUv = true;
        }

        public Color4 Color
        {
            get
            {
                return parameters.GetValueSlow(PhysicsDebugEffectKeys.Color);
            }
            set
            {
                parameters.SetValueSlow(PhysicsDebugEffectKeys.Color, value);
            }
        }

        public Matrix WorldViewProj
        {
            get
            {
                return parameters.GetValueSlow(PhysicsDebugEffectKeys.WorldViewProj);
            }

            set
            {
                parameters.SetValueSlow(PhysicsDebugEffectKeys.WorldViewProj, value);
            }
        }

        public bool UseUv
        {
            get
            {
                return parameters.GetValueSlow(PhysicsDebugEffectKeys.UseUv) > 0.5;
            }
            set
            {
                parameters.SetValueSlow(PhysicsDebugEffectKeys.UseUv, value ? 1.0f : 0.0f);
            }
        }

        public void Apply()
        {
            // TODO GRAPHICS REFACTOR
            //Apply(parameters);
            throw new NotImplementedException();
        }
    }
}