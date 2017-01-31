using System;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.VirtualReality
{
    internal class OculusTouchController : TouchController
    {
        private readonly TouchControllerHand hand;
        private Vector3 currentPos, currentLinearVelocity, currentAngularVelocity;
        private Quaternion currentRot;
        private DeviceState currentState;

        public override Vector3 Position => currentPos;

        public override Quaternion Rotation => currentRot;

        public override Vector3 LinearVelocity => currentLinearVelocity;

        public override Vector3 AngularVelocity => currentAngularVelocity;

        public override DeviceState State => currentState;

        public OculusTouchController(TouchControllerHand hand)
        {
            this.hand = hand;
            currentState = DeviceState.Invalid;
        }

        internal void Update(ref OculusOvr.PosesProperties properties)
        {
            if (hand == TouchControllerHand.Left)
            {
                currentPos = properties.PosLeftHand;
                currentRot = properties.RotLeftHand;
                currentLinearVelocity = properties.LinearVelocityLeftHand;
                currentAngularVelocity = properties.AngularVelocityLeftHand;
                if ((properties.StateLeftHand & 0x0001) == 0x0001)
                {
                    currentState = DeviceState.OutOfRange;

                    if ((properties.StateLeftHand & 0x0002) == 0x0002)
                    {
                        currentState = DeviceState.Valid;
                    }
                }
                else
                {
                    currentState = DeviceState.Invalid;
                }
            }
            else
            {
                currentPos = properties.PosRightHand;
                currentRot = properties.RotRightHand;
                currentLinearVelocity = properties.LinearVelocityRightHand;
                currentAngularVelocity = properties.AngularVelocityRightHand;
                if ((properties.StateRightHand & 0x0001) == 0x0001)
                {
                    currentState = DeviceState.OutOfRange;

                    if ((properties.StateRightHand & 0x0002) == 0x0002)
                    {
                        currentState = DeviceState.Valid;
                    }
                }
                else
                {
                    currentState = DeviceState.Invalid;
                }
            }
        }

        public override bool IsPressedDown(TouchControllerButton button)
        {
            throw new NotImplementedException();
        }

        public override bool IsPressed(TouchControllerButton button)
        {
            throw new NotImplementedException();
        }

        public override bool IsPressReleased(TouchControllerButton button)
        {
            throw new NotImplementedException();
        }

        public override bool IsTouchedDown(TouchControllerButton button)
        {
            throw new NotImplementedException();
        }

        public override bool IsTouched(TouchControllerButton button)
        {
            throw new NotImplementedException();
        }

        public override bool IsTouchReleased(TouchControllerButton button)
        {
            throw new NotImplementedException();
        }
    }
}