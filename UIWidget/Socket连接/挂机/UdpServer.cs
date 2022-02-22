using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using GameDefine;

namespace ServerShared
{
    public struct Message
    {
        readonly public UdpServer Server;
        readonly public IPEndPoint RemoteIep;
        readonly public PacketHeader Packet;

        public Message(UdpServer server, IPEndPoint remoteIep, PacketHeader packet)
        {
            Server = server;
            RemoteIep = remoteIep;
            Packet = packet;
        }

        public async Task ReplyAsync(PacketHeader packet)
        {
            packet.Id = Packet.Id;
            try
            {
                byte[] buffer = await Util.ProtoBufSerializeAsync(packet);
                Monitor.Enter(Server.Uc);
                Server.Uc.Send(buffer, buffer.Length, RemoteIep);
                Monitor.Exit(Server.Uc);
            }
            catch (Exception ex)
            {
                Server.TriggerExceptionEvent(ex);
            }
        }
    }

    public class UdpServer: IDisposable
    {
        public event Action<Exception> ExceptionEv;

        public System.Net.Sockets.UdpClient Uc;
        IPEndPoint _remoteEp;
        bool _start;
        Dictionary<int, Func<Message, Task>> _route = new Dictionary<int, Func<Message, Task>>();

        public UdpServer(int port)
        {
            Uc = new System.Net.Sockets.UdpClient(port);
            _remoteEp = new IPEndPoint(IPAddress.Any, port);
        }

        public void Dispose()
        {
            Stop();
        }

        public void Start()
        {
            _start = true;
            while (_start)
            {
                var buffer = Uc.Receive(ref _remoteEp);
                HandleMsgAsync(_remoteEp, buffer);
            }          
        }

        public async void StartAsync()
        {
            _start = true;
            while (_start)
            {
                UdpReceiveResult ret = await Uc.ReceiveAsync();
                HandleMsgAsync(ret.RemoteEndPoint, ret.Buffer);
            }          
        }

        public void Stop()
        {
            _start = false;
            Uc.Close();
        }

        public void On(int method, Func<Message, Task> func)
        {
            Debug.Assert(func != null);
            _route[method] = func;
        }

        async void HandleMsgAsync(IPEndPoint remote, byte[] buffer)
        {
            PacketHeader packet = await Util.ProtoBufDeserializeAsync<PacketHeader>(buffer);
            if (packet == null)
            {
                return;
            }
            Func<Message, Task> func;
            if (!_route.TryGetValue(packet.Msg, out func))
            {
                return;
            }
            await func(new Message(this, remote, packet));      
        }

        public void TriggerExceptionEvent(Exception ex)
        {
            ExceptionEv?.Invoke(ex);
        }
    }
}

