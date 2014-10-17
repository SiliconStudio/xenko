// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Threading.Tasks;

using SiliconStudio.Core;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Paradox.Games;
using SiliconStudio.Core.MicroThreading;

namespace SiliconStudio.Paradox
{
    /// <summary>
    /// The script system handles scripts scheduling in a game.
    /// </summary>
    public sealed class ScriptSystem : GameSystemBase
    {
        /// <summary>
        /// Gets the scheduler.
        /// </summary>
        /// <value>The scheduler.</value>
        public Scheduler Scheduler { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GameSystemBase" /> class.
        /// </summary>
        /// <param name="registry">The registry.</param>
        /// <remarks>The GameSystem is expecting the following services to be registered: <see cref="IGame" /> and <see cref="AssetManager" />.</remarks>
        public ScriptSystem(IServiceRegistry registry)
            : base(registry)
        {
            Enabled = true;
            Scheduler = new Scheduler();
            Services.AddService(typeof(ScriptSystem), this);
        }

        public override void Update(GameTime gameTime)
        {
            Scheduler.Run();
        }

        /// <summary>
        /// Allows to wait for next frame.
        /// </summary>
        /// <returns>ChannelMicroThreadAwaiter&lt;System.Int32&gt;.</returns>
        public ChannelMicroThreadAwaiter<int> NextFrame()
        {
            return Scheduler.NextFrame();
        }

        /// <summary>
        /// Adds the specified micro thread function.
        /// </summary>
        /// <param name="microThreadFunction">The micro thread function.</param>
        /// <returns>MicroThread.</returns>
        public MicroThread Add(Func<Task> microThreadFunction)
        {
            return Scheduler.Add(microThreadFunction);
        }

        /// <summary>
        /// Adds the specified script.
        /// </summary>
        /// <param name="script">The script.</param>
        /// <returns>MicroThread.</returns>
        public MicroThread Add(IScript script)
        {
            return Scheduler.Add(script.Execute);
        }

        /// <summary>
        /// Waits all micro thread finished their task completion.
        /// </summary>
        /// <param name="microThreads">The micro threads.</param>
        /// <returns>Task.</returns>
        public async Task WhenAll(params MicroThread[] microThreads)
        {
            await Scheduler.WhenAll(microThreads);
        }
    }
}