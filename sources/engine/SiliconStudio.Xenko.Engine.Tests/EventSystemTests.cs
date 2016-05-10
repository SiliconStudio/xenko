using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using SiliconStudio.Xenko.Engine.Events;
using SiliconStudio.Xenko.Graphics.Regression;

namespace SiliconStudio.Xenko.Engine.Tests
{
    internal class EventSystemTestGame : GameTestBase
    {
        
    }

    [TestFixture]
    public class EventSystemTests
    {
        /// <summary>
        /// Make sure that events are able to be consumed immediately
        /// </summary>
        [Test]
        public void SameFrameReceive()
        {
            var key = new EventKey();
            var recv = new EventReceiver(key);

            key.Broadcast();
            Assert.True(recv.ReceiveOne());

            Assert.False(recv.ReceiveOne());
        }

        /// <summary>
        /// Make sure that we can receive events immediately even when using await/async
        /// </summary>
        [Test]
        public void SameFrameReceiveAsync()
        {
            var game = new EventSystemTestGame();

            var frameCounter = 0;

            game.Script.AddTask(async () =>
            {
                while (game.IsRunning)
                {
                    frameCounter++;
                    await game.Script.NextFrame();
                }
            }, 100);

            game.Script.AddTask(async () =>
            {
                var key = new EventKey();
                var recv = new EventReceiver(key);

                key.Broadcast();

                var currentFrame = frameCounter;

                await recv.ReceiveAsync();

                Assert.AreEqual(currentFrame, frameCounter);

                game.Exit();
            });

            game.Run();

            game.Dispose();
        }

        /// <summary>
        /// Make sure that newly created receivers do not receive previously broadcasted events (before creation)
        /// </summary>
        [Test]
        public void DelayedReceiverCreation()
        {
            var game = new EventSystemTestGame();

            var frameCount = 0;

            game.Script.AddTask(async () =>
            {
                var evt = new EventKey();
                EventReceiver rcv = null;
                while (frameCount < 25)
                {
                    if (frameCount == 5)
                    {
                        evt.Broadcast();
                    }
                    if (frameCount == 20)
                    {
                        rcv = new EventReceiver(evt);
                        Assert.False(rcv.ReceiveOne());
                        evt.Broadcast();
                    }
                    if (frameCount == 22)
                    {
                        Assert.NotNull(rcv);
                        Assert.True(rcv.ReceiveOne());

                        game.Exit();
                    }
                    await game.Script.NextFrame();
                    frameCount++;
                }
            });

            game.Run();

            game.Dispose();
        }

        /// <summary>
        /// Test that multiple receivers work
        /// </summary>
        [Test]
        public void MultipleReceivers()
        {
            var game = new EventSystemTestGame();

            var frameCounter = 0;

            var broadcaster = new EventKey<int>();

            game.Script.AddTask(async () =>
            {
                while (game.IsRunning)
                {
                    broadcaster.Broadcast(++frameCounter);
                                   
                    if (frameCounter == 10)
                    {
                        game.Exit();
                    }

                    await game.Script.NextFrame();
                }
            }, 100); //run this script after the others

            game.Script.AddTask(async () =>
            {
                var tests = 5;
                var recv = new EventReceiver<int>(broadcaster);

                while (tests-- > 0)
                {
                    var frame = await recv.ReceiveAsync();
                    Assert.AreEqual(frame, frameCounter);
                }
            });

            game.Script.AddTask(async () =>
            {
                var tests = 5;
                var recv = new EventReceiver<int>(broadcaster);

                while (tests-- > 0)
                {
                    var frame = await recv.ReceiveAsync();
                    Assert.AreEqual(frame, frameCounter);
                }
            });

            game.Script.AddTask(async () =>
            {
                var tests = 5;
                var recv = new EventReceiver<int>(broadcaster);

                while (tests-- > 0)
                {
                    var frame = await recv.ReceiveAsync();
                    Assert.AreEqual(frame, frameCounter);
                }
            });

            game.Run();

            game.Dispose();
        }

        /// <summary>
        /// Test that even if broadcast happens in another thread we receive events in the game schedluer thread
        /// </summary>
        [Test]
        public void DifferentThreadBroadcast()
        {
            var game = new EventSystemTestGame();

            var frameCounter = 0;

            var broadcaster = new EventKey();

            game.Script.AddTask(async () =>
            {
                var tests = 5;
                var recv = new EventReceiver(broadcaster);

                var threadId = Thread.CurrentThread.ManagedThreadId;

                while (tests-- > 0)
                {
                    await recv.ReceiveAsync();
                    Assert.AreEqual(threadId, Thread.CurrentThread.ManagedThreadId);
                }
            });

            game.Script.AddTask(async () =>
            {
                var tests = 5;
                var recv = new EventReceiver(broadcaster);

                var threadId = Thread.CurrentThread.ManagedThreadId;

                while (tests-- > 0)
                {
                    await recv.ReceiveAsync();
                    Assert.AreEqual(threadId, Thread.CurrentThread.ManagedThreadId);
                }
            });

            game.Script.AddTask(async () =>
            {
                var tests = 5;
                var recv = new EventReceiver(broadcaster);

                var threadId = Thread.CurrentThread.ManagedThreadId;

                while (tests-- > 0)
                {
                    await recv.ReceiveAsync();
                    Assert.AreEqual(threadId, Thread.CurrentThread.ManagedThreadId);
                }
            });

            Task.Run(async () =>
            {
                while (!game.IsRunning)
                {
                    await Task.Delay(100);
                }

                while (true)
                {
                    frameCounter++;
                    broadcaster.Broadcast();
                    if (frameCounter == 20)
                    {
                        game.Exit();
                    }
                    await Task.Delay(50);
                }
            });

            game.Run();

            game.Dispose();
        }

        /// <summary>
        /// Test buffered events and receive many in one go
        /// </summary>
        [Test]
        public void ReceiveManyCheck()
        {
            var game = new EventSystemTestGame();

            var frameCount = 0;

            game.Script.AddTask(async () =>
            {
                var evt = new EventKey();
                var rcv = new EventReceiver(evt, EventReceiverOptions.Buffered);
                while (frameCount < 25)
                {
                    evt.Broadcast();

                    if (frameCount == 20)
                    {
                        var manyEvents = rcv.ReceiveMany();
                        Assert.AreEqual(manyEvents.Count, 21);
                        game.Exit();
                    }
                    await game.Script.NextFrame();
                    frameCount++;
                }
            });

            game.Run();

            game.Dispose();
        }

        /// <summary>
        /// Test ClearEveryFrame option flag, which clears events at end of every game frame
        /// </summary>
        [Test]
        public void EveryFrameClear()
        {
            var game = new EventSystemTestGame();

            var frameCount = 0;

            game.Script.AddTask(async () =>
            {
                var evt = new EventKey();
                var rcv = new EventReceiver(evt, game.Script, EventReceiverOptions.ClearEveryFrame | EventReceiverOptions.Buffered);
                while (frameCount < 25)
                {
                    evt.Broadcast();
                    evt.Broadcast();

                    if (frameCount == 20)
                    {
                        var manyEvents = rcv.ReceiveMany();
                        Assert.AreEqual(manyEvents.Count, 2);
                        game.Exit();
                    }

                    await game.Script.NextFrame();

                    frameCount++;
                }
            });

            game.Run();

            game.Dispose();
        }
    }
}
