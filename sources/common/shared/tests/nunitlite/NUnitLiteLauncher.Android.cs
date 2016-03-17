// ***********************************************************************
// Copyright (c) 2009 Charlie Poole
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// ***********************************************************************

using System;
using System.IO;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Android.App;
using Android.NUnitLite.UI;
using Android.OS;
using Java.IO;

using SiliconStudio.Core;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Xenko.Engine.Network;
using SiliconStudio.Xenko.Graphics.Regression;

using Console = System.Console;
using File = System.IO.File;
using StringWriter = System.IO.StringWriter;
using TextUI = SiliconStudio.Xenko.Graphics.Regression.TextUI;
using SiliconStudio;
using static System.Int32;

namespace NUnitLite.Tests
{
    [Activity(MainLauncher = true, Name = "nunitlite.tests.MainActivity")]
    public class MainActivity : Activity
    {
        private const char IpAddressesSplitCharacter = '%';

        public static Logger Logger = GlobalLogger.GetLogger("NUnitLiteLauncher");
        private ConsoleLogListener logAction = new ConsoleLogListener();

        protected TcpClient Connect(string serverAddresses, int serverPort)
        {
            // Connect back to server
            var client = new TcpClient();
            var possibleIpAddresses = serverAddresses.Split(IpAddressesSplitCharacter);
            foreach (var possibleIpAddress in possibleIpAddresses)
            {
                if (String.IsNullOrEmpty(possibleIpAddress))
                    continue;
                try
                {
                    Logger.Debug(@"Trying to connect to the server " + possibleIpAddress + @":" + serverPort + @".");
                    client.Connect(possibleIpAddress, serverPort);

                    Logger.Debug(@"Client connected with ip " + possibleIpAddress + @"... sending data");
                    return client;
                }
                catch (Exception ex)
                {
                    Logger.Error("Error when trying to connect to the server IP {0}.\n{1}", possibleIpAddress, ex);
                }

                Logger.Debug(@"Client connected with ip " + possibleIpAddress + @"... sending data");

                return client;
            }

            Logger.Fatal(@"Could not connect to server. Quitting the application.");
            OnDestroy();
            Finish();

            throw new InvalidObjectException("Could not connect to server.");
        }

        protected override void OnCreate(Bundle bundle)
        {
            GlobalLogger.GlobalMessageLogged += logAction;
            Logger.ActivateLog(LogMessageType.Debug);

            base.OnCreate(bundle);

            // Set the android global context
            if (PlatformAndroid.Context == null)
                PlatformAndroid.Context = this;

            var xenkoVersion = Intent.GetStringExtra(TestRunner.XenkoVersion);
            if (xenkoVersion == null)
            {
                // No explicit intent, switch to UI activity
                StartActivity(typeof(XenkoTestSuiteActivity));
                return;
            }

            Task.Run(() => RunTests());
        }

        private void RunTests()
        {
            var xenkoVersion = Intent.GetStringExtra(TestRunner.XenkoVersion);
            var buildNumber = Parse(Intent.GetStringExtra(TestRunner.XenkoBuildNumber) ?? "-1");
            var branchName = Intent.GetStringExtra(TestRunner.XenkoBranchName) ?? "";

            // Remove extra (if activity is recreated)
            Intent.RemoveExtra(TestRunner.XenkoVersion);
            Intent.RemoveExtra(TestRunner.XenkoBuildNumber);
            Intent.RemoveExtra(TestRunner.XenkoBranchName);


            Logger.Info(@"*******************************************************************************************************************************");
            Logger.Info(@"date: " + DateTime.Now);
            Logger.Info(@"*******************************************************************************************************************************");

            // Connect to server right away to let it know we're alive
            //var client = Connect(serverAddresses, serverPort);

            var url = $"/service/{xenkoVersion}/SiliconStudio.Xenko.SamplesTestServer.exe";

            var socketContext = RouterClient.RequestServer(url).Result;

            // Update build number (if available)
            ImageTester.ImageTestResultConnection.BuildNumber = buildNumber;
            ImageTester.ImageTestResultConnection.BranchName = branchName ?? "";
            
            // Connect beforehand to image tester, so that first test timing is not affected by initial connection
            try
            {
                ImageTester.Connect();
            }
            catch (Exception e)
            {
                Logger.Error("Error connecting to image tester server: {0}", e);
            }

            // Start unit test
            var cachePath = CacheDir.AbsolutePath;
            var timeNow = DateTime.Now;

            // Generate result file name
            var resultFile = Path.Combine(cachePath, string.Format("TestResult-{0:yyyy-MM-dd_hh-mm-ss-tt}.xml", timeNow));

            Logger.Debug(@"Execute tests");

            var stringBuilder = new StringBuilder();
            var stringWriter = new StringWriter(stringBuilder);
            new TextUI(stringWriter).Execute(new [] { "-format:nunit2", string.Format("-result:{0}", resultFile) });

            Logger.Debug(@"Execute tests done");

            // Read result file
            var result = File.ReadAllText(resultFile);

            // Delete result file
            File.Delete(resultFile);

            // Display some useful info
            var output = stringBuilder.ToString();
            Console.WriteLine(output);

            Logger.Debug(@"Sending results to server");

            // Send back result
            var binaryWriter = new BinaryWriter(socketContext.WriteStream);
            binaryWriter.Write(output);
            binaryWriter.Write(result);

            Logger.Debug(@"Close connection");

            ImageTester.Disconnect();

            socketContext.Dispose();

            Finish();
        }

        protected override void OnDestroy()
        {
            GlobalLogger.GlobalMessageLogged -= logAction;

            base.OnDestroy();
        }
    }

    [Activity]
    public class XenkoTestSuiteActivity : RunnerActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            // Set the android global context
            if (PlatformAndroid.Context == null)
                PlatformAndroid.Context = this;

            ImageTester.ImageTestResultConnection.BuildNumber = -1;

            // Test current assembly
            Add(Assembly.GetExecutingAssembly());

            base.OnCreate(bundle);
        }

        protected override void OnDestroy()
        {
            ImageTester.Disconnect();

            base.OnDestroy();
        }
    }
}
