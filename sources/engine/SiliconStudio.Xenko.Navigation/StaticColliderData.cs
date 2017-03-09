using SiliconStudio.Xenko.Physics;

namespace SiliconStudio.Xenko.Navigation
{
    public class StaticColliderData
    {
        public StaticColliderComponent Component;
        internal int ParameterHash = 0;
        internal bool Processed = false;
        internal NavigationMeshInputBuilder InputBuilder;
        internal NavigationMeshCachedBuildObject Previous;
    }
}