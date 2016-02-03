// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Graphics.Internals;

namespace SiliconStudio.Xenko.Graphics
{
    /// <summary>
    ///     Performs primitive-based rendering, creates resources, handles system-level variables, adjusts gamma ramp levels, and creates shaders. See <see cref="The+GraphicsDevice+class"/> to learn more about the class.
    /// </summary>
    public partial class GraphicsDevice : ComponentBase
    {
        public static readonly int ThreadCount = 1; //AppConfig.GetConfiguration<Config>("RenderSystem").ThreadCount;

        private const int MaxRenderTargetCount = 8;

        internal readonly Dictionary<SamplerStateDescription, SamplerState> CachedSamplerStates = new Dictionary<SamplerStateDescription, SamplerState>();
        internal readonly Dictionary<BlendStateDescription, BlendState> CachedBlendStates = new Dictionary<BlendStateDescription, BlendState>();
        internal readonly Dictionary<RasterizerStateDescription, RasterizerState> CachedRasterizerStates = new Dictionary<RasterizerStateDescription, RasterizerState>();
        internal readonly Dictionary<VertexArrayObject.Description, VertexArrayObject> CachedVertexArrayObjects = new Dictionary<VertexArrayObject.Description, VertexArrayObject>();

        /// <summary>
        ///     Gets the features supported by this graphics device.
        /// </summary>
        public GraphicsDeviceFeatures Features;

        internal HashSet<GraphicsResourceBase> Resources = new HashSet<GraphicsResourceBase>();

        internal readonly bool NeedWorkAroundForUpdateSubResource;
        internal readonly ShaderStageSetup StageStatus = new ShaderStageSetup();
        internal Effect CurrentEffect;
        private readonly bool isDeferred;
        private readonly ParameterCollection parameters = new ParameterCollection();

        private bool needViewportUpdate = true;

        private readonly Dictionary<object, IDisposable> sharedDataPerDevice;
        private readonly Dictionary<object, IDisposable> sharedDataPerDeviceContext = new Dictionary<object, IDisposable>();
        private GraphicsPresenter presenter;

        // Current states
        private StateAndTargets currentState;

        private int currentStateIndex;
        private readonly List<StateAndTargets> allocatedStates = new List<StateAndTargets>(10);
        private PrimitiveQuad primitiveQuad;
        private ColorSpace colorSpace;

        public uint FrameTriangleCount, FrameDrawCalls;
        public float BuffersMemory, TextureMemory;

        /// <summary>
        /// Gets the type of the platform that graphics device is using.
        /// </summary>
        public static GraphicsPlatform Platform => GraphicPlatform;

        public string RendererName => GetRendererName();

        /// <summary>
        ///     Initializes a new instance of the <see cref="GraphicsDevice" /> class.
        /// </summary>
        /// <param name="adapter">The graphics adapter.</param>
        /// <param name="profile">The graphics profile.</param>
        /// <param name="deviceCreationFlags">The device creation flags.</param>
        /// <param name="windowHandle">The window handle.</param>
        protected GraphicsDevice(GraphicsAdapter adapter, GraphicsProfile[] profile, DeviceCreationFlags deviceCreationFlags, WindowHandle windowHandle)
        {
            RootDevice = this;

            // Setup IsDeferred to false for the main device
            isDeferred = false;

            // Create shared data
            sharedDataPerDevice = new Dictionary<object, IDisposable>();

            Recreate(adapter, profile, deviceCreationFlags, windowHandle);

            // Helpers
            primitiveQuad = new PrimitiveQuad(this);
        }

