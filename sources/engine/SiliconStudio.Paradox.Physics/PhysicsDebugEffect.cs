// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Paradox.Effects.Modules;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Shaders;

namespace SiliconStudio.Paradox.Physics
{
    public partial class PhysicsDebugEffect : Effect
    {
        private static EffectBytecode bytecode;

        public PhysicsDebugEffect(GraphicsDevice graphicsDevice)
            : base(graphicsDevice, bytecode ?? (bytecode = BinarySerialization.Read<EffectBytecode>(binaryBytecode)))
        {
            Color = new Color4(1.0f);
            WorldViewProj = Matrix.Identity;
            UseUv = true;
        }

        public Color4 Color
        {
            get
            {
                return Parameters.Get(PhysicsDebugEffectKeys.Color);
            }
            set
            {
                Parameters.Set(PhysicsDebugEffectKeys.Color, value);
            }
        }

        public Matrix WorldViewProj
        {
            get
            {
                return Parameters.Get(PhysicsDebugEffectKeys.WorldViewProj);
            }

            set
            {
                Parameters.Set(PhysicsDebugEffectKeys.WorldViewProj, value);
            }
        }

        public bool UseUv
        {
            get
            {
                return Parameters.Get(PhysicsDebugEffectKeys.UseUv) > 0.5;
            }
            set
            {
                Parameters.Set(PhysicsDebugEffectKeys.UseUv, value ? 1.0f : 0.0f);
            }
        }
    }
}