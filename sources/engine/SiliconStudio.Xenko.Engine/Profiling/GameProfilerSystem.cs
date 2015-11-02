using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using SiliconStudio.Core;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.MicroThreading;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Graphics;
using Color = SiliconStudio.Core.Mathematics.Color;

namespace SiliconStudio.Xenko.Profiling
{
    public class GameProfilingSystem : GameSystemBase
    {
        private readonly GcProfiling gcProfiler;

        private readonly StringBuilder gcMemoryString = new StringBuilder();
        private readonly string gcMemoryStringBase;
        private readonly StringBuilder gcCollectionsString = new StringBuilder();
        private readonly string gcCollectionsStringBase;

        private SpriteBatch spriteBatch;
        private SpriteFont spriteFont;

        private readonly StringBuilder profilersString = new StringBuilder();

        struct ProfilingResult : IComparer<ProfilingResult>
        {
            public long AccumulatedTime;
            public long MinTime;
            public long MaxTime;
            public int Count;
            public ProfilingEvent? Event;

            public ProfilingCustomValue? Custom0;
            public ProfilingCustomValue? Custom1;
            public ProfilingCustomValue? Custom2;
            public ProfilingCustomValue? Custom3;

            public int Compare(ProfilingResult x, ProfilingResult y)
            {
                return Math.Sign(x.AccumulatedTime - y.AccumulatedTime);
            }
        }

        private readonly List<ProfilingResult> profilingResults = new List<ProfilingResult>();
        private readonly Dictionary<ProfilingKey, ProfilingResult> profilingResultsDictionary = new Dictionary<ProfilingKey, ProfilingResult>();
        private readonly Dictionary<string, ProfilingResult> scriptsProfilingResultsDictionary = new Dictionary<string, ProfilingResult>();

        public GameProfilingSystem(IServiceRegistry registry) : base(registry)
        {
            registry.AddService(typeof(GameProfilingSystem), this);

            DrawOrder = 0xffffff;

            gcProfiler = new GcProfiling();        

            gcMemoryStringBase =        "Memory>        Total: {0} Peak: {1} Last allocations: {2}";
            gcCollectionsStringBase =   "Collections>   Gen 0: {0} Gen 1: {1} Gen 3: {2}";
        }

        readonly Stopwatch dumpTiming = Stopwatch.StartNew();

        public override void Update(GameTime gameTime)
        {
            if (dumpTiming.ElapsedMilliseconds > 500)
            {
                dumpTiming.Restart();
            }
            else
            {
                return;
            }

            //Advance any profiler that needs it
            gcProfiler.Tick();

            //Copy events from profiler ( this will also clean up the profiler )
            //todo do we really need this copy?
            var eventsCopy = Profiler.GetEvents();
            if(eventsCopy == null) return;

            var elapsedTime = eventsCopy.Length > 0 ? eventsCopy[eventsCopy.Length - 1].TimeStamp - eventsCopy[0].TimeStamp : 0;

            //update strings that need update
            foreach (var e in eventsCopy)
            {
                //gc profiling is a special case
                if (e.Key == GcProfiling.GcMemoryKey && e.Custom0.HasValue && e.Custom1.HasValue && e.Custom2.HasValue)
                {
                    gcMemoryString.Clear();
                    gcMemoryString.AppendFormat(gcMemoryStringBase, e.Custom0.Value.LongValue, e.Custom2.Value.LongValue, e.Custom1.Value.LongValue);
                    gcCollectionsString.AppendLine();
                    continue;
                }

                if (e.Key == GcProfiling.GcCollectionCountKey && e.Custom0.HasValue && e.Custom1.HasValue && e.Custom2.HasValue)
                {
                    gcCollectionsString.Clear();
                    gcCollectionsString.AppendFormat(gcCollectionsStringBase, e.Custom0.Value.IntValue, e.Custom1.Value.IntValue, e.Custom2.Value.IntValue);
                    gcCollectionsString.AppendLine();
                    continue;
                }

                ProfilingResult profilingResult;
                if (!profilingResultsDictionary.TryGetValue(e.Key, out profilingResult))
                {
                    profilingResult.MinTime = long.MaxValue;
                }

                if (e.Type == ProfilingMessageType.End)
                {
                    profilingResult.AccumulatedTime += e.ElapsedTime;
                    if (e.ElapsedTime < profilingResult.MinTime)
                        profilingResult.MinTime = e.ElapsedTime;
                    if (e.ElapsedTime > profilingResult.MaxTime)
                        profilingResult.MaxTime = e.ElapsedTime;

                    profilingResult.Event = e;
                }
                else if (e.Type == ProfilingMessageType.Mark) // counter incremented only Mark!
                {
                    profilingResult.Count++;
                }

                if (e.Custom0.HasValue)
                {
                    profilingResult.Custom0 = e.Custom0.Value;
                }
                if (e.Custom1.HasValue)
                {
                    profilingResult.Custom1 = e.Custom1.Value;
                }
                if (e.Custom2.HasValue)
                {
                    profilingResult.Custom2 = e.Custom2.Value;
                }
                if (e.Custom3.HasValue)
                {
                    profilingResult.Custom3 = e.Custom3.Value;
                }

                if (e.Key == MicroThread.ProfilingKey)
                {
                    scriptsProfilingResultsDictionary[e.Text] = profilingResult;
                }
                else
                {
                    profilingResultsDictionary[e.Key] = profilingResult;
                }  
            }

            profilersString.Clear();
            profilingResults.Clear();

            foreach (var profilingResult in profilingResultsDictionary)
            {
                if (!profilingResult.Value.Event.HasValue) continue;
                profilingResults.Add(profilingResult.Value);
            }

            profilingResultsDictionary.Clear();

            profilingResults.Sort((x1, x2) => Math.Sign(x2.AccumulatedTime - x1.AccumulatedTime));

            foreach (var result in profilingResults)
            {
                AppendEvent(result, result.Event.Value, elapsedTime);
            }

            profilingResults.Clear();

            foreach (var profilingResult in scriptsProfilingResultsDictionary)
            {
                if (!profilingResult.Value.Event.HasValue) continue;
                profilingResults.Add(profilingResult.Value);
            }

            scriptsProfilingResultsDictionary.Clear();

            profilingResults.Sort((x1, x2) => Math.Sign(x2.AccumulatedTime - x1.AccumulatedTime));

            foreach (var result in profilingResults)
            {
                AppendEvent(result, result.Event.Value, elapsedTime);
            }
        }

