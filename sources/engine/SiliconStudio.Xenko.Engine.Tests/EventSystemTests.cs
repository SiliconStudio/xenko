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
        [Test]
        public void SameFrameReceive()
        {
            var key = new EventKey();
            var recv = new EventReceiver(key);

            key.Broadcast();
            Assert.True(recv.ReceiveOne());

            Assert.False(recv.ReceiveOne());
        }

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
                    frameCounter++;
                    broadcaster.Broadcast(frameCounter);
                                   
                    if (frameCounter == 10)
                    {
                        game.Exit();
                    }

                    await game.Script.NextFrame();
                }
            }, 100);

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

        [Test]
        public void DifferentThreadBroadcast()
        {
            var game = new EventSystemTestGame();

            var frameCounter = 0;

            var broadcaster = new EventKey<int>();

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

            Task.Run(async () =>
            {
                while (!game.IsRunning)
                {
                    await Task.Delay(100);
                }

                while (true)
                {
                    frameCounter++;
                    broadcaster.Broadcast(frameCounter);
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


        //todo verify clear behavior
    }
}
