using System;
using System.Runtime.InteropServices;
using SiliconStudio.Core;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Xenko.Native;

namespace SiliconStudio.Xenko.Engine
{
    public class OculusOvrSystem : GameSystem
    {
        private readonly Logger logger = GlobalLogger.GetLogger("OculusOvr");

        private IntPtr sessionPtr;

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
                throw new Exception(NativeInvoke.OculusOvr.GetError());
            }

            var luidString = Marshal.AllocHGlobal(64);
            if (!NativeInvoke.OculusOvr.Create(sessionPtr, luidString))
            {
                Marshal.FreeHGlobal(luidString);
                throw new Exception(NativeInvoke.OculusOvr.GetError());
            }

            Game.GraphicsDeviceManager.RequiredAdapterUid = Marshal.PtrToStringAnsi(luidString);
            Marshal.FreeHGlobal(luidString);
        }

        protected override void Destroy()
        {
            if(sessionPtr != IntPtr.Zero) NativeInvoke.OculusOvr.Destroy(sessionPtr);
            NativeInvoke.OculusOvr.Shutdown();
        }
    }
}