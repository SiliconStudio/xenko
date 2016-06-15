using System.Collections.Generic;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;

namespace SpaceEscape.Background
{
    /// <summary>
    /// The class contains information needed to describe a collidable object.
    /// </summary>
    public class Obstacle
    {
        /// <summary>
        /// The list of bounding boxes used to determine the collision with the obstacle.
        /// </summary>
        public List<BoundingBox> BoundingBoxes = new List<BoundingBox>(); 

        /// <summary>
        /// The entity representing the collidable object.
        /// </summary>
        public Entity Entity { get; set; }
    }
}