// Copyright (c) 2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Input.Tests
{
    /// <summary>
    /// Simple interactive test that logs input events to the screen
    /// </summary>
    public class TestInputEvents : InputTestBase
    {
        private const int MaximumLogEntries = 32;
        private List<EventLog> eventLog = new List<EventLog>();

        private Dictionary<Type, Color> eventColors = new Dictionary<Type, Color>
        {
            [typeof(KeyEvent)] = Color.AliceBlue,
            [typeof(PointerEvent)] = Color.Orange,
            [typeof(GamePadAxisEvent)] = Color.Green,
            [typeof(GamePadButtonEvent)] = Color.Green,
            [typeof(GameControllerAxisEvent)] = Color.Bisque,
            [typeof(GameControllerButtonEvent)] = Color.Bisque,
            [typeof(GameControllerDirectionEvent)] = Color.Bisque,
        };

        public TestInputEvents()
        {
            DefaultTextColor = Color.White;
        }

        protected override async Task LoadContent()
        {
            await base.LoadContent();
            Input.DeviceAdded += InputOnDeviceChanged;
            Input.DeviceRemoved += InputOnDeviceChanged;
        }

        private void InputOnDeviceChanged(object sender, DeviceChangedEventArgs deviceChangedEventArgs)
        {
            var device = deviceChangedEventArgs.Device;
            Log($"{device} ({device.Name}, {device.Id}) {deviceChangedEventArgs.Type} from {deviceChangedEventArgs.Source}", Color.Magenta);
        }

        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            // clear the screen
            GraphicsContext.CommandList.Clear(GraphicsDevice.Presenter.BackBuffer, Color.Black);
            GraphicsContext.CommandList.Clear(GraphicsDevice.Presenter.DepthStencilBuffer, DepthStencilClearOptions.DepthBuffer);
            GraphicsContext.CommandList.SetRenderTargetAndViewport(GraphicsDevice.Presenter.DepthStencilBuffer, GraphicsDevice.Presenter.BackBuffer);

            BeginSpriteBatch();

            foreach (var evt in Input.Events)
            {
                Log(evt.ToString(), GetLogColor(evt));
            }

#if SILICONSTUDIO_PLATFORM_WINDOWS
            // Toggle raw input
            WriteLine($"Raw input: {Input.UseRawInput} (Ctrl+R to toggle)");
            if ((Input.IsKeyDown(Keys.LeftCtrl) || Input.IsKeyDown(Keys.RightCtrl)) && Input.IsKeyPressed(Keys.R))
            {
                Input.UseRawInput = !Input.UseRawInput;
            }
#endif

            WriteLine("Input Events:");
            foreach (var evt in eventLog)
            {
                WriteLine(evt.Message, evt.Color, 1);
            }


            EndSpriteBatch();
        }

        private void Log(string message, Color color)
        {
            eventLog.Add(new EventLog
            {
                Color = color,
                Message = message
            });
            while (eventLog.Count > MaximumLogEntries)
            {
                eventLog.RemoveAt(0);
            }
        }

        private Color GetLogColor(InputEvent evt)
        {
            Color color;
            if (!eventColors.TryGetValue(evt.GetType(), out color))
                return DefaultTextColor;
            return color;
        }

        [Test]
        public void RunTestInputEvents()
        {
            RunGameTest(new TestInputEvents());
        }

        public static void Main(string[] args)
        {
            using (var game = new TestInputEvents())
                game.Run();
        }

        private struct EventLog
        {
            public Color Color;
            public string Message;
        }
    }
}
