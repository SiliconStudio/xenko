using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    public enum ProfilerValueDesc : byte
    {
        None,
        Int,
        Float,
        Long,
        Double
    }

    public class GameProfiler
    {
        public ProfilingState ProfilingState;
        public ProfilerValueDesc[] ValueDescs;
        public string FormatString;
        public ProfilingMessageType LogAt;
        public bool ReportTime;
    }

    public class GameProfilingSystem : GameSystemBase
    {
        private readonly GcProfiling gcProfiler;

        private readonly StringBuilder gcMemoryString = new StringBuilder();
        private readonly string gcMemoryStringBase;
        private readonly StringBuilder gcCollectionsString = new StringBuilder();
        private readonly string gcCollectionsStringBase;

        private SpriteBatch spriteBatch;
        private SpriteFont spriteFont;

        private readonly Dictionary<ProfilingKey, GameProfiler> externalProfilers = new Dictionary<ProfilingKey, GameProfiler>(); 
        private readonly StringBuilder externalProfilersString = new StringBuilder();

        private readonly SortedDictionary<ProfilingResult, ProfilingEvent> microthreadEvents = new SortedDictionary<ProfilingResult, ProfilingEvent>(new ProfilingResult());
        private readonly Dictionary<ProfilingResult, ProfilingEvent> normalEvents = new Dictionary<ProfilingResult, ProfilingEvent>();

        struct ProfilingResult : IComparer<ProfilingResult>
        {
            public long AccumulatedTime;
            public long MinTime;
            public long MaxTime;
            public int Count;
            public ProfilingEvent? Event;
            public int Compare(ProfilingResult x, ProfilingResult y)
            {
                return Math.Sign(x.AccumulatedTime - y.AccumulatedTime);
            }
        }

        private readonly List<ProfilingResult> profilingResults = new List<ProfilingResult>();
        private readonly Dictionary<ProfilingKey, ProfilingResult> profilingResultsDictionary = new Dictionary<ProfilingKey, ProfilingResult>();

        public GameProfilingSystem(IServiceRegistry registry) : base(registry)
        {
            registry.AddService(typeof(GameProfilingSystem), this);

            DrawOrder = 0xffffff;

            gcProfiler = new GcProfiling();        

            gcMemoryStringBase =        "Memory>        Total: {0} Peak: {1} Last allocations: {2}";
            gcCollectionsStringBase =   "Collections>   Gen 0: {0} Gen 1: {1} Gen 3: {2}";
        }

        Stopwatch dumpTiming = Stopwatch.StartNew();

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

                profilingResultsDictionary[e.Key] = profilingResult;

                var processed = false;
//                lock (externalProfilers)
//                {
//                    GameProfiler p;
//                    if (externalProfilers.TryGetValue(e.Key, out p))
//                    {
//                        if (p.LogAt != e.Type) continue;
//
//                        if (p.ValueDescs != null)
//                        {
//                            var values = new object[p.ValueDescs.Length];
//                            for (var index = 0; index < p.ValueDescs.Length; index++)
//                            {
//                                var profilerValueDesc = p.ValueDescs[index];
//                                if (profilerValueDesc == ProfilerValueDesc.None) break;
//
//                                ProfilingCustomValue? value;
//                                switch (index)
//                                {
//                                    case 0:
//                                        value = e.Custom0;
//                                        break;
//                                    case 1:
//                                        value = e.Custom1;
//                                        break;
//                                    case 2:
//                                        value = e.Custom2;
//                                        break;
//                                    case 3:
//                                        value = e.Custom3;
//                                        break;
//                                    default:
//                                        value = null;
//                                        break;
//                                }
//
//                                if (!value.HasValue) continue;
//
//                                switch (profilerValueDesc)
//                                {
//                                    case ProfilerValueDesc.Int:
//                                        values[index] = value.Value.IntValue;
//                                        break;
//                                    case ProfilerValueDesc.Float:
//                                        values[index] = value.Value.FloatValue;
//                                        break;
//                                    case ProfilerValueDesc.Long:
//                                        values[index] = value.Value.LongValue;
//                                        break;
//                                    case ProfilerValueDesc.Double:
//                                        values[index] = value.Value.DoubleValue;
//                                        break;
//                                    default:
//                                        throw new ArgumentOutOfRangeException();
//                                }
//
//                                externalProfilersString.AppendFormat(p.FormatString, values);
//                                externalProfilersString.AppendLine();
//                            }
//                        }
//                        else
//                        {
//                            externalProfilersString.Append(p.FormatString);
//                        }
//
//                        if (p.ReportTime)
//                        {
//                            AppendEvent(profilingResult, e, elapsedTime);
//                        }
//
//                        processed = true;
//                    }
//                }

                if (processed || e.Type != ProfilingMessageType.End) continue;

//                if (e.Key == MicroThread.ProfilingKey)
//                {
//                    microthreadEvents[profilingResult] = e;
//                }
//                else
//                {
//                    normalEvents[profilingResult] = e;
//                }
            }

            externalProfilersString.Clear();
            profilingResults.Clear();

            foreach (var profilingResult in profilingResultsDictionary)
            {
                if (profilingResult.Value.Event.HasValue)
                {
                    profilingResults.Add(profilingResult.Value);
                }
            }

            profilingResultsDictionary.Clear();

            profilingResults.Sort((x1, x2) => Math.Sign(x2.AccumulatedTime - x1.AccumulatedTime));

            foreach (var result in profilingResults)
            {
                AppendEvent(result, result.Event.Value, elapsedTime);
            }          

