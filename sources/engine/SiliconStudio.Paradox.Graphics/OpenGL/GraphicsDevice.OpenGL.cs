// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_PARADOX_GRAPHICS_API_OPENGL 
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
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Effects.Modules;
using SiliconStudio.Paradox.Shaders;
using Color4 = SiliconStudio.Core.Mathematics.Color4;
#if SILICONSTUDIO_PLATFORM_ANDROID
using System.Text;
using System.Runtime.InteropServices;
using OpenTK.Platform.Android;
#elif SILICONSTUDIO_PLATFORM_IOS
using OpenTK.Platform.iPhoneOS;
#endif
#if SILICONSTUDIO_PARADOX_GRAPHICS_API_OPENGLES
using OpenTK.Graphics.ES30;
using FramebufferAttachment = OpenTK.Graphics.ES30.FramebufferSlot;
#else
using OpenTK.Graphics.OpenGL;
#endif

namespace SiliconStudio.Paradox.Graphics
{
    /// <summary>
    /// Performs primitive-based rendering, creates resources, handles system-level variables, adjusts gamma ramp levels, and creates shaders.
    /// </summary>
    public partial class GraphicsDevice
    {
        // Used when locking asyncCreationLockObject
        private bool asyncCreationLockTaken;

        internal bool ApplicationPaused = false;

        internal IWindowInfo deviceCreationWindowInfo;
        internal object asyncCreationLockObject = new object();
        internal OpenTK.Graphics.IGraphicsContext deviceCreationContext;

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
        internal int versionMajor, versionMinor;
        internal RenderTarget windowProvidedRenderTarget;
        internal Texture2D windowProvidedRenderTexture;
        internal DepthStencilBuffer windowProvidedDepthBuffer;
        internal Texture2D windowProvidedDepthTexture;

        internal bool HasVAO;

#if SILICONSTUDIO_PARADOX_GRAPHICS_API_OPENGLES
        internal bool HasDepth24;
        internal bool HasPackedDepthStencilExtension;
        internal bool HasExtTextureFormatBGRA8888;
#endif

        private int windowProvidedFrameBuffer;

        private RenderTarget defaultRenderTarget;
        private GraphicsDevice immediateContext;
        private GraphicsAdapter _adapter;
        private SwapChainBackend _defaultSwapChainBackend;
        private Viewport[] _currentViewports = new Viewport[16];
        private int contextBeginCounter = 0;

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

#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP
        private OpenTK.GameWindow gameWindow;
#elif  SILICONSTUDIO_PLATFORM_ANDROID
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
        private DepthStencilBuffer boundDepthStencilBuffer;
        private RenderTarget[] boundRenderTargets = new RenderTarget[16];
        private int boundFBO;
        internal bool hasRenderTarget, hasDepthStencilBuffer;
        private int boundFBOHeight;
        private int boundProgram = 0;
        private bool needUpdateFBO = true;
        private DrawElementsType drawElementsType;
        private int indexElementSize;
        private IntPtr indexBufferOffset;
        private bool flipRenderTarget = false;
        private FrontFaceDirection currentFrontFace = FrontFaceDirection.Ccw;
        private FrontFaceDirection boundFrontFace = FrontFaceDirection.Ccw;

#if SILICONSTUDIO_PLATFORM_ANDROID
        [DllImport("libEGL.dll", EntryPoint = "eglGetCurrentContext")]
        internal static extern IntPtr EglGetCurrentContext();
#endif
        internal EffectProgram effectProgram;
        private Texture[] boundTextures = new Texture[64];
        private Texture[] textures = new Texture[64];
        private SamplerState[] samplerStates = new SamplerState[64];

#if SILICONSTUDIO_PARADOX_GRAPHICS_API_OPENGLES
        private Buffer constantBuffer;

        // Need to change sampler state depending on if texture has mipmap or not during PreDraw
        private bool[] hasMipmaps = new bool[64];

        private int copyProgram = -1;
        private int copyProgramOffsetLocation = -1;
        private int copyProgramScaleLocation = -1;
        private float[] squareVertices = {
            0.0f, 0.0f,
            1.0f, 0.0f,
            0.0f, 1.0f, 
            1.0f, 1.0f,
        };
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

