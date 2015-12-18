#if !SILICONSTUDIO_PLATFORM_WINDOWS_RUNTIME
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Sockets.Plugin.Abstractions;

// ReSharper disable once CheckNamespace

namespace Sockets.Plugin
{
    /// <summary>
    ///     Binds to a port and listens for TCP connections.
    ///     Use <code>StartListeningAsync</code> to bind to a local port, then handle <code>ConnectionReceived</code> events as
    ///     clients connect.
    /// </summary>
    class TcpSocketListener : ITcpSocketListener
    {
        private TcpListener _backingTcpListener;
        private CancellationTokenSource _listenCanceller;
        private readonly int _bufferSize;

        public TcpSocketListener()
        {
        }

        public TcpSocketListener(int bufferSize)
        {
            _bufferSize = bufferSize;
        }

        /// <summary>
        ///     Fired when a new TCP connection has been received.
        ///     Use the <code>SocketClient</code> property of the <code>TcpSocketListenerConnectEventArgs</code>
        ///     to get a <code>TcpSocketClient</code> representing the connection for sending and receiving data.
        /// </summary>
        public EventHandler<TcpSocketListenerConnectEventArgs> ConnectionReceived { get; set; }

        /// <summary>
        ///     Binds the <code>TcpSocketListener</code> to the specified port on all endpoints and listens for TCP connections.
        /// </summary>
        /// <param name="port">The port to listen on.</param>
        /// <param name="listenOn">The <code>CommsInterface</code> to listen on. If unspecified, all interfaces will be bound.</param>
        /// <returns></returns>
        public Task StartListeningAsync(int port, ICommsInterface listenOn = null)
        {
            return Task.Run(() =>
            {
                if (listenOn != null && !listenOn.IsUsable)
                    throw new InvalidOperationException("Cannot listen on an unusable interface. Check the IsUsable property before attemping to bind.");

                var ipAddress = listenOn != null ? ((CommsInterface)listenOn).NativeIpAddress : IPAddress.Any;

                _listenCanceller = new CancellationTokenSource();

                _backingTcpListener = new TcpListener(ipAddress, port);
                _backingTcpListener.Start();

                WaitForConnections(_listenCanceller.Token);
            });
        }

        /// <summary>
        ///     Stops the <code>TcpSocketListener</code> from listening for new TCP connections.
        ///     This does not disconnect existing connections.
        /// </summary>
        public Task StopListeningAsync()
        {
            return Task.Run(
                () =>
                {
                    _listenCanceller.Cancel();
                    _backingTcpListener.Stop();
                    _backingTcpListener = null;
                });
        }

        /// <summary>
        ///     The port to which the TcpSocketListener is currently bound
        /// </summary>
        public int LocalPort
        {
            get { return ((IPEndPoint) (_backingTcpListener.LocalEndpoint)).Port; }
        }

#pragma warning disable 4014
        private void WaitForConnections(CancellationToken cancelToken)
        {
            Task.Factory.StartNew(async () =>
            {
                while (!cancelToken.IsCancellationRequested)
                {
                    var nativeClient = await _backingTcpListener.AcceptTcpClientAsync();
                    var wrappedClient = new TcpSocketClient(nativeClient, _bufferSize);

                    var eventArgs = new TcpSocketListenerConnectEventArgs(wrappedClient);
                    if (ConnectionReceived != null)
                        ConnectionReceived(this, eventArgs);
                }
            },
                cancelToken,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);
        }
#pragma warning restore 4014

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Allows an object to try to free resources and perform other cleanup operations before it is reclaimed by garbage collection.
        /// </summary>
        ~TcpSocketListener()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_backingTcpListener != null)
                    ((IDisposable)_backingTcpListener).Dispose();
            }
        }
        

    }


}
#endif