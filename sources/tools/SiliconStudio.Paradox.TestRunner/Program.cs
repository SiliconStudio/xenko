// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using Mono.Options;
using SiliconStudio.Paradox.Graphics.Regression;

namespace SiliconStudio.Paradox.TestRunner2
{
    class TestServerHost
    {
        private const char IpAddressesSplitCharacter = '%';

        /// <summary>
        /// The name of the branch the test is done on;
        /// </summary>
        private string branchName;

        /// <summary>
        /// The address of the server.
        /// </summary>
        private string serverAddresses;

        /// <summary>
        /// The current buildNumber.
        /// </summary>
        private int buildNumber = -1;

        /// <summary>
        /// The server used to get results from the test.
        /// </summary>
        private TcpListener server;

        public TestServerHost(int bn, string branch)
        {
            buildNumber = bn;
            branchName = branch;
        }

        int FindAvailablePort(int startRange, int endRange)
        {
            // Tries up to 100 times (in case port are used)
            for (int i = 0; i < 100; ++i)
            {
                // Evaluate TCP connections
                var ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
                var tcpConnInfoArray = ipGlobalProperties.GetActiveTcpConnections();

                // Try each port
                for (int port = startRange; port <= endRange; ++port)
                {
                    bool isAvailable = true;

                    foreach (TcpConnectionInformation tcpi in tcpConnInfoArray)
                    {
                        if (tcpi.LocalEndPoint.Port == port)
                        {
                            isAvailable = false;
                            break;
                        }
                    }

                    if (isAvailable)
                        return port;
                }

                // Wait little bit and try again
                Thread.Sleep(1);
            }

            throw new InvalidOperationException(string.Format("Could not find a valid port in the range {0}-{1}", startRange, endRange));
        }

        /// <summary>
        /// Create the server and start it.
        /// </summary>
        TcpListener StartServer()
        {
            //TODO: IPv6 ?
            var nics = NetworkInterface.GetAllNetworkInterfaces();

            serverAddresses = "";

            // List network interfaces, with the ones having a gateway first
            foreach (var ip in nics.Select(x => x.GetIPProperties()).OrderBy(x => x.GatewayAddresses.Count > 0 ? 0 : 1))
            {
                foreach (var addr in ip.UnicastAddresses)
                {
                    if (addr.Address.AddressFamily == AddressFamily.InterNetwork && !String.IsNullOrEmpty(addr.Address.ToString()) && !addr.Address.ToString().Equals(@"127.0.0.1"))
                        serverAddresses = String.Join(IpAddressesSplitCharacter.ToString(), serverAddresses, addr.Address);
                }
            }

            if (serverAddresses.Equals(""))
            {
                Console.WriteLine(@"No IP address found.");
                return null;
            }

            var serverPort = FindAvailablePort(20000, 20100);
            var server = new TcpListener(IPAddress.Any, serverPort);
            Console.WriteLine(@"Server listening to port {0}", serverPort);
            server.Start();

            return server;
        }

        public int RunAndroidTest(ConnectedDevice device, bool reinstall, string packageName, string packageFile, string resultFile)
        {
            try
            {
                server = StartServer();

                if (reinstall)
                {
                    // force stop - only works for Android 3.0 and above.
                    var o0 = ShellHelper.RunProcessAndGetOutput(@"adb", string.Format(@"-s {0} shell am force-stop {1}", device.Serial, packageName));

                    // uninstall
                    ShellHelper.RunProcessAndGetOutput(@"adb", string.Format(@"-s {0} uninstall {1}", device.Serial, packageName));

                    // install
                    var o1 = ShellHelper.RunProcessAndGetOutput(@"adb", string.Format(@"-s {0} install {1}", device.Serial, packageFile));
                    Console.WriteLine("adb install: exitcode {0}\nOutput: {1}\nErrors: {2}", o1.ExitCode, string.Join(Environment.NewLine, o1.OutputLines), string.Join(Environment.NewLine, o1.OutputErrors));
                    if (o1.ExitCode != 0)
                        throw new InvalidOperationException("Invalid error code from adb install");
                }

                // run
                var parameters = new StringBuilder();
                parameters.Append("-s "); parameters.Append(device.Serial);
                parameters.Append(@" shell am start -a android.intent.action.MAIN -n " + packageName + "/nunitlite.tests.MainActivity");
                AddAndroidParameter(parameters, TestRunner.ParadoxServerIp, serverAddresses);
                AddAndroidParameter(parameters, TestRunner.ParadoxServerPort, ((IPEndPoint)server.Server.LocalEndPoint).Port.ToString());
                AddAndroidParameter(parameters, TestRunner.ParadoxBuildNumber, buildNumber.ToString());
                if (!String.IsNullOrEmpty(branchName))
                    AddAndroidParameter(parameters, TestRunner.ParadoxBranchName, branchName);
                Console.WriteLine(parameters.ToString());

                var o2 = ShellHelper.RunProcessAndGetOutput(@"adb", parameters.ToString());
                Console.WriteLine("adb shell am start: exitcode {0}\nOutput: {1}\nErrors: {2}", o2.ExitCode, string.Join(Environment.NewLine, o2.OutputLines), string.Join(Environment.NewLine, o2.OutputErrors));
                if (o2.ExitCode != 0)
                    throw new InvalidOperationException("Invalid error code from adb shell am start");

                // Wait for client to connect
                var client = server.AcceptTcpClient();

                Console.WriteLine("Device connected, wait for results...");

                var clientStream = client.GetStream();
                var binaryReader = new BinaryReader(clientStream);

                // Read output
                var output = binaryReader.ReadString();
                Console.WriteLine(output);

                // Read XML result
                var result = binaryReader.ReadString();
                Console.WriteLine(result);
                
                // Write XML result to disk
                File.WriteAllText(resultFile, result);
                
                return 0;
            }
            catch (Exception e)
            {
                Console.WriteLine(@"An error was thrown when running the test on Android: {0}", e);
                return -1;
            }
        }

