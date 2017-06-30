using System;
using SiliconStudio.Assets.Compiler;

namespace SiliconStudio.Assets.Analysis
{
    public struct BuildDependencyInfo : IEquatable<BuildDependencyInfo>
    {
        public readonly Type CompilationContext;
        public readonly Type AssetType;
        public readonly BuildDependencyType DependencyType;

        public BuildDependencyInfo(Type assetType, Type compilationContext, BuildDependencyType dependencyType)
        {
            if (!typeof(Asset).IsAssignableFrom(assetType)) throw new ArgumentException($@"{nameof(assetType)} should inherit from Asset", nameof(assetType));
            if (!typeof(ICompilationContext).IsAssignableFrom(compilationContext)) throw new ArgumentException($@"{nameof(compilationContext)} should inherit from ICompilationContext", nameof(compilationContext));
            AssetType = assetType;
            CompilationContext = compilationContext;
            DependencyType = dependencyType;
        }

        public bool Equals(BuildDependencyInfo other)
        {
            return ReferenceEquals(CompilationContext, other.CompilationContext) && ReferenceEquals(AssetType, other.AssetType) && DependencyType == other.DependencyType;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            return obj is BuildDependencyInfo && Equals((BuildDependencyInfo)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (CompilationContext != null ? CompilationContext.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (AssetType != null ? AssetType.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int)DependencyType;
                return hashCode;
            }
        }

        public static bool operator ==(BuildDependencyInfo left, BuildDependencyInfo right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(BuildDependencyInfo left, BuildDependencyInfo right)
        {
            return !left.Equals(right);
        }
    }
}