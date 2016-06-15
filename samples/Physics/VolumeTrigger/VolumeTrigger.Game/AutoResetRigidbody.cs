using System.Threading.Tasks;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Engine.Events;
using SiliconStudio.Xenko.Physics;

namespace VolumeTrigger
{
    public class AutoResetRigidbody : AsyncScript
    {
        private Vector3 startLocation;
        private Quaternion startRotation;

        public Entity TriggerEntity;

        public override async Task Execute()
        {
            startLocation = Entity.Transform.Position;
            startRotation = Entity.Transform.Rotation;

            //grab a reference to the falling sphere's rigidbody
            var rigidBody = Entity.Get<RigidbodyComponent>();

            var triggerKey = TriggerEntity.Get<Trigger>().TriggerEvent;
            var receiver = new EventReceiver<bool>(triggerKey);

            while (!CancellationToken.IsCancellationRequested)
            {
                var state = await receiver.ReceiveAsync();
                if (state)
                {
                    //switch to dynamic and awake the rigid body
                    rigidBody.RigidBodyType = RigidBodyTypes.Dynamic;
                    rigidBody.Activate(true); //need to awake to object
                }
                else
                {
                    //when out revert to kinematic and old starting position
                    rigidBody.RigidBodyType = RigidBodyTypes.Kinematic;
                    //reset position
                    Entity.Transform.Position = startLocation;
                    Entity.Transform.Rotation = startRotation;
                    Entity.Transform.UpdateWorldMatrix();
                    Entity.Get<PhysicsComponent>().UpdatePhysicsTransformation();
                }
            }
        }
    }
}
