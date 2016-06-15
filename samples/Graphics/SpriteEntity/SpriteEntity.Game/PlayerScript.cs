using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Animations;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Input;
using SiliconStudio.Xenko.Rendering.Sprites;

namespace SpriteEntity
{
    public class PlayerScript : AsyncScript
    {
        public SpriteSheet SpriteSheet;

        private enum AgentAnimation
        {
            Run,
            Idle,
            Shoot
        }

        // InputState represents all command inputs from a user
        private enum InputState
        {
            None,
            RunLeft,
            RunRight,
            Shoot,
        }

        // TODO centralize 
        private const float gameWidthX = 16f;       // from -8f to 8f
        private const float gameWidthHalfX = gameWidthX / 2f;

        private const int AgentMoveDistance = 10;       // virtual resolution unit/second
        private const float AgentShootDelay = 0.3f;     // second

        private static readonly Dictionary<AgentAnimation, int> AnimationFps = new Dictionary<AgentAnimation, int> { { AgentAnimation.Run, 12 }, { AgentAnimation.Idle, 7 }, { AgentAnimation.Shoot, 15 } };

        public LogicScript Logic;

        private SpriteComponent agentSpriteComponent;
        private SpriteSheet spriteSheet;

        // Touch input state
        private PointerEvent pointerState;
        private bool isPointerDown; // Cache state if a user is current touching the screen.
        
        [DataMember(Mask = LiveScriptingMask)] // keep the value when reloading the script (live-scripting)
        private bool isAgentFacingRight;

        [DataMember(Mask = LiveScriptingMask)] // keep the value when reloading the script (live-scripting)
        private float shootDelayCounter;

        [DataMember(Mask = LiveScriptingMask)] // keep the value when reloading the script (live-scripting)
        private AgentAnimation currentAgentAnimation;

        private AgentAnimation CurrentAgentAnimation
        {
            get
            {
                return currentAgentAnimation;
            }
            set
            {
                if (currentAgentAnimation == value)
                    return;

                string startFrame;
                string endFrame;
                currentAgentAnimation = value;

                switch (currentAgentAnimation)
                {
                    case AgentAnimation.Run:
                        startFrame = "run0";
                        endFrame = "run4";
                        break;
                    case AgentAnimation.Idle:
                        startFrame = "idle0";
                        endFrame = "idle4"; 
                        break;
                    case AgentAnimation.Shoot:
                        startFrame = "shoot0";
                        endFrame = "shoot4";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(value), value, null);
                }
                SpriteAnimation.Play(agentSpriteComponent, spriteSheet.FindImageIndex(startFrame), spriteSheet.FindImageIndex(endFrame), AnimationRepeatMode.LoopInfinite, AnimationFps[currentAgentAnimation]);
            }
        }

