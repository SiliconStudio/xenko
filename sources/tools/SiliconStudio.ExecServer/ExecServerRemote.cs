using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.ServiceModel;
using System.Threading;

namespace SiliconStudio.ExecServer
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, UseSynchronizationContext = false)]
    public class ExecServerRemote : IExecServerRemote
    {
        private readonly string executablePath;

        private readonly Thread trackingThread;

        private readonly Stopwatch upTime;

        private readonly object singleton;

        public ExecServerRemote(string executablePath, bool trackExecPathLastTime)
        {
            this.executablePath = executablePath;

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
            lock (singleton)
            {
                upTime.Restart();
                var assembly = Assembly.LoadFile(executablePath);
                var result = (int)assembly.EntryPoint.Invoke(null, args);
                return result;
            }
        }

        public void Wait(ServiceHost serviceHost)
        {
            if (serviceHost == null) throw new ArgumentNullException("serviceHost");

            if (trackingThread != null)
            {
                trackingThread.Join();

                // Make sure nothing is running and close the service host
                lock (singleton)
                {
                    serviceHost.Close();
                }
            }
        }

        private void TrackExecutablePath()
        {
            var originalExecutableTime = File.GetLastWriteTime(executablePath);
            while (true)
            {
                Thread.Sleep(100);

                var localUpTime = GetUpTime();
                if (localUpTime > TimeSpan.FromMinutes(10))
                {
                    break;
                }

                // Executable has changed, allow to reload it
                var newTime = File.GetLastWriteTime(executablePath);
                if (newTime != originalExecutableTime)
                {
                    break;
                }
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