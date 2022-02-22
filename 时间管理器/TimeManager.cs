using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
namespace AFW
{
    /// <summary>
    /// 回调函数
    /// </summary>
    /// <param name="callback"></param>
    /// <returns></returns>
    public delegate void TimeCallBack();
    public delegate void DayEndDelegate();
    /// <summary>
    /// 时间管理器
    /// 使用方法
    /// 1.SyncServerClientTimeStamp(long server_timestamp) 同步服务器时间
    /// 2.AddCallBack 加入时间回调函数
    /// </summary>
    public class TimeManager : MonoBehaviour
    {
        /// <summary>
        /// 回调集合
        /// </summary>
        private List<CallBackElement> m_CallBacks = new List<CallBackElement>();
        /// <summary>
        /// Lua回调集合
        /// </summary>
        private List<LuaCallBackElement> m_LuaCallBacks = new List<LuaCallBackElement>();
        /// <summary>
        /// 当天结束时回调
        /// </summary>
        private DayEndDelegate m_DayEnd;
        /// <summary>
        /// 当前系统时间
        /// </summary>
        private DateTime m_Currect;
        /// <summary>
        /// 服务器和客户端时间差
        /// </summary>
        private float m_Server_Client;
        /// <summary>
        /// 和系统校对时客户端游戏开始时间
        /// </summary>
        private float m_StartGameTime;
        /// <summary>
        /// 和系统校对时客户端系统时间
        /// </summary>
        private DateTime m_StartDateTime;

