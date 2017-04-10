using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.VirtualReality
{
    public abstract class VROverlay
    {
        public Vector3 Position;

        public Quaternion Rotation;

        public Vector2 SurfaceSize;

        public bool FollowHeadRotation;

        public virtual bool Enabled { get; set; } = true;

        public int Width { get; protected set; }

        public int Height { get; protected set; }

        public abstract void Dispose();

        public abstract void UpdateSurface(CommandList commandList, Texture texture);
    }
}