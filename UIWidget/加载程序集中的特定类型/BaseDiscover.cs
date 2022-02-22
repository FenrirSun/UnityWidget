using System;
using System.Collections.Generic;

/// <summary>
/// 通过attribute发现模块的基类
/// @data: 2019-01-16
/// @author: LLL
/// </summary>
public abstract class BaseDiscover
{
    public class AttributeCompare<T> : IComparer<T> where T : Attribute
    {
        public int Compare(T x, T y) => x.GetHashCode() - y.GetHashCode();
    }

    /// <summary>
    /// 发现模块
    /// </summary>
    /// <param name="_type"></param>
    /// <param name="_attribute"></param>
    public abstract void OnDiscoverModule(Type _type, Attribute _attribute);

    /// <summary>
    /// 清理
    /// </summary>
    public abstract void OnClear();
}
