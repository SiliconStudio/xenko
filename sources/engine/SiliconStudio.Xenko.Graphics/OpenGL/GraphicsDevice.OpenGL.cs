// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGL 
#if SILICONSTUDIO_PLATFORM_ANDROID
extern alias opentkold;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using OpenTK.Graphics;
using OpenTK.Platform;
using SiliconStudio.Core;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Shaders;
using SiliconStudio.Xenko.Graphics.OpenGL;
using Color4 = SiliconStudio.Core.Mathematics.Color4;
#if SILICONSTUDIO_PLATFORM_ANDROID
using System.Text;
using System.Runtime.InteropServices;
using OpenTK.Platform.Android;
#elif SILICONSTUDIO_PLATFORM_IOS
using OpenTK.Platform.iPhoneOS;
#endif
#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
using OpenTK.Graphics.ES30;
using DrawBuffersEnum = OpenTK.Graphics.ES30.DrawBufferMode;
using PixelFormatGl = OpenTK.Graphics.ES30.PixelFormat;
#if !SILICONSTUDIO_PLATFORM_MONO_MOBILE
using BeginMode = OpenTK.Graphics.ES30.PrimitiveType;
using ProgramParameter = OpenTK.Graphics.ES30.GetProgramParameterName;
#else
using FramebufferAttachment = OpenTK.Graphics.ES30.FramebufferSlot;
#endif
#else
using OpenTK.Graphics.OpenGL;
#endif

#if SILICONSTUDIO_XENKO_UI_SDL
using WindowState = SiliconStudio.Xenko.Graphics.SDL.FormWindowState;
#else
using WindowState = OpenTK.WindowState;
#endif

// TODO: remove these when OpenTK API is consistent between OpenGL, mobile OpenGL ES and desktop OpenGL ES
#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
#if SILICONSTUDIO_PLATFORM_MONO_MOBILE
using PixelInternalFormat_TextureComponentCount = OpenTK.Graphics.ES30.PixelInternalFormat;
using TextureTarget_TextureTarget2d = OpenTK.Graphics.ES30.TextureTarget;
#else
using PixelInternalFormat_TextureComponentCount = OpenTK.Graphics.ES30.TextureComponentCount;
using TextureTarget_TextureTarget2d = OpenTK.Graphics.ES30.TextureTarget2d;
#endif
#else
using PixelInternalFormat_TextureComponentCount = OpenTK.Graphics.OpenGL.PixelInternalFormat;
using TextureTarget_TextureTarget2d = OpenTK.Graphics.OpenGL.TextureTarget;
#endif

namespace SiliconStudio.Xenko.Graphics
{
    /// <summary>
    /// Performs primitive-based rendering, creates resources, handles system-level variables, adjusts gamma ramp levels, and creates shaders.
    /// </summary>
    public partial class GraphicsDevice
    {
        private const int MaxBoundRenderTargets = 16;

        // Used when locking asyncCreationLockObject
        private bool asyncCreationLockTaken;

        internal bool ApplicationPaused = false;

        internal IWindowInfo deviceCreationWindowInfo;
        internal object asyncCreationLockObject = new object();
        internal OpenTK.Graphics.IGraphicsContext deviceCreationContext;

        private const GraphicsPlatform GraphicPlatform =
#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
                                                            GraphicsPlatform.OpenGLES;
#else
                                                            GraphicsPlatform.OpenGL;
#endif

#if SILICONSTUDIO_PLATFORM_ANDROID
        // If context was set before Begin(), try to keep it after End()
        // (otherwise devices with no backbuffer flicker)
        private bool keepContextOnEnd;

        private IntPtr graphicsContextEglPtr;
        internal AndroidAsyncGraphicsContext androidAsyncDeviceCreationContext;
        internal bool AsyncPendingTaskWaiting; // Used when Workaround_Context_Tegra2_Tegra3

        // Workarounds for specific GPUs
        internal bool Workaround_VAO_PowerVR_SGX_540;
        internal bool Workaround_Context_Tegra2_Tegra3;
#endif

        internal SamplerState defaultSamplerState;
        internal DepthStencilState defaultDepthStencilState;
        internal BlendState defaultBlendState;
        internal int versionMajor, versionMinor; // queried version
        internal int currentVersionMajor, currentVersionMinor; // glGetVersion
        internal Texture windowProvidedRenderTexture;
        internal Texture windowProvidedDepthTexture;

        internal bool HasVAO;

#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
        internal bool HasDepth24;
        internal bool HasPackedDepthStencilExtension;
        internal bool HasExtTextureFormatBGRA8888;
        internal bool HasRenderTargetFloat;
        internal bool HasRenderTargetHalf;
        internal bool HasTextureRG;
#endif

        private int windowProvidedFrameBuffer;

        private Texture defaultRenderTarget;
        private GraphicsDevice immediateContext;
        private GraphicsAdapter _adapter;
        private SwapChainBackend _defaultSwapChainBackend;
        private Rectangle[] _currentScissorRectangles = new Rectangle[MaxBoundRenderTargets];
        private int contextBeginCounter = 0;

#if !SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
        private float[] _currentViewportsSetBuffer = new float[4 * MaxBoundRenderTargets];
        private int[] _currentScissorsSetBuffer = new int[4 * MaxBoundRenderTargets];
#endif
        private int activeTexture = 0;

        // TODO: Use some LRU scheme to clean up FBOs if not used frequently anymore.
        internal Dictionary<FBOKey, int> existingFBOs = new Dictionary<FBOKey,int>(); 

        /// <summary>
        /// PrimitiveTopology state
        /// </summary>
        private PrimitiveType _currentPrimitiveType = PrimitiveType.Undefined;

        private static GraphicsDevice _currentGraphicsDevice;

        [ThreadStatic] private static List<GraphicsDevice> _graphicsDevicesInUse;

        public static GraphicsDevice Current
        {
            get
            {
                if (_graphicsDevicesInUse != null && _graphicsDevicesInUse.Count > 0)
                    return _graphicsDevicesInUse[_graphicsDevicesInUse.Count - 1];

                return _currentGraphicsDevice;
            }

            set
            {
                _currentGraphicsDevice = value;
            }
        }

        private OpenTK.Graphics.IGraphicsContext graphicsContext;
        private OpenTK.Platform.IWindowInfo windowInfo;

#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP || SILICONSTUDIO_PLATFORM_LINUX
#if SILICONSTUDIO_XENKO_UI_SDL
        private SiliconStudio.Xenko.Graphics.SDL.Window gameWindow;
#else
        private OpenTK.GameWindow gameWindow;
#endif
#elif SILICONSTUDIO_PLATFORM_ANDROID
        private AndroidGameView gameWindow;
#elif SILICONSTUDIO_PLATFORM_IOS
        private iPhoneOSGameView gameWindow;
#endif

        private VertexArrayObject currentVertexArrayObject;
        private VertexArrayObject boundVertexArrayObject;
        internal uint enabledVertexAttribArrays;
        private DepthStencilState boundDepthStencilState;
        private int boundStencilReference;
        private BlendState boundBlendState;
        private RasterizerState boundRasterizerState;
        private Texture boundDepthStencilBuffer;
        private Texture[] boundRenderTargets = new Texture[MaxBoundRenderTargets];
        private int boundFBO;
        internal bool hasRenderTarget, hasDepthStencilBuffer;
        private int boundFBOHeight;
        private int boundProgram = 0;
        private bool needUpdateFBO = true;
        private DrawElementsType drawElementsType;
        private int indexElementSize;
        private IntPtr indexBufferOffset;
        private bool flipRenderTarget = false;
        private FrontFaceDirection currentFrontFace = FrontFaceDirection.Cw;
        private FrontFaceDirection boundFrontFace = FrontFaceDirection.Cw;

#if SILICONSTUDIO_PLATFORM_ANDROID
        [DllImport("libEGL.dll", EntryPoint = "eglGetCurrentContext")]
        internal static extern IntPtr EglGetCurrentContext();
#endif
        internal EffectProgram effectProgram;
        private Texture[] boundTextures = new Texture[64];
        private Texture[] textures = new Texture[64];
        private SamplerState[] samplerStates = new SamplerState[64];

#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
        public bool IsOpenGLES2 { get; private set; }

        private Buffer constantBuffer;

        // Need to change sampler state depending on if texture has mipmap or not during PreDraw
        private bool[] hasMipmaps = new bool[64];
#endif

        private int copyProgram = -1;
        private int copyProgramOffsetLocation = -1;
        private int copyProgramScaleLocation = -1;

        private int copyProgramSRgb = -1;
        private int copyProgramSRgbOffsetLocation = -1;
        private int copyProgramSRgbScaleLocation = -1;

        private float[] squareVertices = {
            0.0f, 0.0f,
            1.0f, 0.0f,
            0.0f, 1.0f, 
            1.0f, 1.0f,
        };

#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
#if SILICONSTUDIO_PLATFORM_MONO_MOBILE
        private const TextureTarget TextureTargetTexture2D = TextureTarget.Texture2D;
        private const TextureTarget3D TextureTargetTexture3D = TextureTarget3D.Texture3D;
#else
        private const TextureTarget2d TextureTargetTexture2D = TextureTarget2d.Texture2D;
        private const TextureTarget3d TextureTargetTexture3D = TextureTarget3d.Texture3D;
#endif
#else
        private const TextureTarget TextureTargetTexture2D = TextureTarget.Texture2D;
        private const TextureTarget TextureTargetTexture3D = TextureTarget.Texture3D;
#endif

        /// <summary>
        /// Gets the status of this device.
        /// </summary>
        public GraphicsDeviceStatus GraphicsDeviceStatus
        {
            get
            {
#if SILICONSTUDIO_PLATFORM_ANDROID
                if (graphicsContext != gameWindow.GraphicsContext)
                {
                    return GraphicsDeviceStatus.Reset;
                }
#endif

                // TODO implement GraphicsDeviceStatus for OpenGL
                return GraphicsDeviceStatus.Normal;
            }
        }

        public void Use()
        {
            if (_graphicsDevicesInUse == null)
                _graphicsDevicesInUse = new List<GraphicsDevice>();

            if (!_graphicsDevicesInUse.Contains(this))
                _graphicsDevicesInUse.Add(this);
        }

        public void Unuse()
        {
            if (_graphicsDevicesInUse == null)
                return;

            _graphicsDevicesInUse.Remove(this);

            if (_graphicsDevicesInUse.Count == 0)
                _graphicsDevicesInUse = null;
        }

        internal UseOpenGLCreationContext UseOpenGLCreationContext()
        {
            return new UseOpenGLCreationContext(this);
        }

        public void ApplyPlatformSpecificParams(Effect effect)
        {
            //effect.Parameters.Set(ShaderBaseKeys.XenkoFlipRendertarget, flipRenderTarget ? -1.0f : 1.0f);
            Parameters.Set(ShaderBaseKeys.XenkoFlipRendertarget, flipRenderTarget ? 1.0f : -1.0f);
        }

        /// <summary>
        /// Marks context as active on the current thread.
        /// </summary>
        public void Begin()
        {
            ++contextBeginCounter;

#if SILICONSTUDIO_PLATFORM_ANDROID
            if (contextBeginCounter == 1)
            {
                if (Workaround_Context_Tegra2_Tegra3)
                {
                    Monitor.Enter(asyncCreationLockObject, ref asyncCreationLockTaken);
                }
                else
                {
                    // On first set, check if context was not already set before,
                    // in which case we won't unset it during End().
                    keepContextOnEnd = graphicsContextEglPtr == EglGetCurrentContext();

                    if (keepContextOnEnd)
                    {
                        return;
                    }
                }
            }
#endif

            if (contextBeginCounter == 1)
                graphicsContext.MakeCurrent(windowInfo);
        }

        public void BeginProfile(Color profileColor, string name)
        {
        }

        public void EndProfile()
        {
        }

