using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering.Sprites;

namespace JumpyJet
{
    /// <summary>
    /// The script in charge of creating and updating the pipes.
    /// </summary>
    public class PipesScript : SyncScript
    {
        private const int GapBetweenPipe = 400;
        private const int StartPipePosition = 400;

        public SpriteSheet Sprites;

        private readonly List<PipeSet> pipeSetList = new List<PipeSet>();

        /// <summary>
        /// The number of pipe sets move to the end of the screen.
        /// </summary>
        private int numberOfPipeMoved;
        
        private bool isScrolling;
        
        public override void Start()
        {
            // Load assets TODO: replace this by prefab when available.
            var pipeEntity = new Entity("pipe") { new SpriteComponent
            {
                SpriteProvider = new SpriteFromSheet { Sheet = Sprites, CurrentFrame = 2 },
                IgnoreDepth = true
            } };

            // Create PipeSets
            var screenWidth = GraphicsDevice.Presenter.BackBuffer.Width;
            for (int i = 0; i < (int)Math.Ceiling(screenWidth / (float)GapBetweenPipe); i++)
                CreatePipe(pipeEntity, StartPipePosition + i * GapBetweenPipe);
        }

        public override void Update()
        {
            if (!isScrolling)
                return;

            var elapsedTime = (float) Game.UpdateTime.Elapsed.TotalSeconds;

            for (int i = 0; i < pipeSetList.Count; i++)
            {
                // update the position of the pipe
                pipeSetList[i].Update(elapsedTime);
                    
                // move the pipe to the end of screen if not visible anymore
                if (pipeSetList[i].IsOutOfScreenLeft())
                    MovePipeToEnd(i, pipeSetList[i]);
            }
        }

        public override void Cancel()
        {
            // remove all the children pipes.
            Entity.Transform.Children.Clear();
        }

        /// <summary>
        /// Get the next pipe set to come.
        /// </summary>
        /// <param name="positionX">The position along the X axis</param>
        /// <returns>The next pipe to come</returns>
        public PipeSet GetNextPipe(float positionX)
        {
            PipeSet nextPipe = null;
            var nextPipePosition = float.PositiveInfinity;

            foreach (var pipeSet in pipeSetList)
            {
                var pipePosition = pipeSet.Entity.Transform.Position.X;
                if (!pipeSet.HasBeenPassed(positionX) && pipePosition < nextPipePosition)
                {
                    nextPipe = pipeSet;
                    nextPipePosition = pipePosition;
                }
            }

            return nextPipe;
        }

        /// <summary>
        /// Get the number of pipes that have passed provided position
        /// </summary>
        /// <param name="positionX">The position along X</param>
        /// <returns>The number of pipes that passed</returns>
        public int GetPassedPipeNumber(float positionX)
        {
            return numberOfPipeMoved + pipeSetList.Count(x=> x.HasBeenPassed(positionX));
        }
        
        private void CreatePipe(Entity pipeEntity, float startPosX)
        {
            var pipe = new PipeSet(pipeEntity, -GameScript.GameSpeed, startPosX, GraphicsDevice.Presenter.BackBuffer.Width);
            pipeSetList.Add(pipe);
            Entity.AddChild(pipe.Entity);
        }

        private void MovePipeToEnd(int pipeSetIndex, PipeSet pipeSet)
        {
            // When a pipe is determined to be reset,
            // get its next position by adding an offset to the position
            // of a pipe which index is before itself.
            var prevPipeSetIndex = pipeSetIndex - 1;

            if (prevPipeSetIndex < 0)
                prevPipeSetIndex = pipeSetList.Count - 1;

            var nextPosX = pipeSetList[prevPipeSetIndex].Entity.Transform.Position.X + GapBetweenPipe;

            pipeSet.ResetPipe(nextPosX);

            ++numberOfPipeMoved;
        }

        public void Reset()
        {
            numberOfPipeMoved = 0;
            foreach (var pipeSet in pipeSetList)
                pipeSet.ResetPipe();
        }

        public void StartScrolling()
        {
            isScrolling = true;
        }

        public void StopScrolling()
        {
            isScrolling = false;
        }
    }
}
