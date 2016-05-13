// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Games;

namespace SiliconStudio.Xenko.Input
{
    public abstract class InputManager : InputManagerBase
    {
        protected const float G = 9.81f;

        /// <summary>
        /// Does InputManager support raw input? By default true.
        /// </summary>
        public static bool UseRawInput = true;

        protected InputManager(IServiceRegistry registry) : base(registry)
        {
        }

        /// <summary>
        /// Helper method to transform mouse and pointer event positions to sub rectangles
        /// </summary>
        /// <param name="fromSize">the size of the source rectangle</param>
        /// <param name="destinationRectangle">The destination viewport rectangle</param>
        /// <param name="screenCoordinates">The normalized screen coordinates</param>
        /// <returns></returns>
        public static Vector2 TransformPosition(Size2F fromSize, RectangleF destinationRectangle, Vector2 screenCoordinates)
        {
            return new Vector2((screenCoordinates.X * fromSize.Width - destinationRectangle.X) / destinationRectangle.Width, (screenCoordinates.Y * fromSize.Height - destinationRectangle.Y) / destinationRectangle.Height);
        }
    }

    internal abstract class InputManager<TK> : InputManager
    {
        protected InputManager(IServiceRegistry registry) : base(registry)
        {
        }

        protected TK uiControl;

        public sealed override void Initialize()
        {
            base.Initialize();
            var context = Game.Context as GameContext<TK>;
            if (context != null)
            {
                Initialize(context);
            }
            else
            {
                throw new InvalidOperationException("Incompatible Context and InputManager.");
            }
        }

        /// <summary>
        /// Type safe version of Initialize.
        /// </summary>
        /// <param name="context">Matching context type.</param>
        public abstract void Initialize(GameContext<TK> context);
    }
}
