// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Xenko.Engine
{
    /// <summary>
    /// Provides settings for the navigation mesh builder to control granularity and error margins 
    /// </summary>
    [DataContract]
    public struct NavigationMeshBuildSettings
    {
        /// <summary>
        /// The Height of a grid cell in the navigation mesh building steps using heightfields. 
        /// A lower number means higher resolution on the vertical axis but longer build times
        /// </summary>
        [DataMemberRange(0.01, float.MaxValue)]
        public float CellHeight;

        /// <summary>
        /// The Width/Height of a grid cell in the navigation mesh building steps using heightfields. 
        /// A lower number means higher resolution on the horizontal axes but longer build times
        /// </summary>
        [DataMemberRange(0.01, float.MaxValue)]
        public float CellSize;

        /// <summary>
        /// Tile size used for Navigation mesh tiles, the final size of a grid tile is CellSize*TileSize
        /// </summary>
        [DataMemberRange(8,4096,1,8)]
        public int TileSize;

        [DataMemberRange(0, float.MaxValue)]
        public float RegionMinSize;
        [DataMemberRange(0, float.MaxValue)]
        public float RegionMergeSize;
        [DataMemberRange(0, float.MaxValue)]
        public float EdgeMaxLen;
        [DataMemberRange(0.1, float.MaxValue)]
        public float EdgeMaxError;
        [DataMemberRange(1.0, float.MaxValue)]
        public float DetailSampleDistInput;
        [DataMemberRange(0.0, float.MaxValue)]
        public float DetailSampleMaxErrorInput;
        
        public bool Equals(NavigationMeshBuildSettings other)
        {
            return CellHeight.Equals(other.CellHeight) && CellSize.Equals(other.CellSize) && TileSize == other.TileSize && RegionMinSize.Equals(other.RegionMinSize) && RegionMergeSize.Equals(other.RegionMergeSize) && EdgeMaxLen.Equals(other.EdgeMaxLen) && EdgeMaxError.Equals(other.EdgeMaxError) && DetailSampleDistInput.Equals(other.DetailSampleDistInput) && DetailSampleMaxErrorInput.Equals(other.DetailSampleMaxErrorInput);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is NavigationMeshBuildSettings && Equals((NavigationMeshBuildSettings)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = CellHeight.GetHashCode();
                hashCode = (hashCode*397) ^ CellSize.GetHashCode();
                hashCode = (hashCode*397) ^ TileSize;
                hashCode = (hashCode*397) ^ RegionMinSize.GetHashCode();
                hashCode = (hashCode*397) ^ RegionMergeSize.GetHashCode();
                hashCode = (hashCode*397) ^ EdgeMaxLen.GetHashCode();
                hashCode = (hashCode*397) ^ EdgeMaxError.GetHashCode();
                hashCode = (hashCode*397) ^ DetailSampleDistInput.GetHashCode();
                hashCode = (hashCode*397) ^ DetailSampleMaxErrorInput.GetHashCode();
                return hashCode;
            }
        }
    };

    [DataContract]
    [ObjectFactory(typeof(NavigationAgentSettingsFactory))]
    public struct NavigationAgentSettings
    {
        [DataMemberRange(0, float.MaxValue)]
        public float Height;
        [DataMemberRange(0, float.MaxValue)]
        public float Radius;

        /// <summary>
        /// Maximum vertical distance this agent can climb
        /// </summary>
        [DataMemberRange(0, float.MaxValue)]
        public float MaxClimb;

        /// <summary>
        /// Maximum slope angle this agent can climb (in degrees)
        /// </summary>
        public AngleSingle MaxSlope;

        public override int GetHashCode()
        {
            return Height.GetHashCode() + Radius.GetHashCode() + MaxClimb.GetHashCode() + MaxSlope.GetHashCode();
        }
    }

    public class NavigationAgentSettingsFactory : IObjectFactory
    {
        public object New(Type type)
        {
            return new NavigationAgentSettings
            {
                Height = 1.0f,
                MaxClimb = 0.25f,
                MaxSlope = new AngleSingle(45.0f, AngleType.Degree),
                Radius = 0.5f
            };
        }
    }
}