        public override async Task Execute()
        {
            spriteSheet = SpriteSheet;
            agentSpriteComponent = Entity.Get<SpriteComponent>();

            // Calculate offset of the bullet from the Agent if he is facing left and right side // TODO improve this
            var bulletOffset = new Vector3(1f, 0.2f, 0f);

            // Initialize game entities
            if(!IsLiveReloading)
            {
                shootDelayCounter = 0f;
                isAgentFacingRight = true;
                currentAgentAnimation = AgentAnimation.Idle;
            }
            CurrentAgentAnimation = currentAgentAnimation;

            while (Game.IsRunning)
            {
                await Script.NextFrame();

                var inputState = GetKeyboardInputState();

                if (inputState == InputState.None)
                    inputState = GetPointerInputState();

                // Reset the shoot delay, if state changes
                if (inputState != InputState.Shoot && CurrentAgentAnimation == AgentAnimation.Shoot)
                    shootDelayCounter = 0;

                if (inputState == InputState.RunLeft || inputState == InputState.RunRight)
                {
                    // Update Agent's position
                    var dt = (float) Game.UpdateTime.Elapsed.TotalSeconds;

                    Entity.Transform.Position.X += ((inputState == InputState.RunRight) ? AgentMoveDistance : -AgentMoveDistance)*dt;

                    if (Entity.Transform.Position.X < -gameWidthHalfX)
                        Entity.Transform.Position.X = -gameWidthHalfX;

                    if (Entity.Transform.Position.X > gameWidthHalfX)
                        Entity.Transform.Position.X = gameWidthHalfX;

                    isAgentFacingRight = inputState == InputState.RunRight;

                    // If agent face left, flip the sprite
                    Entity.Transform.Scale.X = isAgentFacingRight ? 1f : -1f;

                    // Update the sprite animation and state
                    CurrentAgentAnimation = AgentAnimation.Run;
                }
                else if (inputState == InputState.Shoot)
                {
                    // Update shootDelayCounter, and check whether it is time to create a new bullet
                    shootDelayCounter -= (float) Game.UpdateTime.Elapsed.TotalSeconds;

                    if (shootDelayCounter > 0)
                        continue;


                    // Reset shoot delay
                    shootDelayCounter = AgentShootDelay;

                    // Spawns a new bullet
                    var bullet = new Entity
                    {
                        new SpriteComponent { SpriteProvider = SpriteFromSheet.Create(spriteSheet, "bullet") },

                        // Will make the beam move along a direction at each frame
                        new BeamScript {DirectionX = isAgentFacingRight ? 1f : -1f, SpriteSheet = SpriteSheet},
                    };

                    bullet.Transform.Position = (isAgentFacingRight) ? Entity.Transform.Position + bulletOffset : Entity.Transform.Position + (bulletOffset*new Vector3(-1, 1, 1));

                    SceneSystem.SceneInstance.Scene.Entities.Add(bullet);
                    Logic.WatchBullet(bullet);

                    // Start animation for shooting
                    CurrentAgentAnimation = AgentAnimation.Shoot;
                }
                else
                {
                    CurrentAgentAnimation = AgentAnimation.Idle;
                }
            }
        }

        /// <summary>
        /// Determine input from a user from a keyboard.
        /// Left and Right arrow for running to left and right direction, Space for shooting.
        /// </summary>
        /// <returns></returns>
        private InputState GetKeyboardInputState()
        {
            if (Input.IsKeyDown(Keys.Right))
                return InputState.RunRight;
            if (Input.IsKeyDown(Keys.Left))
                return InputState.RunLeft;

            return Input.IsKeyDown(Keys.Space) ? InputState.Shoot : InputState.None;
        }

        /// <summary>
        /// Determine input from a user from Pointer (Touch/Mouse).
        /// It analyses the input from a user, and transform it to InputState using in the game, which is then returned.
        /// </summary>
        /// <returns></returns>
        private InputState GetPointerInputState()
        {
            // Get new state of Pointer (Touch input)
            if (Input.PointerEvents.Any())
            {
                var lastPointer = Input.PointerEvents.Last();
                isPointerDown = lastPointer.State != PointerState.Up;
                pointerState = lastPointer;
            }

            // If a user does not touch the screen, there is not input
            if (!isPointerDown)
                return InputState.None;

            // Transform pointer's position from normorlize coordinate to virtual resolution coordinate
            var resolution = new Vector2(GraphicsDevice.Presenter.BackBuffer.Width, GraphicsDevice.Presenter.BackBuffer.Height);
            var virtualCoordinatePointerPosition = resolution*pointerState.Position;

            // Get current position of the agent, since the origin of the sprite is at the center, region needs to be shifted to top-left
            var agentSize = spriteSheet["idle0"].SizeInPixels;
            var agentSpriteRegion = new RectangleF
            {
                X = (int) VirtualCoordToPixel(Entity.Transform.Position.X) - agentSize.X/2, Y = (int) VirtualCoordToPixel(Entity.Transform.Position.Y) - agentSize.Y/2, Width = agentSize.X, Height = agentSize.Y
            };

            // Check if the touch position is in the x-axis region of the agent's sprite; if so, input is shoot
            if (agentSpriteRegion.Left <= virtualCoordinatePointerPosition.X && virtualCoordinatePointerPosition.X <= agentSpriteRegion.Right)
                return InputState.Shoot;

            // Check if a pointer falls left or right of the screen, which would correspond to Run to the left or right respectively 
            return ((pointerState.Position.X) <= agentSpriteRegion.Center.X/resolution.X) ? InputState.RunLeft : InputState.RunRight;
        }

        private float VirtualCoordToPixel(float virtualCoord)
        {
            return (virtualCoord + (gameWidthHalfX))/gameWidthX*GraphicsDevice.Presenter.BackBuffer.Width;
        }
    }
}
