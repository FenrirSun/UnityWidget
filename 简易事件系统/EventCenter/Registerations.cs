using System;
///<summary>
///描述：泛型的委托保留的类 为了保存类型信息
///@作者 Ollydbg
///@创建日期 2019-09-26 13-48-51
///@版本号 1.0
///</summary>
public class Registerations<T> : IRegisteration where T : EditorEventBase
{
    /// <summary>
    /// 委托支持 前期初始化 防止空
    /// </summary>
    public Action<T> OnReceives = obj => { };

    /// <summary>
    /// 增加回调
    /// </summary>
    /// <param name="_obj"></param>
    public void AddCallBack(object _obj)
    {
        var callback = _obj as Action<T>;

        OnReceives += callback;
    }

    /// <summary>
    /// 移除回调
    /// </summary>
    /// <param name="_obj"></param>
    public void RemoveCallBack(object _obj)
    {
        var callback = _obj as Action<T>;

        OnReceives -= callback;
    }
}
