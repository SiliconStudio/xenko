using System;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Games;

namespace SiliconStudio.Xenko.VirtualReality
{
    public class OpenVrTouchController : TouchController
    {
        private readonly OpenVR.Controller.Hand hand;
        private int controllerIndex;
        private OpenVR.Controller controller;

        internal OpenVrTouchController(Game game, TouchControllerHand hand) : base(game.Services)
        {
            this.hand = (OpenVR.Controller.Hand)hand;
            Enabled = true;
        }

        public override void Update(GameTime gameTime)
        {
            var index = OpenVR.Controller.GetDeviceIndex(hand);

            if (controllerIndex != index)
            {
                if (index != -1)
                {
                    controller = new OpenVR.Controller(index);
                    controllerIndex = index;
                }
                else
                {
                    controller = null;
                }
            }

            controller?.Update();

            Matrix mat;
            OpenVR.GetControllerPose(controllerIndex, out mat);
            Pose = mat;

            base.Update(gameTime);
        }

        private OpenVR.Controller.ButtonId ToOpenVrButton(TouchControllerButton button)
        {
            switch (button)
            {
                case TouchControllerButton.Trigger:
                    return OpenVR.Controller.ButtonId.ButtonSteamVrTrigger;
                default:
                    throw new ArgumentOutOfRangeException(nameof(button), button, null);
            }
        }

        public override bool IsButtonDown(TouchControllerButton button)
        {
            return controller?.GetPressDown(ToOpenVrButton(button)) ?? false;
        }

        public override bool IsButtonPressed(TouchControllerButton button)
        {
            return controller?.GetPress(ToOpenVrButton(button)) ?? false;
        }

        public override bool IsButtonReleased(TouchControllerButton button)
        {
            return controller?.GetPressUp(ToOpenVrButton(button)) ?? false;
        }
    }
}