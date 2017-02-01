using System;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.VirtualReality
{
    public abstract class VRDevice : IDisposable
    {
        public GameBase Game { get; internal set; }

        protected VRDevice()
        {
            ViewScaling = 1.0f;
        }

        public abstract Size2 OptimalRenderFrameSize { get; }

        public abstract Texture RenderFrame { get; protected set; }

        public abstract Texture RenderFrameDepthStencil { get; protected set; }

        public abstract Texture MirrorTexture { get; protected set; }

        public abstract float RenderFrameScaling { get; set; }

        public virtual Size2 RenderFrameSize
        {
            get
            {
                var width = (int)(OptimalRenderFrameSize.Width * RenderFrameScaling);
                width += width % 2;
                var height = (int)(OptimalRenderFrameSize.Height * RenderFrameScaling);
                height += height % 2;
                return new Size2(width, height);
            }
        }

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

        public abstract void Enable(GraphicsDevice device, GraphicsDeviceManager graphicsDeviceManager, bool depthStencilResource, bool requireMirror);

        public virtual void Recenter()
        {
        }

        public abstract void ReadEyeParameters(Eyes eye, float near, float far, ref Vector3 cameraPosition, ref Matrix cameraRotation, out Matrix view, out Matrix projection);

        public abstract void Commit(CommandList commandList);

        public virtual void Dispose()
        {           
        }

        public abstract void Update(GameTime gameTime);

        public abstract void Draw(GameTime gameTime);
    }
}
