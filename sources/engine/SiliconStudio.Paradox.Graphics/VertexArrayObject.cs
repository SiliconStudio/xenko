// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Core.ReferenceCounting;

namespace SiliconStudio.Paradox.Graphics
{
    public partial class VertexArrayObject : GraphicsResourceBase
    {
        private readonly IndexBufferBinding indexBufferBinding;
        private readonly VertexBufferBinding[] vertexBufferBindings;
        private static readonly VertexBufferBinding[] emptyVertexBufferBindings = new VertexBufferBinding[0];
        private Description description;

        public static VertexArrayObject New(GraphicsDevice graphicsDevice, params VertexBufferBinding[] vertexBufferBindings)
        {
            return New(graphicsDevice, null, null, vertexBufferBindings);
        }

        public static VertexArrayObject New(GraphicsDevice graphicsDevice, IndexBufferBinding indexBufferBinding, params VertexBufferBinding[] vertexBufferBindings)
        {
            return New(graphicsDevice, null, indexBufferBinding, vertexBufferBindings);
        }

        public static VertexArrayObject New(GraphicsDevice graphicsDevice, EffectInputSignature shaderSignature, params VertexBufferBinding[] vertexBufferBindings)
        {
            return New(graphicsDevice, shaderSignature, null, vertexBufferBindings);
        }

        public static VertexArrayObject New(GraphicsDevice graphicsDevice, EffectInputSignature shaderSignature, IndexBufferBinding indexBufferBinding, params VertexBufferBinding[] vertexBufferBindings)
        {
            // Store SamplerState in a cache (D3D seems to have quite bad concurrency when using CreateSampler while rendering)
            VertexArrayObject vertexArrayObject;
            var description = new Description(shaderSignature, vertexBufferBindings, indexBufferBinding);

            lock (graphicsDevice.CachedVertexArrayObjects)
            {
                if (graphicsDevice.CachedVertexArrayObjects.TryGetValue(description, out vertexArrayObject))
                {
                    // TODO: Appropriate destroy
                    vertexArrayObject.AddReferenceInternal();
                }
                else
                {
                    vertexArrayObject = new VertexArrayObject(graphicsDevice, shaderSignature, indexBufferBinding, vertexBufferBindings);

                    // For now store description as is to avoid having to recreate it on Destroy.
                    // It would probably save little bit of memory space to try to reuse existing fields and add only what's missing.
                    vertexArrayObject.description = description;

                    graphicsDevice.CachedVertexArrayObjects.Add(description, vertexArrayObject);
                }
            }

            return vertexArrayObject;
        }

        protected override void Destroy()
        {
            lock (GraphicsDevice.CachedVertexArrayObjects)
            {
                GraphicsDevice.CachedVertexArrayObjects.Remove(description);
            }

            // Release underlying resources
            foreach (var vertexBufferBinding in vertexBufferBindings)
            {
                ((IReferencable)vertexBufferBinding.Buffer).Release();
            }

            if (indexBufferBinding != null)
            {
                ((IReferencable)indexBufferBinding.Buffer).Release();
            }

            base.Destroy();
        }

        internal struct Description : IEquatable<Description>
        {
            private static EqualityComparer<VertexBufferBinding> VertexBufferBindingComparer = EqualityComparer<VertexBufferBinding>.Default;
            private int hashCode;

            public EffectInputSignature ShaderSignature;
            public VertexBufferBinding[] VertexBuffers;
            public IndexBufferBinding IndexBuffer;

            public Description(EffectInputSignature shaderSignature, VertexBufferBinding[] vertexBuffers, IndexBufferBinding indexBuffer)
            {
                ShaderSignature = shaderSignature;
                VertexBuffers = vertexBuffers ?? emptyVertexBufferBindings;
                IndexBuffer = indexBuffer;

                // Precompute hash code
                hashCode = 0;
                hashCode = ComputeHashCode();
            }

            public bool Equals(Description other)
            {
                return Equals(ShaderSignature, other.ShaderSignature)
                    && ArrayExtensions.ArraysEqual(VertexBuffers, other.VertexBuffers, VertexBufferBindingComparer)
                    && Equals(IndexBuffer, other.IndexBuffer);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                return obj is Description && Equals((Description)obj);
            }

            public override int GetHashCode()
            {
                return hashCode;
            }

            private int ComputeHashCode()
            {
                unchecked
                {
                    int hashCode = (ShaderSignature != null ? ShaderSignature.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ VertexBuffers.ComputeHash(VertexBufferBindingComparer);
                    hashCode = (hashCode*397) ^ (IndexBuffer != null ? IndexBuffer.GetHashCode() : 0);
                    return hashCode;
                }
            }
        }
    }
}