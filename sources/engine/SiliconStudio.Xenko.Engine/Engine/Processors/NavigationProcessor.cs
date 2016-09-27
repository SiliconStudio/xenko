using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SiliconStudio.Xenko.Native;

namespace SiliconStudio.Xenko.Engine.Processors
{
    public class NavigationAssociatedData : IDisposable
    {
        IntPtr nativeNavmesh;

        public NavigationAssociatedData(NavigationMesh mesh)
        {
            if(mesh.NavmeshData == null)
                throw new ArgumentNullException("Navigation Mesh was null");
            GCHandle pinnedArray = GCHandle.Alloc(mesh.NavmeshData, GCHandleType.Pinned);
            IntPtr pointer = pinnedArray.AddrOfPinnedObject();
            nativeNavmesh = Navigation.LoadNavmesh(pointer, mesh.NavmeshData.Length);
            if(nativeNavmesh == null)
                throw new Exception("Failed to load navigation mesh");
            pinnedArray.Free();
        }
        public void Dispose()
        {
            Navigation.DestroyNavmesh(nativeNavmesh);
            nativeNavmesh = IntPtr.Zero;
        }
    }

    public class NavigationProcessor : EntityProcessor<NavigationComponent, NavigationAssociatedData>
    {
        /// <summary>
        /// Maps navigation meshed to their natively loaded counterparts
        /// </summary>
        private Dictionary<NavigationMesh, NavigationAssociatedData> loadedNavigationMeshes = new Dictionary<NavigationMesh, NavigationAssociatedData>();

        protected override NavigationAssociatedData GenerateComponentData(Entity entity, NavigationComponent component)
        {
            NavigationAssociatedData data;
            if (loadedNavigationMeshes.TryGetValue(component.NavigationMesh, out data))
                return data;
            data = new NavigationAssociatedData(component.NavigationMesh);
            loadedNavigationMeshes.Add(component.NavigationMesh, data);
            return data;
        }
    }
}
