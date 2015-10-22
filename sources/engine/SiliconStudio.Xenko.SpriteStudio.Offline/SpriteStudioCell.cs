using System.Collections.Generic;
using System.Security.Policy;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Paradox.SpriteStudio.Offline
{
    public class SpriteStudioAnim
    {
        public string Name;
        public int Fps;
        public int FrameCount;
        public Dictionary<string, NodeAnimationData> NodesData = new Dictionary<string, NodeAnimationData>();
    }

    public class SpriteStudioCell
    {
        public string Name;
        public RectangleF Rectangle;
        public Vector2 Pivot;
        public int TextureIndex;
    }
}