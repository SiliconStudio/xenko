// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGL
using System;
using System.Collections.Generic;
#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
using OpenTK.Graphics.ES30;
#else
using OpenTK.Graphics.OpenGL;
#endif
using SiliconStudio.Core;
using SiliconStudio.Core.ReferenceCounting;

namespace SiliconStudio.Xenko.Graphics
{
    public partial class VertexArrayObject
    {
        internal IntPtr indexBufferOffset;
        internal int indexElementSize;
        internal int indexBufferId;
        internal DrawElementsType drawElementsType;

        private readonly EffectInputSignature preferredInputSignature;
        private VertexArrayObjectInstance preferredInstance;
        private VertexAttrib[] vertexAttribs;

        private EffectInputSignature currentShaderSignature;
        private VertexArrayObjectInstance currentInstance;

        private readonly Dictionary<EffectInputSignature, VertexArrayObjectInstance> registeredInstances = new Dictionary<EffectInputSignature, VertexArrayObjectInstance>(ReferenceEqualityComparer<EffectInputSignature>.Default);
        
        private VertexArrayObject(GraphicsDevice graphicsDevice, EffectInputSignature shaderSignature, IndexBufferBinding indexBufferBinding, VertexBufferBinding[] vertexBufferBindings)
            : base(graphicsDevice)
        {
            this.vertexBufferBindings = vertexBufferBindings;
            this.indexBufferBinding = indexBufferBinding;
            this.preferredInputSignature = shaderSignature;
            
            // Increase the reference count on the provided buffers -> we do not want to take the ownership
            foreach (VertexBufferBinding vertexBufferBinding in vertexBufferBindings)
                vertexBufferBinding.Buffer.AddReferenceInternal();

            if (indexBufferBinding != null)
                indexBufferBinding.Buffer.AddReferenceInternal();

            CreateAttributes();
        }

        private void CreateAttributes()
        {
            int vertexAttribCount = 0;
            for (int i = 0; i < vertexBufferBindings.Length; ++i)
            {
                vertexAttribCount += vertexBufferBindings[i].Declaration.VertexElements.Length;
            }

            vertexAttribs = new VertexAttrib[vertexAttribCount];
            int j = 0;
            for (int i = 0; i < vertexBufferBindings.Length; ++i)
            {
                var inputSlot = vertexBufferBindings[i];
                var vertexBuffer = vertexBufferBindings[i].Buffer;

                foreach (var vertexElementAndOffset in inputSlot.Declaration.EnumerateWithOffsets())
                {
                    var vertexElement = vertexElementAndOffset.VertexElement;
                    var attributeName = "a_" + vertexElement.SemanticName + vertexElement.SemanticIndex;

                    var vertexElementFormat = ConvertVertexElementFormat(vertexElement.Format);
#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
                    IntPtr bufferStart = vertexBuffer.ResourceId == 0
                        ? vertexBuffer.StagingData
                        : (IntPtr)vertexBufferBindings[i].Offset;
#else
                    var bufferStart = (IntPtr)vertexBufferBindings[i].Offset;
#endif
                    vertexAttribs[j] = new VertexAttrib
                    {
                        VertexBufferId = vertexBuffer.ResourceId,
                        Index = -1,
                        IsInteger = IsInteger(vertexElementFormat.Type),
                        Size = vertexElementFormat.Size,
                        Type = vertexElementFormat.Type,
                        Normalized = vertexElementFormat.Normalized,
                        Stride = inputSlot.Declaration.VertexStride,
                        Offset = bufferStart + vertexElementAndOffset.Offset,
                        AttributeName = attributeName
                    };

                    j++;
                }
            }

            if (indexBufferBinding != null)
            {
                indexBufferId = indexBufferBinding.Buffer.resourceId;

#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
                if (GraphicsDevice.IsOpenGLES2 && indexBufferBinding.Is32Bit)
                    throw new PlatformNotSupportedException("32 bits index buffer are not supported on OpenGL ES 2.0");
                indexBufferOffset = (indexBufferId == 0 ? indexBufferBinding.Buffer.StagingData : IntPtr.Zero) +
                                    indexBufferBinding.Offset;
#else
                
                indexBufferOffset = (IntPtr)indexBufferBinding.Offset;
#endif
                drawElementsType = indexBufferBinding.Is32Bit ? DrawElementsType.UnsignedInt : DrawElementsType.UnsignedShort;
                indexElementSize = indexBufferBinding.Is32Bit ? 4 : 2;
            }

            // If we have a signature, we can already pre-create the instance
            if (preferredInputSignature != null)
            {
                preferredInstance = GetInstance(preferredInputSignature);
            }

            currentShaderSignature = preferredInputSignature;
            currentInstance = preferredInstance;
        }

