using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using SiliconStudio.Core;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.MicroThreading;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Graphics;
using Color = SiliconStudio.Core.Mathematics.Color;

namespace SiliconStudio.Xenko.Profiling
{
    public enum GameProfilingSorting
    {
        ByTime,
        ByName
    }

    public class GameProfilingSystem : GameSystemBase
    {
        private readonly GcProfiling gcProfiler;

        private readonly StringBuilder gcMemoryStringBuilder = new StringBuilder();
        private string gcMemoryString = "";
        private readonly string gcMemoryStringBase;
        private readonly StringBuilder gcCollectionsStringBuilder = new StringBuilder();
        private string gcCollectionsString = "";
        private readonly string gcCollectionsStringBase;

        private readonly StringBuilder fpsStatStringBuilder = new StringBuilder();
        private string fpsStatString = "";

        private SpriteBatch spriteBatch;

        private readonly StringBuilder profilersStringBuilder = new StringBuilder();
        private string profilersString = "";

        readonly object stringLock = new object();

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

        private Task stringBuilderTask;

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

            if(stringBuilderTask != null && !stringBuilderTask.IsCompleted) return;

            stringBuilderTask = new Task(() =>
            {
                //Advance any profiler that needs it
                gcProfiler.Tick();

                //Copy events from profiler ( this will also clean up the profiler )
                //todo do we really need this copy?
                var eventsCopy = Profiler.GetEvents();
                if (eventsCopy == null) return;

                var elapsedTime = eventsCopy.Length > 0 ? eventsCopy[eventsCopy.Length - 1].TimeStamp - eventsCopy[0].TimeStamp : 0;

                //update strings that need update
                foreach (var e in eventsCopy)
                {
                    //gc profiling is a special case
                    if (e.Key == GcProfiling.GcMemoryKey && e.Custom0.HasValue && e.Custom1.HasValue && e.Custom2.HasValue)
                    {
                        gcMemoryStringBuilder.Clear();
                        gcMemoryStringBuilder.AppendFormat(gcMemoryStringBase, e.Custom0.Value.LongValue, e.Custom2.Value.LongValue, e.Custom1.Value.LongValue);
                        continue;
                    }

                    if (e.Key == GcProfiling.GcCollectionCountKey && e.Custom0.HasValue && e.Custom1.HasValue && e.Custom2.HasValue)
                    {
                        gcCollectionsStringBuilder.Clear();
                        gcCollectionsStringBuilder.AppendFormat(gcCollectionsStringBase, e.Custom0.Value.IntValue, e.Custom1.Value.IntValue, e.Custom2.Value.IntValue);
                        continue;
                    }

                    if (e.Key == GameProfilingKeys.GameDrawFPS && e.Type == ProfilingMessageType.End)
                    {
                        fpsStatStringBuilder.Clear();
                        fpsStatStringBuilder.AppendFormat(e.Text, e.Custom0.Value.IntValue, e.Custom1.Value.DoubleValue, e.Custom2.Value.DoubleValue, e.Custom3.Value.FloatValue);
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

                    if (e.Key == MicroThreadProfilingKeys.ProfilingKey)
                    {
                        scriptsProfilingResultsDictionary[e.Text] = profilingResult;
                    }
                    else
                    {
                        profilingResultsDictionary[e.Key] = profilingResult;
                    }
                }

                profilersStringBuilder.Clear();
                profilingResults.Clear();

                foreach (var profilingResult in profilingResultsDictionary)
                {
                    if (!profilingResult.Value.Event.HasValue) continue;
                    profilingResults.Add(profilingResult.Value);
                }

                profilingResultsDictionary.Clear();

                if (SortingMode == GameProfilingSorting.ByTime)
                {
                    profilingResults.Sort((x1, x2) => Math.Sign(x2.AccumulatedTime - x1.AccumulatedTime));
                }
                else
                {
                    // Can't be null because we skip those events without values
                    // ReSharper disable PossibleInvalidOperationException
                    profilingResults.Sort((x1, x2) => string.Compare(x1.Event.Value.Key.Name, x2.Event.Value.Key.Name, StringComparison.Ordinal));
                    // ReSharper restore PossibleInvalidOperationException
                }

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

                if (SortingMode == GameProfilingSorting.ByTime)
                {
                    profilingResults.Sort((x1, x2) => Math.Sign(x2.AccumulatedTime - x1.AccumulatedTime));
                }
                else
                {
                    // Can't be null because we skip those events without values
                    // ReSharper disable PossibleInvalidOperationException
                    profilingResults.Sort((x1, x2) => string.Compare(x1.Event.Value.Key.Name, x2.Event.Value.Key.Name, StringComparison.Ordinal));
                    // ReSharper restore PossibleInvalidOperationException
                }

                foreach (var result in profilingResults)
                {
                    AppendEvent(result, result.Event.Value, elapsedTime);
                }

                lock (stringLock)
                {
                    gcCollectionsString = gcCollectionsStringBuilder.ToString();
                    gcMemoryString = gcMemoryStringBuilder.ToString();
                    profilersString = profilersStringBuilder.ToString();
                    fpsStatString = fpsStatStringBuilder.ToString();
                }
            });
            stringBuilderTask.Start();
        }

