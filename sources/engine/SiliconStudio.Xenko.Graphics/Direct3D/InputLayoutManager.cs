// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core;
#if SILICONSTUDIO_XENKO_GRAPHICS_API_DIRECT3D
using System;
using System.Collections.Generic;
using SharpDX;
using SharpDX.Direct3D11;
using ComponentBase = SiliconStudio.Core.ComponentBase;
namespace SiliconStudio.Xenko.Graphics
{
    internal class InputLayoutManager : ComponentBase
    {
        private readonly Dictionary<InputKey, InputLayout> registeredSignatures = new Dictionary<InputKey, InputLayout>(1024);
        private readonly GraphicsDevice graphicsDevice;

        public InputLayoutManager(GraphicsDevice graphicsDevice)
        {
            this.graphicsDevice = graphicsDevice;
        }

        public InputLayout GetInputLayout(EffectInputSignature effectInputSignature, VertexArrayLayout vertexArrayLayout)
        {
            var inputKey = new InputKey(effectInputSignature, vertexArrayLayout);

            InputLayout inputLayout;
            // TODO: Check if it is really worth to use ConcurrentDictionary 
            lock (registeredSignatures)
            {
                if (!registeredSignatures.TryGetValue(inputKey, out inputLayout))
                {
                    try
                    {
                        inputLayout = new InputLayout(graphicsDevice.NativeDevice, effectInputSignature.NativeSignature, vertexArrayLayout.InputElements);
                    }
                    catch (Exception)
                    {
                        throw new InvalidOperationException("The provided vertex array input elements are not matching the ones of the shader." + 
                        " [Details: EffectInputSignature='{0}', VertexArrayLayout='{1}]".ToFormat(effectInputSignature.ToString(), vertexArrayLayout.ToString()));
                    }
                    registeredSignatures.Add(inputKey, inputLayout);
                }

                ((IUnknown)inputLayout).AddReference();
            }
            return inputLayout;
        }

        internal void OnDestroyed()
        {
            lock (registeredSignatures)
            {
                foreach (var inputLayout in registeredSignatures)
                {
                    ((IUnknown)inputLayout.Value).Release();
                }
                registeredSignatures.Clear();
            }
        }

        private struct InputKey : IEquatable<InputKey>
        {
            private readonly EffectInputSignature effectInputSignature;

            private readonly VertexArrayLayout vertexArrayLayout;

            public InputKey(EffectInputSignature effectInputSignature, VertexArrayLayout vertexArrayLayout)
            {
                this.effectInputSignature = effectInputSignature;
                this.vertexArrayLayout = vertexArrayLayout;
            }

            public bool Equals(InputKey other)
            {
                return ReferenceEquals(effectInputSignature, other.effectInputSignature) && ReferenceEquals(vertexArrayLayout, other.vertexArrayLayout);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                return obj is InputKey && Equals((InputKey)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (effectInputSignature.GetHashCode() * 397) ^ vertexArrayLayout.GetHashCode();
                }
            }
        }
    }
}
#endif