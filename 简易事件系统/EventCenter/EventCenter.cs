using System;
using System.Collections.Generic;
using UnityEngine;
///<summary>
///描述：事件中心 简化运行时事件机制 专门做一个用于编辑器的事件处理重新
///@作者 Ollydbg
///@创建日期 2019-09-26 13-46-59
///@版本号 1.0
///</summary>
public static class EventCenter
{


    private const string ExceptionMessage = "类型信息不匹配";

    /// <summary>
    /// 消息中心存储
    /// </summary>
    private static Dictionary<Type, IRegisteration> typeEventDic = new Dictionary<Type, IRegisteration>();

    /// <summary>
    /// 注册消息处理函数
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="_onReceive"></param>
    public static void Register<T>(Action<T> _onReceive) where T : EditorEventBase
    {
        var type = typeof(T);

        if (typeEventDic.TryGetValue(type, out IRegisteration iregisteration))
        {
            var registeration = iregisteration as Registerations<T>;

            if (registeration != null)
            {
                registeration.OnReceives += _onReceive;
            }
            else
            {
                throw new Exception(ExceptionMessage);
            }
        }
        else
        {
            var reg = new Registerations<T>();

            reg.OnReceives += _onReceive;

            //抹掉类型信息
            typeEventDic.Add(type, reg);
        }
    }
    /// <summary>
    /// 非泛型的添加注册函数
    /// </summary>
    /// <param name="type"></param>
    /// <param name="_onReceive"></param>
    public static void Register(Type type, object _onReceive)
    {
        if (typeEventDic.TryGetValue(type, out IRegisteration iregisteration))
        {
            iregisteration.AddCallBack(_onReceive);
        }
        else
        {
            var regType = typeof(Registerations<>);
            var genType = regType.MakeGenericType(type);
            var reg = Activator.CreateInstance(genType) as IRegisteration;

            reg.AddCallBack(_onReceive);

            //抹掉类型信息
            typeEventDic.Add(type, reg);
        }
    }

    /// <summary>
    /// 取消注册函数
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="_onReceive"></param>
    public static void UnRegister<T>(Action<T> _onReceive) where T : EditorEventBase
    {
        var type = typeof(T);

        if (typeEventDic.TryGetValue(type, out IRegisteration ireg))
        {
            var reg = ireg as Registerations<T>;

            if (reg != null)
            {
                reg.OnReceives -= _onReceive;
            }
            else
            {
                throw new Exception(ExceptionMessage);
            }
        }
        else
        {
            Debug.LogWarning("移除的时候，发现尚未注册" + type.ToString());
        }
    }

    /// <summary>
    /// 非泛型态的移除注册实现
    /// </summary>
    /// <param name="type"></param>
    /// <param name="_onReceive"></param>
    public static void UnRegister(Type type, object _onReceive)
    {
        if (typeEventDic.TryGetValue(type, out IRegisteration ireg))
        {
            ireg.RemoveCallBack(_onReceive);
        }
        else
        {
            Debug.LogWarning("移除的时候，发现尚未注册" + type.ToString());
        }
    }

    /// <summary>
    /// 发送消息
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="t"></param>
    public static void SendEvent<T>(T t) where T : EditorEventBase
    {
        var type = typeof(T);

        if (typeEventDic.TryGetValue(type, out IRegisteration ireg))
        {
            var reg = ireg as Registerations<T>;

            if (reg != null)
            {
                reg.OnReceives(t);
            }
            else
            {
                throw new Exception(ExceptionMessage);
            }
        }
    }

    /// <summary>
    /// 清理全部的消息注册
    /// </summary>
    public static void Clear()
    {
        typeEventDic.Clear();
    }
}
