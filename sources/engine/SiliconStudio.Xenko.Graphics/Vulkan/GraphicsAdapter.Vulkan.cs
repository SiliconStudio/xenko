// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_XENKO_GRAPHICS_API_VULKAN
// Copyright (c) 2010-2014 SharpDX - Alexandre Mutel
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
using System.Collections.Generic;
using System.Resources;
using System.Runtime.InteropServices;
using System.Text;
using SharpVulkan;
using SiliconStudio.Core;

using ComponentBase = SiliconStudio.Core.ComponentBase;
using Utilities = SiliconStudio.Core.Utilities;

namespace SiliconStudio.Xenko.Graphics
{
    /// <summary>
    /// Provides methods to retrieve and manipulate graphics adapters. This is the equivalent to <see cref="Adapter1"/>.
    /// </summary>
    /// <msdn-id>ff471329</msdn-id>	
    /// <unmanaged>IDXGIAdapter1</unmanaged>	
    /// <unmanaged-short>IDXGIAdapter1</unmanaged-short>	
    public partial class GraphicsAdapter
    {
        private PhysicalDevice defaultPhysicalDevice;
        private PhysicalDevice debugPhysicalDevice;

        private readonly int adapterOrdinal;
        private readonly PhysicalDeviceProperties properties;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphicsAdapter" /> class.
        /// </summary>
        /// <param name="physicalDevice">The default factory.</param>
        /// <param name="adapterOrdinal">The adapter ordinal.</param>
        internal GraphicsAdapter(PhysicalDevice defaultPhysicalDevice, int adapterOrdinal)
        {
            this.adapterOrdinal = adapterOrdinal;
            this.defaultPhysicalDevice = defaultPhysicalDevice;

            defaultPhysicalDevice.GetProperties(out properties);

            // TODO VULKAN
            //var displayProperties = physicalDevice.DisplayProperties;
            //outputs = new GraphicsOutput[displayProperties.Length];
            //for (var i = 0; i < outputs.Length; i++)
            //    outputs[i] = new GraphicsOutput(this, displayProperties[i], i).DisposeBy(this);
            outputs = new[] { new GraphicsOutput() };
        }

        /// <summary>
        /// Gets the description of this adapter.
        /// </summary>
        /// <value>The description.</value>
        public unsafe string Description
        {
            get
            {
                // TODO VULKAN api
                var propertiesCopy = properties;
                return Marshal.PtrToStringAnsi(new IntPtr(&propertiesCopy.DeviceName));
            }
        }

        /// <summary>
        /// Gets or sets the vendor identifier.
        /// </summary>
        /// <value>
        /// The vendor identifier.
        /// </value>
        public int VendorId
        {
            get
            {
                return (int)properties.VendorId;
            }
        }

        /// <summary>
        /// Determines if this instance of GraphicsAdapter is the default adapter.
        /// </summary>
        public bool IsDefaultAdapter
        {
            get
            {
                return adapterOrdinal == 0;
            }
        }

        internal PhysicalDevice GetPhysicalDevice(bool enableValidation)
        {
            if (enableValidation)
            {
                if (debugPhysicalDevice == PhysicalDevice.Null)
                {
                    debugPhysicalDevice = GraphicsAdapterFactory.GetInstance(true).NativeInstance.PhysicalDevices[adapterOrdinal];
                }

                return debugPhysicalDevice;
            }
            else
            {
                return defaultPhysicalDevice;
            }
        }

        /// <summary>
        /// Tests to see if the adapter supports the requested profile.
        /// </summary>
        /// <param name="graphicsProfile">The graphics profile.</param>
        /// <returns>true if the profile is supported</returns>
        public bool IsProfileSupported(GraphicsProfile graphicsProfile)
        {
            // TODO VULKAN
            return true;
            //return SharpDX.Direct3D11.Device.IsSupportedFeatureLevel(this.NativeAdapter, (SharpDX.Direct3D.FeatureLevel)graphicsProfile);
        }
    }
} 
#endif