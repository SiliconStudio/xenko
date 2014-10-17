// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.IO;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace SiliconStudio.Paradox.Graphics.Regression
{
    public static class ImageTester
    {
        public const string ParadoxImageServerHost = "ParadoxBuild.siliconstudio.co.jp";
        public const int ParadoxImageServerPort = 1832;

        private static TcpClient ImageComparisonServer;
        public static ImageTestResultConnection ImageTestResultConnection = PlatformPermutator.GetDefaultImageTestResultConnection();
        
        public static bool Connect()
        {
            if (ImageComparisonServer != null)
                return true;

            try
            {
                ImageComparisonServer = new TcpClient();
                ImageComparisonServer.Connect(ParadoxImageServerHost, ParadoxImageServerPort);

                // Send initial parameters
                var networkStream = ImageComparisonServer.GetStream();
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
                    var networkStream = ImageComparisonServer.GetStream();
                    var binaryWriter = new BinaryWriter(networkStream);
                    binaryWriter.Write((int)ImageServerMessageType.ConnectionFinished);

                    ImageComparisonServer.Close();
                }
                catch (Exception)
                {
                }
                ImageComparisonServer = null;
            }
        }

        public static bool RequestImageComparisonStatus(string testName = null)
        {
            if (!Connect())
                throw new InvalidOperationException("Could not connect to image comparer server");

            try
            {
                if (testName == null && NUnit.Framework.TestContext.CurrentContext == null)
                {
                    testName = NUnit.Framework.TestContext.CurrentContext.Test.FullName;
                }

                var networkStream = ImageComparisonServer.GetStream();
                var binaryWriter = new BinaryWriter(networkStream);
                var binaryReader = new BinaryReader(networkStream);

                // Header
                binaryWriter.Write((int)ImageServerMessageType.RequestImageComparisonStatus);
                binaryWriter.Write(testName);

                return binaryReader.ReadBoolean();
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Send the data of the test to the server.
        /// </summary>
        /// <param name="image">The image to send.</param>
        /// <param name="serial">The serial of the device.</param>
        /// <param name="testName">The name of the test.</param>
        public unsafe static bool SendImage(TestResultImage testResultImage)
        {
            if (!Connect())
                throw new InvalidOperationException("Could not connect to image comparer server");

            try
            {
                if (testResultImage.TestName == null && NUnit.Framework.TestContext.CurrentContext != null)
                {
                    testResultImage.TestName = NUnit.Framework.TestContext.CurrentContext.Test.FullName;
                }


                var networkStream = ImageComparisonServer.GetStream();
                var binaryWriter = new BinaryWriter(networkStream);
                var binaryReader = new BinaryReader(networkStream);

                // Header
                binaryWriter.Write((int)ImageServerMessageType.SendImage);

                Console.WriteLine(@"Sending image information...");
                testResultImage.Write(binaryWriter);

                return binaryReader.ReadBoolean();
            }
            catch (Exception)
            {
                throw;
            }
        } 
    }
}