        private string defaultValue = "";

        private void AppendEvent(ProfilingResult profilingResult, ProfilingEvent e, double elapsedTime)
        {
            profilersString.AppendFormat("{0,-7:P1}", profilingResult.AccumulatedTime / elapsedTime);
            profilersString.Append(" |  ");
            Profiler.AppendTime(profilersString, profilingResult.MinTime);
            profilersString.Append(" |  ");
            Profiler.AppendTime(profilersString, profilingResult.Count != 0 ? profilingResult.AccumulatedTime / profilingResult.Count : 0);
            profilersString.Append(" |  ");
            Profiler.AppendTime(profilersString, profilingResult.MaxTime);

            profilersString.AppendFormat(" | {0} ", e.Key);
            if (!e.Text.IsNullOrEmpty())
            {
                var values = new object[4];
                values[0] = defaultValue;
                values[1] = defaultValue;
                values[2] = defaultValue;
                values[3] = defaultValue;
                if (profilingResult.Custom0.HasValue && profilingResult.Custom0.Value.ValueType != null)
                {
                    FillValue(profilingResult.Custom0.Value, ref values[0]);
                }
                if (profilingResult.Custom1.HasValue && profilingResult.Custom1.Value.ValueType != null)
                {
                    FillValue(profilingResult.Custom1.Value, ref values[1]);
                }
                if (profilingResult.Custom2.HasValue && profilingResult.Custom2.Value.ValueType != null)
                {
                    FillValue(profilingResult.Custom2.Value, ref values[2]);
                }
                if (profilingResult.Custom3.HasValue && profilingResult.Custom3.Value.ValueType != null)
                {
                    FillValue(profilingResult.Custom3.Value, ref values[3]);
                }

                profilersString.AppendFormat(e.Text, values);
            }
            profilersString.AppendLine();
        }

        public void FillValue(ProfilingCustomValue value, ref object boxed)
        {
            if (value.ValueType == typeof(int))
            {
                boxed = value.IntValue;
            }
            else if (value.ValueType == typeof(float))
            {
                boxed = value.FloatValue;
            }
            else if (value.ValueType == typeof(long))
            {
                boxed = value.LongValue;
            }
            else if (value.ValueType == typeof(double))
            {
                boxed = value.DoubleValue;
            }
        }

        protected override void Destroy()
        {
            gcProfiler.Dispose();
        }

        public override void Draw(GameTime gameTime)
        {
            if (spriteBatch == null)
            {
                spriteBatch = new SpriteBatch(Services.GetSafeServiceAs<IGraphicsDeviceService>().GraphicsDevice);
                spriteFont = Asset.Load<SpriteFont>("XenkoDefaultFont");
            }

            spriteBatch.Begin();
            spriteBatch.DrawString(spriteFont, gcMemoryString, new Vector2(10, 10), TextColor);
            spriteBatch.DrawString(spriteFont, gcCollectionsString, new Vector2(10, 20), TextColor);
            spriteBatch.DrawString(spriteFont, profilersString, new Vector2(10, 30), TextColor);        
            spriteBatch.End();
        }

        public void EnableProfiling(params ProfilingKey[] keys)
        {
            Enabled = true;
            Visible = true;
            if (keys.Length == 0)
            {
                Profiler.EnableAll();
            }
            else
            {
                foreach (var profilingKey in keys)
                {
                    Profiler.Enable(profilingKey);
                }
            }
            gcProfiler.Enable();
        }

        public void DisableProfiling()
        {
            Enabled = false;
            Visible = false;
            Profiler.DisableAll();
            gcProfiler.Disable();
        }

        public Color4 TextColor { get; set; } = Color.LightGreen;
    }
}