        public void Recreate(GraphicsAdapter adapter, GraphicsProfile[] profile, DeviceCreationFlags deviceCreationFlags, WindowHandle windowHandle)
        {
            if (adapter == null) throw new ArgumentNullException("adapter");
            if (profile == null) throw new ArgumentNullException("profile");

            Adapter = adapter;
            IsDebugMode = (deviceCreationFlags & DeviceCreationFlags.Debug) != 0;

            // Initialize this instance
            InitializePlatformDevice(profile, deviceCreationFlags, windowHandle);

            InitializeFactories();

            // Create a new graphics device
            Features = new GraphicsDeviceFeatures(this);

            SamplerStates = new SamplerStateFactory(this);
            BlendStates = new BlendStateFactory(this);
            RasterizerStates = new RasterizerStateFactory(this);
            DepthStencilStates = new DepthStencilStateFactory(this);

            currentState = null;
            allocatedStates.Clear();
            currentStateIndex = -1;
            PushState();

            Begin();
            ClearState();
            End();
        }

        protected override void Destroy()
        {
            DestroyPlatformDevice();

            // Notify listeners
            if (Disposing != null)
                Disposing(this, EventArgs.Empty);

            SamplerStates.Dispose();
            BlendStates.Dispose();
            RasterizerStates.Dispose();
            DepthStencilStates.Dispose();
            if (DepthStencilBuffer != null)
                DepthStencilBuffer.Dispose();
            primitiveQuad.Dispose();

            SamplerStates = null;
            BlendStates = null;
            RasterizerStates = null;
            DepthStencilStates = null;

            base.Destroy();
        }

        /// <summary>
        /// Occurs while this component is disposing and before it is disposed.
        /// </summary>
        public event EventHandler<EventArgs> Disposing;

        /// <summary>
        ///     A delegate called to create shareable data. See remarks.
        /// </summary>
        /// <typeparam name="T">Type of the data to create.</typeparam>
        /// <returns>A new instance of the data to share.</returns>
        /// <remarks>
        ///     Because this method is being called from a lock region, this method should not be time consuming.
        /// </remarks>
        public delegate T CreateSharedData<out T>(GraphicsDevice device) where T : class, IDisposable;

        /// <summary>
        ///     Gets the adapter this instance is attached to.
        /// </summary>
        public GraphicsAdapter Adapter { get; private set; }

        /// <summary>
        ///     Gets the render target buffer currently sets on this instance.
        /// </summary>
        /// <value>
        ///     The render target buffer currently sets on this instance.
        /// </value>
        public Texture BackBuffer
        {
            get
            {
                return currentState.RenderTargets[0];
            }
        }

        /// <summary>
        ///     Gets the <see cref="BlendStates" /> factory.
        /// </summary>
        /// <value>
        ///     The <see cref="BlendStates" /> factory.
        /// </value>
        public BlendStateFactory BlendStates { get; private set; }

        /// <summary>
        ///     Gets the depth stencil buffer currently sets on this instance.
        /// </summary>
        /// <value>
        ///     The depth stencil buffer currently sets on this instance.
        /// </value>
        public Texture DepthStencilBuffer
        {
            get
            {
                return currentState.DepthStencilBuffer;
            }
        }

        /// <summary>
        ///     Gets the <see cref="DepthStencilStateFactory" /> factory.
        /// </summary>
        /// <value>
        ///     The <see cref="DepthStencilStateFactory" /> factory.
        /// </value>
        public DepthStencilStateFactory DepthStencilStates { get; private set; }

        /// <summary>
        ///     Gets a value indicating whether this instance is in debug mode.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is debug; otherwise, <c>false</c>.
        /// </value>
        public bool IsDebugMode { get; private set; }

        /// <summary>
        ///     Gets a value indicating whether this instance is a deferred graphics device context.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is deferred; otherwise, <c>false</c>.
        /// </value>
        public bool IsDeferred
        {
            get
            {
                return isDeferred;
            }
        }

        /// <summary>
        ///     Gets a value indicating whether this instance supports GPU markers and profiling.
        /// </summary>
        public bool IsProfilingSupported { get; private set; }

        /// <summary>
        /// Gets the default color space.
        /// </summary>
        /// <value>The default color space.</value>
        public ColorSpace ColorSpace
        {
            get { return Features.HasSRgb ? colorSpace : ColorSpace.Gamma; }
            set
            {
                colorSpace = value;
            }
        }

