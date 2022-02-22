using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;



namespace AFW
{

    public class NetworkManager : MonoBehaviour
    {
        private SocketClient socket;
        static Queue<KeyValuePair<int, ByteBuffer>> sEvents = new Queue<KeyValuePair<int, ByteBuffer>>();

        List<short> listCSHandleMessages = new List<short>();

        public string ServerIP { get; set; }
        public int ServerPort { get; set; }

        public void SetServerIP (string ip)
        {
            ServerIP = ip;
        }
        public void SetServerPort(int port)
        {
            ServerPort = port;
        }

        SocketClient SocketClient {
            get {
                if (socket == null)
                    socket = new SocketClient();
                return socket;                    
            }
        }

        void Awake() {
            Init();
        }

        void Init() {
            SocketClient.OnRegister();
        }

        public void OnInit() {
            CallMethod("Start");
        }

        public void Unload() {
            CallMethod("Unload");
        }

        /// <summary>
        /// 执行Lua方法
        /// </summary>
        public object[] CallMethod(string func, params object[] args) {
            return Util.CallMethod("Network", func, args);
        }

        ///------------------------------------------------------------------------------------
        public static void AddEvent(int _event, ByteBuffer data) {
            sEvents.Enqueue(new KeyValuePair<int, ByteBuffer>(_event, data));
        }

        /// <summary>
        /// 交给Command，这里不想关心发给谁。
        /// </summary>
        void Update() {
            //if (sEvents.Count > 0) {
            //    while (sEvents.Count > 0) {
            //        KeyValuePair<int, ByteBuffer> _event = sEvents.Dequeue();
            //        AppFacade.Instance.SendMessageCommand(NoticeName.DISPATCH_MESSAGE, _event);
            //    }
            //}
        }


        void FixedUpdate()
        {
            if (sEvents.Count > 0)
            {
                while (sEvents.Count > 0)
                {
                    KeyValuePair<int, ByteBuffer> _event = sEvents.Dequeue();
                    AppFacade.Instance.SendMessageCommand(NoticeName.DISPATCH_MESSAGE, _event);
                }
            }
        }

        /// <summary>
        /// 发送链接请求
        /// </summary>
        public void SendConnect() {
            if(!string.IsNullOrEmpty(ServerIP) && ServerPort != 0)
            {
                SocketClient.SendConnect(ServerIP, ServerPort);
                AppFacade.Instance.SendMessageCommand(NoticeName.START_CONNECT);
            }
        }

        /// <summary>
        /// 发送SOCKET消息
        /// </summary>
        public void SendMessage(short msgID,short msgMode, ByteBuffer buffer) {
            SocketClient.SendMessage(msgID,msgMode, buffer);
        }

        public void SendMessage(CSMessage message)
        {
            ByteBuffer buffer = new ByteBuffer();
            message.ToBuffer(buffer);
            SocketClient.SendMessage(message.GetType(), 0, buffer);
        }

        /// <summary>
        /// 析构函数
        /// </summary>
        new void OnDestroy() {
            SocketClient.OnRemove();
            Debug.Log("~NetworkManager was destroy");
        }

        public void AddCSHandleMessage(short messageID)
        {
            listCSHandleMessages.Add(messageID);
        }

        public bool IsCSHandleMessage(short messageID)
        {
            return listCSHandleMessages.Contains(messageID);
        }

        //lua 用
        public ByteBuffer CreateByteBuffer()
        {
            return new ByteBuffer();
        }


    

    }
}