        public void Clear(Texture depthStencilBuffer, DepthStencilClearOptions options, float depth = 1, byte stencil = 0)
        {
#if DEBUG
            EnsureContextActive();
#endif

#if SILICONSTUDIO_PLATFORM_ANDROID
            // Device with no background loading context: check if some loading is pending
            if (AsyncPendingTaskWaiting)
                ExecutePendingTasks();
#endif

            var clearFBO = FindOrCreateFBO(depthStencilBuffer);
            if (clearFBO != boundFBO)
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, clearFBO);

            ClearBufferMask clearBufferMask =
                ((options & DepthStencilClearOptions.DepthBuffer) == DepthStencilClearOptions.DepthBuffer ? ClearBufferMask.DepthBufferBit : 0)
                | ((options & DepthStencilClearOptions.Stencil) == DepthStencilClearOptions.Stencil ? ClearBufferMask.StencilBufferBit : 0);
            GL.ClearDepth(depth);
            GL.ClearStencil(stencil);

            var depthStencilState = boundDepthStencilState ?? DepthStencilStates.Default;
            var depthMask = depthStencilState.Description.DepthBufferWriteEnable && hasDepthStencilBuffer;

            if (!depthMask)
                GL.DepthMask(true);
            GL.Clear(clearBufferMask);
            if (!depthMask)
                GL.DepthMask(false);

            if (clearFBO != boundFBO)
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, boundFBO);
        }

        public void Clear(Texture renderTarget, Color4 color)
        {
#if DEBUG
            EnsureContextActive();
#endif

            var clearFBO = FindOrCreateFBO(renderTarget);
            if (clearFBO != boundFBO)
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, clearFBO);

            var blendState = boundBlendState ?? BlendStates.Default;
            var colorMask = hasRenderTarget && blendState.Description.RenderTargets[0].ColorWriteChannels == ColorWriteChannels.All;
            if (!colorMask)
                GL.ColorMask(true, true, true, true);

            GL.ClearColor(color.R, color.G, color.B, color.A);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            // revert the color mask value as it was before
            if (!colorMask)
                blendState.ApplyColorMask();

            if (clearFBO != boundFBO)
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, boundFBO);
        }

        public unsafe void ClearReadWrite(Buffer buffer, Vector4 value)
        {
#if DEBUG
            EnsureContextActive();
#endif

#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
            throw new NotImplementedException();
#else
            if((buffer.ViewFlags & BufferFlags.UnorderedAccess) != BufferFlags.UnorderedAccess)
                throw new ArgumentException("Buffer does not support unordered access");

            GL.BindBuffer(buffer.bufferTarget, buffer.resourceId);
            GL.ClearBufferData(buffer.bufferTarget, buffer.internalFormat, buffer.glPixelFormat, All.UnsignedInt8888, ref value);
            GL.BindBuffer(buffer.bufferTarget, 0);
#endif
        }

        public unsafe void ClearReadWrite(Buffer buffer, Int4 value)
        {
#if DEBUG
            EnsureContextActive();
#endif

#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
            throw new NotImplementedException();
#else
            if ((buffer.ViewFlags & BufferFlags.UnorderedAccess) != BufferFlags.UnorderedAccess)
                throw new ArgumentException("Buffer does not support unordered access");

            GL.BindBuffer(buffer.bufferTarget, buffer.resourceId);
            GL.ClearBufferData(buffer.bufferTarget, buffer.internalFormat, buffer.glPixelFormat, All.UnsignedInt8888, ref value);
            GL.BindBuffer(buffer.bufferTarget, 0);
#endif
        }

        public unsafe void ClearReadWrite(Buffer buffer, UInt4 value)
        {
#if DEBUG
            EnsureContextActive();
#endif

#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
            throw new NotImplementedException();
#else
            if ((buffer.ViewFlags & BufferFlags.UnorderedAccess) != BufferFlags.UnorderedAccess)
                throw new ArgumentException("Buffer does not support unordered access");

            GL.BindBuffer(buffer.bufferTarget, buffer.resourceId);
            GL.ClearBufferData(buffer.bufferTarget, buffer.internalFormat, buffer.glPixelFormat, All.UnsignedInt8888, ref value);
            GL.BindBuffer(buffer.bufferTarget, 0);
#endif
        }

        public unsafe void ClearReadWrite(Texture texture, Vector4 value)
        {
#if DEBUG
            EnsureContextActive();
#endif

#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
            throw new NotImplementedException();
#else
            GL.BindTexture(texture.Target, texture.resourceId);

            GL.ClearTexImage(texture.resourceId, 0, texture.FormatGl, texture.Type, ref value);

            GL.BindTexture(texture.Target, 0);
#endif
        }

        public unsafe void ClearReadWrite(Texture texture, Int4 value)
        {
#if DEBUG
            EnsureContextActive();
#endif

#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
            throw new NotImplementedException();
#else
            GL.BindTexture(texture.Target, texture.resourceId);

            GL.ClearTexImage(texture.resourceId, 0, texture.FormatGl, texture.Type, ref value);

            GL.BindTexture(texture.Target, 0);
#endif
        }

        public unsafe void ClearReadWrite(Texture texture, UInt4 value)
        {
#if DEBUG
            EnsureContextActive();
#endif

#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
            throw new NotImplementedException();
#else
            GL.BindTexture(texture.Target, texture.resourceId);

            GL.ClearTexImage(texture.resourceId, 0, texture.FormatGl, texture.Type, ref value);

            GL.BindTexture(texture.Target, 0);
#endif
        }

        private void ClearStateImpl()
        {
#if DEBUG
            EnsureContextActive();
#endif
            UnbindVertexArrayObject();
            currentVertexArrayObject = null;

            SetDefaultStates();

            // Clear sampler states
            for (int i = 0; i < samplerStates.Length; ++i)
                samplerStates[i] = null;

            for (int i = 0; i < boundTextures.Length; ++i)
            {
                textures[i] = null;
            }

            // Clear active texture state
            activeTexture = 0;
            GL.ActiveTexture(TextureUnit.Texture0);

            // set default states
            SetBlendState(null);
            SetRasterizerState(null);
            SetDepthStencilState(null);

            // Set default render targets
            SetDepthAndRenderTarget(DepthStencilBuffer, BackBuffer);
        }

        /// <summary>
        /// Copy a region of a <see cref="GraphicsResource"/> into another.
        /// </summary>
        /// <param name="source">The source from which to copy the data</param>
        /// <param name="regionSource">The region of the source <see cref="GraphicsResource"/> to copy.</param>
        /// <param name="destination">The destination into which to copy the data</param>
        /// <remarks>This might alter some states such as currently bound texture.</remarks>
        public void CopyRegion(GraphicsResource source, int sourceSubresource, ResourceRegion? regionSource, GraphicsResource destination, int destinationSubResource, int dstX = 0, int dstY = 0, int dstZ = 0)
        {
#if DEBUG
            EnsureContextActive();
#endif
            var sourceTexture = source as Texture;
            var destTexture = destination as Texture;

            if (sourceTexture == null || destTexture == null)
                throw new NotImplementedException("Copy is only implemented for ITexture2D objects.");

            if (sourceSubresource != 0 || destinationSubResource != 0)
                throw new NotImplementedException("Copy is only implemented for subresource 0 in OpenGL.");

            var sourceRegion = regionSource.HasValue? regionSource.Value : new ResourceRegion(0, 0, 0, sourceTexture.Description.Width, sourceTexture.Description.Height, 0);
            var sourceRectangle = new Rectangle(sourceRegion.Left, sourceRegion.Top, sourceRegion.Right - sourceRegion.Left, sourceRegion.Bottom - sourceRegion.Top);

            if (sourceRectangle.Width == 0 || sourceRectangle.Height == 0)
                return;

            if (destTexture.Description.Usage == GraphicsResourceUsage.Staging)
            {
                if (dstX != 0 || dstY != 0 || dstZ != 0)
                    throw new NotSupportedException("ReadPixels from staging texture using non-zero destination is not supported");

                GL.Viewport(0, 0, destTexture.Description.Width, destTexture.Description.Height);
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, FindOrCreateFBO(source));

#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
                if (IsOpenGLES2)
                {
                    var format = destTexture.FormatGl;
                    var type = destTexture.Type;

                    var srcFormat = sourceTexture.Description.Format;
                    var destFormat = destTexture.Description.Format;

                    if (srcFormat == destFormat && destFormat.SizeInBytes() == 4)   // in this case we just want to copy the data we don't care about format conversion. 
                    {                                                               // RGBA/Unsigned-byte is always a working combination whatever is the internal format (sRGB, etc...)
                        format = PixelFormatGl.Rgba;
                        type = PixelType.UnsignedByte;
                    }

                    GL.ReadPixels(sourceRectangle.Left, sourceRectangle.Top, sourceRectangle.Width, sourceRectangle.Height, format, type, destTexture.StagingData);
                }
                else
#endif
                {
                    GL.BindBuffer(BufferTarget.PixelPackBuffer, destTexture.PixelBufferObjectId);
                    GL.ReadPixels(sourceRectangle.Left, sourceRectangle.Top, sourceRectangle.Width, sourceRectangle.Height, destTexture.FormatGl, destTexture.Type, IntPtr.Zero);
                    GL.BindBuffer(BufferTarget.PixelPackBuffer, 0);
                }
                
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, boundFBO);
                GL.Viewport((int)currentState.Viewports[0].X, (int)currentState.Viewports[0].Y, (int)currentState.Viewports[0].Width, (int)currentState.Viewports[0].Height);
                return;
            }

#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
            if (IsOpenGLES2)
            {
                CopyScaler2D(sourceTexture, destTexture, sourceRectangle, new Rectangle(dstX, dstY, sourceRectangle.Width, sourceRectangle.Height));
            }
            else