        protected override void DestroyImpl()
        {
            // Dispose all instances
            foreach (var vertexArrayObjectInstance in registeredInstances)
            {
                vertexArrayObjectInstance.Value.Dispose();
            }

            registeredInstances.Clear();
        }

        protected internal override bool OnRecreate()
        {
            CreateAttributes();
            return true;
        }

        internal bool RequiresApply(EffectInputSignature effectInputSignature)
        {
            return !ReferenceEquals(effectInputSignature, currentShaderSignature);
        }

        internal void Apply(EffectInputSignature effectInputSignature)
        {
            if (effectInputSignature == null) throw new ArgumentNullException("effectInputSignature");

            // Optimization: If the current VAO and shader signature was previously used, we can use it directly without asking for a proper instance
            if (RequiresApply(effectInputSignature))
            {
                currentInstance = ReferenceEquals(preferredInputSignature, effectInputSignature)
                    ? preferredInstance
                    : GetInstance(effectInputSignature);
                currentShaderSignature = effectInputSignature;
            }

            currentInstance.Apply(GraphicsDevice);
        }

        private VertexArrayObjectInstance GetInstance(EffectInputSignature effectInputSignature)
        {
            VertexArrayObjectInstance inputLayout;
            lock (registeredInstances)
            {
                if (!registeredInstances.TryGetValue(effectInputSignature, out inputLayout))
                {
                    inputLayout = new VertexArrayObjectInstance(GraphicsDevice, effectInputSignature, vertexAttribs, indexBufferId);
                    registeredInstances.Add(effectInputSignature, inputLayout);
                }
            }
            return inputLayout;
        }

        private static bool IsInteger(VertexAttribPointerType type)
        {
            switch (type)
            {
                case VertexAttribPointerType.Byte:
                case VertexAttribPointerType.UnsignedByte:
                case VertexAttribPointerType.Short:
                case VertexAttribPointerType.UnsignedShort:
                case VertexAttribPointerType.Int:
                case VertexAttribPointerType.UnsignedInt:
                    return true;
                default:
                    return false;
            }
        }

        private struct ElementFormat
        {
            public VertexAttribPointerType Type;
            public int Size;
            public bool Normalized;

            public ElementFormat(VertexAttribPointerType type, int size, bool normalized = false)
            {
                Type = type;
                Size = size;
                Normalized = normalized;
            }
        }