//            foreach (var normalEvent in normalEvents)
//            {
//                AppendEvent(normalEvent.Key, normalEvent.Value, elapsedTime);
//            }
//
//            foreach (var microthreadEvent in microthreadEvents)
//            {
//                AppendEvent(microthreadEvent.Key, microthreadEvent.Value, elapsedTime);
//            }
//
//            microthreadEvents.Clear();
//            normalEvents.Clear();
        }

        private void AppendEvent(ProfilingResult profilingResult, ProfilingEvent e, double elapsedTime)
        {
            externalProfilersString.AppendFormat("{0,-7:P1}", profilingResult.AccumulatedTime / elapsedTime);
            externalProfilersString.Append(" |  ");
            Profiler.AppendTime(externalProfilersString, profilingResult.MinTime);
            externalProfilersString.Append(" |  ");
            Profiler.AppendTime(externalProfilersString, profilingResult.Count != 0 ? profilingResult.AccumulatedTime / profilingResult.Count : 0);
            externalProfilersString.Append(" |  ");
            Profiler.AppendTime(externalProfilersString, profilingResult.MaxTime);

            externalProfilersString.AppendFormat(" | {0} ", e.Key);
            if (!e.Text.IsNullOrEmpty()) externalProfilersString.AppendFormat(e.Text);
            externalProfilersString.AppendLine();
        }

        protected override void Destroy()
        {
            gcProfiler.Dispose();
            lock (externalProfilers)
            {
                externalProfilers.Clear();
            }
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
            spriteBatch.DrawString(spriteFont, externalProfilersString, new Vector2(10, 30), TextColor);        
            spriteBatch.End();
        }

        public void AddProfiler(GameProfiler profiler)
        {
            lock (externalProfilers) externalProfilers.Add(profiler.ProfilingState.ProfilingKey, profiler);          
        }

        public void EnableProfiling()
        {
            Enabled = true;
            Visible = true;
            lock (externalProfilers)
            {
                foreach (var externalProfiler in externalProfilers)
                {
                    Profiler.Enable(externalProfiler.Value.ProfilingState.ProfilingKey);
                    externalProfiler.Value.ProfilingState.CheckIfEnabled();
                }
            }

            Profiler.EnableAll();

            gcProfiler.Enable();
        }

        public void DisableProfiling()
        {
            Enabled = false;
            Visible = false;
            lock (externalProfilers)
            {
                foreach (var externalProfiler in externalProfilers)
                {
                    Profiler.Disable(externalProfiler.Value.ProfilingState.ProfilingKey);
                    externalProfiler.Value.ProfilingState.CheckIfEnabled();
                }
            }

            Profiler.DisableAll();

            gcProfiler.Disable();
        }

        public Color4 TextColor { get; set; } = Color.LightGreen;
    }
}