#endif
            {
                // "FindOrCreateFBO" set the frameBuffer on FBO creation -> those 2 calls cannot be made directly in the following "GL.BindFramebuffer" function calls (side effects)
                var sourceFBO = FindOrCreateFBO(source);
                var destinationFBO = FindOrCreateFBO(destination);
                GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, sourceFBO);
                GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, destinationFBO);
                GL.BlitFramebuffer(sourceRegion.Left, sourceRegion.Top, sourceRegion.Right, sourceRegion.Bottom,
                    dstX, dstY, dstX + sourceRegion.Right - sourceRegion.Left, dstY + sourceRegion.Bottom - sourceRegion.Top,
                    ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Nearest);
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, boundFBO);
            }
        }

        internal void CopyScaler2D(Texture sourceTexture, Texture destTexture, Rectangle sourceRectangle, Rectangle destRectangle, bool flipY = false)
        {
            // Use rendering
            GL.Viewport(0, 0, destTexture.Description.Width, destTexture.Description.Height);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FindOrCreateFBO(destTexture));

            if (copyProgram == -1)
            {
                copyProgram = CreateCopyProgram(false, out copyProgramOffsetLocation, out copyProgramScaleLocation);
                copyProgramSRgb = CreateCopyProgram(true, out copyProgramSRgbOffsetLocation, out copyProgramSRgbScaleLocation);
            }

            var sourceRegionSize = new Vector2(sourceRectangle.Width, sourceRectangle.Height);
            var destRegionSize = new Vector2(destRectangle.Width, destRectangle.Height);

            // Source
            var sourceSize = new Vector2(sourceTexture.Width, sourceTexture.Height);
            var sourceRegionLeftTop = new Vector2(sourceRectangle.Left, sourceRectangle.Top);
            var sourceScale = new Vector2(sourceRegionSize.X / sourceSize.X, sourceRegionSize.Y / sourceSize.Y);
            var sourceOffset = new Vector2(sourceRegionLeftTop.X / sourceSize.X, sourceRegionLeftTop.Y / sourceSize.Y);

            // Dest
            var destSize = new Vector2(destTexture.Width, destTexture.Height);
            var destRegionLeftTop = new Vector2(destRectangle.X, flipY ? destRectangle.Bottom : destRectangle.Y);
            var destScale = new Vector2(destRegionSize.X / destSize.X, destRegionSize.Y / destSize.Y);
            var destOffset = new Vector2(destRegionLeftTop.X / destSize.X, destRegionLeftTop.Y / destSize.Y);

            if (flipY)
                destScale.Y = -destScale.Y;

            var enabledColors = new bool[4];
            GL.GetBoolean(GetPName.ColorWritemask, enabledColors);
            var isDepthTestEnabled = GL.IsEnabled(EnableCap.DepthTest);
            var isCullFaceEnabled = GL.IsEnabled(EnableCap.CullFace);
            var isBlendEnabled = GL.IsEnabled(EnableCap.Blend);
            var isStencilEnabled = GL.IsEnabled(EnableCap.StencilTest);
            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.CullFace);
            GL.Disable(EnableCap.Blend);
            GL.Disable(EnableCap.StencilTest);
            GL.ColorMask(true, true, true, true);

            UnbindVertexArrayObject();

            // If we are copying from an SRgb texture to a non SRgb texture, we use a special SRGb copy shader
            var program = copyProgram;
            var offsetLocation = copyProgramOffsetLocation;
            var scaleLocation = copyProgramScaleLocation;
            if (sourceTexture.Description.Format.IsSRgb() && destTexture == windowProvidedRenderTexture)
            {
                program = copyProgramSRgb;
                offsetLocation = copyProgramSRgbOffsetLocation;
                scaleLocation = copyProgramSRgbScaleLocation;
            }

            GL.UseProgram(program);

            activeTexture = 0;
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, sourceTexture.resourceId);
            boundTextures[0] = null;
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            sourceTexture.BoundSamplerState = SamplerStates.PointClamp;

            GL.EnableVertexAttribArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 0, squareVertices);
            GL.Uniform4(offsetLocation, sourceOffset.X, sourceOffset.Y, destOffset.X, destOffset.Y);
            GL.Uniform4(scaleLocation, sourceScale.X, sourceScale.Y, destScale.X, destScale.Y);
            GL.Viewport(0, 0, destTexture.Width, destTexture.Height);
            GL.DrawArrays(BeginMode.TriangleStrip, 0, 4);
            GL.DisableVertexAttribArray(0);
            GL.UseProgram(boundProgram);

            // Restore context
            if (isDepthTestEnabled)
                GL.Enable(EnableCap.DepthTest);
            if (isCullFaceEnabled)
                GL.Enable(EnableCap.CullFace);
            if (isBlendEnabled)
                GL.Enable(EnableCap.Blend);
            if (isStencilEnabled)
                GL.Enable(EnableCap.StencilTest);
            GL.ColorMask(enabledColors[0], enabledColors[1], enabledColors[2], enabledColors[3]);

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, boundFBO);
            GL.Viewport((int)currentState.Viewports[0].X, (int)currentState.Viewports[0].Y, (int)currentState.Viewports[0].Width, (int)currentState.Viewports[0].Height);
        }

        private int CreateCopyProgram(bool srgb, out int offsetLocation, out int scaleLocation)
        {
            const string copyVertexShaderSource =
                "attribute vec2 aPosition;   \n" +
                "varying vec2 vTexCoord;     \n" +
                "uniform vec4 uScale;     \n" +
                "uniform vec4 uOffset;     \n" +
                "void main()                 \n" +
                "{                           \n" +
                "   vec4 transformedPosition = aPosition.xyxy * uScale + uOffset;" +
                "   gl_Position = vec4(transformedPosition.zw * 2.0 - 1.0, 0.0, 1.0); \n" +
                "   vTexCoord = transformedPosition.xy;   \n" +
                "}                           \n";

            const string copyFragmentShaderSource =
                "precision mediump float;                            \n" +
                "varying vec2 vTexCoord;                             \n" +
                "uniform sampler2D s_texture;                        \n" +
                "void main()                                         \n" +
                "{                                                   \n" +
                "    gl_FragColor = texture2D(s_texture, vTexCoord); \n" +
                "}                                                   \n";

            const string copyFragmentShaderSourceSRgb =
                "precision mediump float;                            \n" +
                "varying vec2 vTexCoord;                             \n" +
                "uniform sampler2D s_texture;                        \n" +
                "void main()                                         \n" +
                "{                                                   \n" +
                "    vec4 color = texture2D(s_texture, vTexCoord);   \n" +
                "    gl_FragColor = vec4(sqrt(color.rgb), color.a); \n" +  // approximation of linear to SRgb
                "}                                                   \n";

            // First initialization of shader program
            int vertexShader = TryCompileShader(ShaderType.VertexShader, copyVertexShaderSource);
            int fragmentShader = TryCompileShader(ShaderType.FragmentShader, srgb ? copyFragmentShaderSourceSRgb : copyFragmentShaderSource);

            int program = GL.CreateProgram();
            GL.AttachShader(program, vertexShader);
            GL.AttachShader(program, fragmentShader);
            GL.BindAttribLocation(program, 0, "aPosition");
            GL.LinkProgram(program);

            int linkStatus;
            GL.GetProgram(program, ProgramParameter.LinkStatus, out linkStatus);

            if (linkStatus != 1)
                throw new InvalidOperationException("Error while linking GLSL shaders.");

            GL.UseProgram(program);
            var textureLocation = GL.GetUniformLocation(program, "s_texture");
            offsetLocation = GL.GetUniformLocation(program, "uOffset");
            scaleLocation = GL.GetUniformLocation(program, "uScale");
            GL.Uniform1(textureLocation, 0);

            return program;
        }

        /// <summary>
        /// Copy a <see cref="GraphicsResource"/> into another.
        /// </summary>
        /// <param name="source">The source from which to copy the data</param>
        /// <param name="destination">The destination into which to copy the data</param>
        /// <remarks>This might alter some states such as currently bound texture.</remarks>
        public void Copy(GraphicsResource source, GraphicsResource destination)
        {
            CopyRegion(source, 0, null, destination, 0);
        }

        public void CopyMultiSample(Texture sourceMsaaTexture, int sourceSubResource, Texture destTexture, int destSubResource, PixelFormat format = PixelFormat.None)
        {
            throw new NotImplementedException();
        }

        public void CopyCount(Buffer sourceBuffer, Buffer destBuffer, int offsetToDest)
        {
#if DEBUG
            EnsureContextActive();
#endif

            throw new NotImplementedException();
        }

        public void Dispatch(int threadCountX, int threadCountY, int threadCountZ)
        {
#if DEBUG
            EnsureContextActive();
#endif

#if !SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
            GL.DispatchCompute(threadCountX, threadCountY, threadCountZ);
#else
            throw new NotImplementedException();
#endif
        }

        public void Dispatch(Buffer indirectBuffer, int offsetInBytes)
        {
#if DEBUG
            EnsureContextActive();
#endif

#if !SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
            GL.BindBuffer(BufferTarget.DispatchIndirectBuffer, indirectBuffer.resourceId);

            GL.DispatchComputeIndirect((IntPtr)offsetInBytes);

            GL.BindBuffer(BufferTarget.DispatchIndirectBuffer, 0);
#else
            throw new NotImplementedException();
#endif
        }

        public void Draw(PrimitiveType primitiveType, int vertexCount, int startVertex = 0)
        {
#if DEBUG
            EnsureContextActive();
#endif
            PreDraw();

            GL.DrawArrays(primitiveType.ToOpenGL(), startVertex, vertexCount);

            FrameTriangleCount += (uint)vertexCount;
            FrameDrawCalls++;
        }

        public void DrawAuto(PrimitiveType primitiveType)
        {
#if DEBUG
            EnsureContextActive();
#endif
            PreDraw();

            //GL.DrawArraysIndirect(primitiveType.ToOpenGL(), (IntPtr)0);
            throw new NotImplementedException();

            FrameDrawCalls++;
        }

        /// <summary>
        /// Draw indexed, non-instanced primitives.
        /// </summary>
        /// <param name="primitiveType">Type of the primitive to draw.</param>
        /// <param name="indexCount">Number of indices to draw.</param>
        /// <param name="startIndexLocation">The location of the first index read by the GPU from the index buffer.</param>
        /// <param name="baseVertexLocation">A value added to each index before reading a vertex from the vertex buffer.</param>
        public void DrawIndexed(PrimitiveType primitiveType, int indexCount, int startIndexLocation = 0, int baseVertexLocation = 0)
        {
#if DEBUG
            EnsureContextActive();
#endif
            PreDraw();

#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
            if(baseVertexLocation != 0)
                throw new NotSupportedException("DrawIndexed with no null baseVertexLocation is not supported on OpenGL ES.");
            GL.DrawElements(primitiveType.ToOpenGL(), indexCount, drawElementsType, indexBufferOffset + (startIndexLocation * indexElementSize)); // conversion to IntPtr required on Android
#else
            GL.DrawElementsBaseVertex(primitiveType.ToOpenGL(), indexCount, drawElementsType, indexBufferOffset + (startIndexLocation * indexElementSize), baseVertexLocation);
#endif

            FrameDrawCalls++;
            FrameTriangleCount += (uint)indexCount;
        }

        /// <summary>
        /// Draw indexed, instanced primitives.
        /// </summary>
        /// <param name="primitiveType">Type of the primitive to draw.</param>
        /// <param name="indexCountPerInstance">Number of indices read from the index buffer for each instance.</param>
        /// <param name="instanceCount">Number of instances to draw.</param>
        /// <param name="startIndexLocation">The location of the first index read by the GPU from the index buffer.</param>
        /// <param name="baseVertexLocation">A value added to each index before reading a vertex from the vertex buffer.</param>
        /// <param name="startInstanceLocation">A value added to each index before reading per-instance data from a vertex buffer.</param>
        public void DrawIndexedInstanced(PrimitiveType primitiveType, int indexCountPerInstance, int instanceCount, int startIndexLocation = 0, int baseVertexLocation = 0, int startInstanceLocation = 0)
        {
#if DEBUG
            EnsureContextActive();
#endif
            PreDraw();
#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
            throw new NotImplementedException();
#else
            GL.DrawElementsInstancedBaseVertex(primitiveType.ToOpenGL(), indexCountPerInstance, DrawElementsType.UnsignedInt, (IntPtr)(startIndexLocation * indexElementSize), instanceCount, baseVertexLocation);
#endif

            FrameDrawCalls++;
            FrameTriangleCount += (uint)(indexCountPerInstance * instanceCount);
        }

        /// <summary>
        /// Draw indexed, instanced, GPU-generated primitives.
        /// </summary>
        /// <param name="primitiveType">Type of the primitive to draw.</param>
        /// <param name="argumentsBuffer">A buffer containing the GPU generated primitives.</param>
        /// <param name="alignedByteOffsetForArgs">Offset in <em>pBufferForArgs</em> to the start of the GPU generated primitives.</param>
        public void DrawIndexedInstanced(PrimitiveType primitiveType, Buffer argumentsBuffer, int alignedByteOffsetForArgs = 0)
        {
            throw new NotImplementedException();

            if (argumentsBuffer == null) throw new ArgumentNullException("argumentsBuffer");

#if DEBUG
            EnsureContextActive();
#endif
            PreDraw();

            FrameDrawCalls++;
        }

        /// <summary>
        /// Draw non-indexed, instanced primitives.
        /// </summary>
        /// <param name="primitiveType">Type of the primitive to draw.</param>
        /// <param name="vertexCountPerInstance">Number of vertices to draw.</param>
        /// <param name="instanceCount">Number of instances to draw.</param>
        /// <param name="startVertexLocation">Index of the first vertex.</param>
        /// <param name="startInstanceLocation">A value added to each index before reading per-instance data from a vertex buffer.</param>
        public void DrawInstanced(PrimitiveType primitiveType, int vertexCountPerInstance, int instanceCount, int startVertexLocation = 0, int startInstanceLocation = 0)
        {
#if DEBUG
            EnsureContextActive();
#endif
            PreDraw();

#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
            if (IsOpenGLES2)
                throw new NotSupportedException("DrawArraysInstanced is not supported on OpenGL ES 2");
            GL.DrawArraysInstanced(primitiveType.ToOpenGLES(), startVertexLocation, vertexCountPerInstance, instanceCount);
#else
            GL.DrawArraysInstanced(primitiveType.ToOpenGL(), startVertexLocation, vertexCountPerInstance, instanceCount);
#endif

            FrameDrawCalls++;
            FrameTriangleCount += (uint)(vertexCountPerInstance * instanceCount);
        }

        /// <summary>
        /// Draw instanced, GPU-generated primitives.
        /// </summary>
        /// <param name="primitiveType">Type of the primitive to draw.</param>
        /// <param name="argumentsBuffer">An arguments buffer</param>
        /// <param name="alignedByteOffsetForArgs">Offset in <em>pBufferForArgs</em> to the start of the GPU generated primitives.</param>
        public void DrawInstanced(PrimitiveType primitiveType, Buffer argumentsBuffer, int alignedByteOffsetForArgs = 0)
        {
            if (argumentsBuffer == null) 
                throw new ArgumentNullException("argumentsBuffer");

#if DEBUG
            EnsureContextActive();
#endif

            PreDraw();

#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
            throw new NotImplementedException();
#else
            GL.BindBuffer(BufferTarget.DrawIndirectBuffer, argumentsBuffer.resourceId);

            GL.DrawArraysIndirect(primitiveType.ToOpenGL(), (IntPtr)alignedByteOffsetForArgs);

            GL.BindBuffer(BufferTarget.DrawIndirectBuffer, 0);
#endif

            FrameDrawCalls++;
        }

        public void EnableProfile(bool enabledFlag)
        {
        }

        /// <summary>
        /// Unmarks context as active on the current thread.
        /// </summary>
        public void End()
        {
#if DEBUG
            EnsureContextActive();
#endif

            --contextBeginCounter;
            if (contextBeginCounter == 0)
            {
                UnbindVertexArrayObject();

#if SILICONSTUDIO_PLATFORM_ANDROID
                if (Workaround_Context_Tegra2_Tegra3)
                {
                    graphicsContext.MakeCurrent(null);

                    // Notify that main context can be used from now on
                    if (asyncCreationLockTaken)
                    {
                        Monitor.Exit(asyncCreationLockObject);
                        asyncCreationLockTaken = false;
                    }
                }
                else if (!keepContextOnEnd)
                {
                    UnbindGraphicsContext(graphicsContext);
                }
#else
                UnbindGraphicsContext(graphicsContext);
#endif
            }
            else if (contextBeginCounter < 0)
            {
                throw new Exception("End context was called more than Begin");
            }
        }

        internal void EnsureContextActive()
        {
            // TODO: Better checks (is active context the expected one?)
#if SILICONSTUDIO_PLATFORM_ANDROID
            if (EglGetCurrentContext() == IntPtr.Zero)
                throw new InvalidOperationException("No OpenGL context bound.");
#else
            if (OpenTK.Graphics.GraphicsContext.CurrentContext == null)
                throw new InvalidOperationException("No OpenGL context bound.");
#endif
        }

        public void ExecuteCommandList(ICommandList commandList)
        {
#if DEBUG
            EnsureContextActive();
#endif

            throw new NotImplementedException();
        }

        internal void BindProgram(int program)
        {
            if (program != boundProgram)
            {
                boundProgram = program;
                GL.UseProgram(program);
            }
        }

        internal int FindOrCreateFBO(GraphicsResourceBase graphicsResource)
        {
            if (graphicsResource == RootDevice.windowProvidedRenderTexture)
                return windowProvidedFrameBuffer;

            var texture = graphicsResource as Texture;
            if (texture != null)
            {
                return FindOrCreateFBO(texture);
            }

            throw new NotSupportedException();
        }

        internal int FindOrCreateFBO(Texture texture)
        {
            var isDepthBuffer = ((texture.Flags & TextureFlags.DepthStencil) != 0);
            lock (RootDevice.existingFBOs)
            {
                foreach (var key in RootDevice.existingFBOs)
                {
                    if ((isDepthBuffer && key.Key.DepthStencilBuffer == texture)
                        || !isDepthBuffer && key.Key.LastRenderTarget == 1 && key.Key.RenderTargets[0] == texture)
                        return key.Value;
                }
            }

            if (isDepthBuffer)
                return FindOrCreateFBO(texture, null);
            return FindOrCreateFBO(null, new[] { texture });
        }

        internal int FindOrCreateFBO(Texture depthStencilBuffer, Texture[] renderTargets)
        {
            int framebufferId;

            // Check for existing FBO matching this configuration
            lock (RootDevice.existingFBOs)
            {
                var fboKey = new FBOKey(depthStencilBuffer, renderTargets);

                // Is it the default provided render target?
                // TODO: Need to disable some part of rendering if either is null
                var isProvidedDepthBuffer = RootDevice.windowProvidedDepthTexture != null && (depthStencilBuffer == RootDevice.windowProvidedDepthTexture);
                var isProvidedRenderTarget = (fboKey.LastRenderTarget == 1 && renderTargets[0] == RootDevice.windowProvidedRenderTexture);
                if ((isProvidedDepthBuffer || depthStencilBuffer == null) && (isProvidedRenderTarget || fboKey.LastRenderTarget == 0)) // device provided framebuffer
                {
                    return windowProvidedFrameBuffer;
                }
                if (isProvidedDepthBuffer || isProvidedRenderTarget)
                {
                    throw new InvalidOperationException("It is impossible to bind device provided and user created buffers with OpenGL");
                }

                if (RootDevice.existingFBOs.TryGetValue(fboKey, out framebufferId))
                    return framebufferId;

                GL.GenFramebuffers(1, out framebufferId);
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebufferId);
                int lastRenderTargetIndex = -1;
                if (renderTargets != null)
                {
                    for (int i = 0; i < renderTargets.Length; ++i)
                    {
                        if (renderTargets[i] != null)
                        {
                            lastRenderTargetIndex = i;
                            // TODO: enable color render buffers when Texture creates one for other types than depth/stencil.
                            //if (renderTargets[i].IsRenderbuffer)
                            //    GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0 + i, RenderbufferTarget.Renderbuffer, renderTargets[i].ResourceId);
                            //else
                                GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0 + i, TextureTargetTexture2D, renderTargets[i].ResourceId, 0);
                        }
                    }
                }

