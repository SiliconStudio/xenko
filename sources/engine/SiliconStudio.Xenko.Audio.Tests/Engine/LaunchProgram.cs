// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.Xenko.Audio.Tests.Engine
{
    public class LaunchProgram
    {
        public static void Main()
        {
            {
                var test = new TestController();
                test.TestExitLoop();
                test.TestPlayState();
                test.TestStop();
                test.TestPause();
                test.TestDefaultValues();
                test.TestVolume();
                test.TestIsLooped();
                test.TestPlay();
            }
            {
                var test = new TestAudioSystem();
                test.TestDopplerCoherency();
                test.TestAttenuationCoherency();
                test.TestLocalizationCoherency();
                test.TestSeveralControllers();
                test.TestEffectsAndMusic();
                test.TestAddRemoveEmitter();
                test.TestRemoveListener();
                test.TestAddListener();
                test.TestAudioEngine();
                test.TestAddRemoveListener();
            }
            {
                var test = new TestAudioEmitterProcessor();
                test.TestEmitterUpdateValues();
                test.TestAddRemoveListeners();
                test.TestAddRemoveEntityWithEmitter();
                test.TestAddRemoveSoundEffect();
            }

//            {
//                var test = new TestAudioEmitterComponent();
//                test.TestInitialization();
//                test.TestAttachDetachSounds();
//                test.TestGetController();
//            }

            {
                var test = new TestAudioListenerProcessor();
                test.TestAddAudioSysThenEntitySys();
                test.TestAddEntitySysThenAudioSys();
                test.TestRemoveListenerFromAudioSystem();
                test.TestRemoveListenerFromEntitySystem();
            }
            {
                var test = new TestGame();
                test.TestAccessToAudio();
                test.TestCreationDestructionOfTheGame();
            }
            {
                var test = new TestScriptContext();
                test.TestScriptCreationDestruction();
                test.TestScriptAudioAccess();
            }
            {
                var test = new TestAssetLoading();
                test.TestSoundEffectLoading();
                test.TestSoundMusicLoading();
            }
        }
    }
}
