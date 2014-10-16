// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;

using System.Linq;

namespace SiliconStudio.Paradox.Graphics.Font
{
    /// <summary>
    /// The system managing the fonts.
    /// </summary>
    public class FontSystem : IFontSystem
    {
        internal int FrameCount { get; private set; }
        internal FontManager FontManager { get; private set; }
        internal GraphicsDevice GraphicsDevice { get; private set; }
        internal FontCacheManager FontCacheManager { get; private set; }
        internal readonly HashSet<SpriteFont> AllocatedSpriteFonts = new HashSet<SpriteFont>();

        /// <summary>
        /// Create a new instance of <see cref="FontSystem"/> base on the provided <see cref="GraphicsDevice"/>.
        /// </summary>
        /// <param name="graphicsDevice">A valid instance of <see cref="GraphicsDevice"/></param>
        /// <exception cref="ArgumentNullException"><paramref name="graphicsDevice"/> is null</exception>
        public FontSystem(GraphicsDevice graphicsDevice)
        {
            if (graphicsDevice == null)
                throw new ArgumentNullException("graphicsDevice");

            GraphicsDevice = graphicsDevice;
            FontManager = new FontManager();
            FontCacheManager = new FontCacheManager(this);
        }

        /// <summary>
        /// Load the fonts.
        /// </summary>
        public void Load()
        {
            // TODO possibly load cached character bitmaps from the disk
        }

        public void Draw()
        {
            ++FrameCount;
        }

        public void Unload()
        {
            // TODO possibly save generated characters bitmaps on the disk

            // Dispose create sprite fonts
            foreach (var allocatedSpriteFont in AllocatedSpriteFonts.ToArray())
                allocatedSpriteFont.Dispose();
        }

        public SpriteFont NewStatic(StaticSpriteFontData data)
        {
            return new StaticSpriteFont(this, data);
        }

        public SpriteFont NewDynamic(DynamicSpriteFontData data)
        {
            return new DynamicSpriteFont(this, data);
        }
    }
}