        /// <summary>
        ///     Gets the parameters attached to this particular device. This Parameters are used to override <see cref="Effect" /> parameters.
        /// </summary>
        /// <value>The parameters used to override all effects.</value>
        public ParameterCollection Parameters
        {
            get
            {
                return parameters;
            }
        }

        /// <summary>
        ///     Gets or sets the current presenter use by the <see cref="Present" /> method.
        /// </summary>
        /// <value>The current presenter.</value>
        public virtual GraphicsPresenter Presenter
        {
            get
            {
                return presenter;
            }
            set
            {
                presenter = value;
                if (presenter != null)
                {
                    Begin();
                    SetDepthAndRenderTargets(presenter.DepthStencilBuffer, presenter.BackBuffer);
                    SetViewport(presenter.DefaultViewport);
                    End();
                }
            }
        }

        /// <summary>
        ///     Gets the <see cref="RasterizerStates" /> factory.
        /// </summary>
        /// <value>
        ///     The <see cref="RasterizerStates" /> factory.
        /// </value>
        public RasterizerStateFactory RasterizerStates { get; private set; }

        /// <summary>
        ///     Gets the root device.
        /// </summary>
        /// <value>The root device.</value>
        public GraphicsDevice RootDevice { get; private set; }

        /// <summary>
        ///     Gets the <see cref="SamplerStateFactory" /> factory.
        /// </summary>
        /// <value>
        ///     The <see cref="SamplerStateFactory" /> factory.
        /// </value>
        public SamplerStateFactory SamplerStates { get; private set; }

        /// <summary>
        ///     Gets or sets the index of the thread.
        /// </summary>
        /// <value>The index of the thread.</value>
        public int ThreadIndex { get; internal set; }

