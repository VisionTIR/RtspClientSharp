using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Threading;
using System.Linq;
using System.Threading.Tasks;
using RtspClientSharp.Utils;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;

namespace RtspClientSharp.Rtsp
{
    internal class RtspTcpTransportClient : RtspTransportClient
    {
        public RtspTcpTransportClient(ConnectionParameters connectionParameters)
            : base(connectionParameters)
        {
        }

        public override EndPoint RemoteEndPoint => _remoteEndPoint;

        public override async Task ConnectAsync(CancellationToken token)
        {
            _tcpClient = NetworkClientFactory.CreateTcpClient();

            Uri connectionUri = ConnectionParameters.ConnectionUri;

            int rtspPort = connectionUri.Port != -1 ? connectionUri.Port : Constants.DefaultRtspPort;

            await _tcpClient.ConnectAsync(connectionUri.Host, rtspPort);

            _remoteEndPoint = _tcpClient.RemoteEndPoint;

            NetworkStream stream = new NetworkStream(_tcpClient, false);

            Regex r = new Regex("^rtsps");
            if (r.IsMatch(connectionUri.AbsoluteUri))
            {
                var sslStream = new SslStream(stream, false, new RemoteCertificateValidationCallback(ValidateServerCertificate), null);
                sslStream.AuthenticateAsClient(connectionUri.AbsolutePath);
                _networkStream = sslStream;
            }
            else
            {
                _networkStream = stream;
            }
        }

        public override void Dispose()
        {
            if (Interlocked.CompareExchange(ref _disposed, 1, 0) != 0)
                return;

            _tcpClient?.Close();
        }

        public override Stream GetStream()
        {
            if (_tcpClient == null || !_tcpClient.Connected)
                throw new InvalidOperationException("Client is not connected");

            return _networkStream;
        }

        protected override Task<int> ReadAsync(byte[] buffer, int offset, int count)
        {
            Debug.Assert(_networkStream != null, "_networkStream != null");
            return _networkStream.ReadAsync(buffer, offset, count);
        }

        protected override Task ReadExactAsync(byte[] buffer, int offset, int count)
        {
            Debug.Assert(_networkStream != null, "_networkStream != null");
            return _networkStream.ReadExactAsync(buffer, offset, count);
        }

        protected override Task WriteAsync(byte[] buffer, int offset, int count)
        {
            Debug.Assert(_networkStream != null, "_networkStream != null");
            return _networkStream.WriteAsync(buffer, offset, count);
        }

        private int _disposed;

        private Stream _networkStream;

        private EndPoint _remoteEndPoint = new IPEndPoint(IPAddress.None, 0);

        private Socket _tcpClient;

        private bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            X509Store store = new X509Store(StoreName.Root, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadOnly);

            bool inStore = store.Certificates.Any(x => x.Thumbprint == certificate.GetCertHashString());
            return inStore;
        }
    }
}