// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_PLATFORM_IOS
namespace SiliconStudio.Xenko.Games
{
    internal interface IAnimatedGameView
    {
        void StartAnimating();
        void StopAnimating();
    }
}
#endif
