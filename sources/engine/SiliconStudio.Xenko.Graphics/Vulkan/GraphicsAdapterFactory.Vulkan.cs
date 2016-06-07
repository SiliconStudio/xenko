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
using SiliconStudio.Core.Collections;

namespace SiliconStudio.Xenko.Graphics
{
    public static partial class GraphicsAdapterFactory
    {
        internal static Instance NativeInstance;

        /// <summary>
        /// Initializes all adapters with the specified factory.
        /// </summary>
        internal static unsafe void InitializeInternal()
        {
            staticCollector.Dispose();

            var applicationInfo = new ApplicationInfo
            {
                StructureType = StructureType.ApplicationInfo,
                ApiVersion = new SharpVulkan.Version(1, 0, 0),
                EngineName = Marshal.StringToHGlobalAnsi("Xenko"),
                //EngineVersion = new SharpVulkan.Version()
            };

            var desiredLayerNames = new[]
            {
                //"VK_LAYER_LUNARG_standard_validation",
                "VK_LAYER_GOOGLE_threading",
                "VK_LAYER_LUNARG_parameter_validation",
                "VK_LAYER_LUNARG_device_limits",
                "VK_LAYER_LUNARG_object_tracker",
                "VK_LAYER_LUNARG_image",
                "VK_LAYER_LUNARG_core_validation",
                "VK_LAYER_LUNARG_swapchain",
                "VK_LAYER_GOOGLE_unique_objects",
                //"VK_LAYER_LUNARG_api_dump",
                //"VK_LAYER_LUNARG_vktrace"
            };

            IntPtr[] enabledLayerNames = new IntPtr[0];

            //if (false)
            {
                var layers = Vulkan.InstanceLayerProperties;
                var availableLayerNames = new HashSet<string>();

                for (int index = 0; index < layers.Length; index++)
                {
                    var properties = layers[index];
                    var namePointer = new IntPtr(Interop.Fixed(ref properties.LayerName));
                    var name = Marshal.PtrToStringAnsi(namePointer);

                    availableLayerNames.Add(name);
                }

                enabledLayerNames = desiredLayerNames
                    .Where(x => availableLayerNames.Contains(x))
                    .Select(Marshal.StringToHGlobalAnsi).ToArray();
            }

            var enabledExtensionNames = new[]
            {
                Marshal.StringToHGlobalAnsi("VK_EXT_debug_report"),

                Marshal.StringToHGlobalAnsi("VK_KHR_surface"),
#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP
                Marshal.StringToHGlobalAnsi("VK_KHR_win32_surface"),
#elif SILICONSTUDIO_PLATFORM_ANDROID
                Marshal.StringToHGlobalAnsi("VK_KHR_android_surface"),
#elif SILICONSTUDIO_PLATFORM_LINUX
                Marshal.StringToHGlobalAnsi("VK_KHR_xcb_surface"),
#endif
            };

            var createDebugReportCallbackName = Marshal.StringToHGlobalAnsi("vkCreateDebugReportCallbackEXT");

            try
            {
                fixed (void* enabledExtensionNamesPointer = &enabledExtensionNames[0])
                {
                    var insatanceCreateInfo = new InstanceCreateInfo
                    {
                        StructureType = StructureType.InstanceCreateInfo,
                        ApplicationInfo = new IntPtr(&applicationInfo),
                        EnabledLayerCount = enabledLayerNames != null ? (uint)enabledLayerNames.Length : 0,
                        EnabledLayerNames = enabledLayerNames?.Length > 0 ? new IntPtr(Interop.Fixed(enabledLayerNames)) : IntPtr.Zero,
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
                    Flags = (uint)(DebugReportFlags.Error | DebugReportFlags.Warning/* | DebugReportFlags.PerformanceWarning | DebugReportFlags.Information | DebugReportFlags.Debug*/),
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
