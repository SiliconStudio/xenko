// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Native
{
    public class Navigation
    {
        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct GeneratedData
        {
            public bool Success;
            public IntPtr NavmeshVertices;
            public int NumNavmeshVertices;
            public IntPtr NavmeshData;
            public int NavmeshDataLength;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct NavigationQuery
        {
            public Vector3 Source;
            public Vector3 Target;
        };

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct NavigationQueryResult
        {
            public bool PathFound;
            public IntPtr PathPoints;
            public int NumPathPoints;
        };

        static Navigation()
        {
            NativeInvoke.PreLoad();
        }

        // Navmesh generation API
        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeInvoke.Library, EntryPoint = "xnNavigationCreateBuilder", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr CreateBuilder();

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeInvoke.Library, EntryPoint = "xnNavigationDestroyBuilder", CallingConvention = CallingConvention.Cdecl)]
        public static extern void DestroyBuilder(IntPtr builder);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeInvoke.Library, EntryPoint = "xnNavigationBuildNavmesh", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Build(IntPtr builder,
            [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] Vector3[] verts, int numVerts,
            [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 4)] int[] inds, int numInds);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeInvoke.Library, EntryPoint = "xnNavigationSetSettings", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetSettings(IntPtr builder, IntPtr settings);

        // Must be called after SetSettings to set agent specific build settings
        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeInvoke.Library, EntryPoint = "xnNavigationSetAgentSettings", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetAgentSettings(IntPtr builder, IntPtr settings);

        // Navmesh Query API
        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeInvoke.Library, EntryPoint = "xnNavigationLoadNavmesh", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr LoadNavmesh(IntPtr data, int dataLength);


        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeInvoke.Library, EntryPoint = "xnNavigationDestroyNavmesh", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr DestroyNavmesh(IntPtr navmesh);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeInvoke.Library, EntryPoint = "xnNavigationQuery", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Query(IntPtr navmesh, NavigationQuery query);
    }
}