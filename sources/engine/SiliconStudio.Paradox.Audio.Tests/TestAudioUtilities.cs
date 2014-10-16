// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Audio.Tests
{
    static internal class TestAudioUtilities
    {
        internal static void ActiveAudioEngineUpdate(AudioEngine engine, int miliSeconds)
        {
            for (int i = 0; i < miliSeconds / 10 + 1; i++)
            {
                engine.Update();
                Utilities.Sleep(10);
            }
        }
    }
}