#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
                if (!IsOpenGLES2)
#endif
                {
#if !SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
                    if (lastRenderTargetIndex <= 0)
                    {
                        GL.DrawBuffer(lastRenderTargetIndex != -1 ? DrawBufferMode.ColorAttachment0 : DrawBufferMode.None);
                    }
                    else
#endif
                    {
                        var drawBuffers = new DrawBuffersEnum[lastRenderTargetIndex + 1];
                        for (var i = 0; i <= lastRenderTargetIndex; ++i)
                            drawBuffers[i] = DrawBuffersEnum.ColorAttachment0 + i;
                        GL.DrawBuffers(lastRenderTargetIndex + 1, drawBuffers);
                    }
                }

                if (depthStencilBuffer != null)
                {
                    bool useSharedAttachment = depthStencilBuffer.ResourceIdStencil == depthStencilBuffer.ResourceId;
#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
                    if (IsOpenGLES2)  // FramebufferAttachment.DepthStencilAttachment is not supported in ES 2
                        useSharedAttachment = false;
#endif
                    var attachmentType = useSharedAttachment ? FramebufferAttachment.DepthStencilAttachment : FramebufferAttachment.DepthAttachment;

                    if (depthStencilBuffer.IsRenderbuffer)
                    {
                        // Bind depth-only or packed depth-stencil buffer
                        GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, attachmentType, RenderbufferTarget.Renderbuffer, depthStencilBuffer.ResourceId);

                        // If stencil buffer is separate, it's resource id might be stored in depthStencilBuffer.Texture.ResouceIdStencil
                        if (depthStencilBuffer.HasStencil && !useSharedAttachment)
                            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.StencilAttachment, RenderbufferTarget.Renderbuffer, depthStencilBuffer.ResourceIdStencil);
                    }
                    else
                    {
                        // Bind depth-only or packed depth-stencil buffer
                        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, attachmentType, TextureTargetTexture2D, depthStencilBuffer.ResourceId, 0);

                        // If stencil buffer is separate, it's resource id might be stored in depthStencilBuffer.Texture.ResouceIdStencil
                        if (depthStencilBuffer.HasStencil && !useSharedAttachment)
                            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.StencilAttachment, TextureTargetTexture2D, depthStencilBuffer.ResourceIdStencil, 0);
                    }
                }

                var framebufferStatus = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
                if (framebufferStatus != FramebufferErrorCode.FramebufferComplete)
                {
                    throw new InvalidOperationException(string.Format("FBO is incomplete: RT {0} Depth {1} (error: {2})", renderTargets != null && renderTargets.Length > 0 ? renderTargets[0].ResourceId : 0, depthStencilBuffer != null ? depthStencilBuffer.ResourceId : 0, framebufferStatus));
                }

                GL.BindFramebuffer(FramebufferTarget.Framebuffer, boundFBO);

                RootDevice.existingFBOs.Add(new GraphicsDevice.FBOKey(depthStencilBuffer, renderTargets != null ? renderTargets.ToArray() : null), framebufferId);
            }

            return framebufferId;
        }

        public ICommandList FinishCommandList()
        {
#if DEBUG
            EnsureContextActive();
#endif

            throw new NotImplementedException();
        }

        protected void InitializeFactories()
        {
        }

        public MappedResource MapSubresource(GraphicsResource resource, int subResourceIndex, MapMode mapMode, bool doNotWait = false, int offsetInBytes = 0, int lengthInBytes = 0)
        {
#if DEBUG
            EnsureContextActive();
#endif

            var buffer = resource as Buffer;
            if (buffer != null)
            {
                if (lengthInBytes == 0)
                    lengthInBytes = buffer.Description.SizeInBytes;

#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
                if (buffer.StagingData != IntPtr.Zero)
                {
                    // Specific case for constant buffers
                    return new MappedResource(resource, subResourceIndex, new DataBox { DataPointer = buffer.StagingData + offsetInBytes, SlicePitch = 0, RowPitch = 0 }, offsetInBytes,
                        lengthInBytes);
                }
                
                if (IsOpenGLES2)
                    throw new NotImplementedException();
#endif
                
                IntPtr mapResult = IntPtr.Zero;

                UnbindVertexArrayObject();
                GL.BindBuffer(buffer.bufferTarget, buffer.ResourceId);

#if !SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
                if (mapMode != MapMode.WriteDiscard && mapMode != MapMode.WriteNoOverwrite)
                    mapResult = GL.MapBuffer(buffer.bufferTarget, mapMode.ToOpenGL());
                else
#endif
                {
                    mapResult = GL.MapBufferRange(buffer.bufferTarget, (IntPtr)offsetInBytes, (IntPtr)lengthInBytes, mapMode.ToOpenGLMask());
                }

                GL.BindBuffer(buffer.bufferTarget, 0);

                return new MappedResource(resource, subResourceIndex, new DataBox { DataPointer = mapResult, SlicePitch = 0, RowPitch = 0 });
            }

            var texture = resource as Texture;
            if (texture != null)
            {
#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
                if (lengthInBytes == 0)
                    lengthInBytes = texture.DepthPitch;
#endif

                if (mapMode == MapMode.Read)
                {
                    if (texture.Description.Usage != GraphicsResourceUsage.Staging)
                        throw new NotSupportedException("Only staging textures can be mapped.");

#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
                    if (IsOpenGLES2 || texture.StagingData != IntPtr.Zero)
                    {
                        return new MappedResource(resource, subResourceIndex,
                            new DataBox { DataPointer = texture.StagingData + offsetInBytes, SlicePitch = texture.DepthPitch, RowPitch = texture.RowPitch }, offsetInBytes, lengthInBytes);
                    }
                    else
#endif
                    {
                        return MapTexture(texture, BufferTarget.PixelPackBuffer, mapMode, subResourceIndex, offsetInBytes, lengthInBytes);
                    }
                }
                else if (mapMode == MapMode.WriteDiscard)
                {
#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
                    if (IsOpenGLES2)
                        throw new NotImplementedException();
#endif
                    if (texture.Description.Usage != GraphicsResourceUsage.Dynamic)
                        throw new NotSupportedException("Only dynamic texture can be mapped.");

                    return MapTexture(texture, BufferTarget.PixelUnpackBuffer, mapMode, subResourceIndex, offsetInBytes, lengthInBytes);
                }
            }

            throw new NotImplementedException("MapSubresource not implemented for type " + resource.GetType());
        }

        private MappedResource MapTexture(Texture texture, BufferTarget pixelPackUnpack, MapMode mapMode, int subResourceIndex, int offsetInBytes, int lengthInBytes)
        {
            GL.BindBuffer(pixelPackUnpack, texture.PixelBufferObjectId);
#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
            
            var mapResult = GL.MapBufferRange(pixelPackUnpack, (IntPtr)offsetInBytes, (IntPtr)lengthInBytes, mapMode.ToOpenGLMask());
            GL.BindBuffer(pixelPackUnpack, 0);
#else
            offsetInBytes = 0;
            lengthInBytes = -1;
            var mapResult = GL.MapBuffer(pixelPackUnpack, mapMode.ToOpenGL());
#endif
            GL.BindBuffer(pixelPackUnpack, 0);

            return new MappedResource(texture, subResourceIndex, new DataBox { DataPointer = mapResult, SlicePitch = texture.DepthPitch, RowPitch = texture.RowPitch }, offsetInBytes, lengthInBytes);
        }

        public GraphicsDevice NewDeferred()
        {
            throw new NotImplementedException();
        }

        internal void PreDraw()
        {
#if SILICONSTUDIO_PLATFORM_ANDROID
            // Device with no background loading context: check if some loading is pending
            if (AsyncPendingTaskWaiting)
                ExecutePendingTasks();
#endif

            var inputSignature = effectProgram.InputSignature;
            if (currentVertexArrayObject != boundVertexArrayObject || (currentVertexArrayObject != null && currentVertexArrayObject.RequiresApply(inputSignature)))
            {
                if (currentVertexArrayObject == null)
                {
                    UnbindVertexArrayObject();
                }
                else
                {
                    drawElementsType = currentVertexArrayObject.drawElementsType;
                    indexBufferOffset = currentVertexArrayObject.indexBufferOffset;
                    indexElementSize = currentVertexArrayObject.indexElementSize;
                    currentVertexArrayObject.Apply(inputSignature);
                    boundVertexArrayObject = currentVertexArrayObject;
                }
            }

            foreach (var textureInfo in effectProgram.Textures)
            {
                var boundTexture = boundTextures[textureInfo.TextureUnit];
                var texture = textures[textureInfo.TextureUnit];

                if (texture != null)
                {
                    var boundSamplerState = texture.BoundSamplerState ?? defaultSamplerState;
                    var samplerState = samplerStates[textureInfo.TextureUnit] ?? SamplerStates.LinearClamp;

                    bool hasMipmap = texture.Description.MipLevels > 1;

                    bool textureChanged = texture != boundTexture;
                    bool samplerStateChanged = samplerState != boundSamplerState;

                    // TODO: Lazy update for texture
                    if (textureChanged || samplerStateChanged)
                    {
                        if (activeTexture != textureInfo.TextureUnit)
                        {
                            activeTexture = textureInfo.TextureUnit;
                            GL.ActiveTexture(TextureUnit.Texture0 + textureInfo.TextureUnit);
                        }

                        // Lazy update for texture
                        if (textureChanged)
                        {
                            boundTextures[textureInfo.TextureUnit] = texture;
                            GL.BindTexture(texture.Target, texture.resourceId);
                        }

                        // Lazy update for sampler state
                        if (samplerStateChanged)
                        {
                            samplerState.Apply(hasMipmap, boundSamplerState, texture.Target);
                            texture.BoundSamplerState = samplerState;
                        }
                    }
                }
            }

            // Change face culling if the rendertarget is flipped
            var newFrontFace = currentFrontFace;
            if (!flipRenderTarget)
                newFrontFace = newFrontFace == FrontFaceDirection.Cw ? FrontFaceDirection.Ccw : FrontFaceDirection.Cw;

            // Update viewports
            SetViewportImpl();

            if (newFrontFace != boundFrontFace)
            {
                boundFrontFace = newFrontFace;
                GL.FrontFace(boundFrontFace);
            }
            
#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
            unsafe
            {
                fixed(byte* boundUniforms = effectProgram.BoundUniforms)
                {
                    if (constantBuffer != null)
                    {
                        var constantBufferData = constantBuffer.StagingData;
                        foreach (var uniform in effectProgram.Uniforms)
                        {
                            var firstUniformIndex = uniform.UniformIndex;
                            var lastUniformIndex = firstUniformIndex + uniform.Count;
                            var offset = uniform.Offset;
                            var boundData = (IntPtr)boundUniforms + offset;
                            var currentData = constantBufferData + offset;

                            // Already updated? Early exit.
                            // TODO: Not optimal for float1/float2 arrays (rare?)
                            // Better to do "sparse" comparison, not sure if C# code would behave well though
                            if (SiliconStudio.Core.Utilities.CompareMemory(boundData, currentData, uniform.CompareSize))
                                continue;

                            // Update bound cache for early exit
                            SiliconStudio.Core.Utilities.CopyMemory(boundData, currentData, uniform.CompareSize);

                            switch (uniform.Type)
                            {
                                case ActiveUniformType.Float:
                                    for (int uniformIndex = firstUniformIndex; uniformIndex < lastUniformIndex; ++uniformIndex)
                                    {
                                        GL.Uniform1(uniformIndex, 1, (float*)currentData);
                                        currentData += 16; // Each array element is spaced by 16 bytes
                                    }
                                    break;
                                case ActiveUniformType.FloatVec2:
                                    for (int uniformIndex = firstUniformIndex; uniformIndex < lastUniformIndex; ++uniformIndex)
                                    {
                                        GL.Uniform2(uniformIndex, 1, (float*)currentData);
                                        currentData += 16; // Each array element is spaced by 16 bytes
                                    }
                                    break;
                                case ActiveUniformType.FloatVec3:
                                    for (int uniformIndex = firstUniformIndex; uniformIndex < lastUniformIndex; ++uniformIndex)
                                    {
                                        GL.Uniform3(uniformIndex, 1, (float*)currentData);
                                        currentData += 16; // Each array element is spaced by 16 bytes
                                    }
                                    break;
                                case ActiveUniformType.FloatVec4:
                                    GL.Uniform4(firstUniformIndex, uniform.Count, (float*)currentData);
                                    break;
                                case ActiveUniformType.FloatMat4:
                                    GL.UniformMatrix4(uniform.UniformIndex, uniform.Count, false, (float*)currentData);
                                    break;
                                case ActiveUniformType.Bool:
                                case ActiveUniformType.Int:
                                    for (int uniformIndex = firstUniformIndex; uniformIndex < lastUniformIndex; ++uniformIndex)
                                    {
                                        GL.Uniform1(uniformIndex, 1, (int*)currentData);
                                        currentData += 16; // Each array element is spaced by 16 bytes
                                    }
                                    break;
                                case ActiveUniformType.BoolVec2:
                                case ActiveUniformType.IntVec2:
                                    for (int uniformIndex = firstUniformIndex; uniformIndex < lastUniformIndex; ++uniformIndex)
                                    {
                                        GL.Uniform2(uniformIndex, 1, (int*)currentData);
                                        currentData += 16; // Each array element is spaced by 16 bytes
                                    }
                                    break;
                                case ActiveUniformType.BoolVec3:
                                case ActiveUniformType.IntVec3:
                                    for (int uniformIndex = firstUniformIndex; uniformIndex < lastUniformIndex; ++uniformIndex)
                                    {
                                        GL.Uniform3(uniformIndex, 1, (int*)currentData);
                                        currentData += 16; // Each array element is spaced by 16 bytes
                                    }
                                    break;
                                case ActiveUniformType.BoolVec4:
                                case ActiveUniformType.IntVec4:
                                    GL.Uniform4(firstUniformIndex, uniform.Count, (int*)currentData);
                                    break;
                                default:
                                    throw new NotImplementedException();
                            }
                        }
                    }
                }                
            }
#endif
        }

        private void SetBlendStateImpl(BlendState blendState, Color4 blendFactor, int multiSampleMask = -1)
        {
#if DEBUG
            EnsureContextActive();
#endif

            if (multiSampleMask != -1)
                throw new NotImplementedException();

            if (blendState == null)
                blendState = BlendStates.Default;

            if (boundBlendState != blendState)
            {
                blendState.Apply(boundBlendState ?? BlendStates.Default);
                boundBlendState = blendState;
            }

            GL.BlendColor(blendFactor.R, blendFactor.G, blendFactor.B, blendFactor.A);
        }

        /// <summary>
        /// Sets a constant buffer to the shader pipeline.
        /// </summary>
        /// <param name="stage">The shader stage.</param>
        /// <param name="slot">The binding slot.</param>
        /// <param name="buffer">The constant buffer to set.</param>
        internal void SetConstantBuffer(ShaderStage stage, int slot, Buffer buffer)
        {
#if DEBUG
            EnsureContextActive();
#endif

#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
            // TODO: Dirty flags on both constant buffer content and if constant buffer changed
            if (IsOpenGLES2)
            {
                if (stage != ShaderStage.Vertex || slot != 0)
                    throw new InvalidOperationException("Only cbuffer slot 0 of vertex shader stage should be used on OpenGL ES 2.0.");

                constantBuffer = buffer;
            }
            else
#endif
            {
                GL.BindBufferBase(BufferRangeTarget.UniformBuffer, slot, buffer != null ? buffer.resourceId : 0);
            }
        }

        private void SetDepthStencilStateImpl(DepthStencilState depthStencilState, int stencilReference = 0)
        {
#if DEBUG
            EnsureContextActive();
#endif

            if (depthStencilState == null)
                depthStencilState = DepthStencilStates.Default;

            // Only apply a DepthStencilState if it is not already bound
            if (boundDepthStencilState != depthStencilState || boundStencilReference != stencilReference)
            {
                boundDepthStencilState = depthStencilState;
                boundStencilReference = stencilReference;
                boundDepthStencilState.Apply(stencilReference);
            }
        }

        private void SetRasterizerStateImpl(RasterizerState rasterizerState)
        {
#if DEBUG
            EnsureContextActive();
#endif

            if (rasterizerState == null)
                rasterizerState = RasterizerStates.CullBack;

            if (boundRasterizerState != rasterizerState)
            {
                boundRasterizerState = rasterizerState;
                boundRasterizerState.Apply();
            }
        }

        private void SetDepthAndRenderTargetsImpl(Texture depthStencilBuffer, params Texture[] renderTargets)
        {
            var renderTargetsLength = 0;
            if (renderTargets != null && renderTargets.Length > 0 && renderTargets[0] != null)
            {
                renderTargetsLength = renderTargets.Length;
                // ensure size is coherent
                var expectedWidth = renderTargets[0].Width;
                var expectedHeight = renderTargets[0].Height;
                if (depthStencilBuffer != null)
                {
                    if (expectedWidth != depthStencilBuffer.Width || expectedHeight != depthStencilBuffer.Height)
                        throw new Exception("Depth buffer is not the same size as the render target");
                }
                for (int i = 1; i < renderTargets.Length; ++i)
                {
                    if (renderTargets[i] != null && (expectedWidth != renderTargets[i].Width || expectedHeight != renderTargets[i].Height))
                        throw new Exception("Render targets do nt have the same size");
                }
            }

            flipRenderTarget = ChooseFlipRenderTarget(depthStencilBuffer, renderTargets);

#if DEBUG
            EnsureContextActive();
#endif
            for (int i = 0; i < renderTargetsLength; ++i)
                boundRenderTargets[i] = renderTargets[i];
            for (int i = renderTargetsLength; i < boundRenderTargets.Length; ++i)
                boundRenderTargets[i] = null;

            boundDepthStencilBuffer = depthStencilBuffer;

            needUpdateFBO = true;

            SetupTargets();

            var renderTarget = renderTargetsLength > 0 ? renderTargets[0] : null;
            if (renderTarget != null)
            {
                SetViewport(new Viewport(0, 0, renderTarget.Width, renderTarget.Height));
            }
            else if (depthStencilBuffer != null)
            {
                SetViewport(new Viewport(0, 0, depthStencilBuffer.Description.Width, depthStencilBuffer.Description.Height));
            }
        }

        /// <summary>
        /// Check if rendering has to be flipped.
        /// </summary>
        /// <param name="depthStencilBuffer">The depth buffer.</param>
        /// <param name="renderTargets">The render targets.</param>
        /// <returns>The value of flipRenderTarget.</returns>
        private bool ChooseFlipRenderTarget(Texture depthStencilBuffer, params Texture[] renderTargets)
        {
            // TODO: Only OpenGL renders to backbuffer directly and uses defaultRenderTarget, right now
            if (defaultRenderTarget != null)
                return true;

            if (renderTargets != null && renderTargets.Length > 0)
            {
                foreach (var rt in renderTargets)
                {
                    if (rt == BackBuffer)
                    {
                        return false;
                    }
                }
            }
            if (depthStencilBuffer == DepthStencilBuffer)
            {
                return false;
            }
            return true;
        }

        private void ResetTargetsImpl()
        {
            for (int i = 0; i < boundRenderTargets.Length; ++i)
                boundRenderTargets[i] = null;
        }

        /// <summary>
        /// Sets a sampler state to the shader pipeline.
        /// </summary>
        /// <param name="stage">The shader stage.</param>
        /// <param name="slot">The binding slot.</param>
        /// <param name="samplerState">The sampler state to set.</param>
        public void SetSamplerState(ShaderStage stage, int slot, SamplerState samplerState)
        {
#if DEBUG
            EnsureContextActive();
#endif

            samplerStates[slot] = samplerState;
        }

        /// <summary>
        /// Binds a single scissor rectangle to the rasterizer stage.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="top">The top.</param>
        /// <param name="right">The right.</param>
        /// <param name="bottom">The bottom.</param>
        public void SetScissorRectangles(int left, int top, int right, int bottom)
        {
#if DEBUG
            EnsureContextActive();
#endif
            _currentScissorRectangles[0].Left = left;
            _currentScissorRectangles[0].Top = top;
            _currentScissorRectangles[0].Width = right - left;
            _currentScissorRectangles[0].Height = bottom - top;
            
            UpdateScissor(_currentScissorRectangles[0]);
        }

        /// <summary>
        /// Binds a set of scissor rectangles to the rasterizer stage.
        /// </summary>
        /// <param name="scissorRectangles">The set of scissor rectangles to bind.</param>
        public void SetScissorRectangles(params Rectangle[] scissorRectangles)
        {
#if DEBUG
            EnsureContextActive();
#endif
            
#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
            throw new NotImplementedException();
#else
            var scissorCount = scissorRectangles.Length > _currentScissorRectangles.Length ? _currentScissorRectangles.Length : scissorRectangles.Length;

            for (var i = 0; i < scissorCount; ++i)
                _currentScissorRectangles[i] = scissorRectangles[i];

            for (int i = 0; i < scissorCount; ++i)
            {
                var height = scissorRectangles[i].Height;
                _currentScissorsSetBuffer[4*i] = scissorRectangles[i].X;
                _currentScissorsSetBuffer[4 * i + 1] = GetScissorY(scissorRectangles[i].Y, height);
                _currentScissorsSetBuffer[4 * i + 2] = scissorRectangles[i].Width;
                _currentScissorsSetBuffer[4 * i + 3] = height;
            }

            GL.ScissorArray(0, scissorCount, _currentScissorsSetBuffer);
#endif
        }

        private void UpdateScissor(Rectangle scissorRect)
        {
            var height = scissorRect.Height;
            GL.Scissor(scissorRect.Left, GetScissorY(scissorRect.Bottom, height), scissorRect.Right - scissorRect.Left, height);
        }

        private int GetScissorY(int scissorY, int scissorHeight)
        {
            // if we flip the render target, we should modify the scissor accordingly
            if (flipRenderTarget)
                return scissorY;
            return boundFBOHeight - scissorY - scissorHeight;
        }

        /// <summary>
        /// Sets a shader resource view to the shader pipeline.
        /// </summary>
        /// <param name="stage">The shader stage.</param>
        /// <param name="slot">The binding slot.</param>
        /// <param name="shaderResourceView">The shader resource view.</param>
        internal void SetShaderResourceView(ShaderStage stage, int slot, GraphicsResource shaderResourceView)
        {
#if DEBUG
            EnsureContextActive();
#endif
            if (textures[slot] != shaderResourceView)
            {
                textures[slot] = shaderResourceView as Texture;
            }
        }

        /// <inheritdoc/>
        public void SetStreamTargets(params Buffer[] buffers)
        {
#if DEBUG
            EnsureContextActive();
#endif

            throw new NotImplementedException();
        }

        /// <summary>
        /// Sets an unordered access view to the shader pipeline.
        /// </summary>
        /// <param name="stage">The stage.</param>
        /// <param name="slot">The slot.</param>
        /// <param name="unorderedAccessView">The unordered access view.</param>
        /// <exception cref="System.ArgumentException">Invalid stage.;stage</exception>
        internal void SetUnorderedAccessView(ShaderStage stage, int slot, GraphicsResource unorderedAccessView)
        {
#if DEBUG
            EnsureContextActive();
#endif

            if (stage != ShaderStage.Compute)
                throw new ArgumentException("Invalid stage.", "stage");

            throw new NotImplementedException();
        }

        internal void SetupTargets()
        {
            if (needUpdateFBO)
            {
                boundFBO = FindOrCreateFBO(boundDepthStencilBuffer, boundRenderTargets);
            }
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, boundFBO);

            UpdateHasRenderTarget();
            UpdateHasDepthStencilBuffer();

            // Update glViewport (new height)
            if (boundRenderTargets[0] != null)
                boundFBOHeight = (boundRenderTargets[0].Description).Height;
            else if (boundDepthStencilBuffer != null)
                boundFBOHeight = (boundDepthStencilBuffer.Description).Height;
            else
                boundFBOHeight = 0;

            // TODO: support multiple viewports and scissors?
            UpdateViewport(currentState.Viewports[0]);
            UpdateScissor(_currentScissorRectangles[0]);
        }

        private void UpdateHasRenderTarget()
        {
            var hadRenderTarget = hasRenderTarget;
            hasRenderTarget = boundFBO != 0 || boundRenderTargets[0] != null;

            if (hasRenderTarget != hadRenderTarget)
            {
                var blendState = boundBlendState ?? BlendStates.Default;
                blendState.ApplyColorMask();
            }
        }

        private void UpdateHasDepthStencilBuffer()
        {
            var hadDepthStencilBuffer = hasDepthStencilBuffer;
            hasDepthStencilBuffer = boundFBO != 0 || boundDepthStencilBuffer != null;

            if (hasDepthStencilBuffer != hadDepthStencilBuffer)
            {
                var depthStencilState = boundDepthStencilState ?? DepthStencilStates.Default;
                depthStencilState.ApplyDepthMask();
            }
        }

        public void SetVertexArrayObject(VertexArrayObject vertexArrayObject)
        {
            currentVertexArrayObject = vertexArrayObject;
        }

        internal void UnbindVertexArrayObject()
        {
            boundVertexArrayObject = null;
            if (HasVAO)
            {
#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
                if (IsOpenGLES2)
                    OpenTK.Graphics.ES20.GL.Oes.BindVertexArray(0);
                else
#endif
                {
                    GL.BindVertexArray(0);
                }
            }

            // Disable all vertex attribs
            int currentVertexAttribIndex = 0;
            while (enabledVertexAttribArrays != 0)
            {
                if ((enabledVertexAttribArrays & 1) == 1)
                {
                    GL.DisableVertexAttribArray(currentVertexAttribIndex);
                }

                currentVertexAttribIndex++;
                enabledVertexAttribArrays >>= 1;
            }
        }

        private void SetViewportImpl()
        {
#if DEBUG
            EnsureContextActive();
#endif

            if (!needViewportUpdate)
                return;
            needViewportUpdate = false;

#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
            // TODO: Check all non-empty viewports are identical and match what is active in FBO!
            UpdateViewport(currentState.Viewports[0]);
#else
            UpdateViewports();
#endif
        }

        private void UpdateViewport(Viewport viewport)
        {
            GL.Viewport((int)viewport.X, (int)GetViewportY(viewport), (int)viewport.Width, (int)viewport.Height);
        }

