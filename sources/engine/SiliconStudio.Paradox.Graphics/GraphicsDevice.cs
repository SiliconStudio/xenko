// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Graphics.Internals;

namespace SiliconStudio.Paradox.Graphics
{
    /// <summary>
    ///     Performs primitive-based rendering, creates resources, handles system-level variables, adjusts gamma ramp levels, and creates shaders. See <see cref="The+GraphicsDevice+class"/> to learn more about the class.
    /// </summary>
    public partial class GraphicsDevice : ComponentBase
    {
        public static readonly int ThreadCount = 1; //AppConfig.GetConfiguration<Config>("RenderSystem").ThreadCount;

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

        private readonly Dictionary<object, IDisposable> sharedDataPerDevice;
        private readonly Dictionary<object, IDisposable> sharedDataPerDeviceContext = new Dictionary<object, IDisposable>();
        private VertexArrayObject newVertexArrayObject;
        private GraphicsPresenter presenter;

        private PrimitiveQuad primitiveQuad;

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

            // Create a new graphics device
            Features = new GraphicsDeviceFeatures(this);

            InitializeFactories();

            if (SamplerStates == null)
            {
                SamplerStates = new SamplerStateFactory(this);
                BlendStates = new BlendStateFactory(this);
                RasterizerStates = new RasterizerStateFactory(this);
                DepthStencilStates = new DepthStencilStateFactory(this);
            }

            SetDefaultStates();
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
            if (DepthStencilBuffer != null)
                DepthStencilBuffer.Dispose();
            primitiveQuad.Dispose();

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
        public delegate T CreateSharedData<out T>() where T : class, IDisposable;

        /// <summary>
        ///     Gets the adapter this instance is attached to.
        /// </summary>
        public GraphicsAdapter Adapter { get; private set; }

        /// <summary>
        ///     Gets the back buffer sets by the current <see cref="Presenter" /> setup on this device.
        /// </summary>
        /// <value>
        ///     The back buffer. The returned value may be null if no <see cref="GraphicsPresenter" /> are setup on this device.
        /// </value>
        public Texture BackBuffer
        {
            get
            {
                return Presenter != null ? Presenter.BackBuffer : null;
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
        ///     Gets the depth stencil buffer sets by the current <see cref="Presenter" /> setup on this device.
        /// </summary>
        /// <value>
        ///     The depth stencil buffer. The returned value may be null if no <see cref="GraphicsPresenter" /> are setup on this device or no depth buffer was allocated.
        /// </value>
        public Texture DepthStencilBuffer
        {
            get
            {
                return Presenter != null ? Presenter.DepthStencilBuffer : null;
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
                    localValue = sharedDataCreator();
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
    }
}