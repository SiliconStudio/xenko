// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System.Linq;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Input;
using SiliconStudio.Xenko.Physics;

namespace PhysicsSample
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

            foreach(var evt in Input.PointerEvents)
            { 
                switch (evt.EventType)
                {
                    case PointerEventType.Pressed:
                        if (evt.Position.X < 0.5)
                        {
                            pointerVector = -Vector3.UnitX;
                        }
                        else
                        {
                            pointerVector = Vector3.UnitX;
                        }
                        break;
                    case PointerEventType.Released:
                    case PointerEventType.Canceled:
                        pointerVector = Vector3.Zero;
                        break;
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
