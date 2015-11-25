// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Graphics.Internals
{
    public struct EffectParameterResourceBinding
    {
        public delegate void ApplyParameterWithUpdaterDelegate(GraphicsDevice graphicsDevice, ref EffectParameterResourceData resourceBinding, EffectParameterCollectionGroup parameterCollectionGroup);
        
        public delegate void ApplyParameterFromValueDelegate(GraphicsDevice graphicsDevice, ref EffectParameterResourceData resourceBinding, object value);        

        public EffectParameterResourceData Description;

        public ApplyParameterWithUpdaterDelegate ApplyParameterWithUpdater;

        public ApplyParameterFromValueDelegate ApplyParameterDirect;

        public void ApplyParameter<T>(GraphicsDevice graphicsDevice, T value)
        {
            ApplyParameterDirect(graphicsDevice, ref Description, value);
        }
    }
}