        /// <summary>
        /// Gets the first viewport.
        /// </summary>
        /// <value>The first viewport.</value>
        public Viewport Viewport
        {
            get
            {
#if DEBUG
                EnsureContextActive();
#endif

                return _currentViewports[0];
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
            effect.Parameters.Set(ShaderBaseKeys.ParadoxFlipRendertarget, flipRenderTarget ? -1.0f : 1.0f);
        }

        /// <summary>
        /// Marks context as active on the current thread.
        /// </summary>
        public void Begin()
        {
            ++contextBeginCounter;

#if  SILICONSTUDIO_PLATFORM_ANDROID
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

        public void Clear(DepthStencilBuffer depthStencilBuffer, DepthStencilClearOptions options, float depth = 1, byte stencil = 0)
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

        public void Clear(RenderTarget renderTarget, Color4 color)
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

            throw new NotImplementedException();
        }

        public unsafe void ClearReadWrite(Buffer buffer, Int4 value)
        {
#if DEBUG
            EnsureContextActive();
#endif

            throw new NotImplementedException();
        }

        public unsafe void ClearReadWrite(Buffer buffer, UInt4 value)
        {
#if DEBUG
            EnsureContextActive();
#endif

            throw new NotImplementedException();
        }

        public unsafe void ClearReadWrite(Texture texture, Vector4 value)
        {
#if DEBUG
            EnsureContextActive();
#endif

            throw new NotImplementedException();
        }

        public unsafe void ClearReadWrite(Texture texture, Int4 value)
        {
#if DEBUG
            EnsureContextActive();
#endif

            throw new NotImplementedException();
        }

        public unsafe void ClearReadWrite(Texture texture, UInt4 value)
        {
#if DEBUG
            EnsureContextActive();
#endif

            throw new NotImplementedException();
        }

        public void ClearState()
        {
#if DEBUG
            EnsureContextActive();
#endif
            UnbindVertexArrayObject();
            currentVertexArrayObject = null;

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
            SetRenderTarget(DepthStencilBuffer, BackBuffer);
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
            var sourceTexture = source as Texture2D;
            var destTexture = destination as Texture2D;

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
                if(sourceTexture.Width <= 16 || sourceTexture.Height <= 16)
                    throw new NotSupportedException("ReadPixels from texture smaller or equal to 16x16 pixels seems systematically to fails on some android devices (for exp: Galaxy S3)");

                if (dstX != 0 || dstY != 0 || dstZ != 0)
                    throw new NotSupportedException("ReadPixels from staging texture using non-zero destination is not supported");

                GL.Viewport(0, 0, destTexture.Description.Width, destTexture.Description.Height);
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, FindOrCreateFBO(source));
#if SILICONSTUDIO_PARADOX_GRAPHICS_API_OPENGLES
                GL.ReadPixels(sourceRectangle.Left, sourceRectangle.Top, sourceRectangle.Width, sourceRectangle.Height, destTexture.FormatGl, destTexture.Type, destTexture.StagingData);
#else
                GL.BindBuffer(BufferTarget.PixelPackBuffer, destTexture.ResourceId);
                GL.ReadPixels(sourceRectangle.Left, sourceRectangle.Top, sourceRectangle.Width, sourceRectangle.Height, destTexture.FormatGl, destTexture.Type, IntPtr.Zero);
                GL.BindBuffer(BufferTarget.PixelPackBuffer, 0);
#endif
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, boundFBO);
                GL.Viewport((int)_currentViewports[0].X, (int)_currentViewports[0].Y, (int)_currentViewports[0].Width, (int)_currentViewports[0].Height);
                return;
            }

#if SILICONSTUDIO_PARADOX_GRAPHICS_API_OPENGLES
            // Use rendering
            GL.Viewport(0, 0, destTexture.Description.Width, destTexture.Description.Height);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FindOrCreateFBO(destination));

            if (copyProgram == -1)
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

                // First initialization of shader program
                int vertexShader = TryCompileShader(ShaderType.VertexShader, copyVertexShaderSource);
                int fragmentShader = TryCompileShader(ShaderType.FragmentShader, copyFragmentShaderSource);

                copyProgram = GL.CreateProgram();
                GL.AttachShader(copyProgram, vertexShader);
                GL.AttachShader(copyProgram, fragmentShader);
                GL.BindAttribLocation(copyProgram, 0, "aPosition");
                GL.LinkProgram(copyProgram);

                int linkStatus;
                GL.GetProgram(copyProgram, ProgramParameter.LinkStatus, out linkStatus);

                if (linkStatus != 1)
                    throw new InvalidOperationException("Error while linking GLSL shaders.");

                GL.UseProgram(copyProgram);
#if SILICONSTUDIO_PLATFORM_ANDROID
                var textureLocation = GL.GetUniformLocation(copyProgram, new StringBuilder("s_texture"));
                copyProgramOffsetLocation = GL.GetUniformLocation(copyProgram, new StringBuilder("uOffset"));
                copyProgramScaleLocation = GL.GetUniformLocation(copyProgram, new StringBuilder("uScale"));
#else
                var textureLocation = GL.GetUniformLocation(copyProgram, "s_texture");
                copyProgramOffsetLocation = GL.GetUniformLocation(copyProgram, "uOffset");
                copyProgramScaleLocation = GL.GetUniformLocation(copyProgram, "uScale");
#endif
                GL.Uniform1(textureLocation, 0);
            }

            var regionSize = new Vector2(sourceRectangle.Width, sourceRectangle.Height);

            // Source
            var sourceSize = new Vector2(sourceTexture.Width, sourceTexture.Height);
            var sourceRegionLeftTop = new Vector2(sourceRectangle.Left, sourceRectangle.Top);
            var sourceScale = new Vector2(regionSize.X / sourceSize.X, regionSize.Y / sourceSize.Y);
            var sourceOffset = new Vector2(sourceRegionLeftTop.X / sourceSize.X, sourceRegionLeftTop.Y / sourceSize.Y);

            // Dest
            var destSize = new Vector2(destTexture.Width, destTexture.Height);
            var destRegionLeftTop = new Vector2(dstX, dstY);
            var destScale = new Vector2(regionSize.X / destSize.X, regionSize.Y / destSize.Y);
            var destOffset = new Vector2(destRegionLeftTop.X / destSize.X, destRegionLeftTop.Y / destSize.Y);

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
            
            GL.UseProgram(copyProgram);

            activeTexture = 0;
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, sourceTexture.resourceId);
            boundTextures[0] = null;
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            ((Texture)source).BoundSamplerState = SamplerStates.PointClamp;

            GL.EnableVertexAttribArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 0, squareVertices);
            GL.Uniform4(copyProgramOffsetLocation, sourceOffset.X, sourceOffset.Y, destOffset.X, destOffset.Y);
            GL.Uniform4(copyProgramScaleLocation, sourceScale.X, sourceScale.Y, destScale.X, destScale.Y);
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
            if(isStencilEnabled)
                GL.Enable(EnableCap.StencilTest);
            GL.ColorMask(enabledColors[0], enabledColors[1], enabledColors[2], enabledColors[3]);

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, boundFBO);
            GL.Viewport((int)_currentViewports[0].X, (int)_currentViewports[0].Y, (int)_currentViewports[0].Width, (int)_currentViewports[0].Height);
