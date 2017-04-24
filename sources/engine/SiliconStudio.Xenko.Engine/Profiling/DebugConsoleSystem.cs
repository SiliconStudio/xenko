// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Profiling
{
    public class DebugConsoleSystem : GameSystemBase
    {
        internal struct DebugOverlayMessage
        {
            public string Message;
            public Vector2 Position;
            public Color4 TextColor;
            public SpriteFont TextFont;
        }

        private SpriteBatch spriteBatch;
        private readonly Queue<DebugOverlayMessage> overlayMessages = new Queue<DebugOverlayMessage>();

        public DebugConsoleSystem(IServiceRegistry registry) : base(registry)
        {
            registry.AddService(typeof(DebugConsoleSystem), this);

            Visible = true;

            DrawOrder = 0xffffff;
            UpdateOrder = -100100; //before script
        }

        /// <summary>
        /// Print a custom overlay message
        /// </summary>
        /// <param name="message"></param>
        /// <param name="position"></param>
        public void Print(string message, Vector2 position)
        {
            Print(message, position, TextColor, null);
        }

        /// <summary>
        /// Print a custom overlay message
        /// </summary>
        /// <param name="message"></param>
        /// <param name="position"></param>
        /// <param name="color"></param>
        public void Print(string message, Vector2 position, Color4 color)
        {
            Print(message, position, color, null);
        }

        /// <summary>
        /// Print a custom overlay message
        /// </summary>
        /// <param name="message"></param>
        /// <param name="position"></param>
        /// <param name="color"></param>
        /// <param name="font"></param>
        public void Print(string message, Vector2 position, Color4 color, SpriteFont font)
        {
            var msg = new DebugOverlayMessage { Message = message, Position = position, TextColor = color, TextFont = font };
            overlayMessages.Enqueue(msg);
            //drop one old message if the tail size has been reached
            if (overlayMessages.Count > TailSize)
            {
                overlayMessages.Dequeue();
            }
        }

        /// <summary>
        /// Sets or gets the color to use when drawing the profiling system fonts.
        /// </summary>
        public Color4 TextColor { get; set; } = Color.LightGreen;

        /// <summary>
        /// Sets or gets the font to use when drawing the profiling system text.
        /// </summary>
        public SpriteFont Font { get; set; }

        /// <summary>
        /// Sets or gets the size of the messages queue, older messages will be discarded if the size is greater.
        /// </summary>
        public int TailSize { get; set; } = 100;

        public override void Update(GameTime gameTime)
        {
            overlayMessages.Clear();
        }

        public override void Draw(GameTime gameTime)
        {
            if (overlayMessages.Count == 0)
            {
                return;
            }

            //TODO, this is not so nice
            if (spriteBatch == null)
            {
                spriteBatch = new SpriteBatch(Game.GraphicsDevice);
            }

            //TODO, this is not so nice
            if (Font == null)
            {
                try
                {
                    Font = Content.Load<SpriteFont>("XenkoDefaultFont");
                }
                catch (Exception)
                {
                    Visible = false;
                    return;
                }
            }

            // TODO GRAPHICS REFACTOR where to get command list from?
            Game.GraphicsContext.CommandList.SetRenderTargetAndViewport(null, Game.GraphicsDevice.Presenter.BackBuffer);
            spriteBatch.Begin(Game.GraphicsContext, depthStencilState: DepthStencilStates.None);

            foreach (var msg in overlayMessages)
            {
                spriteBatch.DrawString(msg.TextFont ?? Font, msg.Message, msg.Position, msg.TextColor);
            }

            spriteBatch.End();
        }
    }
}
