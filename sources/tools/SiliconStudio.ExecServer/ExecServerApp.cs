// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace SiliconStudio.ExecServer
{
    /// <summary>
    /// ExecServer allows to keep in memory an exec loaded into an AppDomain with the benefits that JIT
    /// has been ran already once and the code will run quicker on next run. This is more convenient 
    /// alternative than using NGEN, as we have the benefit of a better code gen while still having the 
    /// benefits of fast startup.
    /// Also the server doesn't lock original assemblies but it shadows copy them (including native dlls
    /// from DllImport), and tracks if original assemblies changed (in this case it will automatically
    /// shutdown).
    /// </summary>
    public class ExecServerApp
    {
        private string execServerPath;

        private const string DisableExecServerAppDomainCaching = "DisableExecServerAppDomainCaching";
        private const int MaxRetryProcess = 10;
        private const int RetryWait = 500; // in ms

        /// <summary>
        /// Runs the specified arguments copy.
        /// </summary>
        /// <param name="argsCopy">The arguments copy.</param>
        /// <returns>System.Int32.</returns>
        public int Run(string[] argsCopy)
        {
            if (argsCopy.Length == 0)
            {
                Console.WriteLine("Usage ExecServer.exe [/direct|/server] executablePath [executableArguments]");
                return 0;
            }
            var args = new List<string>(argsCopy);

            if (args[0] == "/direct")
            {
                args.RemoveAt(0);
                var executablePath = ExtractPath(args, "executable");
                var execServerApp = new ExecServerRemote(executablePath, false, false);
                int result = execServerApp.Run(Environment.CurrentDirectory, new Dictionary<string, string>(), args.ToArray());
                return result;
            }

            if (args[0] == "/server")
            {
                args.RemoveAt(0);
                var executablePath = ExtractPath(args, "executable");
                RunServer(executablePath);
                return 0;
            }
            else
            {
                var executablePath = ExtractPath(args, "executable");
                var workingDirectory = ExtractPath(args, "working directory");
                execServerPath = Path.Combine(Path.GetDirectoryName(executablePath), Path.GetFileNameWithoutExtension(executablePath) + "_ExecServer.exe");

                // Collect environment variables
                var environmentVariables = new Dictionary<string, string>();
                foreach (DictionaryEntry environmentVariable in Environment.GetEnvironmentVariables())
                    environmentVariables.Add((string)environmentVariable.Key, (string)environmentVariable.Value);

                var result = RunClient(executablePath, workingDirectory, environmentVariables, args);
                return result;
            }
        }

        /// <summary>
        /// Runs ExecServer in server mode (waiting for connection from ExecServer clients)
        /// </summary>
        /// <param name="executablePath">Path of the executable to run from this ExecServer instance</param>
        private void RunServer(string executablePath)
        {
            var address = GetEndpointAddress(executablePath);

            // TODO: The setting of disabling caching should be done per EXE (via config file) instead of global settings for ExecServer
            var useAppDomainCaching = Environment.GetEnvironmentVariable(DisableExecServerAppDomainCaching) != "true";

            // Start WCF pipe for communication with process
            var execServerApp = new ExecServerRemote(executablePath, true, useAppDomainCaching);
            var host = new ServiceHost(execServerApp);
            host.AddServiceEndpoint(typeof(IExecServerRemote), new NetNamedPipeBinding(NetNamedPipeSecurityMode.None)
            {
                MaxReceivedMessageSize = int.MaxValue,
                // TODO: Check if we need to tweak timeouts
            }, address);

            try
            {
                host.Open();
            }
            catch (AddressAlreadyInUseException)
            {
                // Silently exit if the server is already running
                return;
            }

            Console.WriteLine("Server [{0}] is running", executablePath);

            // Register for shutdown
            execServerApp.ShuttingDown += (sender, args) => host.Close();

            // Wait for the server to shutdown
            execServerApp.Wait();
        }

        /// <summary>
        /// Runs the client side by calling ExecServer remote server and passing arguments. If ExecServer remote is not running,
        /// it will start it automatically.
        /// </summary>
        /// <param name="executablePath">The executable path.</param>
        /// <param name="workingDirectory">The working directory.</param>
        /// <param name="args">The arguments.</param>
        /// <returns>Return status.</returns>
        private int RunClient(string executablePath, string workingDirectory, Dictionary<string, string> environmentVariables, List<string> args)
        {
            var address = GetEndpointAddress(executablePath);

            var binding = new NetNamedPipeBinding(NetNamedPipeSecurityMode.None)
            {
                MaxReceivedMessageSize = int.MaxValue,
                OpenTimeout = TimeSpan.FromMilliseconds(100),
                SendTimeout = TimeSpan.FromHours(1),
                ReceiveTimeout = TimeSpan.FromHours(1),
            };

            bool tryToRunServerProcess = false;
            for (int i = 0; i < MaxRetryProcess; i++)
            {
                var redirectLog = new RedirectLogger();
                var client = new ExecServerRemoteClient(redirectLog, binding, new EndpointAddress(address));
                // Console.WriteLine("{0}: ExecServer Try to connect", DateTime.Now);

                var service = client.ChannelFactory.CreateChannel();
                try
                {
                    service.Check();

                    //Console.WriteLine("{0}: ExecServer - running start", DateTime.Now);
                    try
                    {
                        var result = service.Run(workingDirectory, environmentVariables, args.ToArray());
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
                finally
                {
                    try
                    {
                        //Console.WriteLine("Closing client");
                        client.Close();
                    }
                    catch (Exception ex)
                    {
                        //Console.WriteLine("Exception while closing {0}", client);
                    }
                }

                // Wait for 

                Console.WriteLine("Waiting {0}ms for the proxy server to start and connect to it", RetryWait);
                Thread.Sleep(RetryWait);
            }

            Console.WriteLine("ERROR cannot connect to proxy server: {0} {1}", execServerPath, string.Join(" ", args));
            return 1;
        }

        /// <summary>
        /// Closes a WCF service.
        /// </summary>
        /// <param name="service">The service.</param>
        private static void CloseService(IExecServerRemote service)
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

        /// <summary>
        /// Runs the server process when it does not exist.
        /// </summary>
        /// <param name="executablePath">The executable path.</param>
        private void RunServerProcess(string executablePath)
        {
            var originalExecServerAppPath = typeof(ExecServerApp).Assembly.Location;
            var originalTime = File.GetLastWriteTimeUtc(originalExecServerAppPath);

            // Avoid locking ExecServer.exe original file, so we are using the name of the executable path and append _ExecServer.exe
            var copyExecFile = false;
            if (File.Exists(execServerPath))
            {
                var copyExecServerTime = File.GetLastWriteTimeUtc(execServerPath);
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
                    File.Copy(originalExecServerAppPath, execServerPath, true);

                    // Copy the .config file as well
                    var executableConfigFile = executablePath + ".config";
                    if (File.Exists(executableConfigFile))
                    {
                        File.Copy(executableConfigFile, execServerPath + ".config", true);
                    }
                }
                catch (IOException)
                {
                }
            }

            // NOTE: We are not using Process.Start as it is for some unknown reasons blocking the process calling this process on Process.ExitProcess
            // Handling directly the creation of the process with Win32 function solves this. Not sure why.
            // TODO: We might want the process to not inherit environment
            var arguments = string.Format("/server \"{0}\"", executablePath);
            if (!ProcessHelper.LaunchProcess(execServerPath, arguments))
            {
                Console.WriteLine("Error, unable to launch process [{0}]", execServerPath);
            }
        }

        private static string GetEndpointAddress(string executablePath)
        {
            var executableKey = Regex.Replace(executablePath, "[:\\/#]", "_");
            var address = "net.pipe://localhost/" + executableKey;
            return address;
        }

        private static string ExtractPath(List<string> args, string type)
        {
            if (args.Count == 0)
            {
                throw new InvalidOperationException(string.Format("Expecting path to {0} argument", type));
            }

            var path = args[0];
            args.RemoveAt(0);
            path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
            return path;
        }

        private class ExecServerRemoteClient : DuplexClientBase<IExecServerRemote>
        {
            public ExecServerRemoteClient(IServerLogger logger, Binding binding, EndpointAddress remoteAddress)
                : base(logger, binding, remoteAddress) { }
        }

        /// <summary>
        /// Loggers that receive logs from the exec server for the running app.
        /// </summary>
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