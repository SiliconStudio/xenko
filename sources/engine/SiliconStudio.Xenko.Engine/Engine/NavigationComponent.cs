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
    [DataContract("NavigationComponent")]
    [Display("Navigation", Expand = ExpandRule.Once)]
    [ComponentOrder(20000)]
    [DefaultEntityComponentProcessor(typeof(NavigationProcessor))]
    public class NavigationComponent : EntityComponent
    {
        [DataMember(10)]
        public NavigationMesh NavigationMesh { get; set; }

        [DataMemberIgnore]
        internal IntPtr nativeNavmesh;

        // TODO: Move this to a game system
        public Vector3[] FindPath(Vector3 end)
        {
            if(nativeNavmesh == IntPtr.Zero)
                return null;

            Navigation.NavigationQuery query;
            query.Source = Entity.Transform.WorldMatrix.TranslationVector;
            query.Target = end;
            unsafe
            {
                Navigation.NavigationQueryResult* queryResult = (Navigation.NavigationQueryResult*)Navigation.Query(nativeNavmesh, query);
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
        }
    }
}
