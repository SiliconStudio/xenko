using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SiliconStudio.Core;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Engine.Design;
using SiliconStudio.Xenko.Engine.Processors;
using SiliconStudio.Xenko.Native;

namespace SiliconStudio.Xenko.Engine
{
    [DataContract("NavigationComponent")]
    [Display("Navigation", Expand = ExpandRule.Once)]
    [ComponentOrder(20000)]
    public class NavigationComponent : EntityComponent
    {
        [DataMember(10)]
        public Navmesh Navmesh
        {
            get { return currentNavmesh; }
            set { SetNavmesh(value); }
        }

        private Navmesh currentNavmesh;
        private IntPtr navigationQuery = IntPtr.Zero;

        ~NavigationComponent()
        {
            if (navigationQuery != IntPtr.Zero)
            {
                Navigation.DestroyNavmesh(navigationQuery);
            }
            navigationQuery = IntPtr.Zero;
        }

        private void SetNavmesh(Navmesh value)
        {
            currentNavmesh = value;
            if(navigationQuery != IntPtr.Zero)
            {
                Navigation.DestroyNavmesh(navigationQuery);
            }
            navigationQuery = IntPtr.Zero;
            EnsureNavmeshInitialized();
        }
        private bool EnsureNavmeshInitialized()
        {
            if(navigationQuery == IntPtr.Zero)
            {
                if (currentNavmesh != null && currentNavmesh.NavmeshData != null)
                {
                    // Load navmesh from raw data to create navigation object
                    bool loaded = false;
                    GCHandle pinnedArray = GCHandle.Alloc(Navmesh.NavmeshData, GCHandleType.Pinned);
                    IntPtr pointer = pinnedArray.AddrOfPinnedObject();
                    navigationQuery = Navigation.LoadNavmesh(pointer, currentNavmesh.NavmeshData.Length);
                    pinnedArray.Free();
                }
            }
            return navigationQuery != IntPtr.Zero;
        }
        
        public Vector3[] FindPath(Vector3 end)
        {
            if (!EnsureNavmeshInitialized())
                return null;

            Navigation.NavmeshQuery query;
            query.Source = Entity.Transform.WorldMatrix.TranslationVector;
            query.Target = end;
            unsafe
            {
                Navigation.NavmeshQueryResult* queryResult = (Navigation.NavmeshQueryResult*)Navigation.Query(navigationQuery, query);
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
