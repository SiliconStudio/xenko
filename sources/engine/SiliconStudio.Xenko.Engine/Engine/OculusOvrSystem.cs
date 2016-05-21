using System;
using SiliconStudio.Core;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Xenko.Native;

namespace SiliconStudio.Xenko.Engine
{
    public class OculusOvrSystem : GameSystem
    {
        private Logger logger = GlobalLogger.GetLogger("OculusOvr");

        public OculusOvrSystem(IServiceRegistry registry) : base(registry)
        {
            registry.AddService(typeof(OculusOvrSystem), this);
        }

        public override void Initialize()
        {
#if DEBUG
            Game.ConsoleLogMode = ConsoleLogMode.Always;
            logger.Info("Initialize");
#endif

            if (!NativeInvoke.OculusOvr.Startup())
            {
                throw new Exception("Failed to initialize Oculus VR");
            }
        }

        protected override void Destroy()
        {
            NativeInvoke.OculusOvr.Shutdown();
        }
    }
}