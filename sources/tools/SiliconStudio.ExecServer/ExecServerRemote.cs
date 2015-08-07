using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.Policy;
using System.ServiceModel;
using System.Threading;
using System.Xml;

using SiliconStudio.Core;

namespace SiliconStudio.ExecServer
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, UseSynchronizationContext = false)]
    public class ExecServerRemote : IExecServerRemote
    {
        private readonly AppDomainShadowManager shadowManager;

        private readonly Thread trackingThread;

        private readonly Stopwatch upTime;

        private readonly object singleton;

        public ExecServerRemote(string executablePath, bool trackExecPathLastTime)
        {
            // TODO: List of native dll directory is hardcoded here. Instead, it should be extracted from .exe.config file for example
            shadowManager = new AppDomainShadowManager(executablePath, IntPtr.Size == 8 ? "x64" : "x86");

            upTime = Stopwatch.StartNew();
            singleton = new object();

            if (trackExecPathLastTime)
            {
                trackingThread = new Thread(TrackExecutablePath)
                {
                    IsBackground = true
                };

                trackingThread.Start();
            }
        }

        public void Check()
        {
        }

        public int Run(string[] args)
        {
            Console.WriteLine("Run Received {0}", string.Join(" ", args));

            lock (singleton)
            {
                upTime.Restart();

                var result = shadowManager.Run(args);
                return result;
            }
        }

        public void Wait(ServiceHost serviceHost)
        {
            if (trackingThread != null)
            {
                trackingThread.Join();

                shadowManager.Dispose();

                // Make sure nothing is running and close the service host
                lock (singleton)
                {
                    if (serviceHost != null)
                    {
                        serviceHost.Close();
                    }
                }
            }
        }

        private void TrackExecutablePath()
        {
            while (true)
            {
                Thread.Sleep(200);

                var localUpTime = GetUpTime();
                if (localUpTime > TimeSpan.FromMinutes(10))
                {
                    break;
                }

                shadowManager.Recycle();
            }
        }

        private TimeSpan GetUpTime()
        {
            lock (upTime)
            {
                return upTime.Elapsed;
            }
        }
    }
}