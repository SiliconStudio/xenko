// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System.Linq;
using System.Threading.Tasks;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Audio;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Input;
using SiliconStudio.Xenko.UI;
using SiliconStudio.Xenko.UI.Controls;
using SiliconStudio.Xenko.UI.Panels;

namespace SimpleAudio
{
    /// <summary>
    /// The main script in charge of the sound.
    /// </summary>
    public class SoundScript : AsyncScript
    {
        /// <summary>
        /// The page containing the UI elements
        /// </summary>
        public UIPage Page {get; set; }
        
        public Sound SoundMusic;
        private SoundInstance music;
        public Sound SoundEffect;
        private SoundInstance effect;

        [DataMember(Mask = LiveScriptingMask)] // keep the value when reloading the script (live-scripting)
        private float originalPositionX;

        [DataMember(Mask = LiveScriptingMask)] // keep the value when reloading the script (live-scripting)
        private float fontColor;

        public override async Task Execute()
        {
            var imgLeft  = Page?.RootElement.FindVisualChildOfType<ImageElement>("LeftWave");
            var imgRight = Page?.RootElement.FindVisualChildOfType<ImageElement>("RightWave");
            
            music = SoundMusic.CreateInstance();
            effect = SoundEffect.CreateInstance();

            if (!IsLiveReloading)
            {
                // start ambient music
                music.IsLooping = true;
                music.Play();

                fontColor = 0;
                originalPositionX = (imgRight != null) ? imgRight.GetCanvasRelativePosition().X : 0.65f;
            }

            while (Game.IsRunning)
            {
                if (Input.PointerEvents.Any(item => item.State == PointerState.Down)) // New click
                {
                    if (imgLeft != null && imgRight != null)
                    {
                        // reset wave position
                        imgLeft.SetCanvasRelativePosition(new Vector3(1 - originalPositionX, 0.5f, 0));
                        imgLeft.Opacity = 0;
                    
                        imgRight.SetCanvasRelativePosition(new Vector3(originalPositionX, 0.5f, 0));
                        imgRight.Opacity = 0;
                    }
                    
                    // reset transparency
                    fontColor = 1;

                    // play the sound effect on each touch on the screen
                    effect.Stop();
                    effect.Play();
                }
                else
                {
                    if (imgLeft != null && imgRight != null)
                    {
                        imgLeft.SetCanvasRelativePosition(imgLeft.GetCanvasRelativePosition()   - new Vector3(0.0025f, 0, 0));
                        imgRight.SetCanvasRelativePosition(imgRight.GetCanvasRelativePosition() + new Vector3(0.0025f, 0, 0));
                        
                        // changing font transparency
                        fontColor = 0.93f * fontColor;
                        imgLeft.Opacity = fontColor;
                        imgRight.Opacity = fontColor;
                    }
                }

                // wait for next frame
                await Script.NextFrame();
            }
        }
    }
}
