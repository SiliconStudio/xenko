using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiliconStudio.Core;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Graphics.Font;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.UI;
using SiliconStudio.Xenko.UI.Controls;
using SiliconStudio.Xenko.UI.Panels;
using Color = SiliconStudio.Core.Mathematics.Color;
using FontStyle = SiliconStudio.Xenko.Graphics.Font.FontStyle;

namespace SiliconStudio.Xenko.Profiling
{
    public class GameProfilerSystem : GameSystemBase
    {
        private readonly GcProfiling gcProfiler;

        private string gcMemoryString;
        private readonly string gcMemoryStringBase;
        private string gcCollectionsString;
        private readonly string gcCollectionsStringBase;

        private readonly SpriteBatch spriteBatch;
        private readonly SpriteFont spriteFont;

        public GameProfilerSystem(IServiceRegistry registry) : base(registry)
        {
            Enabled = true;
            Visible = true;
            DrawOrder = 0xffffff;

            Profiler.Enable(GcProfiling.GcCollectionCountKey);
            Profiler.Enable(GcProfiling.GcMemoryKey);

            gcProfiler = new GcProfiling();

            spriteBatch = new SpriteBatch(Services.GetSafeServiceAs<IGraphicsDeviceService>().GraphicsDevice);
            spriteFont = Asset.Load<SpriteFont>("XenkoDefaultFont");

            gcMemoryStringBase = string.Intern(         "Memory>        Total: {0} Peak: {1} Last allocations: {2}");
            gcCollectionsStringBase = string.Intern(    "Collections>   Gen 0: {0} Gen 1: {1} Gen 3: {2}");
        }

        public override void Update(GameTime gameTime)
        {
            //Advance any profiler that needs it
            gcProfiler.Tick();

            //Copy events from profiler ( this will also clean up the profiler )
            //todo do we really need this copy?
            var eventsCopy = Profiler.GetEvents();
            if(eventsCopy == null) return;

            //update strings that need update
            foreach (var e in eventsCopy)
            {
                if (e.Key == GcProfiling.GcMemoryKey)
                {
                    gcMemoryString = string.Format(gcMemoryStringBase, e.Custom0.LongValue, e.Custom2.LongValue, e.Custom1.LongValue);
                }

                if (e.Key == GcProfiling.GcCollectionCountKey)
                {
                    gcCollectionsString = string.Format(gcCollectionsStringBase, e.Custom0.IntValue, e.Custom1.IntValue, e.Custom2.IntValue);
                }
            }
        }

        protected override void Destroy()
        {
            gcProfiler.Dispose();
        }

        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin();
            spriteBatch.DrawString(spriteFont, gcMemoryString, new Vector2(10, 10), Color.LightGreen);
            spriteBatch.DrawString(spriteFont, gcCollectionsString, new Vector2(10, 20), Color.LightGreen);
            spriteBatch.End();
        }
    }
}
