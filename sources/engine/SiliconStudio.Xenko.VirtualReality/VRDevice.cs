using System;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.VirtualReality
{
    public abstract class VROverlay
    {
        public Vector3 Position;

        public Quaternion Rotation;

        public Vector2 SurfaceSize;

        public bool FollowHeadRotation;

        public int Width { get; protected set; }

        public int Height { get; protected set; }

        public abstract void Dispose();

        public abstract void UpdateSurface(CommandList commandList, Texture texture);
    }

    public abstract class VRDevice : IDisposable
    {
        public GameBase Game { get; internal set; }

        protected VRDevice()
        {
            ViewScaling = 1.0f;
        }

        public abstract Size2 OptimalRenderFrameSize { get; }

        public abstract Size2 ActualRenderFrameSize { get; protected set; }

        public abstract Texture MirrorTexture { get; protected set; }

        public abstract float RenderFrameScaling { get; set; }

        public abstract DeviceState State { get; }

        public abstract Vector3 HeadPosition { get; }

        public abstract Quaternion HeadRotation { get; }

        public abstract Vector3 HeadLinearVelocity { get; }

        public abstract Vector3 HeadAngularVelocity { get; }

        public abstract TouchController LeftHand { get; }

        public abstract TouchController RightHand { get; }

        /// <summary>
        /// Allows you to scale the view, effectively it will change the size of the player in respect to the world, turning it into a giant or a tiny ant.
        /// </summary>
        /// <remarks>This will reduce the near clip plane of the cameras, it might induce depth issues.</remarks>
        public float ViewScaling { get; set; }

        public abstract bool CanInitialize { get; }

        public bool SupportsOverlays { get; protected set; } = false;

        public virtual VROverlay CreateOverlay(int width, int height, int mipLevels, int sampleCount)
        {
            return null;
        }

        public virtual void ReleaseOverlay(VROverlay overlay)
        {         
        }

        public abstract void Enable(GraphicsDevice device, GraphicsDeviceManager graphicsDeviceManager, bool requireMirror, int mirrorWidth, int mirrorHeight);

        public virtual void Recenter()
        {
        }

        public abstract void ReadEyeParameters(Eyes eye, float near, float far, ref Vector3 cameraPosition, ref Matrix cameraRotation, out Matrix view, out Matrix projection);

        public abstract void Commit(CommandList commandList, Texture renderFrame);

        public virtual void Dispose()
        {           
        }

        public abstract void Update(GameTime gameTime);

        public abstract void Draw(GameTime gameTime);
    }
}
