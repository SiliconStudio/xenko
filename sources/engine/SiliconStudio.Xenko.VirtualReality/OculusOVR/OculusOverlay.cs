#if SILICONSTUDIO_XENKO_GRAPHICS_API_DIRECT3D11

using System;
using SharpDX.Direct3D11;
using SiliconStudio.Xenko.Graphics;
using CommandList = SiliconStudio.Xenko.Graphics.CommandList;

namespace SiliconStudio.Xenko.VirtualReality
{
    internal class OculusOverlay : VROverlay, IDisposable
    {
        private readonly IntPtr ovrSession;
        internal IntPtr OverlayPtr;
        private readonly Texture[] textures;

        public OculusOverlay(IntPtr ovrSession, GraphicsDevice device, int width, int height, int mipLevels, int sampleCount)
        {
            int textureCount;
            this.ovrSession = ovrSession;

            OverlayPtr = OculusOvr.CreateQuadLayerTexturesDx(ovrSession, device.NativeDevice.NativePointer, out textureCount, width, height, mipLevels, sampleCount);
            if (OverlayPtr == null)
            {
                throw new Exception(OculusOvr.GetError());
            }

            textures = new Texture[textureCount];
            for (var i = 0; i < textureCount; i++)
            {

                var ptr = OculusOvr.GetQuadLayerTextureDx(ovrSession, OverlayPtr, OculusOvrHmd.Dx11Texture2DGuid, i);
                if (ptr == IntPtr.Zero)
                {
                    throw new Exception(OculusOvr.GetError());
                }

                textures[i] = new Texture(device);
                textures[i].InitializeFromImpl(new Texture2D(ptr), false);
            }
        }

        public override void Dispose()
        {
        }

        public override void UpdateSurface(CommandList commandList, Texture texture)
        {
            OculusOvr.SetQuadLayerParams(OverlayPtr, ref Position, ref Rotation, ref SurfaceSize, FollowHeadRotation);
            var index = OculusOvr.GetCurrentQuadLayerTargetIndex(ovrSession, OverlayPtr);
            commandList.Copy(texture, textures[index]);
        }
    }
}

#endif