using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;

namespace SpaceEscape.Background
{
    /// <summary>
    /// This class contains information needed to describe a hole in the background.
    /// </summary>
    [DataContract("BackgroundElement")]
    public class Hole
    {
        /// <summary>
        /// The area of the hole.
        /// </summary>
        public RectangleF Area { get; set; }

        /// <summary>
        /// The height of the hole.
        /// </summary>
        public float Height { get; set; }
    }
}