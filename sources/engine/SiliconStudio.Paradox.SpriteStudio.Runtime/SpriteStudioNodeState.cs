using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.SpriteStudio.Runtime
{
    [DataContract]
    public class SpriteStudioNodeState
    {
        public SpriteStudioNodeState()
        {
            DefaultPixelsPerUnit = new Vector2(Sprite.DefaultPixelsPerUnit);
        }

        public Matrix LocalTransform;

        public Matrix ModelTransform;

        public bool HFlipped;

        public bool VFlipped;

        public Vector4 CurrentXyPrioAngle;

        public Vector2 Scale;

        public float Transparency;

        public bool Hide;

        public int SpriteId;

        [DataMemberIgnore]
        public Sprite Sprite;

        [DataMemberIgnore]
        public SpriteStudioNode BaseNode;

        [DataMemberIgnore]
        public Vector2 DefaultPixelsPerUnit;

        [DataMemberIgnore]
        public SpriteStudioNodeState ParentNode;

        [DataMemberIgnore]
        public List<SpriteStudioNodeState> ChildrenNodes { get; } = new List<SpriteStudioNodeState>();

        internal void UpdateTransformation()
        {
            var unit = Sprite?.PixelsPerUnit ?? DefaultPixelsPerUnit;
            var scale = Matrix.Scaling(HFlipped ? -Scale.X : Scale.X, VFlipped ? -Scale.Y : Scale.Y, 1.0f);
            var rot = Matrix.RotationZ(CurrentXyPrioAngle.W);
            var pos = Matrix.Translation(CurrentXyPrioAngle.X / unit.X, CurrentXyPrioAngle.Y / unit.Y, 0.0f);
            LocalTransform = scale*rot*pos;

            if (ParentNode != null)
            {
                Matrix.Multiply(ref LocalTransform, ref ParentNode.ModelTransform, out ModelTransform);
            }
            else
            {
                ModelTransform = LocalTransform;
            }

            foreach (var childrenNode in ChildrenNodes)
            {
                childrenNode.UpdateTransformation();
            }
        }
    }
}