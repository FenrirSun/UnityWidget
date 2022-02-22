using System;

/// <summary>
/// 发现 attribute
/// @data: 2019-01-16
/// @author: LLL
/// </summary>
public class DiscoverAttribute : Attribute
{
    public Type attributeType { get; private set; }

    /// <summary>
    /// 构造方法
    /// </summary>
    /// <param name="_type"></param>
    public DiscoverAttribute(Type _type)
    {
        this.attributeType = _type;
    }
}
