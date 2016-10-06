// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine.Design;
using SiliconStudio.Xenko.Engine.Processors;
using SiliconStudio.Xenko.Native;

namespace SiliconStudio.Xenko.Engine
{
    /// <summary>
    /// Result for a raycast query on a navigation mesh
    /// </summary>
    public struct NavigationRaycastResult
    {
        /// <summary>
        /// true if the raycast hit something
        /// </summary>
        public bool Hit;
        public Vector3 Position;
        public Vector3 Normal;
    }

    /// <summary>
    /// Provides advanced settings to be passed to navigation mesh queries
    /// </summary>
    public struct NavigationQuerySettings
    {
        /// <summary>
        /// Used as the extend for the find nearest poly bounding box used when scanning for a polygon corresponding to the given starting/ending position
        /// Making this bigger will allow you to find paths that allow the entity to start further away or higher from the navigation mesh bounds for example
        /// </summary>
        public Vector3 FindNearestPolyExtent;

        /// <summary>
        /// The maximum number of path points used internally and also the maximum number of output points
        /// </summary>
        public int MaxPathPoints;

        public static readonly NavigationQuerySettings Default = new NavigationQuerySettings
        {
            FindNearestPolyExtent = new Vector3(2.0f, 4.0f, 2.0f),
            MaxPathPoints = 1024
        };
    }

    [DataContract("NavigationComponent")]
    [Display("Navigation", Expand = ExpandRule.Once)]
    [ComponentOrder(20000)]
    [DefaultEntityComponentProcessor(typeof(NavigationProcessor))]
    public class NavigationComponent : EntityComponent
    {
        [DataMember(10)]
        public NavigationMesh NavigationMesh { get; set; }

        /// <summary>
        /// The layer of the navigation mesh to use, corresponds to the agent setting with the same index
        /// </summary>
        [DataMember(20)]
        public int NavigationMeshLayer { get; set; }

        [DataMemberIgnore]
        internal IntPtr nativeNavmesh;

        public Vector3 DebugTarget = Vector3.Zero;

        /// <summary>
        /// Finds a path from the entity's current location to <see cref="end"/>
        /// </summary>
        /// <param name="end"></param>
        /// <param name="querySettings">Advanced settings to be provided to the navigation mesh query</param>
        /// <returns>The found path points or null</returns>
        public Vector3[] FindPath(Vector3 end, NavigationQuerySettings? querySettings = null)
        {
            return FindPath(Entity.Transform.WorldMatrix.TranslationVector, end, querySettings);
        }

        /// <summary>
        /// Finds a path from point <see cref="start"/> to <see cref="end"/>
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="querySettings">Advanced settings to be provided to the navigation mesh query</param>
        /// <returns>The found path points or null</returns>
        public unsafe Vector3[] FindPath(Vector3 start, Vector3 end, NavigationQuerySettings? querySettings = null)
        {
            if(!querySettings.HasValue)
                querySettings = NavigationQuerySettings.Default;
            if (nativeNavmesh == IntPtr.Zero)
                return null;

            Navigation.PathFindQuery query;
            query.Source = start;
            query.Target = end;
            query.MaxPathPoints = querySettings.Value.MaxPathPoints;
            query.FindNearestPolyExtent = querySettings.Value.FindNearestPolyExtent;
            Navigation.PathFindResult* queryResult = (Navigation.PathFindResult*)Navigation.DoPathFindQuery(nativeNavmesh, query);
            if(!queryResult->PathFound)
                return null;
            
            Vector3[] ret = new Vector3[queryResult->NumPathPoints];
            // Unsafe copy
            Vector3* points = (Vector3*)queryResult->PathPoints;
            for(int i = 0; i < queryResult->NumPathPoints; i++)
            {
                ret[i] = points[i];
            }
            return ret;
        }

        /// <summary>
        /// Performs a raycast on the navigation mesh to perform line of sight or similar checks
        /// </summary>
        /// <param name="start">Starting point</param>
        /// <param name="end">Ending point</param>
        /// <param name="querySettings">Advanced settings to be provided to the navigation mesh query</param>
        /// <returns>The found raycast hit if <see cref="NavigationRaycastResult.Hit"/> is true</returns>
        public unsafe NavigationRaycastResult Raycast(Vector3 start, Vector3 end, NavigationQuerySettings? querySettings = null)
        {
            NavigationRaycastResult result = new NavigationRaycastResult { Hit = false };

            if (!querySettings.HasValue)
                querySettings = NavigationQuerySettings.Default;
            if (nativeNavmesh == IntPtr.Zero)
                return result;


            Navigation.RaycastQuery query;
            query.Source = start;
            query.Target = end;
            query.MaxPathPoints = querySettings.Value.MaxPathPoints;
            query.FindNearestPolyExtent = querySettings.Value.FindNearestPolyExtent;
            Navigation.RaycastResult* queryResult = (Navigation.RaycastResult*)Navigation.DoRaycastQuery(nativeNavmesh, query);
            if (!queryResult->Hit)
                return result;

            result.Hit = true;
            result.Position = queryResult->Position;
            result.Normal = queryResult->Normal;
            return result;
        }
    }
}
