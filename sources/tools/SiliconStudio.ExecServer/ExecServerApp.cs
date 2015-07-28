using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.ServiceModel;
using System.Threading;

namespace SiliconStudio.ExecServer
{
    public class ExecServerApp
    {
        private const int MaxRetryProcess = 10;

        public int Run(string[] argsCopy)
        {
            if (argsCopy.Length == 0)
            {
                return 0;
            }
            var args = new List<string>(argsCopy);

            if (args[0] == "/direct")
            {
                args.RemoveAt(0);
                var executablePath = ExtractExePath(args);
                var execServerApp = new ExecServerRemote(executablePath, false);
                int result = execServerApp.Run(args.ToArray());
                return result;
            }

            if (args[0] == "/server")
            {
                args.RemoveAt(0);
                var executablePath = ExtractExePath(args);
                RunServer(executablePath);
                return 0;
            }
            else
            {
                var executablePath = ExtractExePath(args);
                return RunClient(executablePath, args);
            }
        }

        private void RunServer(string executablePath)
        {
            var address = GetEndpointAddress(executablePath);

            // Start WCF pipe for communication with process
            var execServerApp = new ExecServerRemote(executablePath, true);
            var host = new ServiceHost(execServerApp);
            host.AddServiceEndpoint(typeof(IExecServerRemote), new NetNamedPipeBinding(NetNamedPipeSecurityMode.None) { MaxReceivedMessageSize = int.MaxValue }, address);

            host.Open();

            Console.WriteLine("Server [{0}] is running", executablePath);

            // Wait for the server to finish
            execServerApp.Wait(host);
        }

        private int RunClient(string executablePath, List<string> args)
        {
            var address = GetEndpointAddress(executablePath);

            var binding = new NetNamedPipeBinding(NetNamedPipeSecurityMode.None)
            {
                MaxReceivedMessageSize = int.MaxValue,
                OpenTimeout = TimeSpan.FromMilliseconds(100)
            };

            bool tryToRunServerProcess = false;
            for (int i = 0; i < MaxRetryProcess; i++)
            {
                var service = ChannelFactory<IExecServerRemote>.CreateChannel(binding, new EndpointAddress(address));
                bool hasException = false;

                try
                {
                    service.Check();
                }
                catch (Exception exception)
                {
                    hasException = true;
                    if (!tryToRunServerProcess)
                    {
                        RunServerProcess(executablePath);
                        tryToRunServerProcess = true;
                    }
                    // The server is not running, we need to runit
                }

                if (!hasException)
                {
                    return service.Run(args.ToArray());
                }

                // Wait for 
                Thread.Sleep(50);
            }

            Console.WriteLine("ERROR cannot run command: {0} {1}", Assembly.GetEntryAssembly().Location, string.Join(" ", args));
            return 1;
        }

        private void RunServerProcess(string executablePath)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = Assembly.GetEntryAssembly().Location,
                Arguments = string.Format("/server \"{0}\"",executablePath),
                WorkingDirectory = Environment.CurrentDirectory,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = false,
                RedirectStandardError = false,
            };

            Process.Start(startInfo);
        }

        private static string GetEndpointAddress(string executablePath)
        {
            var executableKey = executablePath.Replace(":", "_");
            executableKey = executableKey.Replace("\\", "_");
            executableKey = executableKey.Replace("/", "_");
            var address = "net.pipe://localhost/" + executableKey;
            return address;
        }

        private static string ExtractExePath(List<string> args)
        {
            if (args.Count == 0)
            {
                throw new InvalidOperationException("Expecting path to executable argument");
            }

            var fullExePath = args[0];
            args.RemoveAt(0);
            return fullExePath;
        }
    }
}