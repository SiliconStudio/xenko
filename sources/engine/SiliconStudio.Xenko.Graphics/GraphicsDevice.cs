// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Rendering;

namespace SiliconStudio.Xenko.Graphics
{
    /// <summary>
    /// Used for GPU resources creation (buffers, textures, states, shaders), and <see cref="CommandList"/> manipulations.
    /// </summary>
    public partial class GraphicsDevice : ComponentBase
    {
        public static readonly int ThreadCount = 1; //AppConfig.GetConfiguration<Config>("RenderSystem").ThreadCount;

        internal readonly Dictionary<SamplerStateDescription, SamplerState> CachedSamplerStates = new Dictionary<SamplerStateDescription, SamplerState>();

        /// <summary>
        ///     Gets the features supported by this graphics device.
        /// </summary>
        public GraphicsDeviceFeatures Features;

        internal HashSet<GraphicsResourceBase> Resources = new HashSet<GraphicsResourceBase>();

        internal readonly bool NeedWorkAroundForUpdateSubResource;
        internal Effect CurrentEffect;
        private readonly bool isDeferred;

        private readonly List<IDisposable> sharedDataToDispose = new List<IDisposable>();
        private readonly Dictionary<object, IDisposable> sharedDataPerDevice;
        private readonly Dictionary<object, IDisposable> sharedDataPerDeviceContext = new Dictionary<object, IDisposable>();
        private GraphicsPresenter presenter;

        internal PipelineState DefaultPipelineState;

        internal PrimitiveQuad PrimitiveQuad;
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
            // Setup IsDeferred to false for the main device
            isDeferred = false;

            // Create shared data
            sharedDataPerDevice = new Dictionary<object, IDisposable>();

            Recreate(adapter, profile, deviceCreationFlags, windowHandle);

            // Helpers
            PrimitiveQuad = new PrimitiveQuad(this);
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

            InitializePostFeatures();

            SamplerStates = new SamplerStateFactory(this);

            var defaultPipelineStateDescription = new PipelineStateDescription();
            defaultPipelineStateDescription.SetDefaults();
            AdjustDefaultPipelineStateDescription(ref defaultPipelineStateDescription);
            DefaultPipelineState = PipelineState.New(this, ref defaultPipelineStateDescription);
        }

        protected override void Destroy()
        {
            // Clear shared data
            for (int index = sharedDataToDispose.Count - 1; index >= 0; index--)
                sharedDataToDispose[index].Dispose();
            sharedDataPerDevice.Clear();
            sharedDataPerDeviceContext.Clear();

            SamplerStates.Dispose();
            SamplerStates = null;

            DefaultPipelineState.Dispose();

            PrimitiveQuad.Dispose();

            // Notify listeners
            if (Disposing != null)
                Disposing(this, EventArgs.Empty);

            // Destroy resources
            lock (Resources)
            {
                foreach (var resource in Resources)
                {
                    // Destroy leftover resources (note: should not happen if ResumeManager.OnDestroyed has properly been called)
                    if (resource.LifetimeState != GraphicsResourceLifetimeState.Destroyed)
                    {
                        resource.OnDestroyed();
                        resource.LifetimeState = GraphicsResourceLifetimeState.Destroyed;
                    }

                    // Remove Reload code in case it was preventing objects from being GC
                    resource.Reload = null;
                }
                Resources.Clear();
            }

            DestroyPlatformDevice();

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
        ///     Gets or sets the current presenter used to display the frame.
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
            }
        }

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

                    sharedDataToDispose.Add(localValue);
                    dictionary.Add(key, localValue);
                }
                return (T)localValue;
            }
        }
    }
}