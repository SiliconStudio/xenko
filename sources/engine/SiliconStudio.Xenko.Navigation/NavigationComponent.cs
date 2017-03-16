// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Engine.Design;
using SiliconStudio.Xenko.Navigation.Processors;

namespace SiliconStudio.Xenko.Navigation
{
    /// <summary>
    /// This is used to interface with the navigation mesh. Supports TryFindPath and Raycast
    /// </summary>
    [DataContract("NavigationComponent")]
    [Display("Navigation", Expand = ExpandRule.Once)]
    [ComponentOrder(20000)]
    [DefaultEntityComponentProcessor(typeof(NavigationProcessor), ExecutionMode = ExecutionMode.Runtime)]
    public class NavigationComponent : EntityComponent
    {
        [DataMemberIgnore] internal IntPtr NavigationMeshInternal;
        
        /// <summary>
        /// The navigation mesh which is being used
        /// </summary>
        [DataMember(10)]
        public NavigationMesh NavigationMesh { get; set; }

        /// <summary>
        /// The layer of the navigation mesh to use, corresponds to the agent setting with the same index (specified during build)
        /// </summary>
        [DataMember(20)]
        public int NavigationMeshLayer { get; set; }

        /// <summary>
        /// Finds a path from the entity's current location to <see cref="end"/>
        /// </summary>
        /// <param name="end">The ending location of the pathfinding query</param>
        /// <param name="path">The waypoints for the found path, if any (at least 2 if a path was found)</param>
        /// <returns>The found path points or null</returns>
        public bool TryFindPath(Vector3 end, ICollection<Vector3> path)
        {
            return TryFindPath(Entity.Transform.WorldMatrix.TranslationVector, end, path, NavigationQuerySettings.Default);
        }

        /// <summary>
        /// Finds a path from the entity's current location to <see cref="end"/>
        /// </summary>
        /// <param name="end">The ending location of the pathfinding query</param>
        /// <param name="querySettings">Advanced settings to be provided to the navigation mesh query</param>
        /// <param name="path">The waypoints for the found path, if any (at least 2 if a path was found)</param>
        /// <returns>The found path points or null</returns>
        public bool TryFindPath(Vector3 end, ICollection<Vector3> path, NavigationQuerySettings querySettings)
        {
            return TryFindPath(Entity.Transform.WorldMatrix.TranslationVector, end, path, querySettings);
        }

        /// <summary>
        /// Finds a path from point <see cref="start"/> to <see cref="end"/>
        /// </summary>
        /// <param name="start">The starting location of the pathfinding query</param>
        /// <param name="end">The ending location of the pathfinding query</param>
        /// <param name="path">The waypoints for the found path, if any (at least 2 if a path was found)</param>
        /// <returns>The found path points or null</returns>
        public bool TryFindPath(Vector3 start, Vector3 end, ICollection<Vector3> path)
        {
            return TryFindPath(start, end, path, NavigationQuerySettings.Default);
        }

        /// <summary>
        /// Finds a path from point <see cref="start"/> to <see cref="end"/>
        /// </summary>
        /// <param name="start">The starting location of the pathfinding query</param>
        /// <param name="end">The ending location of the pathfinding query</param>
        /// <param name="querySettings">Advanced settings to be provided to the navigation mesh query</param>
        /// <param name="path">The waypoints for the found path, if any (at least 2 if a path was found)</param>
        /// <returns>The found path points or null</returns>
        public unsafe bool TryFindPath(Vector3 start, Vector3 end, ICollection<Vector3> path, NavigationQuerySettings querySettings)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));
            if (NavigationMeshInternal == IntPtr.Zero)
                return false;

            Navigation.PathFindQuery query;
            query.Source = start;
            query.Target = end;
            query.MaxPathPoints = querySettings.MaxPathPoints;
            query.FindNearestPolyExtent = querySettings.FindNearestPolyExtent;
            Navigation.PathFindResult queryResult;
            Vector3[] generatedPathPoints = new Vector3[querySettings.MaxPathPoints];
            fixed (Vector3* generatedPathPointsPtr = generatedPathPoints)
            {
                queryResult.PathPoints = new IntPtr(generatedPathPointsPtr);
                Navigation.DoPathFindQuery(NavigationMeshInternal, query, new IntPtr(&queryResult));
                if (!queryResult.PathFound)
                    return false;
            }

            // Read path from unsafe result
            Vector3* points = (Vector3*)queryResult.PathPoints;
            for (int i = 0; i < queryResult.NumPathPoints; i++)
            {
                path.Add(points[i]);
            }
            return true;
        }

        /// <summary>
        /// Performs a raycast on the navigation mesh to perform line of sight or similar checks. Starts from the entity's current world position
        /// </summary>
        /// <param name="end">Ending point</param>
        /// <returns>The found raycast hit if <see cref="NavigationRaycastResult.Hit"/> is true</returns>
        public NavigationRaycastResult Raycast(Vector3 end)
        {
            return Raycast(Entity.Transform.WorldMatrix.TranslationVector, end, NavigationQuerySettings.Default);
        }

        /// <summary>
        /// Performs a raycast on the navigation mesh to perform line of sight or similar checks.  Starts from the entity's current world position
        /// </summary>
        /// <param name="end">Ending point</param>
        /// <param name="querySettings">Advanced settings to be provided to the navigation mesh query</param>
        /// <returns>The found raycast hit if <see cref="NavigationRaycastResult.Hit"/> is true</returns>
        public NavigationRaycastResult Raycast(Vector3 end, NavigationQuerySettings querySettings)
        {
            return Raycast(Entity.Transform.WorldMatrix.TranslationVector, end, querySettings);
        }

        /// <summary>
        /// Performs a raycast on the navigation mesh to perform line of sight or similar checks.
        /// </summary>
        /// <param name="start">Starting point</param>
        /// <param name="end">Ending point</param>
        /// <returns>The found raycast hit if <see cref="NavigationRaycastResult.Hit"/> is true</returns>
        public NavigationRaycastResult Raycast(Vector3 start, Vector3 end)
        {
            return Raycast(start, end, NavigationQuerySettings.Default);
        }

        /// <summary>
        /// Performs a raycast on the navigation mesh to perform line of sight or similar checks
        /// </summary>
        /// <param name="start">Starting point</param>
        /// <param name="end">Ending point</param>
        /// <param name="querySettings">Advanced settings to be provided to the navigation mesh query</param>
        /// <returns>The found raycast hit if <see cref="NavigationRaycastResult.Hit"/> is true</returns>
        public unsafe NavigationRaycastResult Raycast(Vector3 start, Vector3 end, NavigationQuerySettings querySettings)
        {
            NavigationRaycastResult result = new NavigationRaycastResult { Hit = false };

            if (NavigationMeshInternal == IntPtr.Zero)
                return result;

            Navigation.RaycastQuery query;
            query.Source = start;
            query.Target = end;
            query.MaxPathPoints = querySettings.MaxPathPoints;
            query.FindNearestPolyExtent = querySettings.FindNearestPolyExtent;
            Navigation.RaycastResult queryResult;
            Navigation.DoRaycastQuery(NavigationMeshInternal, query, new IntPtr(&queryResult));
            if (!queryResult.Hit)
                return result;

            result.Hit = true;
            result.Position = queryResult.Position;
            result.Normal = queryResult.Normal;
            return result;
        }
    }
}