using System;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;
using Twino.Core;
using Twino.Core.Protocols;

namespace Twino.Server
{
    /// <summary>
    /// Accept TCP connections
    /// </summary>
    public class ConnectionHandler
    {
        /// <summary>
        /// twino server of connection handler
        /// </summary>
        private readonly TwinoServer _server;

        /// <summary>
        /// Host listener object of connection handler
        /// </summary>
        private readonly HostListener _listener;

        public ConnectionHandler(TwinoServer server, HostListener listener)
        {
            _server = server;
            _listener = listener;
        }

        /// <summary>
        /// Accepts new connection requests until stopped
        /// </summary>
        public async Task Handle()
        {
            _listener.KeepAliveManager = new KeepAliveManager();
            _listener.KeepAliveManager.Start(_server.Options.RequestTimeout * 1000);

            while (_server.IsRunning)
            {
                if (_listener.Listener == null)
                    break;

                try
                {
                    TcpClient tcp = await _listener.Listener.AcceptTcpClientAsync();
                    ThreadPool.UnsafeQueueUserWorkItem(async t =>
                    {
                        try
                        {
                            await AcceptClient(tcp);
                        }
                        catch (Exception ex)
                        {
                            if (_server.Logger != null)
                                _server.Logger.LogException("Unhandled Exception", ex);
                        }
                    }, tcp, false);
                }
                catch
                {
                }
            }
        }

        /// <summary>
        /// After the client connection request is accepted.
        /// Completes first operations for the client
        /// such as firewall authority, SSL authentication, WebSocket handshaking
        /// </summary>
        private async Task AcceptClient(TcpClient tcp)
        {
            if (_listener == null)
                return;

            ConnectionInfo info = new ConnectionInfo(tcp, _listener)
                                  {
                                      State = ConnectionStates.Pending,
                                      MaxAlive = DateTime.UtcNow + TimeSpan.FromSeconds(_server.Options.RequestTimeout)
                                  };

            _listener.KeepAliveManager.Add(info);

            try
            {
                //ssl handshaking
                if (_listener.Options.SslEnabled)
                {
                    SslStream sslStream = _listener.Options.BypassSslValidation
                                              ? new SslStream(tcp.GetStream(), true, (a, b, c, d) => true)
                                              : new SslStream(tcp.GetStream(), true);

                    info.SslStream = sslStream;
                    SslProtocols protocol = GetProtocol(_listener);
                    await sslStream.AuthenticateAsServerAsync(_listener.Certificate, false, protocol, false);
                }

                //read one byte and recognize the protocol
                byte[] pbytes = new byte[8];
                int rc = await info.GetStream().ReadAsync(pbytes, 0, pbytes.Length);
                if (rc == 0)
                {
                    info.Close();
                    return;
                }

                //find matched protocol with client protocol
                foreach (ITwinoProtocol protocol in _server.Protocols)
                {
                    ProtocolHandshakeResult hsresult = await protocol.Handshake(info, pbytes);

                    //matched
                    if (hsresult.Accepted)
                    {
                        info.Protocol = protocol;
                        hsresult.PreviouslyRead = pbytes;
                        info.Socket = hsresult.Socket;

                        //if protocol required to send protocol message from server to client, send it
                        if (hsresult.Response != null)
                            await info.GetStream().WriteAsync(hsresult.Response);

                        //handle connection events for the connection
                        await protocol.HandleConnection(info, hsresult);
                        return;
                    }
                }

                info.Close();
            }
            catch
            {
                info.Close();
            }
        }

        /// <summary>
        /// Disposes connection handler and releases all resources
        /// </summary>
        public void Dispose()
        {
            if (_listener.Listener == null)
                return;

            _listener.Listener.Start();
            try
            {
                _listener.Handle.Interrupt();
            }
            catch
            {
            }

            if (_listener.KeepAliveManager != null)
                _listener.KeepAliveManager.Stop();

            _listener.KeepAliveManager = null;
            _listener.Listener = null;
            _listener.Handle = null;
        }

        /// <summary>
        /// Finds supported SSL protocol from server options
        /// </summary>
        private static SslProtocols GetProtocol(HostListener server)
        {
            return server.Options.SslProtocol switch
            {
                "tls" => SslProtocols.Tls,
                "tls11" => SslProtocols.Tls11,
                "tls12" => SslProtocols.Tls12,
                _ => SslProtocols.None
            };
        }
    }
}