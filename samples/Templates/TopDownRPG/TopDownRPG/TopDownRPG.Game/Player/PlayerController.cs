using System;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Engine.Events;
using SiliconStudio.Xenko.Physics;
using TopDownRPG.Core;

namespace TopDownRPG.Player
{
    public class PlayerController : SyncScript
    {
        // The character controller does only two things - moves the character and makes it attack close targets
        //  If the character is too far from its target it will run after it until it's close enough then halt movement and attack
        //  If the character is walking towards a specific location instead it will run to it then halt movement when close enough


        private readonly EventReceiver<ClickResult> moveDestinationEvent = new EventReceiver<ClickResult>(PlayerInput.MoveDestinationEventKey);

        // Movement

        /// <summary>
        /// The maximum speed the character can run at
        /// </summary>
        [Display("Run Speed")]
        public float MaxRunSpeed { get; set; } = 10;

        // The PlayerController will propagate its speed to the AnimationController
        public static readonly EventKey<float> RunSpeedEventKey = new EventKey<float>();

        private Vector3 moveDestination;

        // Allow some inertia to the movement
        private Vector3 moveDirection = Vector3.Zero;

        private bool isRunning = false;


        // Attacking
        [Display("Punch Collision")]
        public RigidbodyComponent PunchCollision { get; set; }

        /// <summary>
        /// The maximum distance from which the character can perform an attack
        /// </summary>
        [Display("Attack Distance")]
        public float AttackDistance { get; set; } = 1f;

        /// <summary>
        /// Cooldown in seconds required for the character to recover from starting an attack until it can choose another action
        /// </summary>
        [Display("Attack Cooldown")]
        public float AttackCooldown { get; set; } = 0.65f;

        // The PlayerController will propagate if it is attacking to the AnimationController
        public static readonly EventKey<bool> IsAttackingEventKey = new EventKey<bool>();


        // Character Component
        private CharacterComponent character;
        private Entity modelChildEntity;
        private float yawOrientation;

        private Entity attackEntity = null;
        private float attackCooldown = 0f;


        /// <summary>
        /// Called when the script is first initialized
        /// </summary>
        public override void Start()
        {
            base.Start();

            // Will search for an CharacterComponent within the same entity as this script
            character = Entity.Get<CharacterComponent>();
            if (character == null) throw new ArgumentException("Please add a CharacterComponent to the entity containing PlayerController!");

            if (PunchCollision == null) throw new ArgumentException("Please add a RigidbodyComponent as a PunchCollision to the entity containing PlayerController!");

            modelChildEntity = Entity.GetChild(0);

            moveDestination = Entity.Transform.WorldMatrix.TranslationVector;

            PunchCollision.Enabled = false;
        }

        /// <summary>
        /// Called on every frame update
        /// </summary>
        public override void Update()
        {
            Attack();

            Move(MaxRunSpeed);
        }

        private void Attack()
        {
            var dt = (float) Game.UpdateTime.Elapsed.TotalSeconds;
            attackCooldown = (attackCooldown > 0) ? attackCooldown - dt : 0f;

            PunchCollision.Enabled = (attackCooldown > 0);

            if (attackEntity == null)
                return;

            var directionToCharacter = attackEntity.Transform.WorldMatrix.TranslationVector -
                                       modelChildEntity.Transform.WorldMatrix.TranslationVector;
            directionToCharacter.Y = 0;

            var currentDistance = directionToCharacter.Length();
            if (currentDistance <= AttackDistance)
            {
                // Attack!
                HaltMovement();

                attackEntity = null;
                attackCooldown = AttackCooldown;
                PunchCollision.Enabled = true;
                IsAttackingEventKey.Broadcast(true);
            }
            else
            {
                isRunning = true;
                directionToCharacter.Normalize();
                moveDestination = attackEntity.Transform.WorldMatrix.TranslationVector;
            }
        }

        private void HaltMovement()
        {
            isRunning = false;
            moveDirection = Vector3.Zero;
            character.Move(Vector3.Zero);
            moveDestination = modelChildEntity.Transform.WorldMatrix.TranslationVector;
        }

        private void Move(float speed)
        {
            if (attackCooldown > 0)
                return;

            // Use the delta time from physics
            var dt = this.GetSimulation().FixedTimeStep;

            // Character speed
            ClickResult clickResult;
            if (moveDestinationEvent.TryReceive(out clickResult) && clickResult.Type != ClickType.Empty)
            {
                if (clickResult.Type == ClickType.Ground)
                {
                    attackEntity = null;
                    moveDestination = clickResult.WorldPosition;
                    isRunning = true;
                }

                if (clickResult.Type == ClickType.LootCrate)
                {
                    attackEntity = clickResult.ClickedEntity;
                    Attack();
                }
            }

            if (!isRunning)
            {
                RunSpeedEventKey.Broadcast(0);
                return;
            }

            var direction = moveDestination - Entity.Transform.WorldMatrix.TranslationVector;
            direction /= 3;
            var lengthSqr = direction.LengthSquared();

            if (lengthSqr < 0.01f)
            {
                HaltMovement();
                RunSpeedEventKey.Broadcast(0);
                return;
            }

            if (lengthSqr > 1)
                direction.Normalize();

            // Allow very simple inertia to the character to make animation transitions more fluid
            moveDirection = moveDirection*0.85f + direction * 0.15f;

            character.Move(moveDirection * speed * dt);

            // Broadcast speed as per cent of the max speed
            RunSpeedEventKey.Broadcast(moveDirection.Length());

            // Character orientation
            if (moveDirection.Length() > 0.001)
            {
                yawOrientation = MathUtil.RadiansToDegrees((float) Math.Atan2(-moveDirection.Z, moveDirection.X) + MathUtil.PiOverTwo);
            }
            modelChildEntity.Transform.Rotation = Quaternion.RotationYawPitchRoll(MathUtil.DegreesToRadians(yawOrientation), 0, 0);
        }
    }
}
