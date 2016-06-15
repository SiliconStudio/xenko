using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Input;
using SiliconStudio.Xenko.Physics;
using SiliconStudio.Xenko.Rendering;

namespace VolumeTrigger
{
    public class Player : SyncScript
    {
        private const float speed = 0.25f;
        private CharacterComponent character;

        public override void Start()
        {
            character = Entity.Get<CharacterComponent>();
            character.Gravity = -10.0f;
			var rigidBodyComponent = Entity.Get<RigidbodyComponent>();
            if (rigidBodyComponent != null)
            {
				rigidBodyComponent.CanSleep = false;                
            }
        }

        private Vector3 pointerVector;

        public override void Update()
        {
            var move = new Vector3();

            if (Input.IsKeyDown(Keys.A) || Input.IsKeyDown(Keys.Left))
            {
                move = -Vector3.UnitX;
            }
            if (Input.IsKeyDown(Keys.D) || Input.IsKeyDown(Keys.Right))
            {
                move = Vector3.UnitX;
            }

            if (Input.PointerEvents.Any())
            {
                var last = Input.PointerEvents.Last();
                if (last != null)
                {
                    switch (last.State)
                    {
                        case PointerState.Down:
                            if (last.Position.X < 0.5)
                            {
                                pointerVector = -Vector3.UnitX;
                            }
                            else
                            {
                                pointerVector = Vector3.UnitX;
                            }
                            break;
                        case PointerState.Up:
                        case PointerState.Out:
                        case PointerState.Cancel:
                            pointerVector = Vector3.Zero;
                            break;
                    }
                }
            }

            if (pointerVector != Vector3.Zero)
            {
                move = pointerVector;
            }

            move *= speed;

            character.Move(move);
        }
    }
}
