using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using SiliconStudio.Xenko.Engine.Events;
using SiliconStudio.Xenko.Graphics.Regression;

namespace SiliconStudio.Xenko.Engine.Tests
{
    [TestFixture]
    public class EventSystemTests : GameTestBase
    {
        public EventSystemTests()
        {
            Task.Run(() => Run());
        }

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
            var frameCounter = 0;
            var waiting = new AutoResetEvent(false);

            Script.AddTask(async () =>
            {
                while (IsRunning)
                {
                    frameCounter++;
                    await Script.NextFrame();
                }
            }, 100);

            Script.AddTask(async () =>
            {
                var key = new EventKey();
                var recv = new EventReceiver(key);

                key.Broadcast();

                var currentFrame = frameCounter;

                await recv.ReceiveAsync();

                Assert.AreEqual(currentFrame, frameCounter);

                waiting.Set();
            });

            waiting.WaitOne();
        }

        [Test]
        public void DelayedReceiverCreation()
        {
            var frameCount = 0;
            var waiting = new AutoResetEvent(false);

            Script.AddTask(async () =>
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

                        waiting.Set();
                    }
                    await Script.NextFrame();
                    frameCount++;
                }
            });

            waiting.WaitOne();
        }

        //todo verify clear behavior
        //todo broadcast to multiple receivers
        //todo async and non async recv
        //todo receivemany
    }
}