        /// <summary>
        /// Add the parameter as an extra in an Android launch command line
        /// </summary>
        /// <param name="builder">The string builder.</param>
        /// <param name="parameterName">The name of the parameter.</param>
        /// <param name="parameterValue">The value of the parameter.</param>
        private static void AddAndroidParameter(StringBuilder builder, string parameterName, string parameterValue)
        {
            builder.Append(@" -e ");
            builder.Append(parameterName);
            builder.Append(@" ");
            builder.Append(parameterValue);
        }

        /// <summary>
        /// A structure to store information about the connected test devices.
        /// </summary>
        public struct ConnectedDevice
        {
            public string Serial;
            public string Name;
            public TestPlatform Platform;

            public override string ToString()
            {
                return Name + " " + Serial + " " + PlatformPermutator.GetPlatformName(Platform);
            }
        }
    }

    class Program
    {
        static int Main(string[] args)
        {
            var exeName = Path.GetFileName(Assembly.GetExecutingAssembly().Location);
            var showHelp = false;
            int exitCode = 0;
            string resultPath = "TestResults";
            bool reinstall = true;

            var p = new OptionSet
                {
                    "Copyright (C) 2011-2013 Silicon Studio Corporation. All Rights Reserved",
                    "Paradox Test Suite Tool - Version: "
                    +
                    String.Format(
                        "{0}.{1}.{2}",
                        typeof(Program).Assembly.GetName().Version.Major,
                        typeof(Program).Assembly.GetName().Version.Minor,
                        typeof(Program).Assembly.GetName().Version.Build) + string.Empty,
                    string.Format("Usage: {0} [assemblies|apk] -option1 -option2:a", exeName),
                    string.Empty,
                    "=== Options ===",
                    string.Empty,
                    { "h|help", "Show this message and exit", v => showHelp = v != null },
                    { "result-path:", "Result .XML output path", v => resultPath = v },
                    { "no-reinstall-apk", "Do not reinstall APK", v => reinstall = false },
                };

            try
            {
                var commandArgs = p.Parse(args);
                if (showHelp)
                {
                    p.WriteOptionDescriptions(Console.Out);
                    return 0;
                }

                // Make sure path exists
                Directory.CreateDirectory(resultPath);
                exitCode = BuildAndRunAndroidTests(commandArgs, reinstall, resultPath);
            }
            catch (Exception e)
            {
                Console.WriteLine("{0}: {1}", exeName, e);
                if (e is OptionException)
                    p.WriteOptionDescriptions(Console.Out);
                exitCode = 1;
            }

            return exitCode;
        }

        private static int BuildAndRunAndroidTests(List<string> commandArgs, bool reinstall, string resultPath)
        {
            if (commandArgs.Count == 0)
                throw new OptionException("One APK should be provided", "apk");

            // get build number
            int buildNumber;
            if (!Int32.TryParse(Environment.GetEnvironmentVariable("PARADOX_BUILD_NUMBER"), out buildNumber))
                buildNumber = -1;

            // get branch name
            var branchName = Environment.GetEnvironmentVariable("PARADOX_BRANCH_NAME");

            var exitCode = 0;

            foreach (var packageFile in commandArgs)
            {
                if (!packageFile.EndsWith("-Signed.apk"))
                    throw new OptionException("APK should end up with \"-Signed.apk\"", "apk");

                // Remove -Signed.apk suffix
                var packageName = Path.GetFileName(packageFile);
                packageName = packageName.Replace("-Signed.apk", string.Empty);

                var androidDevices = AndroidDeviceEnumerator.ListAndroidDevices();
                if (androidDevices.Length == 0)
                    throw new InvalidOperationException("Could not find any Android device connected.");

                foreach (var device in androidDevices)
                {
                    var testServerHost = new TestServerHost(buildNumber, branchName);
                    Directory.CreateDirectory(resultPath);
                    var deviceResultFile = Path.Combine(resultPath, "TestResult_" + packageName + "_Android_" + device.Name + "_" + device.Serial + ".xml");
                    
                    var currentExitCode = testServerHost.RunAndroidTest(
                        new TestServerHost.ConnectedDevice
                        {
                            Name = device.Name,
                            Serial = device.Serial,
                            Platform = TestPlatform.Android,
                        },
                        reinstall, packageName, packageFile, deviceResultFile);
                    if (currentExitCode != 0)
                        exitCode = currentExitCode;
                }
            }

            return exitCode;
        }
    }
}