#else
            // "FindOrCreateFBO" set the frameBuffer on FBO creation -> those 2 calls cannot be made directly in the following "GL.BindFramebuffer" function calls (side effects)
            var sourceFBO = FindOrCreateFBO(source);    
            var destinationFBO = FindOrCreateFBO(destination);
            GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, sourceFBO);
            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, destinationFBO);
            GL.BlitFramebuffer(sourceRegion.Left, sourceRegion.Top, sourceRegion.Right, sourceRegion.Bottom,
                               dstX, dstY, dstX + sourceRegion.Right - sourceRegion.Left, dstY + sourceRegion.Bottom - sourceRegion.Top,
                               ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Nearest);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, boundFBO);
#endif
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

            throw new NotImplementedException();
        }

        public void Dispatch(Buffer indirectBuffer, int offsetInBytes)
        {
#if DEBUG
            EnsureContextActive();
#endif

            throw new NotImplementedException();
        }

        public void Draw(PrimitiveType primitiveType, int vertexCount, int startVertex = 0)
        {
#if DEBUG
            EnsureContextActive();
#endif

            PreDraw();
            GL.DrawArrays(primitiveType.ToOpenGL(), startVertex, vertexCount);
        }

        public void DrawAuto(PrimitiveType primitiveType)
        {
#if DEBUG
            EnsureContextActive();
#endif
            PreDraw();
            //GL.DrawArraysIndirect(primitiveType.ToOpenGL(), (IntPtr)0);
            throw new NotImplementedException();
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
#if SILICONSTUDIO_PARADOX_GRAPHICS_API_OPENGLES
            if(baseVertexLocation != 0)
                throw new NotSupportedException("DrawIndexed with no null baseVertexLocation is not supported on OpenGL ES2.");
            GL.DrawElements(primitiveType.ToOpenGL(), indexCount, drawElementsType, indexBufferOffset + (startIndexLocation * indexElementSize)); // conversion to IntPtr required on Android
#else
            GL.DrawElementsBaseVertex(primitiveType.ToOpenGL(), indexCount, drawElementsType, indexBufferOffset + (startIndexLocation * indexElementSize), baseVertexLocation);
#endif
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
#if SILICONSTUDIO_PARADOX_GRAPHICS_API_OPENGLES
            throw new NotImplementedException();
#else
            GL.DrawElementsInstancedBaseVertex(primitiveType.ToOpenGL(), indexCountPerInstance, DrawElementsType.UnsignedInt, (IntPtr)(startIndexLocation * indexElementSize), instanceCount, baseVertexLocation);
#endif
        }

        /// <summary>
        /// Draw indexed, instanced, GPU-generated primitives.
        /// </summary>
        /// <param name="primitiveType">Type of the primitive to draw.</param>
        /// <param name="argumentsBuffer">A buffer containing the GPU generated primitives.</param>
        /// <param name="alignedByteOffsetForArgs">Offset in <em>pBufferForArgs</em> to the start of the GPU generated primitives.</param>
        public void DrawIndexedInstanced(PrimitiveType primitiveType, Buffer argumentsBuffer, int alignedByteOffsetForArgs = 0)
        {
            if (argumentsBuffer == null) throw new ArgumentNullException("argumentsBuffer");

#if DEBUG
            EnsureContextActive();
#endif
            PreDraw();
            throw new NotImplementedException();
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
            //TODO: review code
            PreDraw();
#if SILICONSTUDIO_PARADOX_GRAPHICS_API_OPENGLES
            throw new NotImplementedException();
#else
            GL.DrawArraysInstanced(primitiveType.ToOpenGL(), startVertexLocation, vertexCountPerInstance, instanceCount);
#endif
        }

        /// <summary>
        /// Draw instanced, GPU-generated primitives.
        /// </summary>
        /// <param name="primitiveType">Type of the primitive to draw.</param>
        /// <param name="argumentsBuffer">An arguments buffer</param>
        /// <param name="alignedByteOffsetForArgs">Offset in <em>pBufferForArgs</em> to the start of the GPU generated primitives.</param>
        public void DrawInstanced(PrimitiveType primitiveType, Buffer argumentsBuffer, int alignedByteOffsetForArgs = 0)
        {
            if (argumentsBuffer == null) throw new ArgumentNullException("argumentsBuffer");

#if DEBUG
            EnsureContextActive();
#endif
            PreDraw();
            throw new NotImplementedException();
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

        public void EndProfile()
        {
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
            if (graphicsResource == RootDevice.windowProvidedRenderTarget
                || graphicsResource == RootDevice.windowProvidedRenderTexture)
                return windowProvidedFrameBuffer;

            if (graphicsResource is DepthStencilBuffer)
                return FindOrCreateFBO((DepthStencilBuffer)graphicsResource, null);
            if (graphicsResource is RenderTarget)
                return FindOrCreateFBO(null, new[] { (RenderTarget)graphicsResource });
            if (graphicsResource is Texture)
                return FindOrCreateFBO(null, new[] { ((Texture)graphicsResource).GetCachedRenderTarget() });

            throw new NotSupportedException();
        }

        internal int FindOrCreateFBO(DepthStencilBuffer depthStencilBuffer)
        {
            lock (RootDevice.existingFBOs)
            {
                foreach (var key in RootDevice.existingFBOs)
                {
                    if (key.Key.DepthStencilBuffer == depthStencilBuffer)
                        return key.Value;
                }
            } 
            
            return FindOrCreateFBO(depthStencilBuffer, null);
        }

        internal int FindOrCreateFBO(RenderTarget target)
        {
            lock (RootDevice.existingFBOs)
            {
                foreach (var key in RootDevice.existingFBOs)
                {
                    if (key.Key.LastRenderTarget == 1 && key.Key.RenderTargets[0] == target)
                        return key.Value;
                }
            }

            return FindOrCreateFBO(null, new[] { target });
        }

        internal int FindOrCreateFBO(DepthStencilBuffer depthStencilBuffer, RenderTarget[] renderTargets)
        {
            int framebufferId;

            // Check for existing FBO matching this configuration
            lock (RootDevice.existingFBOs)
            {
                var fboKey = new FBOKey(depthStencilBuffer, renderTargets);

                // Is it the default provided render target?
                // TODO: Need to disable some part of rendering if either is null
                var isProvidedDepthBuffer = (depthStencilBuffer == RootDevice.windowProvidedDepthBuffer);
                var isProvidedRenderTarget = (fboKey.LastRenderTarget == 1 && renderTargets[0] == RootDevice.windowProvidedRenderTarget);
                if ((isProvidedDepthBuffer || boundDepthStencilBuffer == null) && (isProvidedRenderTarget || fboKey.LastRenderTarget == 0)) // device provided framebuffer
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
                int firstRenderTargets = -1;
                if (renderTargets != null)
                {
                    for (int i = 0; i < renderTargets.Length; ++i)
                    {
                        // TODO: Add support for render buffer
                        if (renderTargets[i] != null)
                        {
                            firstRenderTargets = i;
                            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0 + i, TextureTarget.Texture2D, renderTargets[i].ResourceId, 0);
                        }
                    }
                }

#if !SILICONSTUDIO_PARADOX_GRAPHICS_API_OPENGLES
                GL.DrawBuffer(firstRenderTargets != -1 ? DrawBufferMode.ColorAttachment0 : DrawBufferMode.None);
                GL.ReadBuffer(firstRenderTargets != -1 ? ReadBufferMode.ColorAttachment0 : ReadBufferMode.None);
#endif

                if (depthStencilBuffer != null)
                {
#if !SILICONSTUDIO_PARADOX_GRAPHICS_API_OPENGLES
                    FramebufferAttachment attachmentType;
                    if (depthStencilBuffer.IsDepthBuffer && depthStencilBuffer.IsStencilBuffer)
                        attachmentType = FramebufferAttachment.DepthStencilAttachment;
                    else if(depthStencilBuffer.IsDepthBuffer)
                        attachmentType = FramebufferAttachment.DepthAttachment;
                    else
                        attachmentType = FramebufferAttachment.StencilAttachment;

                    if (depthStencilBuffer.Texture.IsRenderbuffer)
                        GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, attachmentType, RenderbufferTarget.Renderbuffer, depthStencilBuffer.ResourceId);
                    else
                        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, attachmentType, TextureTarget.Texture2D, depthStencilBuffer.ResourceId, 0);
#else
                    if (depthStencilBuffer.IsDepthBuffer)
                        GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferSlot.DepthAttachment, RenderbufferTarget.Renderbuffer, depthStencilBuffer.ResourceId);

                    // If stencil buffer is separate, it's resource id might be stored in depthStencilBuffer.Texture.ResouceIdStencil
                    if(depthStencilBuffer.IsStencilBuffer)
                        GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferSlot.StencilAttachment, RenderbufferTarget.Renderbuffer, depthStencilBuffer.Texture.ResouceIdStencil != 0 ? depthStencilBuffer.Texture.ResouceIdStencil : depthStencilBuffer.ResourceId);
