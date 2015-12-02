// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Reflection;
using SiliconStudio.Core;
using SiliconStudio.Xenko.Games;

namespace SiliconStudio.Xenko.Graphics.Regression
{
    public class FrameGameSystem : GameSystemBase
    {
        #region Private members

        /// <summary>
        /// List of methods to call in the Update method.
        /// </summary>
        private readonly List<SetupMethodInfo> updateMethods;

        /// <summary>
        /// List of methods to call in the Draw method.
        /// </summary>
        private readonly List<SetupMethodInfo> drawMethods;

        /// <summary>
        /// The frames to take screenshot of.
        /// </summary>
        private readonly HashSet<int> screenshotFrames;

        /// <summary>
        /// The current frame.
        /// </summary>
        private int frameCount;

        /// <summary>
        /// The last screenshot frame.
        /// </summary>
        private int lastFrame;

        #endregion

        #region Public properties

        /// <summary>
        /// Flag stating that all the tests have been rendered.
        /// </summary>
        public bool AllTestsCompleted => frameCount > lastFrame;

        public string TestName;

        /// <summary>
        /// Flag stating that a screenshot should be taken.
        /// </summary>
        public bool TakeSnapshot
        {
            get
            {
                return screenshotFrames.Contains(frameCount);
            }
        }

        /// <summary>
        /// The current frame.
        /// </summary>
        public int CurrentFrame
        {
            get
            {
                return frameCount;
            }
        }

        /// <summary>
        /// The number of frames to render.
        /// </summary>
        public int TestCount
        {
            get
            {
                return screenshotFrames.Count;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is test feeding.
        /// </summary>
        /// <value><c>true</c> if this instance is test feeding; otherwise, <c>false</c>.</value>
        public bool IsUnityTestFeeding { get; set; }

        #endregion

        #region Constructor

        public FrameGameSystem(IServiceRegistry registry)
            : base(registry)
        {
            updateMethods = new List<SetupMethodInfo>();
            drawMethods = new List<SetupMethodInfo>();
            screenshotFrames = new HashSet<int>();
            lastFrame = -1;
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Add a method to call in the update function.
        /// </summary>
        /// <param name="frameIndex">The index of the frame.</param>
        /// <param name="method">The method to call.</param>
        /// <returns>this.</returns>
        public FrameGameSystem Update(int frameIndex, Action method)
        {
            AddTestMethods(method, frameIndex, updateMethods);
            return this;
        }

        /// <summary>
        /// Add a method to call in the update function.
        /// </summary>
        /// <param name="method">The method to call.</param>
        /// <returns>this.</returns>
        public FrameGameSystem Update(Action method)
        {
            AddTestMethods(method, lastFrame + 1, updateMethods);
            return this;
        }

        /// <summary>
        /// Add a method to call in the draw function.
        /// </summary>
        /// <param name="frameIndex">The index of the frame.</param>
        /// <param name="method">The method to call.</param>
        /// <returns>this.</returns>
        public FrameGameSystem Draw(int frameIndex, Action method)
        {
            AddTestMethods(method, frameIndex, drawMethods);
            return this;
        }

        /// <summary>
        /// Add a method to call in the draw function.
        /// </summary>
        /// <param name="method">The method to call.</param>
        /// <returns>this.</returns>
        public FrameGameSystem Draw(Action method)
        {
            AddTestMethods(method, lastFrame + 1, drawMethods);
            return this;
        }

        /// <summary>
        /// Take a screenshot at the desired frame.
        /// </summary>
        /// <param name="frameIndex">the frame index.</param>
        /// <returns>this</returns>
        public FrameGameSystem TakeScreenshot(int frameIndex)
        {
            screenshotFrames.Add(frameIndex);
            if (frameIndex > lastFrame)
                lastFrame = frameIndex;
            return this;
        }

        /// <summary>
        /// Take a screenshot at the desired frame.
        /// </summary>
        /// <returns>this</returns>
        public FrameGameSystem TakeScreenshot()
        {
            return TakeScreenshot(lastFrame + 1);
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            ExecuteFrameMethod(drawMethods);
        }

        public override void Update(GameTime gameTime)
        {
            // Update is called twice before the first draw
            frameCount = gameTime.FrameCount - 1;
            base.Update(gameTime);
            ExecuteFrameMethod(updateMethods);
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Add method to the list.
        /// </summary>
        /// <param name="action">The action to add.</param>
        /// <param name="frameIndex">The index of the frame.</param>
        /// <param name="targetList">The list where to add the method.</param>
        private void AddTestMethods(Action action, int frameIndex, List<SetupMethodInfo> targetList)
        {
            SetupMethodInfo smi;
            smi.Action = action;
            smi.FrameIndex = frameIndex;
            targetList.Add(smi);
        }

        /// <summary>
        /// Execute the test method for the current frame.
        /// </summary>
        /// <param name="targetList">List of methods.</param>
        private void ExecuteFrameMethod(List<SetupMethodInfo> targetList)
        {
            var methodsToRemove = new Stack<int>();
            for (var i = 0; i < targetList.Count; ++i)
            {
                var method = targetList[i];
                if (method.FrameIndex == frameCount)
                {
                    if (method.Action != null)
                    {
                        GameTestBase.TestGameLogger.Debug(@"Executing method in Draw/Update for frame " + frameCount + @": " + method.Action.GetMethodInfo().Name);
                        method.Action.Invoke();
                    }
                    methodsToRemove.Push(i);
                }
            }

            // remove methods so that they are only executed once per frame.
            while (methodsToRemove.Count > 0)
                targetList.RemoveAt(methodsToRemove.Pop());
        }

        #endregion

        #region Helper structures

        /// <summary>
        /// Structure to store the information of the method to run before any test.
        /// </summary>
        private struct SetupMethodInfo
        {
            public Action Action;

            public int FrameIndex;
        }

        #endregion
    }
}