#if !SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
        private void UpdateViewports()
        {
            int nbViewports = currentState.Viewports.Length;
            for (int i = 0; i < nbViewports; ++i)
            {
                var currViewport = currentState.Viewports[i];
                _currentViewportsSetBuffer[4 * i] = currViewport.X;
                _currentViewportsSetBuffer[4 * i + 1] = GetViewportY(currViewport);
                _currentViewportsSetBuffer[4 * i + 2] = currViewport.Width;
                _currentViewportsSetBuffer[4 * i + 3] = currViewport.Height;
            }
            GL.ViewportArray(0, nbViewports, _currentViewportsSetBuffer);
        }
#endif

        private float GetViewportY(Viewport viewport)
        {
            // if we flip the render target, we should modify the viewport accordingly
            if (flipRenderTarget)
                return viewport.Y;
            return boundFBOHeight - viewport.Y - viewport.Height;
        }

        internal int TryCompileShader(ShaderType shaderType, string sourceCode)
        {
            int shaderGL = GL.CreateShader(shaderType);
            GL.ShaderSource(shaderGL, sourceCode);
            GL.CompileShader(shaderGL);

            var log = GL.GetShaderInfoLog(shaderGL);

            int compileStatus;
            GL.GetShader(shaderGL, ShaderParameter.CompileStatus, out compileStatus);

            if (compileStatus != 1)
                throw new InvalidOperationException("Error while compiling GLSL shader: \n" + log);

            return shaderGL;
        }

        public void UnmapSubresource(MappedResource unmapped)
        {
#if DEBUG
            EnsureContextActive();
#endif

            var texture = unmapped.Resource as Texture;
            if (texture != null)
            {
                if (texture.Description.Usage == GraphicsResourceUsage.Staging)
                {
#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
                    // unmapping on OpenGL ES 2 means doing nothing since the buffer is on the CPU memory
                    if (!IsOpenGLES2)
#endif
                    {
                        GL.BindBuffer(BufferTarget.PixelPackBuffer, texture.PixelBufferObjectId);
                        GL.UnmapBuffer(BufferTarget.PixelPackBuffer);
                        GL.BindBuffer(BufferTarget.PixelPackBuffer, 0);
                    }
                }
#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
                else if (!IsOpenGLES2 && texture.Description.Usage == GraphicsResourceUsage.Dynamic)
#else
                else if (texture.Description.Usage == GraphicsResourceUsage.Dynamic)
#endif
                {
                    GL.BindBuffer(BufferTarget.PixelUnpackBuffer, texture.PixelBufferObjectId);
                    GL.UnmapBuffer(BufferTarget.PixelUnpackBuffer);

                    GL.BindTexture(texture.Target, texture.ResourceId);

                    // Bind buffer to texture
                    switch (texture.Target)
                    {
#if !SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
                        case TextureTarget.Texture1D:
                            GL.TexSubImage1D(TextureTarget.Texture1D, 0, 0, texture.Width, texture.FormatGl, texture.Type, IntPtr.Zero);
                            break;
#endif
                        case TextureTarget.Texture2D:
                            GL.TexSubImage2D(TextureTargetTexture2D, 0, 0, 0, texture.Width, texture.Height, texture.FormatGl, texture.Type, IntPtr.Zero);
                            break;
                        case TextureTarget.Texture3D:
                            GL.TexSubImage3D(TextureTargetTexture3D, 0, 0, 0, 0, texture.Width, texture.Height, texture.Depth, texture.FormatGl, texture.Type, IntPtr.Zero);
                            break;
                        default:
                            throw new NotSupportedException("Invalid texture target: " + texture.Target);
                    }
                    GL.BindTexture(texture.Target, 0);
                    GL.BindBuffer(BufferTarget.PixelUnpackBuffer, 0);
                }
                else
                {
                    throw new NotSupportedException("Not supported mapper operation for Usage: " + texture.Description.Usage);
                }
            }
            else
            {
                var buffer = unmapped.Resource as Buffer;
                if (buffer != null)
                {
#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
                    if (IsOpenGLES2)
                    {
                        // Only buffer with StagingData (fake cbuffer) could be mapped
                        if (buffer.StagingData == null)
                            throw new InvalidOperationException();

                        // Is it a real buffer? (fake cbuffer have no real GPU counter-part in OpenGL ES 2.0
                        if (buffer.ResourceId != 0)
                        {
                            UnbindVertexArrayObject();
                            GL.BindBuffer(buffer.bufferTarget, buffer.ResourceId);
                            GL.BufferSubData(buffer.bufferTarget, (IntPtr)unmapped.OffsetInBytes, (IntPtr)unmapped.SizeInBytes, unmapped.DataBox.DataPointer);
                            GL.BindBuffer(buffer.bufferTarget, 0);
                        }
                    }
                    else
#endif
                    {
                        UnbindVertexArrayObject();
                        GL.BindBuffer(buffer.bufferTarget, buffer.ResourceId);
                        GL.UnmapBuffer(buffer.bufferTarget);
                        GL.BindBuffer(buffer.bufferTarget, 0);
                    }
                }
                else // neither texture nor buffer
                {
                    throw new NotImplementedException("UnmapSubresource not implemented for type " + unmapped.Resource.GetType());
                }
            }
        }

        public void UnsetReadWriteBuffers()
        {
#if DEBUG
            EnsureContextActive();
#endif
        }

        public void UnsetRenderTargets()
        {
#if DEBUG
            EnsureContextActive();
#endif

            SetDepthAndRenderTargets((Texture)null, null);
        }

        internal void UpdateSubresource(GraphicsResource resource, int subResourceIndex, DataBox databox)
        {
#if DEBUG
            EnsureContextActive();
#endif
            var buffer = resource as Buffer;
            if (buffer != null)
            {
#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
                if (buffer.StagingData != IntPtr.Zero)
                {
                    // Specific case for constant buffers
                    SiliconStudio.Core.Utilities.CopyMemory(buffer.StagingData, databox.DataPointer, buffer.Description.SizeInBytes);
                    return;
                }
#endif

                UnbindVertexArrayObject();

                GL.BindBuffer(buffer.bufferTarget, buffer.ResourceId);
                GL.BufferData(buffer.bufferTarget, (IntPtr)buffer.Description.SizeInBytes, databox.DataPointer,
                    buffer.bufferUsageHint);
                GL.BindBuffer(buffer.bufferTarget, 0);
            }
            else
            {
                var texture = resource as Texture;
                if (texture != null)
                {
                    if (activeTexture != 0)
                    {
                        activeTexture = 0;
                        GL.ActiveTexture(TextureUnit.Texture0);
                    }

                    // TODO: Handle pitchs
                    // TODO: handle other texture formats
                    var desc = texture.Description;
                    GL.BindTexture(TextureTarget.Texture2D, texture.ResourceId);
                    boundTextures[0] = null; // bound active texture 0 has changed
                    GL.TexImage2D(TextureTargetTexture2D, subResourceIndex, (PixelInternalFormat_TextureComponentCount)texture.InternalFormat, desc.Width, desc.Height, 0, texture.FormatGl, texture.Type, databox.DataPointer);
                }
                else // neither texture nor buffer
                {
                    throw new NotImplementedException("UpdateSubresource not implemented for type " + resource.GetType());
                }
            }
        }

        internal void UpdateSubresource(GraphicsResource resource, int subResourceIndex, DataBox databox, ResourceRegion region)
        {
#if DEBUG
            EnsureContextActive();
#endif
            var texture = resource as Texture;

            if (texture != null)
            {
                var width = region.Right - region.Left;
                var height = region.Bottom - region.Top;

                // determine the opengl read Unpack Alignment
                var packAlignment = 0;
                if ((databox.RowPitch & 1) != 0)
                {
                    if (databox.RowPitch == width)
                        packAlignment = 1; 
                }
                else if ((databox.RowPitch & 2) != 0)
                {
                    var diff = databox.RowPitch - width;
                    if (diff >= 0 && diff < 2)
                        packAlignment = 2;
                }
                else if ((databox.RowPitch & 4) != 0)
                {
                    var diff = databox.RowPitch - width;
                    if (diff >= 0 && diff < 4)
                        packAlignment = 4;
                }
                else if ((databox.RowPitch & 8) != 0)
                {
                    var diff = databox.RowPitch - width;
                    if (diff >= 0 && diff < 8)
                        packAlignment = 8;
                }
                else if(databox.RowPitch == width)
                {
                    packAlignment = 4;
                }
                if(packAlignment == 0)
                    throw new NotImplementedException("The data box RowPitch is not compatible with the region width. This requires additional copy to be implemented.");

                // change the Unpack Alignment
                int previousPackAlignment;
                GL.GetInteger(GetPName.UnpackAlignment, out previousPackAlignment);
                GL.PixelStore(PixelStoreParameter.UnpackAlignment, packAlignment);

                if (activeTexture != 0)
                {
                    activeTexture = 0;
                    GL.ActiveTexture(TextureUnit.Texture0);
                }
                
                // Update the texture region
                GL.BindTexture(texture.Target, texture.resourceId);
                GL.TexSubImage2D((TextureTarget_TextureTarget2d)texture.Target, subResourceIndex, region.Left, region.Top, width, height, texture.FormatGl, texture.Type, databox.DataPointer);
                boundTextures[0] = null; // bound active texture 0 has changed

                // reset the Unpack Alignment
                GL.PixelStore(PixelStoreParameter.UnpackAlignment, previousPackAlignment);
            }
        }

        internal static void UnbindGraphicsContext(IGraphicsContext graphicsContext)
        {
            graphicsContext.MakeCurrent(null);

#if SILICONSTUDIO_PLATFORM_IOS
            // Seems like iPhoneOSGraphicsContext.MakeCurrent(null) doesn't remove current context
            // Let's do it manually
            OpenGLES.EAGLContext.SetCurrentContext(null);
#endif
        }

        private void OnApplicationPaused(object sender, EventArgs e)
        {
            // Block async resource creation
            Monitor.Enter(asyncCreationLockObject, ref asyncCreationLockTaken);

            ApplicationPaused = true;

            using (UseOpenGLCreationContext())
            {
                GL.Finish();
            }

            // Unset graphics context
            UnbindGraphicsContext(graphicsContext);
        }

        private void OnApplicationResumed(object sender, EventArgs e)
        {
            windowInfo = gameWindow.WindowInfo;

            // Reenable graphics context
            graphicsContext.MakeCurrent(windowInfo);

            ApplicationPaused = false;

            // Reenable async resource creation
            if (asyncCreationLockTaken)
            {
                Monitor.Exit(asyncCreationLockObject);
                asyncCreationLockTaken = false;
            }
        }

        protected void InitializePlatformDevice(GraphicsProfile[] graphicsProfiles, DeviceCreationFlags deviceCreationFlags, WindowHandle windowHandle)
        {
#if SILICONSTUDIO_PLATFORM_LINUX || SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP
#if SILICONSTUDIO_XENKO_UI_SDL
            gameWindow = (SiliconStudio.Xenko.Graphics.SDL.Window) windowHandle.NativeHandle;
            graphicsContext = gameWindow.OpenGLContext;
#else
            gameWindow = (OpenTK.GameWindow)windowHandle.NativeHandle;
            graphicsContext = gameWindow.Context;
#endif
#elif SILICONSTUDIO_PLATFORM_ANDROID
            // Force a reference to AndroidGameView from OpenTK 0.9, otherwise linking will fail in release mode for MonoDroid.
            typeof (opentkold::OpenTK.Platform.Android.AndroidGameView).ToString();
            gameWindow = (AndroidGameView)windowHandle.NativeHandle;
            graphicsContext = gameWindow.GraphicsContext;
            gameWindow.Load += OnApplicationResumed;
            gameWindow.Unload += OnApplicationPaused;
#elif SILICONSTUDIO_PLATFORM_IOS
            gameWindow = (iPhoneOSGameView)windowHandle.NativeHandle;
            graphicsContext = gameWindow.GraphicsContext;
            gameWindow.Load += OnApplicationResumed;
            gameWindow.Unload += OnApplicationPaused;
#endif

            windowInfo = gameWindow.WindowInfo;

            // Enable OpenGL context sharing
            GraphicsContext.ShareContexts = true;

            // TODO: How to control Debug flags?
            var creationFlags = GraphicsContextFlags.Default;

            // set default values
            versionMajor = 1;
            versionMinor = 0;

            var requestedGraphicsProfile = GraphicsProfile.Level_9_1;

            // Find the first profile that is compatible with current GL version
            foreach (var graphicsProfile in graphicsProfiles)
            {
                if (Adapter.IsProfileSupported(graphicsProfile))
                {
                    requestedGraphicsProfile = graphicsProfile;
                    break;
                }
            }

            // Find back OpenGL version from requested version
            OpenGLUtils.GetGLVersion(requestedGraphicsProfile, out versionMajor, out versionMinor);

            // check what is actually created
            if (!OpenGLUtils.GetCurrentGLVersion(out currentVersionMajor, out currentVersionMinor))
            {
                currentVersionMajor = versionMajor;
                currentVersionMinor = versionMinor;
            }

#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
            IsOpenGLES2 = (versionMajor < 3);
            creationFlags |= GraphicsContextFlags.Embedded;
#endif

            // Doesn't seems to be working on Android
#if SILICONSTUDIO_PLATFORM_ANDROID
            var renderer = GL.GetString(StringName.Renderer);
            Workaround_VAO_PowerVR_SGX_540 = renderer == "PowerVR SGX 540";
            Workaround_Context_Tegra2_Tegra3 = renderer == "NVIDIA Tegra 3" || renderer == "NVIDIA Tegra 2";

            var androidGraphicsContext = (AndroidGraphicsContext)graphicsContext;
            if (Workaround_Context_Tegra2_Tegra3)
            {
                // On Tegra2/Tegra3, we can't do any background context
                // As a result, we reuse main graphics context even when loading.
                // Of course, main graphics context need to be either available, or we register ourself for next ExecutePendingTasks().
                deviceCreationContext = graphicsContext;
                deviceCreationWindowInfo = windowInfo;

                // We don't want context to be set or it might collide with our internal use to create async resources
                // TODO: Reenabled, since the context seems to change otherwise. Do we need this in the first place, since we only want a single context?
                //gameWindow.AutoSetContextOnRenderFrame = false;
            }
            else
            {
                if (androidAsyncDeviceCreationContext != null)
                {
                    androidAsyncDeviceCreationContext.Dispose();
                    deviceCreationContext.Dispose();
                    deviceCreationWindowInfo.Dispose();
                }
                androidAsyncDeviceCreationContext = new AndroidAsyncGraphicsContext(androidGraphicsContext, (AndroidWindow)windowInfo, versionMajor);
                deviceCreationContext = OpenTK.Graphics.GraphicsContext.CreateDummyContext(androidAsyncDeviceCreationContext.Context);
                deviceCreationWindowInfo = OpenTK.Platform.Utilities.CreateDummyWindowInfo();
            }

            graphicsContextEglPtr = EglGetCurrentContext();
#elif SILICONSTUDIO_PLATFORM_IOS
            var asyncContext = new OpenGLES.EAGLContext(IsOpenGLES2 ? OpenGLES.EAGLRenderingAPI.OpenGLES2 : OpenGLES.EAGLRenderingAPI.OpenGLES3, gameWindow.EAGLContext.ShareGroup);
            OpenGLES.EAGLContext.SetCurrentContext(asyncContext);
            deviceCreationContext = new OpenTK.Graphics.GraphicsContext(new OpenTK.ContextHandle(asyncContext.Handle), null, graphicsContext, versionMajor, versionMinor, creationFlags);
            deviceCreationWindowInfo = windowInfo;
            gameWindow.MakeCurrent();
#else
            deviceCreationWindowInfo = windowInfo;
            deviceCreationContext = new GraphicsContext(graphicsContext.GraphicsMode, deviceCreationWindowInfo, versionMajor, versionMinor, creationFlags);
            GraphicsContext.CurrentContext.MakeCurrent(null);
#endif

            // Create default OpenGL State objects
            defaultSamplerState = SamplerState.New(this, new SamplerStateDescription(TextureFilter.MinPointMagMipLinear, TextureAddressMode.Wrap) { MaxAnisotropy = 1 }).DisposeBy(this);

            this.immediateContext = this;
        }

        protected void DestroyPlatformDevice()
        {
            // Hack: Reset the lock so that UseOpenGLCreationContext works (even if locked by previously called OnApplicationPaused, which might have been done in an unaccessible event thread)
            // TODO: Does it work with Tegra3?
            if (ApplicationPaused)
            {
                asyncCreationLockObject = new object();
            }

#if SILICONSTUDIO_PLATFORM_ANDROID || SILICONSTUDIO_PLATFORM_IOS
            gameWindow.Load -= OnApplicationResumed;
            gameWindow.Unload -= OnApplicationPaused;
#endif
        }

        internal void OnDestroyed()
        {
            EffectInputSignature.OnDestroyed();

            // Clear existing FBOs
            lock (RootDevice.existingFBOs)
            {
                RootDevice.existingFBOs.Clear();
                RootDevice.existingFBOs[new FBOKey(windowProvidedDepthTexture, new[] { windowProvidedRenderTexture })] = windowProvidedFrameBuffer;
            }

            // Clear bound states
            for (int i = 0; i < boundTextures.Length; ++i)
                boundTextures[i] = null;

            boundFrontFace = FrontFaceDirection.Ccw;

            boundVertexArrayObject = null;
            enabledVertexAttribArrays = 0;
            boundDepthStencilState = null;
            boundStencilReference = 0;
            boundBlendState = null;
            boundRasterizerState = null;
            boundDepthStencilBuffer = null;

            for (int i = 0; i < boundRenderTargets.Length; ++i)
                boundRenderTargets[i] = null;

            boundFBO = 0;
            boundFBOHeight = 0;
            boundProgram = 0;
        }

        private void SetDefaultStates()
        {
            Begin();
            SetDepthStencilState(null);
            currentFrontFace = FrontFaceDirection.Cw;
            boundFrontFace = FrontFaceDirection.Cw;
            GL.FrontFace(currentFrontFace);
            End();
        }

        internal void InitDefaultRenderTarget(PresentationParameters presentationParameters)
        {
// TODO: Provide unified ClientSize from GameWindow
#if SILICONSTUDIO_PLATFORM_IOS
            windowProvidedFrameBuffer = gameWindow.Framebuffer;

            // Scale for Retina display
            var width = (int)(gameWindow.Size.Width * gameWindow.ContentScaleFactor);
            var height = (int)(gameWindow.Size.Height * gameWindow.ContentScaleFactor);
#else
#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLCORE
            var width = gameWindow.ClientSize.Width;
            var height = gameWindow.ClientSize.Height;
#else
            var width = gameWindow.Size.Width;
            var height = gameWindow.Size.Height;
#endif
            windowProvidedFrameBuffer = 0;
#endif

            boundFBO = windowProvidedFrameBuffer;
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, windowProvidedFrameBuffer);

            // TODO: iOS (and possibly other platforms): get real render buffer ID for color/depth?
            windowProvidedRenderTexture = Texture.New2D(this, width, height, 1,
                // TODO: As a workaround, because OpenTK(+OpenGLES) doesn't support to create SRgb backbuffer, we fake it by creating a non-SRgb here and CopyScaler2D is responsible to transform it to non SRgb
                presentationParameters.BackBufferFormat.IsSRgb() ? presentationParameters.BackBufferFormat.ToNonSRgb() : presentationParameters.BackBufferFormat, TextureFlags.RenderTarget | Texture.TextureFlagsCustomResourceId);
            windowProvidedRenderTexture.Reload = graphicsResource => { };

            // Extract FBO render target
            int renderTargetTextureId;
            GL.GetFramebufferAttachmentParameter(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, FramebufferParameterName.FramebufferAttachmentObjectName, out renderTargetTextureId);
            windowProvidedRenderTexture.resourceId = renderTargetTextureId;