#endif
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

#if !SILICONSTUDIO_PARADOX_GRAPHICS_API_OPENGLES
            BufferAccess bufferAccess;
            switch (mapMode)
            {
                case MapMode.Read:
                    bufferAccess = BufferAccess.ReadOnly;
                    break;
                case MapMode.Write:
                case MapMode.WriteDiscard:
                case MapMode.WriteNoOverwrite:
                    bufferAccess = BufferAccess.WriteOnly;
                    break;
                case MapMode.ReadWrite:
                    bufferAccess = BufferAccess.ReadWrite;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("mapMode");
            }
#endif

            var buffer = resource as Buffer;
            if (buffer != null)
            {
                if (lengthInBytes == 0)
                    lengthInBytes = buffer.Description.SizeInBytes;

#if SILICONSTUDIO_PARADOX_GRAPHICS_API_OPENGLES
                if (buffer.StagingData != IntPtr.Zero)
                {
                    // TODO: Temporarily accept NoOverwrite as a discard
                    // Shouldn't do that, but for now it fix a big perf issue due to SpriteBatch use
                    //if (buffer.ResourceId != 0 && mapMode == MapMode.WriteDiscard)
                    //{
                    //    // Notify OpenGL ES driver that previous data can be discarded by setting a new empty buffer
                    //    UnbindVertexArrayObject();
                    //    GL.BindBuffer(buffer.bufferTarget, buffer.ResourceId);
                    //    GL.BufferData(buffer.bufferTarget, (IntPtr)buffer.Description.SizeInBytes, IntPtr.Zero, buffer.bufferUsageHint);
                    //    GL.BindBuffer(buffer.bufferTarget, 0);
                    //}

                    // Specific case for constant buffers
                    return new MappedResource(resource, subResourceIndex, new DataBox { DataPointer = buffer.StagingData + offsetInBytes, SlicePitch = 0, RowPitch = 0 }, offsetInBytes, lengthInBytes);
                }
#endif

#if SILICONSTUDIO_PARADOX_GRAPHICS_API_OPENGLES
                throw new NotImplementedException();
#else
                IntPtr mapResult = IntPtr.Zero;

                UnbindVertexArrayObject();
                GL.BindBuffer(buffer.bufferTarget, buffer.ResourceId);

                if (mapMode == MapMode.WriteDiscard)
                    mapResult = GL.MapBufferRange(buffer.bufferTarget, (IntPtr)offsetInBytes, (IntPtr)lengthInBytes, BufferAccessMask.MapWriteBit | BufferAccessMask.MapInvalidateBufferBit);
                else if (mapMode == MapMode.WriteNoOverwrite)
                    mapResult = GL.MapBufferRange(buffer.bufferTarget, (IntPtr)offsetInBytes, (IntPtr)lengthInBytes, BufferAccessMask.MapWriteBit | BufferAccessMask.MapUnsynchronizedBit);
                else
                    mapResult = GL.MapBuffer(buffer.bufferTarget, bufferAccess);
                
                GL.BindBuffer(buffer.bufferTarget, 0);

                return new MappedResource(resource, subResourceIndex, new DataBox { DataPointer = mapResult, SlicePitch = 0, RowPitch = 0 });
#endif
            }

            var texture = resource as Texture;
            if (texture != null)
            {
                if (mapMode == MapMode.Read)
                {
                    if (texture.Description.Usage != GraphicsResourceUsage.Staging)
                        throw new NotSupportedException("Only staging textures can be mapped.");

#if SILICONSTUDIO_PARADOX_GRAPHICS_API_OPENGLES
                    if (lengthInBytes == 0)
                        lengthInBytes = texture.DepthPitch;
                    return new MappedResource(resource, subResourceIndex, new DataBox { DataPointer = texture.StagingData + offsetInBytes, SlicePitch = texture.DepthPitch, RowPitch = texture.RowPitch }, offsetInBytes, lengthInBytes);
#else
                    GL.BindBuffer(BufferTarget.PixelPackBuffer, texture.ResourceId);
                    var mapResult = GL.MapBuffer(BufferTarget.PixelPackBuffer, bufferAccess);
                    GL.BindBuffer(BufferTarget.PixelPackBuffer, 0);
                    return new MappedResource(resource, subResourceIndex, new DataBox { DataPointer = mapResult, SlicePitch = texture.DepthPitch, RowPitch = texture.RowPitch });
#endif
                }
            }

            throw new NotImplementedException();
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
                var boundSamplerState = texture.BoundSamplerState ?? defaultSamplerState;
                var samplerState = samplerStates[textureInfo.TextureUnit] ?? SamplerStates.LinearClamp;