        private void AppendEvent(ProfilingResult profilingResult, ProfilingEvent e, double elapsedTime)
        {
            profilersStringBuilder.AppendFormat("{0,-7:P1}", profilingResult.AccumulatedTime / elapsedTime);
            profilersStringBuilder.Append(" |  ");
            Profiler.AppendTime(profilersStringBuilder, profilingResult.MinTime);
            profilersStringBuilder.Append(" |  ");
            Profiler.AppendTime(profilersStringBuilder, profilingResult.Count != 0 ? profilingResult.AccumulatedTime / profilingResult.Count : 0);
            profilersStringBuilder.Append(" |  ");
            Profiler.AppendTime(profilersStringBuilder, profilingResult.MaxTime);

            profilersStringBuilder.Append(" | ");
            profilersStringBuilder.Append(e.Key);
            profilersStringBuilder.Append(" ");
            // ReSharper disable once ReplaceWithStringIsNullOrEmpty
            // This was creating memory allocation (GetEnumerable())
            if (e.Text != null && e.Text != "")
            {
                profilersStringBuilder.AppendFormat(e.Text, GetValue(profilingResult.Custom0), GetValue(profilingResult.Custom1), GetValue(profilingResult.Custom2), GetValue(profilingResult.Custom3));
            }

            profilersStringBuilder.Append("\n");
        }

        public string GetValue(ProfilingCustomValue? value)
        {
            if (!value.HasValue) return "";

            if (value.Value.ValueType == typeof(int))
            {
                return value.Value.IntValue.ToString();
            }
            if (value.Value.ValueType == typeof(float))
            {
                return value.Value.FloatValue.ToString(CultureInfo.InvariantCulture);
            }
            if (value.Value.ValueType == typeof(long))
            {
                return value.Value.LongValue.ToString();
            }
            if (value.Value.ValueType == typeof(double))
            {
                return value.Value.DoubleValue.ToString(CultureInfo.InvariantCulture);
            }

            return "";
        }

        protected override void Destroy()
        {
            Enabled = false;
            Visible = false;

            if (stringBuilderTask != null && !stringBuilderTask.IsCompleted)
            {
                stringBuilderTask.Wait();
            }

            gcProfiler.Dispose();
        }

        public override void Draw(GameTime gameTime)
        {
            if (spriteBatch == null)
            {
                spriteBatch = new SpriteBatch(Services.GetSafeServiceAs<IGraphicsDeviceService>().GraphicsDevice);
            }

            if (Font == null)
            {
                Font = Asset.Load<SpriteFont>("XenkoDefaultFont");
            }

            // TODO GRAPHICS REFACTOR where to get command list from?
            spriteBatch.Begin(Game.GraphicsContext, depthStencilState: DepthStencilStates.None);
            lock (stringLock)
            {                
                spriteBatch.DrawString(Font, gcMemoryString, new Vector2(10, 10), TextColor);
                spriteBatch.DrawString(Font, gcCollectionsString, new Vector2(10, 20), TextColor);
                spriteBatch.DrawString(Font, fpsStatString, new Vector2(10, 30), TextColor);
                spriteBatch.DrawString(Font, profilersString, new Vector2(10, 40), TextColor);               
            }
            spriteBatch.End();
        }

        public void EnableProfiling(bool excludeKeys = false, params ProfilingKey[] keys)
        {
            Enabled = true;
            Visible = true;

            if (keys.Length == 0)
            {
                Profiler.EnableAll();
            }
            else
            {
                if (excludeKeys)
                {
                    Profiler.EnableAll();
                    foreach (var profilingKey in keys)
                    {
                        Profiler.Disable(profilingKey);
                    }
                }
                else
                {
                    foreach (var profilingKey in keys)
                    {
                        Profiler.Enable(profilingKey);
                    }
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

        public SpriteFont Font { get; set; }

        public GameProfilingSorting SortingMode { get; set; } = GameProfilingSorting.ByTime;
    }
}
