// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_XENKO_GRAPHICS_API_VULKAN
using System;
using System.Collections.Generic;
using SharpVulkan;
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Graphics
{
    public static partial class GraphicsAdapterFactory
    {
        internal static Instance NativeInstance;

        /// <summary>
        /// Initializes all adapters with the specified factory.
        /// </summary>
        internal unsafe static void InitializeInternal()
        {
            staticCollector.Dispose();

            var applicationInfo = new ApplicationInfo
            {
                StructureType = StructureType.ApplicationInfo,
                ApiVersion = Vulkan.ApiVersion,
            };

            var insatanceCreateInfo = new InstanceCreateInfo
            {
                StructureType = StructureType.InstanceCreateInfo,
                ApplicationInfo = new IntPtr(&applicationInfo),
            };

            NativeInstance = Vulkan.CreateInstance(ref insatanceCreateInfo);

            staticCollector.Add(new AnonymousDisposable(() => NativeInstance.Destroy()));

            var nativePhysicalDevices = NativeInstance.PhysicalDevices;
            var adapterList = new List<GraphicsAdapter>();
            for (int i = 0; i < nativePhysicalDevices.Length; i++)
            {
                var adapter = new GraphicsAdapter(nativePhysicalDevices[i], i);
                staticCollector.Add(adapter);
                adapterList.Add(adapter);
            }

            defaultAdapter = adapterList.Count > 0 ? adapterList[0] : null;
            adapters = adapterList.ToArray();
        }

        /// <summary>
        /// Gets the <see cref="Factory1"/> used by all GraphicsAdapter.
        /// </summary>
        internal static Instance Instance
        {
            get
            {
                lock (StaticLock)
                {
                    Initialize();
                    return NativeInstance;
                }
            }
        }
    }
}
#endif 
