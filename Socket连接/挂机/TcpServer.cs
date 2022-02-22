using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using GameDefine;

namespace ServerShared
{
    public class TcpServer
    {
        public event Action<System.Net.Sockets.TcpClient> AcceptEv;
        public event Action<System.Net.Sockets.TcpClient> RemoteSocketCloseEv;
        public event Action<System.Net.Sockets.TcpClient, Exception> ExceptionEv;
        public event Action<System.Net.Sockets.TcpClient, PacketHeader> RecvMsgEv;

        public delegate Task RecvMsgFunc(TcpServer server, System.Net.Sockets.TcpClient client,PacketHeader packet);

        TcpListener _tcpListener;

        Dictionary<int, RecvMsgFunc> _route = new Dictionary<int, RecvMsgFunc>();

        public async void StartAsync(string ip, int port)
        {
            _tcpListener = new TcpListener(IPAddress.Parse(ip), port);
            _tcpListener.Start();

            while (true)
            {
                var newClient = await _tcpListener.AcceptTcpClientAsync();
                AcceptEv?.Invoke(newClient);
                RecvMsgAsync(newClient);
            }
        }

        public void Start(string ip, int port)
        {
            _tcpListener = new TcpListener(IPAddress.Parse(ip), port);
            _tcpListener.Start();

            while (true)
            {
                var newClient = _tcpListener.AcceptTcpClient();
                AcceptEv?.Invoke(newClient);
                RecvMsgAsync(newClient);
            }
        }

        async void RecvMsgAsync(System.Net.Sockets.TcpClient client)
        {
            int readCnt;
            var buffer = new byte[1024];
            byte[] recvBuffer;

            NetworkStream stream = client.GetStream();
            while (true)
            {
                try
                {
                    readCnt = await stream.ReadAsync(buffer, 0, buffer.Length);
                }
                catch (ObjectDisposedException)
                {
                    return;
                }
                catch (IOException)
                {
                    RemoteSocketCloseEv?.Invoke(client);
                    client.Close();
                    return;
                }

                // 远端关闭链接
                if (readCnt == 0)
                {
                    RemoteSocketCloseEv?.Invoke(client);
                    client.Close();
                    return;
                }

                if (readCnt >= buffer.Length)
                {
                    Array.Resize(ref buffer, client.Available);
                    readCnt = await stream.ReadAsync(buffer, readCnt, buffer.Length - readCnt);
                    recvBuffer = buffer;
                }
                else
                {
                    recvBuffer = buffer.Take(readCnt).ToArray();
                }
                await ConsumeMsgAsync(client, recvBuffer);
            }
        }

        async Task ConsumeMsgAsync(System.Net.Sockets.TcpClient client, byte[] buffer)
        {
            // 报文格式不正确直接关闭连接
            PacketHeader packet = await Util.ProtoBufDeserializeAsync<PacketHeader>(buffer);
            if (packet == null)
            {
                return;
            }
            // 路由上找不到消息关闭连接
            RecvMsgFunc func;
            RecvMsgEv?.Invoke(client, packet);            
            if (!_route.TryGetValue(packet.Msg, out func))
            {
                ExceptionEv?.Invoke(client, new Exception("Can't find route!"));
                return;
            }
            await func(this, client, packet);
        }

        public void Stop()
        {
            _tcpListener.Stop();
        }

        public void On(int method, RecvMsgFunc func)
        {
            Debug.Assert(func != null);
            _route[method] = func;
        }

        public async Task SendAsync(System.Net.Sockets.TcpClient client, PacketHeader packet, int inputPacketId = 0)
        {
            if (inputPacketId != 0)
            {
                packet.Id = inputPacketId;
            }

            try
            {
                var buffer = await Util.ProtoBufSerializeAsync(packet);

                NetworkStream stream = client.GetStream();
                Monitor.Enter(stream);
                stream.Write(buffer, 0, buffer.Length);
                Monitor.Exit(stream);
            }
            catch (Exception ex)
            {
                ExceptionEv?.Invoke(client, ex);
            }
        }

        public void Send(System.Net.Sockets.TcpClient client, PacketHeader packet, int inputPacketId = 0)
        {
            if (inputPacketId != 0)
            {
                packet.Id = inputPacketId;
            }

            try
            {
                var buffer = Util.ProtoBufSerialize(packet);
                NetworkStream stream = client.GetStream();
                Monitor.Enter(stream);
                stream.Write(buffer, 0, buffer.Length);
                Monitor.Exit(stream);
            }
            catch (Exception ex)
            {
                ExceptionEv?.Invoke(client, ex);
            }
        }
    }
}