        private static ElementFormat ConvertVertexElementFormat(PixelFormat format)
        {
            switch (format)
            {
                case PixelFormat.R8_SInt:
                    return new ElementFormat(VertexAttribPointerType.Byte, 1);
                case PixelFormat.R8_UInt:
                    return new ElementFormat(VertexAttribPointerType.UnsignedByte, 1);
                case PixelFormat.R16_SInt:
                    return new ElementFormat(VertexAttribPointerType.Short, 1);
                case PixelFormat.R16_UInt:
                    return new ElementFormat(VertexAttribPointerType.UnsignedShort, 1);
                case PixelFormat.R8G8_SInt:
                    return new ElementFormat(VertexAttribPointerType.Byte, 2);
                case PixelFormat.R8G8_UInt:
                    return new ElementFormat(VertexAttribPointerType.UnsignedByte, 2);
                case PixelFormat.R16G16_SInt:
                    return new ElementFormat(VertexAttribPointerType.Short, 2);
                case PixelFormat.R16G16_UInt:
                    return new ElementFormat(VertexAttribPointerType.UnsignedShort, 2);
                case PixelFormat.R8G8B8A8_SInt:
                    return new ElementFormat(VertexAttribPointerType.Byte, 4);
                case PixelFormat.R8G8B8A8_UInt:
                    return new ElementFormat(VertexAttribPointerType.UnsignedByte, 4);
                case PixelFormat.R16G16B16A16_SInt:
                    return new ElementFormat(VertexAttribPointerType.Short, 4);
                case PixelFormat.R16G16B16A16_UInt:
                    return new ElementFormat(VertexAttribPointerType.UnsignedShort, 4);
                case PixelFormat.R32_Float:
                    return new ElementFormat(VertexAttribPointerType.Float, 1);
                case PixelFormat.R32G32_Float:
                    return new ElementFormat(VertexAttribPointerType.Float, 2);
                case PixelFormat.R32G32B32_Float:
                    return new ElementFormat(VertexAttribPointerType.Float, 3);
                case PixelFormat.R32G32B32A32_Float:
                    return new ElementFormat(VertexAttribPointerType.Float, 4);
                case PixelFormat.R8_UNorm:
                    return new ElementFormat(VertexAttribPointerType.UnsignedByte, 1, true);
                case PixelFormat.R8G8_UNorm:
                    return new ElementFormat(VertexAttribPointerType.UnsignedByte, 2, true);
                case PixelFormat.R8G8B8A8_UNorm:
                    return new ElementFormat(VertexAttribPointerType.UnsignedByte, 4, true);
                case PixelFormat.R8_SNorm:
                    return new ElementFormat(VertexAttribPointerType.Byte, 1, true);
                case PixelFormat.R8G8_SNorm:
                    return new ElementFormat(VertexAttribPointerType.Byte, 2, true);
                case PixelFormat.R8G8B8A8_SNorm:
                    return new ElementFormat(VertexAttribPointerType.Byte, 4, true);
                case PixelFormat.R16_UNorm:
                    return new ElementFormat(VertexAttribPointerType.UnsignedShort, 1, true);
                case PixelFormat.R16G16_UNorm:
                    return new ElementFormat(VertexAttribPointerType.UnsignedShort, 2, true);
                case PixelFormat.R16G16B16A16_UNorm:
                    return new ElementFormat(VertexAttribPointerType.UnsignedShort, 4, true);
                case PixelFormat.R16_SNorm:
                    return new ElementFormat(VertexAttribPointerType.Short, 1, true);
                case PixelFormat.R16G16_SNorm:
                    return new ElementFormat(VertexAttribPointerType.Short, 2, true);
                case PixelFormat.R16G16B16A16_SNorm:
                    return new ElementFormat(VertexAttribPointerType.Short, 4, true);
#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
                // HALF_FLOAT for OpenGL ES 2.x (OES extension)
                case PixelFormat.R16G16B16A16_Float:
                    return new ElementFormat((VertexAttribPointerType)0x8D61, 4); // HALF_FLOAT_OES
                case PixelFormat.R16G16_Float:
                    return new ElementFormat((VertexAttribPointerType)0x8D61, 2); // HALF_FLOAT_OES
#else
                // HALF_FLOAT for OpenGL and OpenGL ES 3.x (also used for OpenGL ES 2.0 under 3.0 emulator)
                case PixelFormat.R16G16B16A16_Float:
                    return new ElementFormat((VertexAttribPointerType)0x8D61, 4); // HALF_FLOAT
                case PixelFormat.R16G16_Float:
                    return new ElementFormat((VertexAttribPointerType)0x8D61, 2); // HALF_FLOAT
#endif
                default:
                    throw new NotSupportedException();
            }
        }
    }
} 
#endif
