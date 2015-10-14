// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP && SILICONSTUDIO_RUNTIME_CORECLR
using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Input
{
    public partial class InputManager
    {
        public InputManager(IServiceRegistry registry): base(registry)
        {

        }

        public override void Initialize()
        {
            base.Initialize();
        }

        public override void LockMousePosition(bool forceCenter = false)
        {
        }

        public override void UnlockMousePosition()
        {
        }

        public override bool MultiTouchEnabled { get; set; }
    }
}
#endif