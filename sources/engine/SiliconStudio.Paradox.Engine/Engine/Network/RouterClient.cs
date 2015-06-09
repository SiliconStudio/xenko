using System;
using System.Threading;
using System.Threading.Tasks;

namespace SiliconStudio.Paradox.Engine.Network
{
    public class RouterClient
    {
        /// <summary>
        /// The default port to connect to router server.
        /// </summary>
        public static readonly int DefaultPort = 31244;

        /// <summary>
        /// The default port to listen for connection from router.
        /// </summary>
        public static readonly int DefaultListenPort = 31245;

        /// <summary>
        /// Starts a service.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public static void RegisterService()
        {
            // It will need the control connection (if not started yet)
            // Control connection will be able to list this service as available, and start an instance of it when requested
            throw new NotImplementedException();
        }

        /// <summary>
        /// Requests a specific server.
        /// </summary>
        /// <returns></returns>
        public static async Task<SimpleSocket> RequestServer(string serverUrl)
        {
            var socketContext = await InitiateConnectionToRouter();

            await socketContext.WriteStream.WriteInt16Async((short)ClientRouterMessage.RequestServer);
            await socketContext.WriteStream.WriteStringAsync(serverUrl);
            await socketContext.WriteStream.FlushAsync();

            var result = (ClientRouterMessage)await socketContext.ReadStream.ReadInt16Async();
            if (result != ClientRouterMessage.ServerStarted)
            {
                throw new InvalidOperationException("Could not connect to server");
            }

            var errorCode = await socketContext.ReadStream.ReadInt32Async();
            if (errorCode != 0)
            {
                var errorMessage = await socketContext.ReadStream.ReadStringAsync();
                throw new InvalidOperationException(errorMessage);
            }

            return socketContext;
        }

        /// <summary>
        /// Initiates a connection to the router.
        /// </summary>
        /// <returns></returns>
        private static Task<SimpleSocket> InitiateConnectionToRouter()
        {
            var socketContextTCS = new TaskCompletionSource<SimpleSocket>();
            var socketContext = new SimpleSocket();
            socketContext.Connected = async context =>
            {
                socketContextTCS.TrySetResult(context);
            };

            Task.Run(async () =>
            {
                // Keep trying to establish connections until no errors
                bool hasErrors;
                do
                {
                    try
                    {
                        hasErrors = false;
                        if (PlatformIsPortForward)
                            await socketContext.StartServer(DefaultListenPort, true);
                        else
                            await socketContext.StartClient("127.0.0.1", DefaultPort);
                    }
                    catch (Exception)
                    {
                        hasErrors = true;
                    }
                    if (hasErrors)
                    {
                        // Wait a little bit before next try
                        await Task.Delay(100);
                    }
                } while (hasErrors);
            });

            // Wait for server to connect to us (as a Task)
            return socketContextTCS.Task;
        }

        private static void StartControlConnection()
        {
            // Start control connection
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets a value indicating whether this platform initiates connections by listening on a port and wait for router (true) or connecting to router (false).
        /// </summary>
        private static bool PlatformIsPortForward
        {
            get
            {
#if SILICONSTUDIO_PLATFORM_ANDROID || SILICONSTUDIO_PLATFORM_WINDOWS_PHONE || SILICONSTUDIO_PLATFORM_IOS
                return true;
#else
                return false;
#endif
            }
        }
    }
}