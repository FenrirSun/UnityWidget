///<summary>
///描述：丢掉泛型信息的接口 无实际用处
///@作者 Ollydbg
///@创建日期 2019-09-26 13-47-59
///@版本号 1.0
///</summary>
public interface IRegisteration
{
    /// <summary>
    /// 移除回调
    /// </summary>
    /// <param name="_obj"></param>
    void RemoveCallBack(object _obj);

    /// <summary>
    /// 添加回调
    /// </summary>
    /// <param name="_obj"></param>
    void AddCallBack(object _obj);
}