        /// <summary>
        /// Gets the shader profile.
        /// </summary>
        /// <value>The shader profile.</value>
        internal GraphicsProfile? ShaderProfile { get; set; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="GraphicsDevice" /> class.
        /// </summary>
        /// <param name="creationFlags">The creation flags.</param>
        /// <param name="graphicsProfiles">The graphics profiles.</param>
        /// <returns>
        ///     An instance of <see cref="GraphicsDevice" />
        /// </returns>
        public static GraphicsDevice New(DeviceCreationFlags creationFlags = DeviceCreationFlags.None, params GraphicsProfile[] graphicsProfiles)
        {
            return New(GraphicsAdapterFactory.Default, creationFlags, graphicsProfiles);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphicsDevice" /> class.
        /// </summary>
        /// <param name="adapter">The adapter.</param>
        /// <param name="creationFlags">The creation flags.</param>
        /// <param name="graphicsProfiles">The graphics profiles.</param>
        /// <returns>An instance of <see cref="GraphicsDevice" /></returns>
        public static GraphicsDevice New(GraphicsAdapter adapter, DeviceCreationFlags creationFlags = DeviceCreationFlags.None, params GraphicsProfile[] graphicsProfiles)
        {
            return new GraphicsDevice(adapter ?? GraphicsAdapterFactory.Default, graphicsProfiles, creationFlags, null);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphicsDevice" /> class.
        /// </summary>
        /// <param name="adapter">The adapter.</param>
        /// <param name="creationFlags">The creation flags.</param>
        /// <param name="windowHandle">The window handle.</param>
        /// <param name="graphicsProfiles">The graphics profiles.</param>
        /// <returns>An instance of <see cref="GraphicsDevice" /></returns>
        public static GraphicsDevice New(GraphicsAdapter adapter, DeviceCreationFlags creationFlags = DeviceCreationFlags.None, WindowHandle windowHandle = null, params GraphicsProfile[] graphicsProfiles)
        {
            return new GraphicsDevice(adapter ?? GraphicsAdapterFactory.Default, graphicsProfiles, creationFlags, windowHandle);
        }


        /// <summary>
        ///     Gets the first viewport.
        /// </summary>
        /// <value>The first viewport.</value>
        public Viewport Viewport
        {
            get
            {
                return currentState.Viewports[0];
            }
        }

        /// <summary>
        /// Clears the state and restore the state of the device.
        /// </summary>
        public void ClearState()
        {
            ClearStateImpl();

            currentStateIndex = 0;
            currentState = allocatedStates[currentStateIndex];

            // Setup empty viewports
            for (int i = 0; i < currentState.Viewports.Length; i++)
                currentState.Viewports[i] = new Viewport();

            // Setup default states
            SetBlendState(BlendStates.Default);
            SetRasterizerState(RasterizerStates.CullBack);
            SetDepthStencilState(DepthStencilStates.Default);

            // Setup the default render target
            Texture depthStencilBuffer = null;
            Texture backBuffer = null;
            if (Presenter != null)
            {
                depthStencilBuffer = Presenter.DepthStencilBuffer;
                backBuffer = Presenter.BackBuffer;
            }
            needViewportUpdate = true;
            SetDepthAndRenderTarget(depthStencilBuffer, backBuffer);
        }

        /// <summary>
        /// Draws a full screen quad. An <see cref="Effect"/> must be applied before calling this method.
        /// </summary>
        public void DrawQuad()
        {
            primitiveQuad.Draw();
        }

        /// <summary>
        /// Draws a fullscreen texture using a <see cref="SamplerStateFactory.LinearClamp"/> sampler. See <see cref="Draw+a+texture"/> to learn how to use it.
        /// </summary>
        /// <param name="texture">The texture. Expecting an instance of <see cref="Texture"/>.</param>
        /// <param name="applyEffectStates">The flag to apply effect states.</param>
        public void DrawTexture(Texture texture, bool applyEffectStates = false)
        {
            DrawTexture(texture, null, Color4.White, applyEffectStates);
        }

        /// <summary>
        /// Draws a fullscreen texture using the specified sampler. See <see cref="Draw+a+texture"/> to learn how to use it.
        /// </summary>
        /// <param name="texture">The texture. Expecting an instance of <see cref="Texture"/>.</param>
        /// <param name="sampler">The sampler.</param>
        /// <param name="applyEffectStates">The flag to apply effect states.</param>
        public void DrawTexture(Texture texture, SamplerState sampler, bool applyEffectStates = false)
        {
            DrawTexture(texture, sampler, Color4.White, applyEffectStates);
        }

        /// <summary>
        /// Draws a fullscreen texture using a <see cref="SamplerStateFactory.LinearClamp"/> sampler
        /// and the texture color multiplied by a custom color. See <see cref="Draw+a+texture"/> to learn how to use it.
        /// </summary>
        /// <param name="texture">The texture. Expecting an instance of <see cref="Texture"/>.</param>
        /// <param name="color">The color.</param>
        /// <param name="applyEffectStates">The flag to apply effect states.</param>
        public void DrawTexture(Texture texture, Color4 color, bool applyEffectStates = false)
        {
            DrawTexture(texture, null, color, applyEffectStates);
        }

        /// <summary>
        /// Draws a fullscreen texture using the specified sampler
        /// and the texture color multiplied by a custom color. See <see cref="Draw+a+texture"/> to learn how to use it.
        /// </summary>
        /// <param name="texture">The texture. Expecting an instance of <see cref="Texture"/>.</param>
        /// <param name="sampler">The sampler.</param>
        /// <param name="color">The color.</param>
        /// <param name="applyEffectStates">The flag to apply effect states.</param>
        public void DrawTexture(Texture texture, SamplerState sampler, Color4 color, bool applyEffectStates = false)
        {
            primitiveQuad.Draw(texture, sampler, color, applyEffectStates);
        }

        /// <summary>
        ///     Presents the current Presenter.
        /// </summary>
        public void Present()
        {
            if (Presenter != null)
            {
                Presenter.Present();
            }
        }

        /// <summary>
        /// Set the blend state of the output-merger stage with a white default blend color and sample mask set to 0xffffffff. See <see cref="Render+states"/> to learn how to use it.
        /// </summary>
        /// <param name="blendState">a blend-state</param>
        public void SetBlendState(BlendState blendState)
        {
            SetBlendState(blendState, blendState == null ? Color.White : blendState.BlendFactor, blendState == null ? -1 : blendState.MultiSampleMask);
        }

        /// <summary>
        /// Set the blend state of the output-merger stage. See <see cref="Render+states"/> to learn how to use it.
        /// </summary>
        /// <param name="blendState">a blend-state</param>
        /// <param name="blendFactor">Blend factors, one for each RGBA component. This requires a blend state object that specifies the <see cref="Blend.BlendFactor" /></param>
        /// <param name="multiSampleMask">32-bit sample coverage. The default value is 0xffffffff.</param>
        public void SetBlendState(BlendState blendState, Color4 blendFactor, int multiSampleMask = -1)
        {
            currentState.BlendState = blendState;
            currentState.BlendFactor = blendFactor;
            currentState.BlendMultiSampleMask = multiSampleMask;
            SetBlendStateImpl(blendState, blendFactor, multiSampleMask);
        }

        /// <summary>
        /// Sets the depth-stencil state of the output-merger stage. See <see cref="Render+states"/> to learn how to use it.
        /// </summary>
        /// <param name="depthStencilState">a depth-stencil state</param>
        /// <param name="stencilReference">Reference value to perform against when doing a depth-stencil test.</param>
        public void SetDepthStencilState(DepthStencilState depthStencilState, int stencilReference = 0)
        {
            currentState.DepthStencilState = depthStencilState;
            currentState.StencilReference = stencilReference;
            SetDepthStencilStateImpl(depthStencilState, stencilReference);
        }

        /// <summary>
        /// Set the <strong>rasterizer state</strong> for the rasterizer stage of the pipeline. See <see cref="Render+states"/> to learn how to use it.
        /// </summary>
        /// <param name="rasterizerState">The rasterizser state to set on this device.</param>
        public void SetRasterizerState(RasterizerState rasterizerState)
        {
            currentState.RasterizerState = rasterizerState;
            SetRasterizerStateImpl(rasterizerState);
        }

        /// <summary>
        /// Set the blend state of the output-merger stage. See <see cref="Render+states"/> to learn how to use it.
        /// </summary>
        /// <param name="blendState">a blend-state</param>
        /// <param name="blendFactor">Blend factors, one for each RGBA component. This requires a blend state object that specifies the <see cref="Blend.BlendFactor" /></param>
        /// <param name="multiSampleMask">32-bit sample coverage. The default value is 0xffffffff.</param>
        public void SetBlendState(BlendState blendState, Color4 blendFactor, uint multiSampleMask = 0xFFFFFFFF)
        {
            SetBlendState(blendState, blendFactor, unchecked((int)multiSampleMask));
        }


        /// <summary>
        ///     Sets a new depthStencilBuffer to this GraphicsDevice. If there is any RenderTarget already bound, it will be unbinded. See <see cref="Textures+and+render+targets"/> to learn how to use it.
        /// </summary>
        /// <param name="depthStencilBuffer">The depth stencil.</param>
        public void SetDepthTarget(Texture depthStencilBuffer)
        {
            SetDepthAndRenderTarget(depthStencilBuffer, null);
        }

        /// <summary>
        /// Binds a single render target to the output-merger stage. See <see cref="Textures+and+render+targets"/> to learn how to use it.
        /// </summary>
        /// <param name="renderTargetView">A view of the render target to bind.</param>
        public void SetRenderTarget(Texture renderTargetView)
        {
            SetDepthAndRenderTarget(null, renderTargetView);
        }

        /// <summary>
        ///     <p>Bind one or more render targets atomically and the depth-stencil buffer to the output-merger stage. See <see cref="Textures+and+render+targets"/> to learn how to use it.</p>
        /// </summary>
        /// <param name="renderTargetViews">A set of render target views to bind.</param>
        public void SetRenderTargets(params Texture[] renderTargetViews)
        {
            SetDepthAndRenderTargets(null, renderTargetViews);
        }

        /// <summary>
        /// Unbinds all depth-stencil buffer and render targets from the output-merger stage.
        /// </summary>
        public void ResetTargets()
        {
            ResetTargetsImpl();

            currentState.DepthStencilBuffer = null;
            for (int i = 0; i < currentState.RenderTargets.Length; i++)
                currentState.RenderTargets[i] = null;
        }

        /// <summary>
        /// Sets the viewport for the first render target.
        /// </summary>
        /// <value>The viewport.</value>
        public void SetViewport(Viewport value)
        {
            SetViewport(0, value);
        }

        /// <summary>
        /// Sets the viewport for the specified render target.
        /// </summary>
        /// <value>The viewport.</value>
        public void SetViewport(int index, Viewport value)
        {
            currentState.Viewports[index] = value;
            needViewportUpdate = true;
        }

        /// <summary>
        /// Pushes the state of <see cref="DepthStencilState"/>, <see cref="RasterizerState"/>, <see cref="BlendState"/> and the Render targets bound to this instance. To restore the state, use <see cref="PopState"/>.
        /// </summary>
        public void PushState()
        {
            var previousState = currentState;

            // Check if we need to allocate a new StateAndTargets
            if (currentStateIndex == (allocatedStates.Count - 1))
            {
                currentState = new StateAndTargets();
                allocatedStates.Add(currentState);
            }
            currentStateIndex++;
            currentState = allocatedStates[currentStateIndex];
            currentState.Initialize(this, previousState);
        }

        /// <summary>
        /// Restore the state and targets.
        /// </summary>
        public void PopState()
        {
            if (currentStateIndex <= 0)
            {
                throw new InvalidOperationException("Cannot pop more than push");
            }
            currentStateIndex--;

            currentState = allocatedStates[currentStateIndex];
            currentState.Restore(this);
        }

        /// <summary>
        /// Binds a depth-stencil buffer and a single render target to the output-merger stage. See <see cref="Textures+and+render+targets"/> to learn how to use it.
        /// </summary>
        /// <param name="depthStencilView">A view of the depth-stencil buffer to bind.</param>
        /// <param name="renderTargetView">A view of the render target to bind.</param>
        public void SetDepthAndRenderTarget(Texture depthStencilView, Texture renderTargetView)
        {
            currentState.DepthStencilBuffer = depthStencilView;
            currentState.RenderTargets[0] = renderTargetView;

            // Clear the other render targets bound
            for (int i = 1; i < currentState.RenderTargets.Length; i++)
            {
                currentState.RenderTargets[i] = null;
            }

            CommonSetDepthAndRenderTargets(currentState.DepthStencilBuffer, currentState.RenderTargets);
        }

        /// <summary>
        /// Binds a depth-stencil buffer and a set of render targets to the output-merger stage. See <see cref="Textures+and+render+targets"/> to learn how to use it.
        /// </summary>
        /// <param name="depthStencilView">A view of the depth-stencil buffer to bind.</param>
        /// <param name="renderTargetViews">A set of render target views to bind.</param>
        /// <exception cref="System.ArgumentNullException">renderTargetViews</exception>
        public void SetDepthAndRenderTargets(Texture depthStencilView, params Texture[] renderTargetViews)
        {
            currentState.DepthStencilBuffer = depthStencilView;

            if (renderTargetViews != null)
            {
                for (int i = 0; i < currentState.RenderTargets.Length; i++)
                {
                    currentState.RenderTargets[i] = i < renderTargetViews.Length ? renderTargetViews[i] : null;
                }
            }
            else
            {
                for (int i = 0; i < currentState.RenderTargets.Length; i++)
                {
                    currentState.RenderTargets[i] = null;
                }
            }

            CommonSetDepthAndRenderTargets(currentState.DepthStencilBuffer, currentState.RenderTargets);
        }

        private void CommonSetDepthAndRenderTargets(Texture depthStencilView, Texture[] renderTargetViews)
        {
            if (depthStencilView != null)
            {
                SetViewport(new Viewport(0, 0, depthStencilView.ViewWidth, depthStencilView.ViewHeight));
            }
            else
            {
                // Setup the viewport from the rendertarget view
                foreach (var rtv in renderTargetViews)
                {
                    if (rtv != null)
                    {
                        SetViewport(new Viewport(0, 0, rtv.ViewWidth, rtv.ViewHeight));
                        break;
                    }
                }
            }

            SetDepthAndRenderTargetsImpl(depthStencilView, renderTargetViews);
        }


        /// <summary>
        ///     Gets a shared data for this device context with a delegate to create the shared data if it is not present.
        /// </summary>
        /// <typeparam name="T">Type of the shared data to get/create.</typeparam>
        /// <param name="type">Type of the data to share.</param>
        /// <param name="key">The key of the shared data.</param>
        /// <param name="sharedDataCreator">The shared data creator.</param>
        /// <returns>
        ///     An instance of the shared data. The shared data will be disposed by this <see cref="GraphicsDevice" /> instance.
        /// </returns>
        public T GetOrCreateSharedData<T>(GraphicsDeviceSharedDataType type, object key, CreateSharedData<T> sharedDataCreator) where T : class, IDisposable
        {
            Dictionary<object, IDisposable> dictionary = (type == GraphicsDeviceSharedDataType.PerDevice) ? sharedDataPerDevice : sharedDataPerDeviceContext;

            lock (dictionary)
            {
                IDisposable localValue;
                if (!dictionary.TryGetValue(key, out localValue))
                {
                    localValue = sharedDataCreator(this);
                    if (localValue == null)
                    {
                        return null;
                    }

                    localValue = localValue.DisposeBy(this);
                    dictionary.Add(key, localValue);
                }
                return (T)localValue;
            }
        }

        /// <summary>
        /// Holds blend, rasterizer and depth stencil, current viewports and render targets.
        /// </summary>
        private class StateAndTargets
        {
            public BlendState BlendState;
            public Color4 BlendFactor;
            public int BlendMultiSampleMask;

            public DepthStencilState DepthStencilState;
            public int StencilReference;

            public RasterizerState RasterizerState;

            public Viewport[] Viewports;

            public Texture DepthStencilBuffer;

            public Texture[] RenderTargets;

            public void Initialize(GraphicsDevice device, StateAndTargets parentState)
            {
                int renderTargetCount = MaxRenderTargetCount;
                switch (device.Features.Profile)
                {
                    case GraphicsProfile.Level_9_1:
                    case GraphicsProfile.Level_9_2:
                    case GraphicsProfile.Level_9_3:
                        renderTargetCount = 1;
                        break;
                }

                if (RenderTargets == null || RenderTargets.Length != renderTargetCount)
                {
                    RenderTargets = new Texture[renderTargetCount];
                    Viewports = new Viewport[renderTargetCount];
                }

                if (parentState != null)
                {
                    this.BlendState = parentState.BlendState;
                    this.BlendFactor = parentState.BlendFactor;
                    BlendMultiSampleMask = parentState.BlendMultiSampleMask;

                    this.DepthStencilState = parentState.DepthStencilState;
                    this.StencilReference = parentState.StencilReference;

                    DepthStencilBuffer = parentState.DepthStencilBuffer;

                    for (int i = 0; i < renderTargetCount; i++)
                    {
                        Viewports[i] = parentState.Viewports[i];
                        RenderTargets[i] = parentState.RenderTargets[i];
                    }
                }

                device.needViewportUpdate = true;
            }

            public void Restore(GraphicsDevice graphicsDevice)
            {
                graphicsDevice.SetDepthAndRenderTargets(DepthStencilBuffer, RenderTargets);

                graphicsDevice.SetBlendState(BlendState, BlendFactor, BlendMultiSampleMask);
                graphicsDevice.SetDepthStencilState(DepthStencilState, StencilReference);
                graphicsDevice.SetRasterizerState(RasterizerState);

                graphicsDevice.needViewportUpdate = true;
            }
        }
    }
}