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
        public struct BuildSettings
        {
            public BoundingBox BoundingBox;
            public float CellHeight;
            public float CellSize;
            public int TileSize;
            public Point TilePosition;
        }
        
        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct AgentSettings
        {
            public float Height;
            public float Radius;
            public float MaxClimb;
            public float MaxSlope;
        }

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

        /// <summary>
        /// Creates a new navigation mesh object
        /// You must add tiles to it with AddTile before you can perform navigation queries using Query
        /// </summary>
        /// <returns></returns>
        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeInvoke.Library, EntryPoint = "xnNavigationCreateNavmesh", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr CreateNavmesh(float cellTileSize);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeInvoke.Library, EntryPoint = "xnNavigationDestroyNavmesh", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr DestroyNavmesh(IntPtr query);

        /// <summary>
        /// Adds a new tile to the navigation mesh object
        /// </summary>
        /// <param name="navmesh"></param>
        /// <param name="tileCoordinate">Coordinate of the tile to add</param>
        /// <param name="data">Navigation mesh binary data in the detour format to load</param>
        /// <param name="dataLength">Length of the binary mesh data</param>
        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeInvoke.Library, EntryPoint = "xnNavigationAddTile", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool AddTile(IntPtr navmesh, Point tileCoordinate, IntPtr data, int dataLength);

        /// <summary>
        /// Removes a tile from the navigation mesh object
        /// </summary>
        /// <param name="navmesh"></param>
        /// <param name="tileCoordinate">Coordinate of the tile to remove</param>
        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeInvoke.Library, EntryPoint = "xnNavigationRemoveTile", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool RemoveTile(IntPtr navmesh, Point tileCoordinate);

        /// <summary>
        /// Perform a navigation query on the navigation mesh
        /// </summary>
        /// <param name="query"></param>
        /// <param name="navigationQuery">The query to perform</param>
        /// <returns>A NavigationQueryResult</returns>
        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeInvoke.Library, EntryPoint = "xnNavigationQuery", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Query(IntPtr query, NavigationQuery navigationQuery);
    }
}