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
    public class SpriteStudioNode
    {
        public string Name;
        public int Id = -1;
        public int ParentId;
        public bool IsNull;
        public SpriteStudioAlphaBlending AlphaBlending;
    }
}