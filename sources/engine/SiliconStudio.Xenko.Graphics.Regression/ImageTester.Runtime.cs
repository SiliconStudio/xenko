// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#if SILICONSTUDIO_PLATFORM_WINDOWS_STORE || SILICONSTUDIO_PLATFORM_WINDOWS_PHONE

using System;
using System.IO;
using System.Threading.Tasks;

using Windows.Networking.Sockets;
using Windows.Networking;
using Windows.Storage.Streams;

namespace SiliconStudio.Xenko.Graphics.Regression
{
    public static partial class ImageTester
    {
        private static StreamSocket imageComparisonServer;

        public static bool Connect()
        {
            var task = ConnectAsync();
            task.Wait();
            return task.Result;
        }

        public static void Disconnect()
        {
            DisconnectAsync().Wait();
        }
        
        public static bool RequestImageComparisonStatus(string testName = null)
        {
            var task = RequestImageComparisonStatusAsync(testName);
            task.Wait();
            return task.Result;
        }

        /// <summary>
        /// Send the data of the test to the server.
        /// </summary>
        /// <param name="testResultImage">The image to send.</param>
        public static bool SendImage(TestResultImage testResultImage)
        {
            var task = SendImageAsync(testResultImage);
            task.Wait();
            return task.Result;
        }

        public static async Task<bool> ConnectAsync()
        {
            if (imageComparisonServer != null)
                return true;
			
            try
            {
                imageComparisonServer = new StreamSocket();
                await imageComparisonServer.ConnectAsync(new HostName(XenkoImageServerHost), XenkoImageServerPort.ToString());
			
                // Send initial parameters
                using (var memoryStream = new MemoryStream())
                {
                    var binaryWriter = new BinaryWriter(memoryStream);
                    ImageTestResultConnection.Write(binaryWriter);

                    var dataWriter = new DataWriter(imageComparisonServer.OutputStream);
                    dataWriter.WriteBytes(memoryStream.ToArray());
                    await dataWriter.StoreAsync();
                    await dataWriter.FlushAsync();
                    dataWriter.DetachStream();
                }

                return true;
            }
            catch (Exception)
            {
                imageComparisonServer = null;
			
                return false;
            }
        }

        public static async Task DisconnectAsync()
        {
            if (imageComparisonServer != null)
            {
                try
                {
                    // Properly sends a message notifying we want to close the connection
                    using (var dataWriter = new DataWriter(imageComparisonServer.OutputStream))
                    {
                        dataWriter.WriteInt32((int)ImageServerMessageType.ConnectionFinished);
                        await dataWriter.StoreAsync();
                        await dataWriter.FlushAsync();
                        dataWriter.DetachStream();
                    }
			
                    imageComparisonServer.Dispose();
                }
                catch (Exception)
                {
                }
                imageComparisonServer = null;
            }
        }

        public static async Task<bool> RequestImageComparisonStatusAsync(string testName = null)
        {
            if (!Connect())
                throw new InvalidOperationException("Could not connect to image comparer server");
			
            try
            {
                if (testName == null && NUnit.Framework.TestContext.CurrentContext == null)
                {
                    testName = NUnit.Framework.TestContext.CurrentContext.Test.FullName;
                }

                // Send initial parameters
                using (var memoryStream = new MemoryStream())
                {
                    var binaryWriter = new BinaryWriter(memoryStream);

                    // Header
                    binaryWriter.Write((int)ImageServerMessageType.RequestImageComparisonStatus);
                    binaryWriter.Write(testName);

                    var dataWriter = new DataWriter(imageComparisonServer.OutputStream);
                    dataWriter.WriteBytes(memoryStream.ToArray());
                    await dataWriter.StoreAsync();
                    await dataWriter.FlushAsync();
                    dataWriter.DetachStream();
                }

                var dataReader = new DataReader(imageComparisonServer.InputStream);
                await dataReader.LoadAsync(1);
                var result = dataReader.ReadBoolean();
                dataReader.DetachStream();

                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Send the data of the test to the server.
        /// </summary>
        /// <param name="testResultImage">The image to send.</param>
        public static async Task<bool> SendImageAsync(TestResultImage testResultImage)
        {
            if (!Connect())
                throw new InvalidOperationException("Could not connect to image comparer server");
			
            try
            {
                if (testResultImage.TestName == null && NUnit.Framework.TestContext.CurrentContext != null)
                {
                    testResultImage.TestName = NUnit.Framework.TestContext.CurrentContext.Test.FullName;
                }

                // Send initial parameters
                using (var memoryStream = new MemoryStream())
                {
                    var binaryWriter = new BinaryWriter(memoryStream);

                    // Header
                    binaryWriter.Write((int)ImageServerMessageType.SendImage);
                    GameTestBase.TestGameLogger.Info(@"Sending image information...");
                    testResultImage.Write(binaryWriter);

                    var dataWriter = new DataWriter(imageComparisonServer.OutputStream);
                    dataWriter.WriteBytes(memoryStream.ToArray());
                    await dataWriter.StoreAsync();
                    await dataWriter.FlushAsync();
                    dataWriter.DetachStream();
                }

                var dataReader = new DataReader(imageComparisonServer.InputStream);
                await dataReader.LoadAsync(1);
                var result = dataReader.ReadBoolean();
                dataReader.DetachStream();

                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}

#endif