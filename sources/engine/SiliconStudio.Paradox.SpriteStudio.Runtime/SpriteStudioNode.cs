using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Paradox.SpriteStudio.Runtime
{
    public enum SpriteStudioBlending
    {
        Mix,
        Multiplication,
        Addition,
        Subtraction
    }

    [DataContract]
    public class SpriteStudioNode
    {
        public SpriteStudioNode()
        {
            BaseState = new SpriteStudioNodeState
            {
                CurrentXyPrioAngle = Vector4.Zero,
                Scale = Vector2.One,
                Transparency = 1.0f,
                Hide = true,
                SpriteId = -1,
                BlendColor = Color.White,
                BlendType = SpriteStudioBlending.Mix,
                BlendFactor = 0.0f
            };
        }

        public string Name;
        public int Id = -1;
        public int ParentId;
        public bool IsNull;
        public SpriteStudioBlending AlphaBlending;

        public SpriteStudioNodeState BaseState;
    }
}