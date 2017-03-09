namespace SiliconStudio.Assets.Analysis
{
    public struct BuildNodeDesc
    {
        public AssetId AssetId;
        public BuildDependencyType BuildDependencyType;

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            var other = (BuildNodeDesc)obj;
            return AssetId == other.AssetId && BuildDependencyType == other.BuildDependencyType;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = (int)2166136261;
                hash = (hash * 16777619) ^ AssetId.GetHashCode();
                hash = (hash * 16777619) ^ BuildDependencyType.GetHashCode();
                return hash;
            }
        }

        public static bool operator ==(BuildNodeDesc x, BuildNodeDesc y)
        {
            return x.AssetId == y.AssetId && x.BuildDependencyType == y.BuildDependencyType;
        }

        public static bool operator !=(BuildNodeDesc x, BuildNodeDesc y)
        {
            return x.AssetId != y.AssetId || x.BuildDependencyType != y.BuildDependencyType;
        }
    }
}