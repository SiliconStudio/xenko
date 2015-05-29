// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
//
// Copyright (c) 2010-2013 SharpDX - Alexandre Mutel
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;

using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Core;
using SiliconStudio.Core.Serialization.Assets;

namespace SiliconStudio.Paradox.Games
{
    /// <summary>
    /// Base class for a <see cref="GameSystemBase"/> component.
    /// </summary>
    /// <remarks>
    /// A <see cref="GameSystemBase"/> component can be used to 
    /// </remarks>
    public abstract class GameSystemBase : ComponentBase, IGameSystemBase, IUpdateable, IDrawable, IContentable
    {
        private readonly IServiceRegistry registry;
        private int drawOrder;
        private bool enabled;
        private readonly GameBase game;
        private int updateOrder;
        private bool visible;
        private readonly IAssetManager assetManager;
        private IGraphicsDeviceService graphicsDeviceService;

        /// <summary>
        /// Initializes a new instance of the <see cref="GameSystemBase" /> class.
        /// </summary>
        /// <param name="registry">The registry.</param>
        /// <remarks>
        /// The GameSystem is expecting the following services to be registered: <see cref="IGame"/> and <see cref="IAssetManager"/>.
        /// </remarks>
        protected GameSystemBase(IServiceRegistry registry)
        {
            if (registry == null) throw new ArgumentNullException("registry");
            this.registry = registry;
            game = (GameBase)Services.GetServiceAs<IGame>();
            assetManager = Services.GetSafeServiceAs<IAssetManager>();
        }

        /// <summary>
        /// Gets the <see cref="Game"/> associated with this <see cref="GameSystemBase"/>. This value can be null in a mock environment.
        /// </summary>
        /// <value>The game.</value>
        /// <remarks>This value can be null</remarks>
        public GameBase Game
        {
            get { return game; }
        }

        /// <summary>
        /// Gets the services registry.
        /// </summary>
        /// <value>The services registry.</value>
        public IServiceRegistry Services
        {
            get
            {
                return registry;
            }
        }

        /// <summary>
        /// Gets the content manager.
        /// </summary>
        /// <value>The content.</value>
        protected IAssetManager Asset
        {
            get
            {
                return assetManager;
            }
        }

        /// <summary>
        /// Gets the graphics device.
        /// </summary>
        /// <value>The graphics device.</value>
        protected GraphicsDevice GraphicsDevice
        {
            get
            {
                return graphicsDeviceService != null ? graphicsDeviceService.GraphicsDevice : null;
            }
        }

        #region IDrawable Members

        public event EventHandler<EventArgs> DrawOrderChanged;

        public event EventHandler<EventArgs> VisibleChanged;

        public virtual bool BeginDraw()
        {
            return true;
        }

        public virtual void Draw(GameTime gameTime)
        {
        }

        public virtual void EndDraw()
        {
        }

        public bool Visible
        {
            get { return visible; }
            set
            {
                if (visible != value)
                {
                    visible = value;
                    OnVisibleChanged(EventArgs.Empty);
                }
            }
        }

        public int DrawOrder
        {
            get { return drawOrder; }
            set
            {
                if (drawOrder != value)
                {
                    drawOrder = value;
                    OnDrawOrderChanged(this, EventArgs.Empty);
                }
            }
        }

        #endregion

        #region IGameSystemBase Members

        public virtual void Initialize()
        {
        }

        private void InitGraphicsDeviceService()
        {
            if (graphicsDeviceService == null)
            {
                graphicsDeviceService = (IGraphicsDeviceService)registry.GetService(typeof(IGraphicsDeviceService));
            }
        }

        #endregion

        #region IUpdateable Members

        public event EventHandler<EventArgs> EnabledChanged;

        public event EventHandler<EventArgs> UpdateOrderChanged;

        public virtual void Update(GameTime gameTime)
        {
        }

        public bool Enabled
        {
            get { return enabled; }
            set
            {
                if (enabled != value)
                {
                    enabled = value;
                    OnEnabledChanged(EventArgs.Empty);
                }
            }
        }

        public int UpdateOrder
        {
            get { return updateOrder; }
            set
            {
                if (updateOrder != value)
                {
                    updateOrder = value;
                    OnUpdateOrderChanged(this, EventArgs.Empty);
                }
            }
        }

        #endregion

        protected virtual void OnDrawOrderChanged(object source, EventArgs e)
        {
            EventHandler<EventArgs> handler = DrawOrderChanged;
            if (handler != null) handler(source, e);
        }

        private void OnVisibleChanged(EventArgs e)
        {
            EventHandler<EventArgs> handler = VisibleChanged;
            if (handler != null) handler(this, e);
        }

        private void OnEnabledChanged(EventArgs e)
        {
            EventHandler<EventArgs> handler = EnabledChanged;
            if (handler != null) handler(this, e);
        }

        protected virtual void OnUpdateOrderChanged(object source, EventArgs e)
        {
            EventHandler<EventArgs> handler = UpdateOrderChanged;
            if (handler != null) handler(source, e);
        }

        #region Implementation of IContentable

        void IContentable.LoadContent()
        {
            InitGraphicsDeviceService();

            LoadContent();
        }

        void IContentable.UnloadContent()
        {
            UnloadContent();
        }

        protected virtual void LoadContent()
        {
        }

        protected virtual void UnloadContent()
        {
        }

        #endregion
    }
}

