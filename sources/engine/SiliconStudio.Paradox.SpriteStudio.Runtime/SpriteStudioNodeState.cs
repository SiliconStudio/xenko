using System.Collections.Generic;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.SpriteStudio.Runtime
{
    public class SpriteStudioNodeState
    {
        public Matrix LocalTransform;

        public Matrix WorldTransform;

        public SpriteStudioNodeState ParentNode;

        public List<SpriteStudioNodeState> ChildrenNodes { get; } = new List<SpriteStudioNodeState>();

        public Vector4 CurrentXyPrioAngle;

        public Vector2 Scale;

        public float Transparency;

        public bool Hide;

        public Sprite Sprite;

        public SpriteStudioNode BaseNode;

        internal void UpdateTransformation()
        {
            var unit = Sprite.PixelsPerUnit;
            var scale = Matrix.Scaling(Scale.X, Scale.Y, 1.0f);
            var rot = Matrix.RotationZ(CurrentXyPrioAngle.W);
            var pos = Matrix.Translation(CurrentXyPrioAngle.X / unit.X, CurrentXyPrioAngle.Y / unit.Y, 0.0f);
            LocalTransform = scale*rot*pos;

            if (ParentNode != null)
            {
                Matrix.Multiply(ref LocalTransform, ref ParentNode.WorldTransform, out WorldTransform);
            }
            else
            {
                WorldTransform = LocalTransform;
            }

            foreach (var childrenNode in ChildrenNodes)
            {
                childrenNode.UpdateTransformation();
            }
        }
    }
}