#if SILICONSTUDIO_PARADOX_GRAPHICS_API_OPENGLES
                bool hasMipmap = texture.Description.MipLevels > 1;
#else
                bool hasMipmap = false;
#endif

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
                        samplerState.Apply(hasMipmap, boundSamplerState);
                        texture.BoundSamplerState = samplerState;
                    }
                }
            }

            // Change face culling if the rendertarget is flipped
            var newFrontFace = currentFrontFace;
            if (flipRenderTarget)
                newFrontFace = newFrontFace == FrontFaceDirection.Cw ? FrontFaceDirection.Ccw : FrontFaceDirection.Cw;

            if (newFrontFace != boundFrontFace)
            {
                boundFrontFace = newFrontFace;
                GL.FrontFace(boundFrontFace);
            }
            
#if SILICONSTUDIO_PARADOX_GRAPHICS_API_OPENGLES
            unsafe
            {
                fixed(byte* boundUniforms = effectProgram.BoundUniforms)
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
#endif
        }

        public void SetBlendState(BlendState blendState)
        {
#if DEBUG
            EnsureContextActive();
#endif

            if (blendState == null)
                blendState = BlendStates.Default;

            if (boundBlendState != blendState)
            {
                blendState.Apply(boundBlendState ?? BlendStates.Default);
                boundBlendState = blendState;
            }
        }

        public void SetBlendState(BlendState blendState, Color blendFactor, int multiSampleMask = -1)
        {
#if DEBUG
            EnsureContextActive();
#endif

            if (multiSampleMask != -1)
                throw new NotImplementedException();

            SetBlendState(blendState);
            GL.BlendColor(blendFactor.R, blendFactor.G, blendFactor.B, blendFactor.A);
        }

        public void SetBlendState(BlendState blendState, Color blendFactor, uint multiSampleMask = 0xFFFFFFFF)
        {
#if DEBUG
            EnsureContextActive();
#endif

            SetBlendState(blendState, blendFactor, unchecked((int)multiSampleMask));
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

#if SILICONSTUDIO_PARADOX_GRAPHICS_API_OPENGLES
            // TODO: Dirty flags on both constant buffer content and if constant buffer changed
            if (stage != ShaderStage.Vertex || slot != 0)
                throw new InvalidOperationException("Only cbuffer slot 0 of vertex shader stage should be used on OpenGL ES 2.0.");
            
            constantBuffer = buffer;
#else
            GL.BindBufferBase(BufferTarget.UniformBuffer, slot, buffer != null ? buffer.resourceId : 0);
#endif
        }

        public void SetDepthStencilState(DepthStencilState depthStencilState, int stencilReference = 0)
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

        public void SetRasterizerState(RasterizerState rasterizerState)
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

        /// <summary>
        /// Sets a new depth stencil buffer and render target to this GraphicsDevice.
        /// </summary>
        /// <param name="depthStencilBuffer">The depth stencil buffer.</param>
        /// <param name="renderTarget">The render target.</param>
        public void SetRenderTarget(DepthStencilBuffer depthStencilBuffer, RenderTarget renderTarget)
        {
            SetRenderTargets(depthStencilBuffer, (renderTarget == null) ? null : new[] { renderTarget });

            if (renderTarget != null)
            {
                SetViewport(new Viewport(0, 0, renderTarget.Width, renderTarget.Height));
            }
            else if (depthStencilBuffer != null)
            {
                SetViewport(new Viewport(0, 0, depthStencilBuffer.Description.Width, depthStencilBuffer.Description.Height));
            }
        }

        public void SetRenderTargets(DepthStencilBuffer depthStencilBuffer, params RenderTarget[] renderTargets)
        {
            if (renderTargets == null)
            {
                throw new ArgumentNullException("renderTargets");
            }

            // ensure size is coherent
            var expectedWidth = renderTargets[0].Width;
            var expectedHeight = renderTargets[0].Height;
            if (depthStencilBuffer != null)
            {
                if (expectedWidth != depthStencilBuffer.Texture.Width || expectedHeight != depthStencilBuffer.Texture.Height)
                    throw new Exception("Depth buffer is not the same size as the render target");
            }
            for (int i = 1; i < renderTargets.Length; ++i)
            {
                if (expectedWidth != renderTargets[i].Width || expectedHeight != renderTargets[i].Height)
                    throw new Exception("Render targets do nt have the same size");
            }

            flipRenderTarget = true;
            foreach (var rt in renderTargets)
            {
                if (rt == BackBuffer)
                {
                    flipRenderTarget = false;
                    break;
                }
            }

#if DEBUG
            EnsureContextActive();
#endif

            for (int i = 0; i < renderTargets.Length; ++i)
                boundRenderTargets[i] = renderTargets[i];
            for (int i = renderTargets.Length; i < boundRenderTargets.Length; ++i)
                boundRenderTargets[i] = null;

            boundDepthStencilBuffer = depthStencilBuffer;

            needUpdateFBO = true;

            SetupTargets();

            var renderTarget = renderTargets.Length > 0 ? renderTargets[0] : null;
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
        /// Unbinds all depth-stencil buffer and render targets from the output-merger stage.
        /// </summary>
        public void ResetTargets()
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
            //TODO: verify the range of the values
            GL.Scissor(left, bottom, right-left, top-bottom);
        }

        /// <summary>
        /// Binds a set of scissor rectangles to the rasterizer stage.
        /// </summary>
        /// <param name="scissorRectangles">The set of scissor rectangles to bind.</param>
        public unsafe void SetScissorRectangles(params Rectangle[] scissorRectangles)
        {
#if DEBUG
            EnsureContextActive();
#endif
            
#if SILICONSTUDIO_PARADOX_GRAPHICS_API_OPENGLES
            throw new NotImplementedException();
#else
            //TODO: verify the range of the values
            var rectangleValues = new int[4*scissorRectangles.Length];

            for (int i = 0; i < scissorRectangles.Length; ++i)
            {
                rectangleValues[4*i] = scissorRectangles[i].X;
                rectangleValues[4*i + 1] = scissorRectangles[i].Y;
                rectangleValues[4*i + 2] = scissorRectangles[i].Width;
                rectangleValues[4*i + 3] = scissorRectangles[i].Height;
            }

            GL.ScissorArray(0, scissorRectangles.Length, rectangleValues);
#endif
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

            UpdateViewport(_currentViewports[0]);
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
#if SILICONSTUDIO_PARADOX_GRAPHICS_API_OPENGLES
                OpenTK.Graphics.ES20.GL.Oes.BindVertexArray(0);
#else
                GL.BindVertexArray(0);
#endif
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


        /// <summary>
        ///     Gets or sets the 1st viewport.
        /// </summary>
        /// <value>The viewport.</value>
        public void SetViewport(Viewport value)
        {
#if DEBUG
            EnsureContextActive();
#endif

            _currentViewports[0] = value;
            UpdateViewport(value);
        }

        public void SetViewport(int index, Viewport value)
        {
#if DEBUG
            EnsureContextActive();
#endif

#if !SILICONSTUDIO_PARADOX_GRAPHICS_API_OPENGLES
            _currentViewports[index] = value;
            UpdateViewports();
#endif
            
            throw new NotImplementedException();
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
                if (texture.Description.Usage != GraphicsResourceUsage.Staging)
                    throw new NotSupportedException("Only staging textures can be mapped.");

#if !SILICONSTUDIO_PARADOX_GRAPHICS_API_OPENGLES
                GL.BindBuffer(BufferTarget.PixelPackBuffer, texture.ResourceId);
                GL.UnmapBuffer(BufferTarget.PixelPackBuffer);
                GL.BindBuffer(BufferTarget.PixelPackBuffer, 0);
#endif
            }
            else
            {
                var buffer = unmapped.Resource as Buffer;
                if (buffer != null)
                {
#if SILICONSTUDIO_PARADOX_GRAPHICS_API_OPENGLES
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
#else
                    UnbindVertexArrayObject();
                    GL.BindBuffer(buffer.bufferTarget, buffer.ResourceId);
                    GL.UnmapBuffer(buffer.bufferTarget);
                    GL.BindBuffer(buffer.bufferTarget, 0);
#endif
                }
                else
                {
                    throw new NotImplementedException();
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

            SetRenderTargets((DepthStencilBuffer)null, null);
        }

        internal void UpdateSubresource(GraphicsResource resource, int subResourceIndex, DataBox databox)
        {
#if DEBUG
            EnsureContextActive();
#endif
            var buffer = resource as Buffer;
            if (buffer != null)
            {
#if SILICONSTUDIO_PARADOX_GRAPHICS_API_OPENGLES
                if (buffer.StagingData != IntPtr.Zero)
                {
                    // Specific case for constant buffers
                    SiliconStudio.Core.Utilities.CopyMemory(buffer.StagingData, databox.DataPointer, buffer.Description.SizeInBytes);
                    return;
                }
#endif

                UnbindVertexArrayObject();

                GL.BindBuffer(buffer.bufferTarget, buffer.ResourceId);
                GL.BufferData(buffer.bufferTarget, (IntPtr) buffer.Description.SizeInBytes, databox.DataPointer,
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
                    var desc = texture.Description;
                    GL.BindTexture(TextureTarget.Texture2D, texture.ResourceId);
                    boundTextures[0] = null;
                    GL.TexImage2D(TextureTarget.Texture2D, subResourceIndex, texture.InternalFormat, desc.Width, desc.Height, 0, texture.FormatGl, texture.Type, databox.DataPointer);
                }
                else
                {
                    throw new NotImplementedException();
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
                GL.TexSubImage2D(texture.Target, subResourceIndex, region.Left, region.Top, width, height, texture.FormatGl, texture.Type, databox.DataPointer);
                boundTextures[0] = null;

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
            MonoTouch.OpenGLES.EAGLContext.SetCurrentContext(null);
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

        internal void UpdateViewport(Viewport viewport)
        {
            GL.Viewport((int)viewport.X, boundFBOHeight - (int)viewport.Y - (int)viewport.Height, (int)viewport.Width, (int)viewport.Height);
        }

#if !SILICONSTUDIO_PARADOX_GRAPHICS_API_OPENGLES
        internal void UpdateViewports()
        {
            int nbViewports = _currentViewports.Length;
            float[] viewports = new float[nbViewports * 4];
            for (int i = 0; i < nbViewports; ++i)
            {
                var currViewport = _currentViewports[i];
                viewports[4 * i] = currViewport.X;
                viewports[4 * i + 1] = currViewport.Height - currViewport.Y;
                viewports[4 * i + 2] = currViewport.Width;
                viewports[4 * i + 3] = currViewport.Height;
            }
            GL.ViewportArray(0, nbViewports, viewports);
        }
#endif

        protected void InitializePlatformDevice(GraphicsProfile[] graphicsProfile, DeviceCreationFlags deviceCreationFlags, WindowHandle windowHandle)
        {
#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP
            gameWindow = (OpenTK.GameWindow)windowHandle.NativeHandle;
            graphicsContext = gameWindow.Context;
#elif  SILICONSTUDIO_PLATFORM_ANDROID
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
#if SILICONSTUDIO_PARADOX_GRAPHICS_API_OPENGLES
            versionMajor = 2;
            versionMinor = 0;
#if !SILICONSTUDIO_PLATFORM_MONO_MOBILE
            OpenTK.Platform.Utilities.ForceEmbedded = true;
#endif
            creationFlags |= GraphicsContextFlags.Embedded;
#else
            versionMajor = 4;
            versionMinor = 2;
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
                gameWindow.AutoSetContextOnRenderFrame = false;
            }
            else
            {
                if (androidAsyncDeviceCreationContext != null)
                {
                    androidAsyncDeviceCreationContext.Dispose();
                    deviceCreationContext.Dispose();
                    deviceCreationWindowInfo.Dispose();
                }
                androidAsyncDeviceCreationContext = new AndroidAsyncGraphicsContext(androidGraphicsContext, (AndroidWindow)windowInfo);
                deviceCreationContext = OpenTK.Graphics.GraphicsContext.CreateDummyContext(androidAsyncDeviceCreationContext.Context);
                deviceCreationWindowInfo = OpenTK.Platform.Utilities.CreateDummyWindowInfo();
            }

            graphicsContextEglPtr = EglGetCurrentContext();
#elif SILICONSTUDIO_PLATFORM_IOS
            var asyncContext = new MonoTouch.OpenGLES.EAGLContext(MonoTouch.OpenGLES.EAGLRenderingAPI.OpenGLES2, gameWindow.EAGLContext.ShareGroup);
            MonoTouch.OpenGLES.EAGLContext.SetCurrentContext(asyncContext);
            deviceCreationContext = new OpenTK.Graphics.GraphicsContext(new OpenTK.ContextHandle(asyncContext.Handle), null, graphicsContext, versionMajor, versionMinor, creationFlags);
            deviceCreationWindowInfo = windowInfo;
            gameWindow.MakeCurrent();
#else
            deviceCreationWindowInfo = windowInfo;
            deviceCreationContext = new GraphicsContext(graphicsContext.GraphicsMode, deviceCreationWindowInfo, versionMajor, versionMinor, creationFlags);
            GraphicsContext.CurrentContext.MakeCurrent(null);
#endif

            // Create default OpenGL State objects
            defaultSamplerState = SamplerState.New(this, new SamplerStateDescription(TextureFilter.MinPointMagMipLinear, TextureAddressMode.Wrap) { MaxAnisotropy = 1 }).KeepAliveBy(this);

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
                RootDevice.existingFBOs[new FBOKey(windowProvidedDepthBuffer, new[] { windowProvidedRenderTarget })] = windowProvidedFrameBuffer;
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
            // TODO: iOS (and possibly other platforms): get real render buffer ID for color/depth?
            windowProvidedRenderTexture = new Texture2D(this, new TextureDescription
                {
                    Dimension = TextureDimension.Texture2D,
                    Format = presentationParameters.BackBufferFormat,
                    Width = presentationParameters.BackBufferWidth,
                    Height = presentationParameters.BackBufferHeight,
                    Flags = TextureFlags.RenderTarget,
                    Depth = 1,
                    MipLevels = 1,
                    ArraySize = 1
                }, null, false);
            windowProvidedRenderTexture.Reload = (graphicsResource) => { };
            windowProvidedRenderTarget = windowProvidedRenderTexture.ToRenderTarget();

            if (presentationParameters.DepthStencilFormat != PixelFormat.None)
            {
                windowProvidedDepthTexture = new Texture2D(this, new TextureDescription
                    {
                        Dimension = TextureDimension.Texture2D,
                        Format = presentationParameters.DepthStencilFormat,
                        Width = presentationParameters.BackBufferWidth,
                        Height = presentationParameters.BackBufferHeight,
                        Flags = TextureFlags.DepthStencil,
                        Depth = 1,
                        MipLevels = 1,
                        ArraySize = 1
                    }, null, false);
                windowProvidedDepthTexture.Reload = (graphicsResource) => { };
                windowProvidedDepthBuffer = new DepthStencilBuffer(this, windowProvidedDepthTexture, false);
            }

#if SILICONSTUDIO_PLATFORM_IOS
            // TODO: This can probably be valid for Android/PC as well (everything 0)
            windowProvidedFrameBuffer = gameWindow.Framebuffer;
            boundFBO = windowProvidedFrameBuffer;

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, windowProvidedFrameBuffer);

            // Extract FBO render target
            int renderTargetTextureId;
            GL.GetFramebufferAttachmentParameter(FramebufferTarget.Framebuffer, FramebufferSlot.ColorAttachment0, FramebufferParameterName.FramebufferAttachmentObjectName, out renderTargetTextureId);
            windowProvidedRenderTexture.resourceId = renderTargetTextureId;
            windowProvidedRenderTexture.Reload = (graphicsResource) => { throw new NotImplementedException(); };
            windowProvidedRenderTarget.resourceId = renderTargetTextureId;

            // Extract FBO depth target
            GL.GetFramebufferAttachmentParameter(FramebufferTarget.Framebuffer, FramebufferSlot.DepthAttachment, FramebufferParameterName.FramebufferAttachmentObjectName, out renderTargetTextureId);
            windowProvidedDepthTexture.resourceId = renderTargetTextureId;
            windowProvidedDepthTexture.Reload = (graphicsResource) => { throw new NotImplementedException(); };
            windowProvidedDepthBuffer.resourceId = renderTargetTextureId;
#endif

            RootDevice.existingFBOs[new FBOKey(windowProvidedDepthBuffer, new[] { windowProvidedRenderTarget })] = windowProvidedFrameBuffer;

            // TODO: Provide some flags to choose user prefers either:
            // - Auto-Blitting while allowing default RenderTarget to be associable with any DepthStencil
            // - No blitting, but default RenderTarget won't work with a custom FBO
            // - Later we should be able to detect that automatically?
            //defaultRenderTarget = Texture2D.New(this, presentationParameters.BackBufferWidth, presentationParameters.BackBufferHeight, PixelFormat.R8G8B8A8_UNorm, TextureFlags.ShaderResource | TextureFlags.RenderTarget).ToRenderTarget();
            defaultRenderTarget = windowProvidedRenderTarget;
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
        SwapChainBackend CreateSwapChainBackend(PresentationParameters presentationParameters)
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
        internal RenderTarget DefaultRenderTarget
        {
            get
            {
                return defaultRenderTarget;
            }
        }

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
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
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

        internal struct FBOKey
        {
            public readonly DepthStencilBuffer DepthStencilBuffer;
            public readonly RenderTarget[] RenderTargets;
            public readonly int LastRenderTarget;

            public FBOKey(DepthStencilBuffer depthStencilBuffer, RenderTarget[] renderTargets)
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

            public override bool Equals(object obj)
            {
                if (!(obj is FBOKey)) return false;

                var obj2 = (FBOKey)obj;

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
