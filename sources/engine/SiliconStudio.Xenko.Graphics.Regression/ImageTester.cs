// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.IO;
using System.Threading.Tasks;
using Sockets.Plugin;

namespace SiliconStudio.Xenko.Graphics.Regression
{
    public static class ImageTester
    {
        public const string XenkoImageServerHost = "XenkoBuild.siliconstudio.co.jp";
        public const int XenkoImageServerPort = 1832;

        public static ImageTestResultConnection ImageTestResultConnection = PlatformPermutator.GetDefaultImageTestResultConnection();

        private static TcpSocketClient ImageComparisonServer;

        public static bool Connect()
        {
            if (ImageComparisonServer != null)
                return true;

            try
            {
                ImageComparisonServer = new TcpSocketClient();
                var t = Task.Run(async () => await ImageComparisonServer.ConnectAsync(XenkoImageServerHost, XenkoImageServerPort));
                t.Wait();

                // Send initial parameters
                var networkStream = ImageComparisonServer.WriteStream;
                var binaryWriter = new BinaryWriter(networkStream);
                ImageTestResultConnection.Write(binaryWriter);

                return true;
            }
            catch (Exception)
            {
                ImageComparisonServer = null;

                return false;
            }
        }

        public static void Disconnect()
        {
            if (ImageComparisonServer != null)
            {
                try
                {
                    // Properly sends a message notifying we want to close the connection
                    var networkStream = ImageComparisonServer.WriteStream;
                    var binaryWriter = new BinaryWriter(networkStream);
                    binaryWriter.Write((int)ImageServerMessageType.ConnectionFinished);

                    ImageComparisonServer.Dispose();
                }
                catch (Exception)
                {
                    // Ignore failures on disconnect
                }
                ImageComparisonServer = null;
            }
        }

        public static bool RequestImageComparisonStatus(string testName)
        {
            if (!Connect())
                throw new InvalidOperationException("Could not connect to image comparer server");

            if (testName == null && NUnit.Framework.TestContext.CurrentContext != null)
            {
                testName = NUnit.Framework.TestContext.CurrentContext.Test.FullName;
            }

            var binaryWriter = new BinaryWriter(ImageComparisonServer.WriteStream);
            var binaryReader = new BinaryReader(ImageComparisonServer.ReadStream);

            // Header
            binaryWriter.Write((int)ImageServerMessageType.RequestImageComparisonStatus);
            binaryWriter.Write(testName ?? "Unable to fetch test name");

            return binaryReader.ReadBoolean();
        }

        /// <summary>
        /// Send the data of the test to the server.
        /// </summary>
        /// <param name="testResultImage">The image to send.</param>
        public static bool SendImage(TestResultImage testResultImage)
        {
            if (!Connect())
                throw new InvalidOperationException("Could not connect to image comparer server");

            if (testResultImage.TestName == null && NUnit.Framework.TestContext.CurrentContext != null)
            {
                testResultImage.TestName = NUnit.Framework.TestContext.CurrentContext.Test.FullName;
            }

            var binaryWriter = new BinaryWriter(ImageComparisonServer.WriteStream);
            var binaryReader = new BinaryReader(ImageComparisonServer.ReadStream);

            // Header
            binaryWriter.Write((int)ImageServerMessageType.SendImage);

            GameTestBase.TestGameLogger.Info(@"Sending image information...");
            testResultImage.Write(binaryWriter);

            return binaryReader.ReadBoolean();
        }
    }
}