#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLCORE
            windowProvidedDepthTexture = Texture.New2D(this, width, height, 1, presentationParameters.DepthStencilFormat, TextureFlags.DepthStencil | Texture.TextureFlagsCustomResourceId);
            windowProvidedDepthTexture.Reload = graphicsResource => { };

            // Extract FBO depth target
            GL.GetFramebufferAttachmentParameter(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, FramebufferParameterName.FramebufferAttachmentObjectName, out renderTargetTextureId);
            windowProvidedDepthTexture.resourceId = renderTargetTextureId;
#endif

            RootDevice.existingFBOs[new FBOKey(windowProvidedDepthTexture, new[] { windowProvidedRenderTexture })] = windowProvidedFrameBuffer;

            // TODO: Provide some flags to choose user prefers either:
            // - Auto-Blitting while allowing default RenderTarget to be associable with any DepthStencil
            // - No blitting, but default RenderTarget won't work with a custom FBO
            // - Later we should be able to detect that automatically?
            //defaultRenderTarget = Texture.New2D(this, presentationParameters.BackBufferWidth, presentationParameters.BackBufferHeight, PixelFormat.R8G8B8A8_UNorm, TextureFlags.ShaderResource | TextureFlags.RenderTarget).ToRenderTarget();
#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLCORE
            defaultRenderTarget = windowProvidedRenderTexture;
