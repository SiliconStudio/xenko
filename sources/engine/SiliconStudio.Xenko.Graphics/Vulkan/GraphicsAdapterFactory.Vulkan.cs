// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_XENKO_GRAPHICS_API_VULKAN
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
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
                EngineName = Marshal.StringToHGlobalAnsi("Xenko"),
                //EngineVersion = new SharpVulkan.Version()
            };

            var enabledLayerNames = new[]
            {
                Marshal.StringToHGlobalAnsi("VK_LAYER_GOOGLE_threading"),
                Marshal.StringToHGlobalAnsi("VK_LAYER_LUNARG_param_checker"),
                Marshal.StringToHGlobalAnsi("VK_LAYER_LUNARG_device_limits"),
                Marshal.StringToHGlobalAnsi("VK_LAYER_LUNARG_object_tracker"),
                Marshal.StringToHGlobalAnsi("VK_LAYER_LUNARG_image"),
                Marshal.StringToHGlobalAnsi("VK_LAYER_LUNARG_mem_tracker"),
                Marshal.StringToHGlobalAnsi("VK_LAYER_LUNARG_draw_state"),
                Marshal.StringToHGlobalAnsi("VK_LAYER_LUNARG_swapchain"),
                //Marshal.StringToHGlobalAnsi("VK_LAYER_GOOGLE_unique_objects"), // Fails on swapchain creation?

                //Marshal.StringToHGlobalAnsi("VK_LAYER_LUNARG_api_dump"),
                //Marshal.StringToHGlobalAnsi("VK_LAYER_LUNARG_vktrace"),
            };

            var enabledExtensionNames = new[]
            {
                Marshal.StringToHGlobalAnsi("VK_EXT_debug_report"),

                Marshal.StringToHGlobalAnsi("VK_KHR_surface"),
#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP
                Marshal.StringToHGlobalAnsi("VK_KHR_win32_surface"),
#elif SILICONSTUDIO_PLATFORM_ANDROID
                Marshal.StringToHGlobalAnsi("VK_KHR_android_surface"),
#elif SILICONSTUDIO_PLATFORM_LINUX
                Marshal.StringToHGlobalAnsi("VK_KHR_xlib_surface"),
#endif
            };

            var createDebugReportCallbackName = Marshal.StringToHGlobalAnsi("vkCreateDebugReportCallbackEXT");

            try
            {
                fixed (void* enabledLayerNamesPointer = &enabledLayerNames[0])
                fixed (void* enabledExtensionNamesPointer = &enabledExtensionNames[0])
                {

                    var insatanceCreateInfo = new InstanceCreateInfo
                    {
                        StructureType = StructureType.InstanceCreateInfo,
                        ApplicationInfo = new IntPtr(&applicationInfo),
                        EnabledLayerCount = (uint)enabledLayerNames.Length,
                        EnabledLayerNames = new IntPtr(enabledLayerNamesPointer),
                        EnabledExtensionCount = (uint)enabledExtensionNames.Length,
                        EnabledExtensionNames = new IntPtr(enabledExtensionNamesPointer)
                    };

                    NativeInstance = Vulkan.CreateInstance(ref insatanceCreateInfo);
                }

                var createDebugReportCallback = (CreateDebugReportCallbackDelegate)Marshal.GetDelegateForFunctionPointer(NativeInstance.GetProcAddress((byte*)createDebugReportCallbackName), typeof(CreateDebugReportCallbackDelegate));

                DebugReportCallback callback;
                debugReport = DebugReport;
                var createInfo = new DebugReportCallbackCreateInfo
                {
                    StructureType = StructureType.DebugReportCallbackCreateInfo,
                    Flags = (uint)(DebugReportFlags.Error | DebugReportFlags.Warning | DebugReportFlags.PerformanceWarning/* | DebugReportFlags.Information | DebugReportFlags.Debug*/),
                    Callback = Marshal.GetFunctionPointerForDelegate(debugReport)
                };
                createDebugReportCallback(NativeInstance, ref createInfo, null, out callback);
            }
            finally
            {
                foreach (var enabledExtensionName in enabledExtensionNames)
                {
                    Marshal.FreeHGlobal(enabledExtensionName);
                }

                foreach (var enabledLayerName in enabledLayerNames)
                {
                    Marshal.FreeHGlobal(enabledLayerName);
                }

                Marshal.FreeHGlobal(applicationInfo.EngineName);
                Marshal.FreeHGlobal(createDebugReportCallbackName);
            }

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

        private static DebugReportCallbackDelegate debugReport;

        private static RawBool DebugReport(DebugReportFlags flags, DebugReportObjectType objectType, ulong @object, PointerSize location, int messageCode, string layerPrefix, string message, IntPtr userData)
        {
            Debug.WriteLine($"{flags}: {message} ([{messageCode}] {layerPrefix})");
            return true;
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

        struct DebugReportCallback
        {
            internal ulong InternalHandle;
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        private unsafe delegate RawBool DebugReportCallbackDelegate(DebugReportFlags flags, DebugReportObjectType objectType, ulong @object, PointerSize location, int messageCode, string layerPrefix, string message, IntPtr userData);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private unsafe delegate Result CreateDebugReportCallbackDelegate(Instance instance, ref DebugReportCallbackCreateInfo createInfo, AllocationCallbacks* allocator, out DebugReportCallback callback);

    }
}
#endif 
