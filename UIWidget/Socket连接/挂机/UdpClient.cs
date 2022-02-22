using System;
using System.Net;
using System.Collections.Generic;
using GameDefine;

namespace GameShare
{
    public class UdpClient: IDisposable
    {
        private class PacketSending
        {
            public DateTime Time = DateTime.Now;
            public int ReSendCnt;
            public PacketHeader Packet;
            public Action<PacketHeader> RecvCb;
        }

        public event Action<PacketHeader> TimeOutEv;
        public event Action<Exception> ExceptionEv;

        public int MaxReSendCnt = 5;
        public int ReSendTime = 3000;
        private System.Net.Sockets.UdpClient _handler = new System.Net.Sockets.UdpClient();
        private int _packetSequenceNum;
        private IPEndPoint _remoteIep;
        private Dictionary<int, PacketSending> _sendingList = new Dictionary<int, PacketSending>();
        private List<int> _removeList = new List<int>();
        private List<PacketSending> _addList = new List<PacketSending>();

        public UdpClient(string ip, int port)
        {
            _remoteIep = new IPEndPoint(IPAddress.Parse(ip), port);
            _handler.Connect(_remoteIep);
        }

        public void Dispose()
        {
            _handler.Close();
        }

        private void SerializeAndSend(PacketHeader packet)
        {
            try
            {
                var buffer = Util.ProtoBufSerialize(packet);
                _handler.Send(buffer, buffer.Length);
            }
            catch (Exception ex)
            {
                if (ExceptionEv != null)
                {
                    ExceptionEv(ex);
                }
            }
        }

        private void Send(PacketSending ps)
        {
            ps.Packet.Id = _packetSequenceNum++;
            ps.Time = DateTime.Now;
            _addList.Add(ps);
            SerializeAndSend(ps.Packet);
        }

        public void Send(PacketHeader packet, Action<PacketHeader> recvCb = null)
        {
            packet.Id = _packetSequenceNum++;
            _addList.Add(new PacketSending
                {
                    Packet = packet,
                    RecvCb = recvCb
                });
            SerializeAndSend(packet);
        }

        private void RecvMsg()
        {
            if (_handler.Available == 0)
            {
                return;
            }
    
            byte[] buffer = null;
            try
            {
                buffer = _handler.Receive(ref _remoteIep);
            }
            catch (Exception ex)
            {
                if (ExceptionEv != null)
                {
                    ExceptionEv(ex);
                }
                return;
            }
    
            PacketHeader packet = null;
            try
            {
                packet = Util.ProtoBufDeserialize<PacketHeader>(buffer);
            }
            catch (Exception ex)
            {
                if (ExceptionEv != null)
                {
                    ExceptionEv(ex);
                }
                return;
            }
    
            PacketSending sendingPacket = null;
            if (_sendingList.TryGetValue(packet.Id, out sendingPacket))
            {
                if (sendingPacket.RecvCb != null)
                {
                    sendingPacket.RecvCb(packet);
                }
                _sendingList.Remove(packet.Id);
            }
        }

        public void Update()
        {
            // 检查是否需要重传或者超时
            foreach (var item in _sendingList.Values)
            {
                int time = (int)(DateTime.Now - item.Time).TotalMilliseconds;
                if (time > ReSendTime)
                {
                    ++item.ReSendCnt;
                    _removeList.Add(item.Packet.Id);
                    if (item.ReSendCnt >= MaxReSendCnt)
                    {                            
                        if (TimeOutEv != null)
                        {
                            TimeOutEv(item.Packet);
                        }
                        continue;
                    }
                    Send(item);
                }
            }
                
            foreach (var packetId in _removeList)
            {
                _sendingList.Remove(packetId);
            }
            _removeList.Clear();
    
            foreach (var ps in _addList)
            {
                _sendingList.Add(ps.Packet.Id, ps);
            }
            _addList.Clear();
    
            // 尝试收包
            RecvMsg();
        }
    }
}
