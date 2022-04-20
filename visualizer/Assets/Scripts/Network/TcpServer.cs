using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Network
{
    public class TcpServer
    {
        public event Action<TcpPeer> OnPeerConected;

        private readonly bool _isConnected;
        private readonly TcpListener _listener;
        private readonly ConcurrentBag<TcpPeer> _peers;

        public TcpServer(int port)
        {
            _listener = new TcpListener(IPAddress.Any, port);
            _peers = new ConcurrentBag<TcpPeer>();
            _isConnected = true;
            Task.Run(StartAccept).ConfigureAwait(false);
        }


        private async void StartAccept()
        {
            _listener.Start();
            while (_isConnected)
            {
                TcpClient client = await _listener.AcceptTcpClientAsync();
                Console.WriteLine($"New client accepted: {client.Client.RemoteEndPoint}");
                TcpPeer peer = new TcpPeer(client);
                _peers.Add(peer);
                OnPeerConected?.Invoke(peer);
            }
        }
    }
}