using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Navigation;
using SiliconStudio.Xenko.Physics;

namespace SiliconStudio.Xenko.Navigation.Tests
{
    public class PlayerController : SyncScript
    {
        /// <summary>
        /// The maximum speed the character can run at
        /// </summary>
        [Display("Run Speed")]
        public float Speed { get; set; } = 10;

        /// <summary>
        /// The distance from the destination at which the character will stop moving
        /// </summary>
        public float DestinationThreshold { get; set; } = 0.2f;

        /// <summary>
        /// A number from 0 to 1 indicating how much a character should slow down when going around corners
        /// </summary>
        /// <remarks>0 is no slowdown and 1 is completely stopping (on >90 degree angles)</remarks>
        public float CornerSlowdown { get; set; } = 0.6f;

        /// <summary>
        /// Multiplied by the distance to the target and clamped to 1 and used to slow down when nearing the destination
        /// </summary>
        public float DestinationSlowdown { get; set; } = 0.4f;

        public NavigationComponent Navigation { get; set; }

        // Allow some inertia to the movement
        private Vector3 moveDirection = Vector3.Zero;

        private bool isRunning = false;
        
        // Character Component
        private CharacterComponent character;
        private float yawOrientation;
        
        // Pathfinding state
        private readonly List<Vector3> pathToDestination = new List<Vector3>();
        private int waypointIndex;
        private Vector3 moveDestination;
        
        private Vector3 spawnLocation;

        private bool ReachedDestination => waypointIndex >= pathToDestination.Count;

        private Vector3 CurrentWaypoint => waypointIndex < pathToDestination.Count ? pathToDestination[waypointIndex] : Vector3.Zero;

        private MoveResult pendingResult;
        private TaskCompletionSource<MoveResult> taskCompletionSource;

        /// <summary>
        /// Called when the script is first initialized
        /// </summary>
        public override void Start()
        {
            base.Start();

            // Get the navigation component on the same entity as this script
            Navigation = Entity.Get<NavigationComponent>();
            if (Navigation == null) throw new ArgumentException("Please add a NavigationComponent to the entity containing PlayerController with the correct navigation mesh!");

            // Will search for an CharacterComponent within the same entity as this script
            character = Entity.Get<CharacterComponent>();
            if (character == null) throw new ArgumentException("Please add a CharacterComponent to the entity containing PlayerController!");
            
            spawnLocation = Entity.Transform.WorldMatrix.TranslationVector;
            moveDestination = spawnLocation;
        }

        public void Reset()
        {
            pathToDestination.Clear();
            HaltMovement();
            Entity.Transform.Position = Entity.Transform.WorldToLocal(spawnLocation);
            moveDestination = spawnLocation;
        }

        public override void Update()
        {
            if (!ReachedDestination)
            {
                var direction = CurrentWaypoint - Entity.Transform.WorldMatrix.TranslationVector;

                // Get distance towards next point and normalize the direction at the same time
                var length = direction.Length();
                direction /= length;

                // Check when to advance to the next waypoint
                bool advance = false;

                // Check to see if an intermediate point was passed by projecting the position along the path
                if (pathToDestination.Count > 0 && waypointIndex > 0 && waypointIndex != pathToDestination.Count - 1)
                {
                    Vector3 pointNormal = CurrentWaypoint - pathToDestination[waypointIndex - 1];
                    pointNormal.Normalize();
                    float current = Vector3.Dot(Entity.Transform.WorldMatrix.TranslationVector, pointNormal);
                    float target = Vector3.Dot(CurrentWaypoint, pointNormal);
                    if (current > target)
                    {
                        advance = true;
                    }
                }
                if (length < DestinationThreshold) // Check distance to final point
                {
                    advance = true;
                }

                // Advance waypoint?
                if (advance)
                {
                    waypointIndex++;
                    if (ReachedDestination)
                    {
                        // Final waypoint reached
                        HaltMovement();

                        pendingResult.EndTime = Game.UpdateTime.Total;
                        pendingResult.End = Entity.Transform.WorldMatrix.TranslationVector;
                        pendingResult.Success = true;
                        taskCompletionSource.SetResult(pendingResult);
                        taskCompletionSource = null;

                        return;
                    }
                }

                // Calculate speed based on distance from final destination
                float moveSpeed = (moveDestination - Entity.Transform.WorldMatrix.TranslationVector).Length() * DestinationSlowdown;
                if (moveSpeed > 1.0f)
                    moveSpeed = 1.0f;

                // Slow down around corners
                float cornerSpeedMultiply = Math.Max(0.0f, Vector3.Dot(direction, moveDirection)) * CornerSlowdown + (1.0f - CornerSlowdown);

                // Allow a very simple inertia to the character to make animation transitions more fluid
                moveDirection = moveDirection * 0.85f + direction * moveSpeed * cornerSpeedMultiply * 0.15f;

                character.SetVelocity(moveDirection * Speed);

                // Character orientation
                if (moveDirection.Length() > 0.001)
                {
                    yawOrientation = MathUtil.RadiansToDegrees((float)Math.Atan2(-moveDirection.Z, moveDirection.X) + MathUtil.PiOverTwo);
                }
            }
            else
            {
                // No target
                HaltMovement();
            }
        }

        public class MoveResult
        {
            public bool Success;
            public Vector3 Start;
            public Vector3 End;
            public TimeSpan StartTime;
            public TimeSpan EndTime;
        }
        
        public Task<MoveResult> TryMove(Vector3 destination)
        {
            if(taskCompletionSource != null)
                throw new InvalidOperationException("Overlapping update commands");

            pendingResult = new MoveResult();
            pendingResult.Start = Entity.Transform.WorldMatrix.TranslationVector;
            pendingResult.StartTime = Game.UpdateTime.Total;
            pendingResult.Success = false;

            // Generate a new path using the navigation component
            pathToDestination.Clear();
            if (Navigation.TryFindPath(destination, pathToDestination))
            {
                // Skip the points that are too close to the player
                waypointIndex = 0;
                while (!ReachedDestination && (CurrentWaypoint - Entity.Transform.WorldMatrix.TranslationVector).Length() < 0.25f)
                {
                    waypointIndex++;
                }

                // Add destination point
                pathToDestination.Add(destination);

                // If this path still contains more points, set the player to running
                if (!ReachedDestination)
                {
                    isRunning = true;
                    moveDestination = destination;
                }

                taskCompletionSource = new TaskCompletionSource<MoveResult>();
                return taskCompletionSource.Task;
            }
            else
            {
                // Could not find a path to the target location
                pathToDestination.Clear();
                HaltMovement();
                return Task.FromResult(pendingResult);
            }
        }

        private void HaltMovement()
        {
            isRunning = false;
            moveDirection = Vector3.Zero;
            character.SetVelocity(Vector3.Zero);
            moveDestination = Entity.Transform.WorldMatrix.TranslationVector;
        }
    }
}