#endif
        }

        public GraphicsDevice ImmediateContext
        {
            get { return this.immediateContext; }
        }

        public bool IsDeferredContextSupported
        {
            get { return false; }
        }

        private class SwapChainBackend
        {
            public PresentationParameters PresentationParameters;
            public int PresentCount;
        }

        /// <summary>
        /// Creates a swap chain from presentation parameters.
        /// </summary>
        /// <param name="presentationParameters">The presentation parameters.</param>
        /// <returns></returns>
        private SwapChainBackend CreateSwapChainBackend(PresentationParameters presentationParameters)
        {
            var swapChainBackend = new SwapChainBackend();
            return swapChainBackend;
        }

        /// <summary>
        /// Gets the default presentation parameters associated with this graphics device.
        /// </summary>
        public PresentationParameters PresentationParameters
        {
            get
            {
                if (_defaultSwapChainBackend == null) throw new InvalidOperationException(FrameworkResources.NoDefaultRenterTarget);
                return _defaultSwapChainBackend.PresentationParameters;
            }
        }

        /// <summary>
        /// Gets the default render target associated with this graphics device.
        /// </summary>
        /// <value>The default render target.</value>
        internal Texture DefaultRenderTarget => defaultRenderTarget;

        /// <summary>
        /// Presents the display with the contents of the next buffer in the sequence of back buffers owned by the GraphicsDevice.
        /// </summary>
        /*public void Present()
        {
            ImmediateContext.Copy(DefaultRenderTarget.Texture, windowProvidedRenderTarget.Texture);
#if SILICONSTUDIO_PLATFORM_ANDROID
            ((AndroidGraphicsContext)graphicsContext).Swap();
#else
            GraphicsContext.CurrentContext.SwapBuffers();
#endif
            //throw new NotImplementedException();
        }*/
        /// <summary>
        /// Gets or sets a value indicating whether this GraphicsDevice is in fullscreen.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this GraphicsDevice is fullscreen; otherwise, <c>false</c>.
        /// </value>
        public bool IsFullScreen
        {
            get
            {
#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP || SILICONSTUDIO_PLATFORM_LINUX
                return gameWindow.WindowState == WindowState.Fullscreen;
#else
                throw new NotImplementedException();
#endif
            }

            set
            {
#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP || SILICONSTUDIO_PLATFORM_LINUX
                if (value ^ (gameWindow.WindowState == WindowState.Fullscreen))
                    gameWindow.WindowState = value ? WindowState.Fullscreen : WindowState.Normal;
#else
                throw new NotImplementedException();
#endif
            }
        }

        // Notify render state that we used first texture and that it needs to be bound again
        internal void UseTemporaryFirstTexture()
        {
            activeTexture = 0;
            GL.ActiveTexture(TextureUnit.Texture0);
            boundTextures[0] = null;
        }

