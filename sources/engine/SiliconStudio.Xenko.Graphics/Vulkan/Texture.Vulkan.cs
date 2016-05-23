// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#if SILICONSTUDIO_XENKO_GRAPHICS_API_VULKAN
// Copyright (c) 2010-2012 SharpDX - Alexandre Mutel
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using System.Linq;
using SharpVulkan;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Graphics
{
    public partial class Texture
    {
        internal SharpVulkan.Image NativeImage;
        internal ImageView NativeColorAttachmentView;
        internal ImageView NativeDepthStencilView;
        internal ImageView NativeImageView;

        private bool isNotOwningResources;

        internal Format NativeFormat;
        internal int RowPitch;
        internal int DepthPitch;
        internal bool HasStencil;

        internal ImageLayout NativeLayout;
        internal AccessFlags NativeAccessMask;
        internal ImageAspectFlags NativeImageAspect;

        public void Recreate(DataBox[] dataBoxes = null)
        {
            InitializeFromImpl(dataBoxes);
        }

        public static bool IsDepthStencilReadOnlySupported(GraphicsDevice device)
        {
            // TODO VULKAN
            return true;
        }

        internal Texture InitializeFromPersistent(TextureDescription description, SharpVulkan.Image nativeImage)
        {
            NativeImage = nativeImage;

            return InitializeFrom(description);
        }

        internal Texture InitializeWithoutResources(TextureDescription description)
        {
            isNotOwningResources = true;
            return InitializeFrom(description);
        }

        internal void SetNativeHandles(SharpVulkan.Image image, ImageView attachmentView)
        {
            NativeImage = image;
            NativeColorAttachmentView = attachmentView;
        }

        private unsafe void InitializeFromImpl(DataBox[] dataBoxes = null)
        {
            int pixelSize;
            bool compressed;
            VulkanConvertExtensions.ConvertPixelFormat(ViewFormat, out NativeFormat, out pixelSize, out compressed);

            DepthPitch = Description.Width * Description.Height * pixelSize;
            RowPitch = Description.Width * pixelSize;

            NativeLayout =
                IsRenderTarget ? ImageLayout.ColorAttachmentOptimal :
                IsDepthStencil ? ImageLayout.DepthStencilAttachmentOptimal :
                IsShaderResource ? ImageLayout.ShaderReadOnlyOptimal :
                ImageLayout.General;

            if (ParentTexture != null)
            {
                // Create only a view
                NativeImage = ParentTexture.NativeImage;
                NativeMemory = ParentTexture.NativeMemory;
            }
            else
            {
                if (NativeImage == SharpVulkan.Image.Null)
                {
                    if (!isNotOwningResources)
                    {
                        // Create a new image
                        var createInfo = new ImageCreateInfo
                        {
                            StructureType = StructureType.ImageCreateInfo,
                            ArrayLayers = (uint)ArraySize,
                            Extent = new Extent3D((uint)Width, (uint)Height, (uint)Depth),
                            MipLevels = (uint)MipLevels,
                            Samples = SampleCountFlags.Sample1,
                            Format = NativeFormat,
                            Flags = ImageCreateFlags.None,
                            Tiling = ImageTiling.Optimal,
                            InitialLayout = ImageLayout.Undefined
                        };

                        switch (Dimension)
                        {
                            case TextureDimension.Texture1D:
                                createInfo.ImageType = ImageType.Image1D;
                                break;
                            case TextureDimension.Texture2D:
                                createInfo.ImageType = ImageType.Image2D;
                                break;
                            case TextureDimension.Texture3D:
                                createInfo.ImageType = ImageType.Image3D;
                                break;
                            case TextureDimension.TextureCube:
                                createInfo.ImageType = ImageType.Image2D;
                                createInfo.Flags |= ImageCreateFlags.CubeCompatible;
                                break;
                        }

                        if (IsRenderTarget)
                            createInfo.Usage |= ImageUsageFlags.ColorAttachment;

                        if (IsDepthStencil)
                            createInfo.Usage |= ImageUsageFlags.DepthStencilAttachment;

                        if (IsShaderResource)
                            createInfo.Usage |= ImageUsageFlags.Sampled; // TODO VULKAN: Input attachments

                        // TODO VULKAN: Can we restrict more based on GraphicsResourceUsage? 
                        createInfo.Usage |= ImageUsageFlags.TransferSource | ImageUsageFlags.TransferDestination;
                        
                        // TODO VULKAN: Simulate staging textures?
                        var memoryProperties = MemoryPropertyFlags.DeviceLocal;
                        if (Usage == GraphicsResourceUsage.Dynamic || Usage == GraphicsResourceUsage.Staging)
                        {
                            createInfo.Tiling = ImageTiling.Linear;
                            memoryProperties = MemoryPropertyFlags.HostVisible;
                        }

                        // TODO: Multisampling, flags, usage, etc.

                        NativeImage = GraphicsDevice.NativeDevice.CreateImage(ref createInfo);

                        MemoryRequirements memoryRequirements;
                        GraphicsDevice.NativeDevice.GetImageMemoryRequirements(NativeImage, out memoryRequirements);

                        var allocateInfo = new MemoryAllocateInfo
                        {
                            StructureType = StructureType.MemoryAllocateInfo,
                            AllocationSize = memoryRequirements.Size,
                        };

                        PhysicalDeviceMemoryProperties physicalDeviceMemoryProperties;
                        GraphicsDevice.Adapter.PhysicalDevice.GetMemoryProperties(out physicalDeviceMemoryProperties);
                        var typeBits = memoryRequirements.MemoryTypeBits;
                        for (uint i = 0; i < physicalDeviceMemoryProperties.MemoryTypeCount; i++)
                        {
                            if ((typeBits & 1) == 1)
                            {
                                // Type is available, does it match user properties?
                                var memoryType = *((MemoryType*)&physicalDeviceMemoryProperties.MemoryTypes + i);
                                if ((memoryType.PropertyFlags & memoryProperties) == memoryProperties)
                                {
                                    allocateInfo.MemoryTypeIndex = i;
                                    break;
                                }
                            }
                            typeBits >>= 1;
                        }

                        NativeMemory = GraphicsDevice.NativeDevice.AllocateMemory(ref allocateInfo);

                        GraphicsDevice.NativeDevice.BindImageMemory(NativeImage, NativeMemory, 0);

                        //AllocateMemory(IntPtr.Zero, memoryProperties);

                        //if (dataBoxes != null && dataBoxes.Length > 0)
                        //{
                        //    if ((memoryProperties & MemoryPropertyFlags.HostVisible) != 0)
                        //    {
                        //        long offset = 0;
                        //        for (var i = 0; i < dataBoxes.Length; i++)
                        //        {
                        //            var subresource = new ImageSubresource
                        //            {
                        //                Aspect = ImageAspect.Color,
                        //                ArraySlice = i / MipLevels,
                        //                MipLevel = i % MipLevels
                        //            };

                        //            var mipMapDesc = GetMipMapDescription(i % MipLevels);

                        //            // TODO: Not reporting correct values for subresources for linear tiling textures?
                        //            var subresourceLayout = GraphicsDevice.NativeDevice.GetImageSubresourceLayout(NativeImage, subresource);

                        //            subresourceLayout.Offset = offset;
                        //            subresourceLayout.Size = mipMapDesc.Depth * mipMapDesc.DepthStride;

                        //            var dataPointer = GraphicsDevice.NativeDevice.MapMemory(NativeMemory, subresourceLayout.Offset, subresourceLayout.Size, 0);
                        //            Utilities.CopyMemory(dataPointer, dataBoxes[i].DataPointer, (int)subresourceLayout.Size);
                        //            GraphicsDevice.NativeDevice.UnmapMemory(NativeMemory);

                        //            offset += subresourceLayout.Size;
                        //        }
                        //    }
                        //    else
                        //    {
                        //        using (var stagingTexture = New(GraphicsDevice, Description.ToStagingDescription(), dataBoxes))
                        //        {
                        //            GraphicsDevice.Copy(stagingTexture, this);
                        //        }
                        //    }

                        //// Trigger copy
                        //var commandList = GraphicsDevice.NativeCopyCommandList;
                        //commandList.Reset(GraphicsDevice.NativeCopyCommandAllocator, null);
                        //commandList.CopyResource(NativeResource, nativeUploadTexture);
                        //commandList.ResourceBarrierTransition(NativeResource, ResourceStates.CopyDestination, ResourceStates.Common);
                        //commandList.Close();

                        //GraphicsDevice.NativeCommandQueue.ExecuteCommandList(commandList);
                        //}
                    }

                    //GraphicsDevice.SetImageLayout(this, IsDepthStencil ? ImageAspectFlags.Depth : ImageAspectFlags.Color, ImageLayout.Undefined, NativeLayout);
                    //GraphicsDevice.Flush();
                }
            }

            if (NativeLayout == ImageLayout.TransferDestinationOptimal)
                NativeAccessMask = AccessFlags.TransferRead;

            if (NativeLayout == ImageLayout.ColorAttachmentOptimal)
                NativeAccessMask = AccessFlags.ColorAttachmentWrite;

            if (NativeLayout == ImageLayout.DepthStencilAttachmentOptimal)
                NativeAccessMask = AccessFlags.DepthStencilAttachmentWrite;

            if (NativeLayout == ImageLayout.ShaderReadOnlyOptimal)
                NativeAccessMask = AccessFlags.ShaderRead | AccessFlags.InputAttachmentRead;

            if (!isNotOwningResources && Usage != GraphicsResourceUsage.Staging)
            {
                NativeImageView = GetImageView(ViewType, ArraySlice, MipLevel);
                NativeColorAttachmentView = GetColorAttachmentView(ViewType, ArraySlice, MipLevel);
                NativeDepthStencilView = GetDepthStencilView(out HasStencil);
            }

            NativeImageAspect = IsDepthStencil ? ImageAspectFlags.Depth : ImageAspectFlags.Color;
            if (HasStencil)
                NativeImageAspect |= ImageAspectFlags.Stencil;

            if (NativeImage != SharpVulkan.Image.Null && ParentTexture == null)
            {
                var commandBuffer = GraphicsDevice.NativeCopyCommandBuffer;
                var beginInfo = new CommandBufferBeginInfo
                {
                    StructureType = StructureType.CommandBufferBeginInfo,
                };
                commandBuffer.Begin(ref beginInfo);

                if (dataBoxes != null && dataBoxes.Length > 0)
                {
                    int totalSize = dataBoxes.Length * 4;
                    for (int i = 0; i < dataBoxes.Length; i++)
                    {
                        totalSize += dataBoxes[i].SlicePitch;
                    }

                    SharpVulkan.Buffer uploadResource;
                    int uploadOffset;
                    var uploadMemory = GraphicsDevice.AllocateUploadBuffer(totalSize, out uploadResource, out uploadOffset);

                    var bufferMemoryBarrier = new BufferMemoryBarrier
                    {
                        StructureType = StructureType.BufferMemoryBarrier,
                        Buffer = uploadResource,
                        SourceAccessMask = AccessFlags.HostWrite,
                        DestinationAccessMask = AccessFlags.TransferRead,
                    };

                    var initialBarrier = new ImageMemoryBarrier
                    {
                        StructureType = StructureType.ImageMemoryBarrier,
                        OldLayout = ImageLayout.Undefined,
                        NewLayout = ImageLayout.TransferDestinationOptimal,
                        Image = NativeImage,
                        SubresourceRange = new ImageSubresourceRange(NativeImageAspect, 0, (uint)ArraySize, 0, (uint)MipLevels),
                        SourceAccessMask = AccessFlags.None,
                        DestinationAccessMask = AccessFlags.TransferWrite
                    };
                    commandBuffer.PipelineBarrier(PipelineStageFlags.TopOfPipe, PipelineStageFlags.Transfer, DependencyFlags.None, 0, null, 1, &bufferMemoryBarrier, 1, &initialBarrier);

                    var copies = new BufferImageCopy[dataBoxes.Length];
                    for (int i = 0; i < copies.Length; i++)
                    {
                        var slicePitch = dataBoxes[i].SlicePitch;

                        int arraySlice = i / MipLevels;
                        int mipSlice = i % MipLevels;
                        var mipMapDescription = GetMipMapDescription(mipSlice);

                        SubresourceLayout layout;
                        GraphicsDevice.NativeDevice.GetImageSubresourceLayout(NativeImage, new ImageSubresource { AspectMask = NativeImageAspect, ArrayLayer = (uint)arraySlice, MipLevel = (uint)mipSlice}, out layout);

                        var alignment = ((uploadOffset + 3) & ~3) - uploadOffset;
                        uploadMemory += alignment;
                        uploadOffset += alignment;

                        Utilities.CopyMemory(uploadMemory, dataBoxes[i].DataPointer, slicePitch);

                        // TODO VULKAN: Check if pitches are valid
                        copies[i] = new BufferImageCopy
                        {
                            BufferOffset = (ulong)uploadOffset,
                            ImageSubresource = new ImageSubresourceLayers { AspectMask = ImageAspectFlags.Color, BaseArrayLayer = (uint)arraySlice, LayerCount = 1, MipLevel = (uint)mipSlice },
                            BufferRowLength = 0,//(uint)(dataBoxes[i].RowPitch / pixelSize),
                            BufferImageHeight = 0,//(uint)(dataBoxes[i].SlicePitch / dataBoxes[i].RowPitch),
                            ImageOffset = new Offset3D(0, 0, arraySlice),
                            ImageExtent = new Extent3D((uint)mipMapDescription.Width, (uint)mipMapDescription.Height, 1)
                        };

                        uploadMemory += slicePitch;
                        uploadOffset += slicePitch;
                    }

                    fixed (BufferImageCopy* copiesPointer = &copies[0])
                    {
                        commandBuffer.CopyBufferToImage(uploadResource, NativeImage, ImageLayout.TransferDestinationOptimal, (uint)copies.Length, copiesPointer);
                    }
                }

                // Transition to default layout
                var imageMemoryBarrier = new ImageMemoryBarrier
                {
                    StructureType = StructureType.ImageMemoryBarrier,
                    OldLayout = dataBoxes == null || dataBoxes.Length == 0 ? ImageLayout.Undefined : ImageLayout.TransferDestinationOptimal,
                    NewLayout = NativeLayout,
                    Image = NativeImage,
                    SubresourceRange = new ImageSubresourceRange(NativeImageAspect, 0, (uint)ArraySize, 0, (uint)MipLevels),
                    SourceAccessMask = dataBoxes == null || dataBoxes.Length == 0 ? AccessFlags.None : AccessFlags.TransferWrite,
                    DestinationAccessMask = NativeAccessMask
                };
                commandBuffer.PipelineBarrier(PipelineStageFlags.Transfer, PipelineStageFlags.AllCommands, DependencyFlags.None, 0, null, 0, null, 1, &imageMemoryBarrier);

                // Close and submit
                commandBuffer.End();

                var submitInfo = new SubmitInfo
                {
                    StructureType = StructureType.SubmitInfo,
                    CommandBufferCount = 1,
                    CommandBuffers = new IntPtr(&commandBuffer),
                };

                GraphicsDevice.NativeCommandQueue.Submit(1, &submitInfo, Fence.Null);
                GraphicsDevice.NativeCommandQueue.WaitIdle();
                commandBuffer.Reset(CommandBufferResetFlags.None);
            }
        }

        protected override unsafe void DestroyImpl()
        {
            if (ParentTexture != null || isNotOwningResources)
            {
                NativeImage = SharpVulkan.Image.Null;
                NativeMemory = DeviceMemory.Null;
            }

            if (!isNotOwningResources)
            {
                if (NativeMemory != DeviceMemory.Null)
                {
                    GraphicsDevice.NativeDevice.FreeMemory(NativeMemory);
                    NativeMemory = DeviceMemory.Null;
                }

                if (NativeImage != SharpVulkan.Image.Null)
                {
                    GraphicsDevice.NativeDevice.DestroyImage(NativeImage);
                    NativeImage = SharpVulkan.Image.Null;
                }

                if (NativeImageView != ImageView.Null)
                {
                    GraphicsDevice.NativeDevice.DestroyImageView(NativeImageView);
                    NativeImageView = ImageView.Null;
                }

                if (NativeColorAttachmentView != ImageView.Null)
                {
                    GraphicsDevice.NativeDevice.DestroyImageView(NativeColorAttachmentView);
                    NativeColorAttachmentView = ImageView.Null;
                }

                if (NativeDepthStencilView != ImageView.Null)
                {
                    GraphicsDevice.NativeDevice.DestroyImageView(NativeDepthStencilView);
                    NativeDepthStencilView = ImageView.Null;
                }
            }

            base.DestroyImpl();
        }

        /// <inheritdoc/>
        protected internal override void OnDestroyed()
        {
            base.OnDestroyed();
            DestroyImpl();
        }

        private void OnRecreateImpl()
        {
            // Dependency: wait for underlying texture to be recreated
            if (ParentTexture != null && ParentTexture.LifetimeState != GraphicsResourceLifetimeState.Active)
                return;

            // Render Target / Depth Stencil are considered as "dynamic"
            if ((Usage == GraphicsResourceUsage.Immutable
                    || Usage == GraphicsResourceUsage.Default)
                && !IsRenderTarget && !IsDepthStencil)
                return;

            if (ParentTexture == null && GraphicsDevice != null)
            {
                GraphicsDevice.TextureMemory -= (Depth * DepthStride) / (float)0x100000;
            }

            InitializeFromImpl();
        }

        private unsafe ImageView GetImageView(ViewType viewType, int arrayOrDepthSlice, int mipIndex)
        {
            if (!IsShaderResource)
                return ImageView.Null;

            if (viewType == ViewType.MipBand)
                throw new NotSupportedException("ViewSlice.MipBand is not supported for render targets");

            int arrayCount;
            int mipCount;
            GetViewSliceBounds(viewType, ref arrayOrDepthSlice, ref mipIndex, out arrayCount, out mipCount);

            Format nativeViewFormat;
            int pixelSize;
            bool compressed;
            VulkanConvertExtensions.ConvertPixelFormat(ViewFormat, out nativeViewFormat, out pixelSize, out compressed);

            var createInfo = new ImageViewCreateInfo
            {
                StructureType = StructureType.ImageViewCreateInfo,
                Format = nativeViewFormat,
                Image = NativeImage,
                Components = ComponentMapping.Identity,
                SubresourceRange = new ImageSubresourceRange(IsDepthStencil ? ImageAspectFlags.Depth : ImageAspectFlags.Color, (uint)arrayOrDepthSlice, (uint)arrayCount, (uint)mipIndex, (uint)mipCount)
            };

            if (IsMultiSample)
                throw new NotImplementedException();

            if (this.ArraySize > 1)
            {
                if (IsMultiSample && Dimension != TextureDimension.Texture2D)
                    throw new NotSupportedException("Multisample is only supported for 2D Textures");

                if (Dimension == TextureDimension.Texture3D)
                    throw new NotSupportedException("Texture Array is not supported for Texture3D");

                switch (Dimension)
                {
                    case TextureDimension.Texture1D:
                        createInfo.ViewType = ImageViewType.Image1DArray;
                        break;
                    case TextureDimension.Texture2D:
                        createInfo.ViewType = ImageViewType.Image2DArray;
                        break;
                    case TextureDimension.TextureCube:
                        createInfo.ViewType = ImageViewType.ImageCubeArray;
                        break;
                }
            }
            else
            {
                if (IsMultiSample && Dimension != TextureDimension.Texture2D)
                    throw new NotSupportedException("Multisample is only supported for 2D RenderTarget Textures");

                if (Dimension == TextureDimension.TextureCube)
                    throw new NotSupportedException("TextureCube dimension is expecting an arraysize > 1");

                switch (Dimension)
                {
                    case TextureDimension.Texture1D:
                        createInfo.ViewType = ImageViewType.Image1D;
                        break;
                    case TextureDimension.Texture2D:
                        createInfo.ViewType = ImageViewType.Image2D;
                        break;
                    case TextureDimension.Texture3D:
                        createInfo.ViewType = ImageViewType.Image3D;
                        break;
                    case TextureDimension.TextureCube:
                        createInfo.ViewType = ImageViewType.ImageCube;
                        break;
                }
            }

            return GraphicsDevice.NativeDevice.CreateImageView(ref createInfo);
        }

        private unsafe ImageView GetColorAttachmentView(ViewType viewType, int arrayOrDepthSlice, int mipIndex)
        {
            if (!IsRenderTarget)
                return ImageView.Null;

            if (viewType == ViewType.MipBand)
                throw new NotSupportedException("ViewSlice.MipBand is not supported for render targets");

            int arrayCount;
            int mipCount;
            GetViewSliceBounds(viewType, ref arrayOrDepthSlice, ref mipIndex, out arrayCount, out mipCount);

            Format backBufferFormat;
            int pixelSize;
            bool compressed;
            VulkanConvertExtensions.ConvertPixelFormat(ViewFormat, out backBufferFormat, out pixelSize, out compressed);

            var createInfo = new ImageViewCreateInfo
            {
                StructureType = StructureType.ImageViewCreateInfo,
                ViewType = ImageViewType.Image2D,
                Format = backBufferFormat,
                Image = NativeImage,
                Components = ComponentMapping.Identity,
                SubresourceRange = new ImageSubresourceRange
                {
                    BaseArrayLayer = (uint)arrayOrDepthSlice,
                    LayerCount = (uint)arrayCount,
                    BaseMipLevel = (uint)mipIndex,
                    LevelCount = (uint)mipCount,
                    AspectMask = ImageAspectFlags.Color
                }
            };

            if (IsMultiSample)
                throw new NotImplementedException();

            if (this.ArraySize > 1)
            {
                if (IsMultiSample && Dimension != TextureDimension.Texture2D)
                    throw new NotSupportedException("Multisample is only supported for 2D Textures");

                if (Dimension == TextureDimension.Texture3D)
                    throw new NotSupportedException("Texture Array is not supported for Texture3D");
            }
            else
            {
                if (IsMultiSample && Dimension != TextureDimension.Texture2D)
                    throw new NotSupportedException("Multisample is only supported for 2D RenderTarget Textures");

                if (Dimension == TextureDimension.TextureCube)
                    throw new NotSupportedException("TextureCube dimension is expecting an arraysize > 1");
            }

            return GraphicsDevice.NativeDevice.CreateImageView(ref createInfo);
        }

        private unsafe ImageView GetDepthStencilView(out bool hasStencil)
        {
            hasStencil = false;
            if (!IsDepthStencil)
                return ImageView.Null;

            Format nativeFormat;
            int pixelSize;
            bool compressed;
            VulkanConvertExtensions.ConvertPixelFormat(ViewFormat, out nativeFormat, out pixelSize, out compressed);

            // Check that the format is supported
            //if (ComputeShaderResourceFormatFromDepthFormat(ViewFormat) == PixelFormat.None)
            //    throw new NotSupportedException("Depth stencil format [{0}] not supported".ToFormat(ViewFormat));

            // Setup the HasStencil flag
            hasStencil = IsStencilFormat(ViewFormat);

            // Create a Depth stencil view on this texture2D
            var createInfo = new ImageViewCreateInfo
            {
                StructureType = StructureType.ImageViewCreateInfo,
                ViewType = ImageViewType.Image2D,
                Format = nativeFormat,
                Image = NativeImage,
                Components = ComponentMapping.Identity,
                SubresourceRange = new ImageSubresourceRange(ImageAspectFlags.Depth | (HasStencil ? ImageAspectFlags.Stencil : ImageAspectFlags.None), 0, 1, 0, 1)
            };

            //if (IsDepthStencilReadOnly)
            //{
            //    if (!IsDepthStencilReadOnlySupported(GraphicsDevice))
            //        throw new NotSupportedException("Cannot instantiate ReadOnly DepthStencilBuffer. Not supported on this device.");

            //    // Create a Depth stencil view on this texture2D
            //    createInfo.SubresourceRange.AspectMask =  ? ;
            //    if (HasStencil)
            //        createInfo.Flags |= (int)AttachmentViewCreateFlags.AttachmentViewCreateReadOnlyStencilBit;
            //}

            return GraphicsDevice.NativeDevice.CreateImageView(ref createInfo);
        }

        private bool IsFlipped()
        {
            return false;
        }
        
        internal static PixelFormat ComputeShaderResourceFormatFromDepthFormat(PixelFormat format)
        {
            return format;
        }
        
        /// <summary>
        /// Check and modify if necessary the mipmap levels of the image (Troubles with DXT images whose resolution in less than 4x4 in DX9.x).
        /// </summary>
        /// <param name="device">The graphics device.</param>
        /// <param name="description">The texture description.</param>
        /// <returns>The updated texture description.</returns>
        private static TextureDescription CheckMipLevels(GraphicsDevice device, ref TextureDescription description)
        {
            if (device.Features.CurrentProfile < GraphicsProfile.Level_10_0 && (description.Flags & TextureFlags.DepthStencil) == 0 && description.Format.IsCompressed())
            {
                description.MipLevels = Math.Min(CalculateMipCount(description.Width, description.Height), description.MipLevels);
            }
            return description;
        }

        /// <summary>
        /// Calculates the mip level from a specified size.
        /// </summary>
        /// <param name="size">The size.</param>
        /// <param name="minimumSizeLastMip">The minimum size of the last mip.</param>
        /// <returns>The mip level.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">Value must be > 0;size</exception>
        private static int CalculateMipCountFromSize(int size, int minimumSizeLastMip = 4)
        {
            if (size <= 0)
            {
                throw new ArgumentOutOfRangeException("Value must be > 0", "size");
            }

            if (minimumSizeLastMip <= 0)
            {
                throw new ArgumentOutOfRangeException("Value must be > 0", "minimumSizeLastMip");
            }

            int level = 1;
            while ((size / 2) >= minimumSizeLastMip)
            {
                size = Math.Max(1, size / 2);
                level++;
            }
            return level;
        }

        /// <summary>
        /// Calculates the mip level from a specified width,height,depth.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="minimumSizeLastMip">The minimum size of the last mip.</param>
        /// <returns>The mip level.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">Value must be &gt; 0;size</exception>
        private static int CalculateMipCount(int width, int height, int minimumSizeLastMip = 4)
        {
            return Math.Min(CalculateMipCountFromSize(width, minimumSizeLastMip), CalculateMipCountFromSize(height, minimumSizeLastMip));
        }


        internal static bool IsStencilFormat(PixelFormat format)
        {
            switch (format)
            {
                case PixelFormat.R24G8_Typeless:
                case PixelFormat.D24_UNorm_S8_UInt:
                case PixelFormat.R32G8X24_Typeless:
                case PixelFormat.D32_Float_S8X24_UInt:
                    return true;
            }

            return false;
        }
    }
}
#endif