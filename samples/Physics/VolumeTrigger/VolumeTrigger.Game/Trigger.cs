using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Physics;
using System.Threading.Tasks;
using SiliconStudio.Core;
using SiliconStudio.Xenko.Engine.Events;

namespace VolumeTrigger
{
    public class Trigger : AsyncScript
    {
		[DataMemberIgnore]
		public EventKey<bool> TriggerEvent = new EventKey<bool>();
		
        public override async Task Execute()
        {
            var trigger = Entity.Get<PhysicsComponent>();
            trigger.ProcessCollisions = true;

            //start out state machine
            while (Game.IsRunning)
            {
                //wait for entities coming in
                var firstCollision = await trigger.NewCollision();

                TriggerEvent.Broadcast(true);

                //now wait for entities exiting
                Collision collision;
                do
                {
                    collision = await trigger.CollisionEnded();
                } while (collision != firstCollision);
               
                TriggerEvent.Broadcast(false);
            }
        }
    }
}
