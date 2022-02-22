using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Timers;
using GameDefine;

namespace GameShared
{
    public class TcpClient : IDisposable
    {
        public event Action ConnectEv;
        public event Action ConnectTimeoutEv;
        public event Action DisconnectEv;
        public event Action<Exception> ExceptionEv;

        private Dictionary<int, Action<TcpClient, PacketHeader>> _route = new Dictionary<int, Action<TcpClient, PacketHeader>>();
        private Dictionary<int, Action<TcpClient, PacketHeader>> _onceCbRoute = new Dictionary<int, Action<TcpClient, PacketHeader>>();
        private Socket _s;

        private Socket _socket
        {
            get
            {
                if (_s == null)
                {
                    _s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                    {
                        SendTimeout = 1000,
                        ReceiveTimeout = 1000
                    };
                }
                return _s;
            }
    
            set { _s = value; }
        }

        readonly private IPEndPoint _address;

        public double ConnectTimeOut
        {
            set
            {
                _timer.Interval = value;
            }
            get
            {
                return _timer.Interval;
            }
        }

        private System.Timers.Timer _timer = new System.Timers.Timer();
        private bool _connected;
        private int _packetSerialNum;
        private List<byte[]> _waitForSendList = new List<byte[]>();

        public TcpClient(string ip, int port)
        {
            _timer.AutoReset = false;
            _timer.Elapsed += OnSocketConnectTimeout;
            _address = new IPEndPoint(IPAddress.Parse(ip), port);
        }

        public void Start()
        {
            try
            {
                _socket.BeginConnect(_address, OnSocketConnect, null);
                _timer.Start();
            }
            catch (System.Exception ex)
            {
                TriggerExceptionEvOrReThrow(ex);
            }
        }

        #region IDisposable implementation

        public void Dispose()
        {
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
    
            _timer.Stop();
            _timer.Dispose();
        }

        #endregion

        private void OnSocketConnect(IAsyncResult iar)
        {
            try
            {
                _socket.EndConnect(iar);
            }
            catch (System.Exception ex)
            {
                TriggerExceptionEvOrReThrow(ex);
            }
            finally
            {
                _timer.Stop();
            }

            // 发送connect成功前send的buffer
            if (_waitForSendList.Count != 0)
            {
                for (int i = 0; i < _waitForSendList.Count; i++)
                {
                    var buffer = _waitForSendList[i];
                    Send(buffer);
                }
                _waitForSendList.Clear();
            }
        }

        private void OnSocketConnectTimeout(object sender, ElapsedEventArgs e)
        {
            if (ConnectTimeoutEv != null)
            {
                ConnectTimeoutEv();

            }
            else
            {
                throw new Exception("Socket connect timeout!");
            }
            _socket.Close();
            _socket = null;
        }

        public void Stop()
        {
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
            _socket = null;
        }

        private void ConsumeMsg()
        {
            var buffer = new byte[_socket.Available];
            PacketHeader packet = null;
            try
            {
                _socket.Receive(buffer);        
                packet = Util.ProtoBufDeserialize<PacketHeader>(buffer);
            }
            catch (Exception ex)
            {
                TriggerExceptionEvOrReThrow(ex);
                return;
            }

            Action<TcpClient, PacketHeader> func = null;
            if (_onceCbRoute.TryGetValue(packet.Id, out func))
            {
                func(this, packet);
                _onceCbRoute.Remove(packet.Id);
                return;
            }

            if (!_route.TryGetValue(packet.Msg, out func))
            {
                return;
            }
            func(this, packet);         
        }

        public void Update()
        {
            // 检查从未连接到连接
            if (_socket.Connected && !_connected)
            {
                _connected = true;
                if (ConnectEv != null)
                {
                    ConnectEv();
                }
            }
            
            if (!_socket.Poll(1, SelectMode.SelectRead))
            {
                return;
            }
    
            // 链接断开
            if (_socket.Available == 0)
            {
                if (DisconnectEv != null)
                {
                    DisconnectEv();
                }
                else
                {
                    throw new Exception("Remote close connect!");
                }

                _connected = false;
                _socket.Close();
                _socket = null;
                // 断线重连
                Start();
                return;
            }
            ConsumeMsg();
        }

        public void Send(PacketHeader packet, Action<TcpClient, PacketHeader> serverResponseCb = null)
        {
            if (serverResponseCb != null)
            {
                packet.Id = ++_packetSerialNum;
                _onceCbRoute.Add(packet.Id, serverResponseCb);
            }

            byte[] buffer = null;
            try
            {
                buffer = Util.ProtoBufSerialize(packet);
            }
            catch (Exception ex)
            {
                TriggerExceptionEvOrReThrow(ex);
            }

            if (!_socket.Connected)
            {
                _waitForSendList.Add(buffer);
                return;
            }

            Send(buffer);   
        }

        private void Send(byte[] buffer)
        {
            try
            {
                _socket.Send(buffer);
            }
            catch (System.Exception ex)
            {
                TriggerExceptionEvOrReThrow(ex);
            }   
        }

        public void On(int msg, Action<TcpClient, PacketHeader> func)
        {
            _route[msg] = func;
        }

        private void TriggerExceptionEvOrReThrow(Exception ex)
        {
            if (ExceptionEv == null)
            {
                throw ex;
            }
            ExceptionEv(ex);
        }
    }
}
