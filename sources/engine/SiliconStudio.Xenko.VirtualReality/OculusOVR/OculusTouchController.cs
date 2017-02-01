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
        private float currentTrigger, currentGrip;

        public override Vector3 Position => currentPos;

        public override Quaternion Rotation => currentRot;

        public override Vector3 LinearVelocity => currentLinearVelocity;

        public override Vector3 AngularVelocity => currentAngularVelocity;

        public override DeviceState State => currentState;

        public override float Trigger => currentTrigger;

        public override float Grip => currentGrip;

        public OculusTouchController(TouchControllerHand hand)
        {
            this.hand = hand;
            currentState = DeviceState.Invalid;
        }

        internal void UpdateInputs(ref OculusOvr.InputProperties properties)
        {
            currentTrigger = hand == TouchControllerHand.Left ? properties.IndexTriggerLeft : properties.IndexTriggerRight;
            currentGrip = hand == TouchControllerHand.Left ? properties.HandTriggerLeft : properties.HandTriggerRight;
        }

        internal void UpdatePoses(ref OculusOvr.PosesProperties properties)
        {
            if (hand == TouchControllerHand.Left)
            {
                currentPos = properties.PosLeftHand;
                currentRot = properties.RotLeftHand;
                currentLinearVelocity = properties.LinearVelocityLeftHand;
                currentAngularVelocity = new Vector3(MathUtil.DegreesToRadians(properties.AngularVelocityLeftHand.X), MathUtil.DegreesToRadians(properties.AngularVelocityLeftHand.Y), MathUtil.DegreesToRadians(properties.AngularVelocityLeftHand.Z));
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
                currentAngularVelocity = new Vector3(MathUtil.DegreesToRadians(properties.AngularVelocityRightHand.X), MathUtil.DegreesToRadians(properties.AngularVelocityRightHand.Y), MathUtil.DegreesToRadians(properties.AngularVelocityRightHand.Z));
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
            return false;
        }

        public override bool IsPressed(TouchControllerButton button)
        {
            return false;
        }

        public override bool IsPressReleased(TouchControllerButton button)
        {
            return false;
        }

        public override bool IsTouchedDown(TouchControllerButton button)
        {
            return false;
        }

        public override bool IsTouched(TouchControllerButton button)
        {
            return false;
        }

        public override bool IsTouchReleased(TouchControllerButton button)
        {
            return false;
        }
    }
}