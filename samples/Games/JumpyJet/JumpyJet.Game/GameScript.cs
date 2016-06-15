using System;
using SiliconStudio.Xenko.Engine;

namespace JumpyJet
{
    /// <summary>
    /// GameScript wraps all entities in that game, update pipe-sets,
    /// checking for collision between CharacterScript and pipe-sets, and draw those sprites.
    /// </summary>
    public class GameScript : SyncScript
    {
        /// <summary>
        /// A reference to the UI script.
        /// </summary>
        public UIScript UI;

        /// <summary>
        /// A reference to the Background script.
        /// </summary>
        public BackgroundScript Background;

        /// <summary>
        /// A reference to the Character script.
        /// </summary>
        public CharacterScript Character;

        /// <summary>
        /// A reference to the Pipes script.
        /// </summary>
        public PipesScript Pipes;

        public const float GameSpeed = 290f;

        public const int PipeDepth = 4;

        private const int FloorLimit = -568 + 279 + 27;

        /// <summary>
        /// Add in game scripts, and UpdateLoop in Script system to start the scripts.
        /// </summary>
        public override void Start()
        {
            UI.StartButton.Click += StartPlayMode;
            UI.RetryButton.Click += StartPlayMode;
            UI.MenuButton.Click += StartMainMenuMode;

            StartMainMenuMode(this, EventArgs.Empty);
        }

        // Executed once a frame. It checks position between pipe sets and the CharacterScript,
        // and checks whether a score should be increased or not,
        // if so, trigger an event ScoreUpdated to update UI.
        public override void Update()
        {
            // get the next pipe set to come
            var nextPipeSet = Pipes.GetNextPipe(Character.PositionBack);

            // Update the score in the UI
            UI.SetScore(Pipes.GetPassedPipeNumber(Character.PositionBack));

            // Check if the character is colliding with the floor
            if (Character.Entity.Transform.Position.Y < FloorLimit)
                StartGameOverMode();

            // Determine if the character is colliding, if so start the game over mode
            if (Character.IsColliding(nextPipeSet))
                StartGameOverMode();
        }

        public override void Cancel()
        {
            UI.StartButton.Click -= StartPlayMode;
            UI.RetryButton.Click -= StartPlayMode;
            UI.MenuButton.Click -= StartMainMenuMode;
        }

        private void StartMainMenuMode(object obj, EventArgs args)
        {
            Pipes.Reset();
            Pipes.StopScrolling();
            UI.StartMainMenuMode();
            Background.StartScrolling();
            Character.Reset();
        }

        private void StartPlayMode(object obj, EventArgs args)
        {
            Pipes.Reset();
            Pipes.StartScrolling();
            UI.StartPlayMode();
            Background.StartScrolling();
            Character.Restart();
        }

        private void StartGameOverMode()
        {
            UI.StartGameOverMode();
            Pipes.StopScrolling();
            Background.StopScrolling();
            Character.Stop();
        }
    }
}