#if SILICONSTUDIO_PLATFORM_ANDROID
        // Execute pending asynchronous object creation
        // (on android devices where we can't create background context such as Tegra2/Tegra3)
        internal void ExecutePendingTasks()
        {
            // Unbind context
            graphicsContext.MakeCurrent(null);

            // Release and reacquire lock
            Monitor.Wait(asyncCreationLockObject);

            // Rebind context
            graphicsContext.MakeCurrent(windowInfo);
        }
#endif

        internal struct FBOKey : IEquatable<FBOKey>
        {
            public readonly Texture DepthStencilBuffer;
            public readonly Texture[] RenderTargets;
            public readonly int LastRenderTarget;

            public FBOKey(Texture depthStencilBuffer, Texture[] renderTargets)
            {
                DepthStencilBuffer = depthStencilBuffer;

                LastRenderTarget = 0;
                if (renderTargets != null)
                {
                    for (int i = 0; i < renderTargets.Length; ++i)
                    {
                        if (renderTargets[i] != null)
                        {
                            LastRenderTarget = i + 1;
                            break;
                        }
                    }
                }

                RenderTargets = LastRenderTarget != 0 ? renderTargets : null;
            }

            public bool Equals(FBOKey obj2)
            {
                if (obj2.DepthStencilBuffer != DepthStencilBuffer) return false;

                // Should have same number of render targets
                if (LastRenderTarget != obj2.LastRenderTarget)
                    return false;

                // Since both object have same LastRenderTarget, array is valid at least until this spot.
                for (int i = 0; i < LastRenderTarget; ++i)
                    if (obj2.RenderTargets[i] != RenderTargets[i])
                        return false;

                return true;
            }

            public override bool Equals(object obj)
            {
                if (!(obj is FBOKey)) return false;

                var obj2 = (FBOKey)obj;

                return Equals(obj2);
            }

            public override int GetHashCode()
            {
                var result = DepthStencilBuffer != null ? DepthStencilBuffer.GetHashCode() : 0;
                if (RenderTargets != null)
                {
                    for (int index = 0; index < LastRenderTarget; index++)
                    {
                        var renderTarget = RenderTargets[index];
                        result ^= renderTarget != null ? renderTarget.GetHashCode() : 0;
                    }
                }
                return result;
            }
        }
    }
}

#endif
