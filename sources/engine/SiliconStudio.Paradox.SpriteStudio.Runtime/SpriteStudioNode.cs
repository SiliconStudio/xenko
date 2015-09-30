using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.SpriteStudio.Runtime
{
    public enum SpriteStudioAlphaBlending
    {
        Mix,
        Multiplication,
        Addition,
        Subtraction
    }

    //Everything that has a Base prefix can be animated, in NodeState
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

        public Vector4 BaseXyPrioAngle;
        public Vector2 BaseScale = Vector2.One;
        public float BaseTransparency = 1.0f;
        public bool BaseHide;
        public bool BaseHFlipped;
        public bool BaseVFlipped;
        public SpriteStudioAlphaBlending BaseAlphaBlending = SpriteStudioAlphaBlending.Mix;

        public Sprite Sprite;
    }
}