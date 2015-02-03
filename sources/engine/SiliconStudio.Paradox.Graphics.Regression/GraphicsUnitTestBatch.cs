using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using NUnit.Framework;

using SiliconStudio.Paradox.Games;

namespace SiliconStudio.Paradox.Graphics.Regression
{
    public class GraphicsUnitTestBatch : GraphicsTestBase
    {
        private readonly ManualResetEvent onLoadContentDone;
        private readonly ManualResetEvent onGameExit;
        private readonly List<DrawAction> drawActions = new List<DrawAction>();

        private bool isGameInitialized;

        public GraphicsUnitTestBatch()
        {
            onLoadContentDone = new ManualResetEvent(false);
            onGameExit = new ManualResetEvent(false);
        }

        protected override void Update(GameTime gameTime)
        {
            if (!isGameInitialized)
            {
                isGameInitialized = true;
                onLoadContentDone.Set();
            }
            else
            {
                // Schedule the draw action
                lock (drawActions)
                {
                    if (drawActions.Count > 0)
                    {
                        FrameGameSystem.Draw(drawActions[0].Run);
                        drawActions.RemoveAt(0);
                    }
                }
            }
            base.Update(gameTime);
        }

        [TestFixtureSetUp]
        public void InitializeThisGame()
        {
            FrameGameSystem.IsUnityTestFeeding = true;
            Task.Run(
                () =>
                {
                    RunGameTest(this);
                    onGameExit.Set();
                });
            onLoadContentDone.WaitOne();
        }

        [TestFixtureTearDown]
        public void DisposeThisGame()
        {
            FrameGameSystem.AllTestsCompleted = true;
            onGameExit.WaitOne();
        }

        protected void RunDrawTest(Action action)
        {
            var deferredAction = new DrawAction(action);
            lock (drawActions)
            {
                drawActions.Add(deferredAction);
            }
            deferredAction.WaitResult();
        }

        private class DrawAction
        {
            private readonly ManualResetEvent taskExecuted;
            private readonly Action actionToRun;
            private Exception actionException;

            public DrawAction(Action action)
            {
                actionToRun = action;
                taskExecuted = new ManualResetEvent(false);
            }

            public void WaitResult()
            {
                taskExecuted.WaitOne();
                if (actionException != null)
                {
                    throw actionException;
                }
            }

            public void Run()
            {
                try
                {
                    actionToRun();
                }
                catch (Exception ex)
                {
                    actionException = ex;
                }
                taskExecuted.Set();
            }
        }
    }
}