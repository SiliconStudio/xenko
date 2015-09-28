using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.SpriteStudio.Runtime
{
    [DataContract]
    [ContentSerializer(typeof(DataContentSerializer<SpriteStudioNode>))]
    public class SpriteStudioNode
    {
        public string Name;
        public int Id = -1;
        public int ParentId;
        public int PictureId = -1;

        public RectangleF Rectangle;
        public Vector2 Pivot;
        public bool HFlipped;
        public bool VFlipped;

        public Vector4 BaseXyPrioAngle;
        public Sprite Sprite;
    }
}