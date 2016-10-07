using System;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Xenko.Engine
{
    [DataContract]
    public struct NavigationMeshBuildSettings
    {
        /// <summary>
        /// The Height of a grid cell in the navigation mesh building steps using heightfields
        /// A lower number means higher resolution on the vertical axis but longer build times
        /// </summary>
        public float CellHeight;
        /// <summary>
        /// The Width/Height of a grid cell in the navigation mesh building steps using heightfields
        /// A lower number means higher resolution on the horizontal axes but longer build times
        /// </summary>
        public float CellSize;

        /// <summary>
        /// Tile size used for Navigation mesh tiles, the final size of a grid tile is CellSize*TileSize
        /// </summary>
        public int TileSize;

        public override int GetHashCode()
        {
            return CellHeight.GetHashCode() + CellSize.GetHashCode() + TileSize.GetHashCode();
        }
    };

    [DataContract]
    [ObjectFactory(typeof(NavigationAgentSettingsFactory))]
    public struct NavigationAgentSettings
    {
        public float Height;
        public float Radius;

        /// <summary>
        /// Maximum vertical distance this agent can climb
        /// </summary>
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
