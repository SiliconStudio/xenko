using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading;

namespace SiliconStudio.ExecServer
{
    public class ExecServerApp
    {
        private const string DisableExecServerAppDomainCaching = "DisableExecServerAppDomainCaching";

        // TODO: This setting must be configured by the executable directly
        private const int MaxConcurrentAppDomainProcess = 1;

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
                var execServerApp = new ExecServerRemote(executablePath, false, false, 1);
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
                var result = RunClient(executablePath, args);
                return result;
            }
        }

        private void RunServer(string executablePath)
        {
            var address = GetEndpointAddress(executablePath);

            // TODO: The setting of disabling caching should be done per EXE (via config file) instead of global settings for ExecServer
            var useAppDomainCaching = Environment.GetEnvironmentVariable(DisableExecServerAppDomainCaching) != "true";

            // Start WCF pipe for communication with process
            var execServerApp = new ExecServerRemote(executablePath, true, useAppDomainCaching, MaxConcurrentAppDomainProcess);
            var host = new ServiceHost(execServerApp);
            host.AddServiceEndpoint(typeof(IExecServerRemote), new NetNamedPipeBinding(NetNamedPipeSecurityMode.None) { MaxReceivedMessageSize = int.MaxValue }, address);

            host.Open();

            Console.WriteLine("Server [{0}] is running", executablePath);

            execServerApp.ShuttingDown += (sender, args) => host.Close();

            // Wait for the server to finish
            execServerApp.Wait();
        }

        private int RunClient(string executablePath, List<string> args)
        {
            var address = GetEndpointAddress(executablePath);

            var binding = new NetNamedPipeBinding(NetNamedPipeSecurityMode.None)
            {
                MaxReceivedMessageSize = int.MaxValue,
                OpenTimeout = TimeSpan.FromMilliseconds(100)
            };

            var redirectLog = new RedirectLogger();
            var client = new ExecServerRemoteClient(redirectLog, binding, new EndpointAddress(address));
            try
            {
                bool tryToRunServerProcess = false;
                for (int i = 0; i < MaxRetryProcess; i++)
                {
                    //Console.WriteLine("{0}: ExecServer Try to connect", DateTime.Now);

                    var service = client.ChannelFactory.CreateChannel();
                    try
                    {
                        service.Check();

                        //Console.WriteLine("{0}: ExecServer - running start", DateTime.Now);
                        try
                        {
                            var result = service.Run(args.ToArray());
                            //Console.WriteLine("{0}: ExecServer - running end", DateTime.Now);
                            return result;
                        }
                        finally
                        {
                            CloseService(service);
                        }
                    }
                    catch (EndpointNotFoundException ex)
                    {
                        CloseService(service);

                        if (!tryToRunServerProcess)
                        {
                            // The server is not running, we need to run it
                            RunServerProcess(executablePath);
                            tryToRunServerProcess = true;
                        }
                    }

                    // Wait for 
                    Thread.Sleep(100);
                }
            }
            finally
            {
                try
                {
                    client.Close();
                }
                catch (Exception ex)
                {
                    //Console.WriteLine("Exception while closing {0}", client);
                }
            }

            Console.WriteLine("ERROR cannot run command: {0} {1}", Assembly.GetEntryAssembly().Location, string.Join(" ", args));
            return 1;
        }

        private void CloseService(IExecServerRemote service)
        {
            try
            {
                var clientChannel = ((IClientChannel)service);
                if (clientChannel.State == CommunicationState.Faulted)
                {
                    clientChannel.Abort();
                }
                else
                {
                    clientChannel.Close();
                }

                clientChannel.Dispose();
                //Console.WriteLine("ExecServer - Close connection - Client channel state: {0}", clientChannel.State);
            }
            catch (Exception ex)
            {
                //Console.WriteLine("Exception while closing connection {0}", ex);
            }
        }

        private void RunServerProcess(string executablePath)
        {
            var originalExecServerAppPath = Assembly.GetEntryAssembly().Location;
            var originalTime = File.GetLastWriteTimeUtc(originalExecServerAppPath);

            var copyExecServer = Path.Combine(Path.GetDirectoryName(executablePath), Path.GetFileNameWithoutExtension(executablePath) + "_ExecServerProxy.exe");
            var copyExecFile = false;
            if (File.Exists(copyExecServer))
            {
                var copyExecServerTime = File.GetLastWriteTimeUtc(copyExecServer);
                // If exec server has changed, we need to copy the new version to it
                copyExecFile = originalTime != copyExecServerTime;
            }
            else
            {
                copyExecFile = true;
            }

            if (copyExecFile)
            {
                try
                {
                    File.Copy(originalExecServerAppPath, copyExecServer, true);
                }
                catch (IOException)
                {
                }
            }

            // NOTE: We are not using Process.Start as it is for some unknown reasons blocking the process calling this process on Process.ExitProcess
            // Handling directly the creation of the process with Win32 function solves this. Not sure why.

            //var startInfo = new ProcessStartInfo
            //{
            //    FileName = copyExecServer,
            //    Arguments = string.Format("/server \"{0}\"",executablePath),
            //    WorkingDirectory = Path.GetDirectoryName(executablePath),
            //    CreateNoWindow = false,
            //    UseShellExecute = false,
            //};

            //var process = new Process { StartInfo = startInfo };
            //process.Start();

            var arguments = string.Format("/server \"{0}\"", executablePath);

            if (!ProcessHelper.LaunchProcess(copyExecServer, arguments))
            {
                Console.WriteLine("Error, unable to launch process [{0}]", copyExecServer);
            }
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

            // Make sure the executable has a directory
            fullExePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fullExePath);

            return fullExePath;
        }

        private class ExecServerRemoteClient : DuplexClientBase<IExecServerRemote>
        {
            public ExecServerRemoteClient(IServerLogger logger, Binding binding, EndpointAddress remoteAddress)
                : base(logger, binding, remoteAddress) { }
        }

        [CallbackBehavior(UseSynchronizationContext = false, AutomaticSessionShutdown = true)]
        private class RedirectLogger : IServerLogger
        {
            public void OnLog(string text, ConsoleColor color)
            {
                var backupColor = Console.ForegroundColor;
                Console.ForegroundColor = color;
                Console.Out.WriteLine(text);
                Console.ForegroundColor = backupColor;
            }
        }
    }
}