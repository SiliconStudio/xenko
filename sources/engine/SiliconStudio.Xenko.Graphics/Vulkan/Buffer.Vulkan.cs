// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_XENKO_GRAPHICS_API_VULKAN
using System;
using System.Collections.Generic;

using SharpVulkan;

namespace SiliconStudio.Xenko.Graphics
{
    public partial class Buffer
    {
        //private SharpDX.Direct3D12.ResourceDescription nativeDescription;
        //internal ResourceStates NativeResourceStates;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="Buffer" /> class.
        /// </summary>
        /// <param name="device">The <see cref="GraphicsDevice"/>.</param>
        protected Buffer(GraphicsDevice device)
            : base(device)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Buffer" /> class.
        /// </summary>
        /// <param name="description">The description.</param>
        /// <param name="viewFlags">Type of the buffer.</param>
        /// <param name="viewFormat">The view format.</param>
        /// <param name="dataPointer">The data pointer.</param>
        protected Buffer InitializeFromImpl(BufferDescription description, BufferFlags viewFlags, PixelFormat viewFormat, IntPtr dataPointer)
        {
            //bufferDescription = description;
            //nativeDescription = ConvertToNativeDescription(Description);
            //ViewFlags = viewFlags;
            //InitCountAndViewFormat(out this.elementCount, ref viewFormat);
            //ViewFormat = viewFormat;
            //Recreate(dataPointer);

            //if (GraphicsDevice != null)
            //{
            //    GraphicsDevice.BuffersMemory += SizeInBytes/(float)0x100000;
            //}

            return this;
        }

        /// <inheritdoc/>
        protected internal override void OnDestroyed()
        {
            if (GraphicsDevice != null)
            {
                GraphicsDevice.BuffersMemory -= SizeInBytes/(float)0x100000;
            }
            base.OnDestroyed();
            DestroyImpl();
        }

        /// <inheritdoc/>
        protected internal override bool OnRecreate()
        {
            base.OnRecreate();

            if (Description.Usage == GraphicsResourceUsage.Immutable
                || Description.Usage == GraphicsResourceUsage.Default)
                return false;

            Recreate(IntPtr.Zero);

            return true;
        }

        /// <summary>
        /// Explicitly recreate buffer with given data. Usually called after a <see cref="GraphicsDevice"/> reset.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataPointer"></param>
        public void Recreate(IntPtr dataPointer)
        {
            //// TODO D3D12 where should that go longer term? should it be precomputed for future use? (cost would likely be additional check on SetDescriptorSets/Draw)
            //NativeResourceStates = ResourceStates.Common;
            //var bufferFlags = bufferDescription.BufferFlags;

            //if ((bufferFlags & BufferFlags.ConstantBuffer) != 0)
            //    NativeResourceStates |= ResourceStates.VertexAndConstantBuffer;

            //if ((bufferFlags & BufferFlags.IndexBuffer) != 0)
            //    NativeResourceStates |= ResourceStates.IndexBuffer;

            //if ((bufferFlags & BufferFlags.VertexBuffer) != 0)
            //    NativeResourceStates |= ResourceStates.VertexAndConstantBuffer;

            //if ((bufferFlags & BufferFlags.ShaderResource) != 0)
            //    NativeResourceStates |= ResourceStates.PixelShaderResource | ResourceStates.NonPixelShaderResource;

            //if ((bufferFlags & BufferFlags.UnorderedAccess) != 0)
            //    NativeResourceStates |= ResourceStates.UnorderedAccess;

            //if ((bufferFlags & BufferFlags.StructuredBuffer) != 0)
            //{
            //    throw new NotImplementedException();
            //    if (bufferDescription.StructureByteStride == 0)
            //        throw new ArgumentException("Element size cannot be set to 0 for structured buffer");
            //}

            //if ((bufferFlags & BufferFlags.RawBuffer) == BufferFlags.RawBuffer)
            //    throw new NotImplementedException();

            //if ((bufferFlags & BufferFlags.ArgumentBuffer) == BufferFlags.ArgumentBuffer)
            //    NativeResourceStates |= ResourceStates.IndirectArgument;

            //// TODO D3D12 move that to a global allocator in bigger committed resources
            //NativeDeviceChild = GraphicsDevice.NativeDevice.CreateCommittedResource(new HeapProperties(HeapType.Default), HeapFlags.None, nativeDescription, dataPointer != IntPtr.Zero ? ResourceStates.CopyDestination : NativeResourceStates);

            //if (dataPointer != IntPtr.Zero)
            //{
            //    // Copy data in upload heap for later copy
            //    // TODO D3D12 move that to a shared upload heap
            //    SharpDX.Direct3D12.Resource uploadResource;
            //    int uploadOffset;
            //    var uploadMemory = GraphicsDevice.AllocateUploadBuffer(SizeInBytes, out uploadResource, out uploadOffset);
            //    Utilities.CopyMemory(uploadMemory, dataPointer, SizeInBytes);

            //    // TODO D3D12 lock NativeCopyCommandList usages
            //    var commandList = GraphicsDevice.NativeCopyCommandList;
            //    commandList.Reset(GraphicsDevice.NativeCopyCommandAllocator, null);
            //    // Copy from upload heap to actual resource
            //    commandList.CopyBufferRegion(NativeResource, 0, uploadResource, uploadOffset, SizeInBytes);

            //    // Switch resource to proper read state
            //    commandList.ResourceBarrierTransition(NativeResource, 0, ResourceStates.CopyDestination, NativeResourceStates);

            //    commandList.Close();

            //    GraphicsDevice.NativeCommandQueue.ExecuteCommandList(commandList);

            //    // TODO D3D12 release uploadResource (using a fence to know when copy is done)
            //}
        }

        private void InitCountAndViewFormat(out int count, ref PixelFormat viewFormat)
        {
            if (Description.StructureByteStride == 0)
            {
                // TODO: The way to calculate the count is not always correct depending on the ViewFlags...etc.
                if ((ViewFlags & BufferFlags.RawBuffer) != 0)
                {
                    count = Description.SizeInBytes / sizeof(int);
                }
                else if ((ViewFlags & BufferFlags.ShaderResource) != 0)
                {
                    count = Description.SizeInBytes / viewFormat.SizeInBytes();
                }
                else
                {
                    count = 0;
                }
            }
            else
            {
                // For structured buffer
                count = Description.SizeInBytes / Description.StructureByteStride;
                viewFormat = PixelFormat.None;
            }
        }

        //private static SharpDX.Direct3D12.ResourceDescription ConvertToNativeDescription(BufferDescription bufferDescription)
        //{
        //    var size = bufferDescription.SizeInBytes;

        //    // TODO D3D12 for now, ensure size is multiple of 256 (for cbuffer views)
        //    size = (size + 255) & ~255;

        //    return SharpDX.Direct3D12.ResourceDescription.Buffer(size);
        //}
    }
} 
#endif 