        /// <summary>
        /// 更新客户端时间戳
        /// </summary>
        /// <param name="server_timestamp"></param>
        public void SyncServerClientTimeStamp(long server_timestamp)
        {
            //Debug.Log("NowTime: " + DateTime.Now.ToString());
            
            long client_timestamp = DateTimeToUnixTimestamp(DateTime.Now);

            //Debug.Log("ServerTime: " + server_timestamp.ToString() + "Client:" + client_timestamp);

            m_Server_Client = server_timestamp - client_timestamp;

            m_StartGameTime = Time.realtimeSinceStartup;

            m_StartDateTime = DateTime.Now;

            m_Currect = GetServerTime();

            //Debug.Log("m_Currect: " + m_StartDateTime.ToString() + "..." + m_Currect.ToString());
        }
        public void Update()
        {
            int day = m_Currect.Day;
            m_Currect = GetServerTime();

            for (int i = 0; i < m_CallBacks.Count; i++)
            {
                if (m_CallBacks[i].dateTime <= m_Currect)
                {
                    m_CallBacks[i].callback();
                    m_CallBacks.RemoveAt(i);
                }
            }

            for (int i = 0; i < m_LuaCallBacks.Count; i++)
            {
                if (m_LuaCallBacks[i].dateTime <= m_Currect)
                {
                    Util.CallMethod(m_LuaCallBacks[i].LuaModelName, m_LuaCallBacks[i].LuaMethodName, m_LuaCallBacks[i].args);
                    m_LuaCallBacks.RemoveAt(i);
                }
            }

            if (day != m_Currect.Day)
            {
                if (m_DayEnd != null)
                {
                    //清理当前回调
                    m_CallBacks.Clear();
                    //当天结束时，重新加入新一天回调
                    m_DayEnd();
                }

                m_DayEnd = null;
            }
        }
        /// <summary>
        /// 是否到目标时间
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public bool IsTime(DateTime dateTime)
        {
            if (dateTime >= GetServerTime())
            {
                return true;
            }

            return false;
        }
        /// <summary>
        /// 当前时间是否在目标时间区间
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public bool IsBetweenTime(DateTime start, DateTime end)
        {
            if (start >= GetServerTime()
                && end < GetServerTime())
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// 当前处于星期几
        /// </summary>
        /// <param name="week"></param>
        /// <returns></returns>
        public bool IsWeek(DayOfWeek week)
        {
            if (GetServerTime().DayOfWeek == week)
            {
                return true;
            }

            return false;
        }
        /// <summary>
        /// 获得服务器时间
        /// </summary>
        /// <returns></returns>
        public DateTime GetServerTime()
        {
            //TimeSpan toNow = new TimeSpan((long)(Time.realtimeSinceStartup - m_StartGameTime + m_Server_Client) * 10000000);
            //return m_StartDateTime.Add(toNow);
            return m_StartDateTime.AddSeconds(Time.realtimeSinceStartup - m_StartGameTime + m_Server_Client);

            //return DateTime.Now.AddSeconds(m_Server_Client);
        }

        /// <summary>
        /// 获得长整型时间
        /// </summary>
        /// <returns></returns>
        public long GetServerTimeLong()
        {
            return DateTimeToUnixTimestamp(GetServerTime());
        }

        /// <summary>
        /// 创建时间
        /// </summary>
        /// <param name="hour"></param>
        /// <param name="minute"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public DateTime CreateTodayDateTime(int hour, int minute, int second)
        {
            DateTime server = GetServerTime();

            DateTime date_time = new DateTime(server.Year, server.Month, server.Day, hour, minute, second, DateTime.Now.Kind);

            return date_time;
        }
        /// <summary>
        /// 添加回调函数
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="callback"></param>
        /// <param name="callbackParams"></param>
        public void AddCallBack(DateTime dateTime, TimeCallBack callback)
        {
            if (dateTime <= GetServerTime())
            {
                callback();
                return;
            }
            m_CallBacks.Add(new CallBackElement(dateTime, callback));
        }
        /// <summary>
        /// 添加Lua回调函数
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="callback"></param>
        /// <param name="callbackParams"></param>
        public void AddLuaCallBack(long delayTime, string model, string method)
        {
            DateTime dateTime = UnixTimestampToDateTime((long)delayTime);
            if (dateTime <= GetServerTime())
            {
                Util.CallMethod(model, method);
                return;
            }
            m_LuaCallBacks.Add(new LuaCallBackElement(dateTime, model, method, new object[]{}));
        }
        /// <summary>
        /// 添加回调函数
        /// </summary>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        /// <param name="being_callback"></param>
        /// <param name="end_callback"></param>
        public void AddCallBack(DateTime begin, DateTime end, TimeCallBack being_callback, TimeCallBack end_callback)
        {
            DateTime server = GetServerTime();
            if (end < server)
            {
                return;
            }
            else if (begin > server)
            {
                m_CallBacks.Add(new CallBackElement(begin, being_callback));
                m_CallBacks.Add(new CallBackElement(end, end_callback));
            }
            else if (begin <= server && end > server)
            {
                being_callback();
                m_CallBacks.Add(new CallBackElement(end, end_callback));
            }
            else if (begin <= server && end == server)
            {
                being_callback();
                end_callback();
            }
        }
        /// <summary>
        /// 添加结束回调函数
        /// </summary>
        /// <param name="dele"></param>
        public void AddDayEndDelegate(DayEndDelegate dele)
        {
            m_DayEnd += dele;
        }
        /// <summary>
        /// 日期转换成unix时间戳
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static long DateTimeToUnixTimestamp(DateTime dateTime)
        {
            DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            return (long)((dateTime - dtStart).TotalSeconds);
        }
        /// <summary>
        /// unix时间戳转换成日期
        /// </summary>
        /// <param name="unixTimeStamp">时间戳（秒）</param>
        /// <returns></returns>
        public static DateTime UnixTimestampToDateTime(long timestamp)
        {
            //var start = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Unspecified);
            //return start.AddSeconds(timestamp);

            DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            TimeSpan toNow = new TimeSpan(timestamp * 10000000); 
            return dtStart.Add(toNow);
        }
    }

    /// <summary>
    /// 时间回调元素
    /// </summary>
    class CallBackElement
    {
        public DateTime dateTime;
        public TimeCallBack callback;

        public CallBackElement(DateTime dateTime, TimeCallBack callback)
        {
            this.dateTime = dateTime;
            this.callback = callback;
        }
    }

    /// <summary>
    /// Lua时间回调元素
    /// </summary>
    class LuaCallBackElement
    {
        public DateTime dateTime;
        public string LuaModelName;
        public string LuaMethodName;
        public object[] args;

        public LuaCallBackElement(DateTime dateTime, string model, string method, object[] args)
        {
            this.dateTime = dateTime;
            this.LuaModelName = model;
            this.LuaMethodName = method;
            this.args = args;
        }
    }
}
