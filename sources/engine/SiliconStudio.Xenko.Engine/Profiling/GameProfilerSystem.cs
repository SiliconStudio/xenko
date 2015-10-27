using System;
using System.Collections.Generic;
using System.Text;
using SiliconStudio.Core;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.UI;
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

    public class GameProfilerSystem : GameSystemBase
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

        struct ProfilingResult
        {
            public long AccumulatedTime;
            public long MinTime;
            public long MaxTime;
            public int Count;
        }

        private readonly Dictionary<ProfilingKey, ProfilingResult> profilingResults = new Dictionary<ProfilingKey, ProfilingResult>(); 

        public GameProfilerSystem(IServiceRegistry registry) : base(registry)
        {
            registry.AddService(typeof(GameProfilerSystem), this);

            DrawOrder = 0xffffff;

            Profiler.Enable(GcProfiling.GcCollectionCountKey);
            Profiler.Enable(GcProfiling.GcMemoryKey);

            gcProfiler = new GcProfiling();        

            gcMemoryStringBase = string.Intern(         "Memory>        Total: {0} Peak: {1} Last allocations: {2}");
            gcCollectionsStringBase = string.Intern(    "Collections>   Gen 0: {0} Gen 1: {1} Gen 3: {2}");

            Profiler.EnableAll();
        }

        public override void Update(GameTime gameTime)
        {
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
                if (e.Key == GcProfiling.GcMemoryKey && e.Custom0.HasValue && e.Custom1.HasValue && e.Custom2.HasValue)
                {
                    gcMemoryString.Clear();
                    gcMemoryString.AppendFormat(gcMemoryStringBase, e.Custom0.Value.LongValue, e.Custom2.Value.LongValue, e.Custom1.Value.LongValue);
                }

                if (e.Key == GcProfiling.GcCollectionCountKey && e.Custom0.HasValue && e.Custom1.HasValue && e.Custom2.HasValue)
                {
                    gcCollectionsString.Clear();
                    gcCollectionsString.AppendFormat(gcCollectionsStringBase, e.Custom0.Value.IntValue, e.Custom1.Value.IntValue, e.Custom2.Value.IntValue);
                }

                ProfilingResult profilingResult;
                if (!profilingResults.TryGetValue(e.Key, out profilingResult))
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
                }
                else if (e.Type == ProfilingMessageType.Mark) // counter incremented only Mark!
                {
                    profilingResult.Count++;
                }

                profilingResults[e.Key] = profilingResult;

                lock (externalProfilers)
                {
                    GameProfiler p;
                    if (externalProfilers.TryGetValue(e.Key, out p))
                    {
                        if (p.LogAt != e.Type) continue;

                        if (p.ValueDescs != null)
                        {
                            var values = new object[p.ValueDescs.Length];
                            for (var index = 0; index < p.ValueDescs.Length; index++)
                            {
                                var profilerValueDesc = p.ValueDescs[index];
                                if (profilerValueDesc == ProfilerValueDesc.None) break;

                                ProfilingCustomValue? value;
                                switch (index)
                                {
                                    case 0:
                                        value = e.Custom0;
                                        break;
                                    case 1:
                                        value = e.Custom1;
                                        break;
                                    case 2:
                                        value = e.Custom2;
                                        break;
                                    case 3:
                                        value = e.Custom3;
                                        break;
                                    default:
                                        value = null;
                                        break;
                                }

                                if (!value.HasValue) continue;

                                switch (profilerValueDesc)
                                {
                                    case ProfilerValueDesc.Int:
                                        values[index] = value.Value.IntValue;
                                        break;
                                    case ProfilerValueDesc.Float:
                                        values[index] = value.Value.FloatValue;
                                        break;
                                    case ProfilerValueDesc.Long:
                                        values[index] = value.Value.LongValue;
                                        break;
                                    case ProfilerValueDesc.Double:
                                        values[index] = value.Value.DoubleValue;
                                        break;
                                    default:
                                        throw new ArgumentOutOfRangeException();
                                }

                                externalProfilersString.AppendFormat(p.FormatString, values);
                                externalProfilersString.AppendLine();
                            }
                        }
                        else
                        {
                            externalProfilersString.Append(p.FormatString);
                        }

                        if (p.ReportTime)
                        {
                            profilingResults.Remove(p.ProfilingState.ProfilingKey);
                            externalProfilersString.AppendFormat("{0,-7:P1} | ", profilingResult.AccumulatedTime/(double)elapsedTime);
                            Profiler.AppendTime(externalProfilersString, profilingResult.AccumulatedTime);

                            externalProfilersString.Append(" |  ");
                            Profiler.AppendTime(externalProfilersString, profilingResult.MinTime);
                            externalProfilersString.Append(" |  ");
                            Profiler.AppendTime(externalProfilersString, profilingResult.Count != 0 ? profilingResult.AccumulatedTime/profilingResult.Count : 0);
                            externalProfilersString.Append(" |  ");
                            Profiler.AppendTime(externalProfilersString, profilingResult.MaxTime);

                            externalProfilersString.AppendFormat(" | {0:00000} | {1}", profilingResult.Count, e.Key);
                            externalProfilersString.AppendLine();
                        }
                    }
                    else
                    {
                        if(e.Type != ProfilingMessageType.End) continue;

                        profilingResults.Remove(e.Key);
                        externalProfilersString.AppendFormat("{0,-7:P1} | ", profilingResult.AccumulatedTime / (double)elapsedTime);
                        Profiler.AppendTime(externalProfilersString, profilingResult.AccumulatedTime);

                        externalProfilersString.Append(" |  ");
                        Profiler.AppendTime(externalProfilersString, profilingResult.MinTime);
                        externalProfilersString.Append(" |  ");
                        Profiler.AppendTime(externalProfilersString, profilingResult.Count != 0 ? profilingResult.AccumulatedTime / profilingResult.Count : 0);
                        externalProfilersString.Append(" |  ");
                        Profiler.AppendTime(externalProfilersString, profilingResult.MaxTime);

                        externalProfilersString.AppendFormat(" | {0:00000} | {1}", profilingResult.Count, e.Key);
                        externalProfilersString.AppendLine();
                    }
                }
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
            spriteBatch.DrawString(spriteFont, gcMemoryString, new Vector2(10, 10), Color.LightGreen);
            spriteBatch.DrawString(spriteFont, gcCollectionsString, new Vector2(10, 20), Color.LightGreen);
            spriteBatch.DrawString(spriteFont, externalProfilersString, new Vector2(10, 10), Color.LightGreen);
            externalProfilersString.Clear();
            spriteBatch.End();
        }

        public void AddExternalProfiler(GameProfiler profiler)
        {
            Profiler.Enable(profiler.ProfilingState.ProfilingKey);
            profiler.ProfilingState.CheckIfEnabled();
            lock (externalProfilers) externalProfilers.Add(profiler.ProfilingState.ProfilingKey, profiler);          
        }
    }
}
